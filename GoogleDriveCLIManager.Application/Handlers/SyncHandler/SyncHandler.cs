using GoogleDriveCLIManager.Application.Configuration;
using GoogleDriveCLIManager.Application.DTOs;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.Entities;
using GoogleDriveCLIManager.Domain.RepositoryInterfaces;
using GoogleDriveCLIManager.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace GoogleDriveCLIManager.Application.Handlers.SyncHandler;

public class SyncHandler
{
    private readonly IGoogleDriveClient _googleDriveClient;
    private readonly ILocalFileSystem _localFileSystem;
    private readonly IManifestRepository _manifestRepository;
    private readonly SyncOptions _options;

    public SyncHandler(IGoogleDriveClient googleDriveClient, ILocalFileSystem localFileSystem, IManifestRepository manifestRepository, IOptions<SyncOptions> options)
    {
        _googleDriveClient = googleDriveClient;
        _localFileSystem = localFileSystem;
        _manifestRepository = manifestRepository;
        _options = options.Value;
    }

    public async Task<SyncSummaryDto> HandleAsync(SyncCommand command, CancellationToken cancellationToken)
    {
        // Setting up the statistics object
        var stats = new SyncStatistics();
        stats.StartTimer();

        // activeDirectory is variable used to check if the user has inserted target directory
        string activeDirectory = string.IsNullOrWhiteSpace(command.TargetDirectory)
            ? _options.DefaultDownloadPath
            : command.TargetDirectory;

        _localFileSystem.CreateDirectory(activeDirectory);

        // Fetching the Manifest and the items from Google
        var manifestEntries = await _manifestRepository
            .LoadManifestAsync(cancellationToken);

        var cloudFiles = await _googleDriveClient
            .GetAllItemsAsync(null, cancellationToken);

        var currentCloudIds = cloudFiles.Select(c => c.Id).ToHashSet();

        foreach (var receipt in manifestEntries.Values)
        {
            if (!currentCloudIds.Contains(receipt.DriveFileId))
            {
                if (_localFileSystem.FileExists(receipt.LocalPath))
                {
                    _localFileSystem.DeleteFile(receipt.LocalPath);
                    stats.RecordDeleted();
                }
            }
        }

        var updatedManifest = new ConcurrentDictionary<string, Manifest>(StringComparer.OrdinalIgnoreCase);
        var downloadPlans = new List<DriveFileItem>();

        // Loop thorugh each file checking the path, creating the empty folders if requested, checking and updating the Manifest and
        // adding the Files to the Queue
        foreach (var cloudFile in cloudFiles)
        {
            var expectedLocalPath = Path.Combine(activeDirectory, cloudFile.FullCloudPath);

            if (cloudFile.IsFolder)
            {
                if (command.CreateEmptyFolders)
                    _localFileSystem.CreateDirectory(expectedLocalPath);
                continue;
            }

            stats.RecordCloudFileFound();

            manifestEntries.TryGetValue(cloudFile.Id, out var existingReceipt);
            bool localFileExists = _localFileSystem.FileExists(expectedLocalPath);

            if (existingReceipt != null && existingReceipt.IsUpToDate(cloudFile, expectedLocalPath, localFileExists))
            {
                stats.RecordSkipped();
                updatedManifest[cloudFile.Id] = existingReceipt;
            }
            else
            {
                stats.RecordScheduled();
                downloadPlans.Add(cloudFile);
            }
        }
        // Parallel Engine that download the files from the Cloud
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxParallelDownloads,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(downloadPlans, parallelOptions, async (file, token) =>
        {
            var savePath = Path.Combine(activeDirectory, file.FullCloudPath);

            if (file.IsGoogleWorkspaceFile && !savePath.EndsWith(file.GetExportExtension(), StringComparison.OrdinalIgnoreCase))
            {
                savePath += file.GetExportExtension();
            }
        try
            {
                await using (var fileStream = _localFileSystem.CreateFileStream(savePath))
                {
                    await _googleDriveClient.DownloadFileAsStreamAsync(file, fileStream, token);
                }

                stats.RecordSuccess(file.SizeBytes?.Bytes ?? 0);

                updatedManifest[file.Id] = Manifest.Create(file, savePath);
            }
            catch (Exception ex)
            {
                stats.RecordFailure(file.Name, ex.Message);

                if (_localFileSystem.FileExists(savePath))
                    _localFileSystem.DeleteFile(savePath);
            }
        });

        await _manifestRepository.SaveManifestAsync(updatedManifest.Values, cancellationToken);
        stats.StopTimer();

        
        var totalSize = FileSize.Create(stats.DownloadedBytes);

        return new SyncSummaryDto(
            stats.TotalCloudFiles,
            stats.ScheduledForDownload,
            stats.SkippedUpToDate,
            stats.SuccessfulDownloads,
            stats.FailedDownloads,
            stats.DeletedFiles,
            totalSize.ToString(),
            stats.TotalTime,
            stats.Errors
        );
    
    }
}
