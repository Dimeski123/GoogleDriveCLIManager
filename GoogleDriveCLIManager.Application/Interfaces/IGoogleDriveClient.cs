using GoogleDriveCLIManager.Domain.Entities;

namespace GoogleDriveCLIManager.Application.Interfaces;

public interface IGoogleDriveClient
{
    Task<IReadOnlyList<DriveFileItem>> GetAllItemsAsync(string? searchQuery = null, CancellationToken cancellationToken = default);
    Task DownloadFileAsStreamAsync(string fileId, Stream destinationStream, CancellationToken cancellationToken = default);
    Task<DriveFileItem> UploadFileAsync(Stream fileStream, string fileName, string? parentFolderId = null, CancellationToken cancellationToken = default);
}
