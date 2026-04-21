using GoogleDriveCLIManager.Application.DTOs;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.ValueObjects;
using System.Diagnostics;

namespace GoogleDriveCLIManager.Application.Handlers.UpdateHandler;

public class UploadHandler
{
    private readonly IGoogleDriveClient _googleDriveClient;
    private readonly ILocalFileSystem _localFileSystem;
    public UploadHandler(IGoogleDriveClient googleDriveClient, ILocalFileSystem localFileSystem)
    {
        _googleDriveClient = googleDriveClient;
        _localFileSystem = localFileSystem;
    }

    public async Task<UploadResultDto> HandleAsync(UploadCommand command, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (!_localFileSystem.FileExists(command.LocalFilePath))
        {
            stopwatch.Stop();

            return new UploadResultDto(
                FileName: Path.GetFileName(command.LocalFilePath),
                CloudFileId: string.Empty,
                FormattedSize: "0 B",
                UploadTime: TimeSpan.Zero,
                IsSuccess: false,
                Message: "Error: Local file does not exist."
            );
        }

        string fileName = Path.GetFileName(command.LocalFilePath);
        long bytes = _localFileSystem.GetFileSizeBytes(command.LocalFilePath);
        var fileSize = FileSize.Create(bytes);

        try
        {
            using var fileStream = _localFileSystem
                .OpenReadStream(command.LocalFilePath);

            var uploadedCloudFile = await _googleDriveClient
                .UploadFileAsync(
                    fileStream,
                    fileName,
                    command.TargetCloudFolderId,
                    cancellationToken
                );

            stopwatch.Stop();

            return new UploadResultDto(
                FileName: fileName,
                CloudFileId: uploadedCloudFile.Id,
                FormattedSize: fileSize.ToString(),
                UploadTime: stopwatch.Elapsed,
                IsSuccess: true,
                Message: "Successfully uploaded to Google Drive."
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // 5. Return failure safely
            return new UploadResultDto(
                FileName: fileName,
                CloudFileId: string.Empty,
                FormattedSize: fileSize.ToString(),
                UploadTime: stopwatch.Elapsed,
                IsSuccess: false,
                Message: $"Upload failed: {ex.Message}"
            );
        }
    }
}
