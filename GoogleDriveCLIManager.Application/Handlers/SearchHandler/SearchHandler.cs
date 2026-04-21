using GoogleDriveCLIManager.Application.Configuration;
using GoogleDriveCLIManager.Application.DTOs;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;

namespace GoogleDriveCLIManager.Application.Handlers.SearchHandler;

public class SearchHandler
{
    private readonly IGoogleDriveClient _googleDriveClient;
    private readonly IManifestRepository _manifestRepository;
    private readonly ILocalFileSystem _localFileSystem;
    private readonly SyncOptions _options;

    public SearchHandler(IGoogleDriveClient googleDriveClient, IManifestRepository manifestRepository, ILocalFileSystem localFileSystem, IOptions<SyncOptions> options)
    {
        _googleDriveClient = googleDriveClient;
        _manifestRepository = manifestRepository;
        _localFileSystem = localFileSystem;
        _options = options.Value;
    }
    public async Task<IReadOnlyList<SearchResultDto>> HandleAsync(SearchCommand command, CancellationToken cancellationToken)
    {
        string activeDirectory = string.IsNullOrWhiteSpace(command.TargetDirectory)
            ? _options.DefaultDownloadPath
            : command.TargetDirectory;

        var cloudFiles = await _googleDriveClient.GetAllItemsAsync(command.SearchTerm, cancellationToken);

        var manifestEntries = await _manifestRepository.LoadManifestAsync(cancellationToken);

        var results = new List<SearchResultDto>();

        foreach (var file in cloudFiles)
        {
            string statusText = "-";

            if (!file.IsFolder)
            {
                var expectedLocalPath = Path.Combine(activeDirectory, file.FullCloudPath);
                bool localFileExists = _localFileSystem.FileExists(expectedLocalPath);

                manifestEntries.TryGetValue(file.Id, out var receipt);

                if (receipt != null && receipt.IsUpToDate(file, expectedLocalPath, localFileExists))
                {
                    statusText = "File Exists and it is Up To Date";
                }
                else if (localFileExists)
                {
                    statusText = "File Exists but it Needs Sync (Outdated)";
                }
                else
                {
                    statusText = "Not Downloaded";
                }
            }

            // 4. Map to DTO
            results.Add(new SearchResultDto(
                Id: file.Id,
                Name: file.Name,
                IsFolder: file.IsFolder,
                FormattedSize: file.SizeBytes?.ToString() ?? "-",
                ModifiedTimeUtc: file.ModifiedTimeUtc,
                FullCloudPath: file.FullCloudPath,
                SyncStatus: statusText
            ));
        }

        return results;
    }



    //var domainFiles = await _googleDriveClient.GetAllItemsAsync(query.SearchTerm, cancellationToken);

    //var result = domainFiles.Select(file => new SearchResultDto(
    //        Id: file.Id,
    //        Name: file.Name,
    //        IsFolder: file.IsFolder,
    //        FormattedSize: file.SizeBytes?.ToString() ?? "-", 
    //        ModifiedTimeUtc: file.ModifiedTimeUtc,
    //        FullCloudPath: file.FullCloudPath
    //    )).ToList();
}
