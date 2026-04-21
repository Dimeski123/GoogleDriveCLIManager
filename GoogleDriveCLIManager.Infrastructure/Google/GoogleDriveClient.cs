using Google.Apis.Drive.v3;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.Entities;
using GoogleDriveCLIManager.Domain.ValueObjects;
using GoogleDriveCLIManager.Infrastructure.Interfaces;
using GoogleFile = global::Google.Apis.Drive.v3.Data.File;

namespace GoogleDriveCLIManager.Infrastructure.Google;

public class GoogleDriveClient : IGoogleDriveClient
{
    private readonly IGoogleInternalAuthenticator _googleInternalAuthenticator;
    private readonly IRetryPolicyWrapper _retryPolicyWrapper;
    private const string FolderMimeType = "application/vnd.google-apps.folder";

    public GoogleDriveClient(IGoogleInternalAuthenticator googleInternalAuthenticator, IRetryPolicyWrapper retryPolicyWrapper)
    {
        _googleInternalAuthenticator = googleInternalAuthenticator;
        _retryPolicyWrapper = retryPolicyWrapper;
    }

    public async Task DownloadFileAsStreamAsync(string fileId, Stream destinationStream, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator
            .GetDriveServiceAsync(cancellationToken);

        var request = driveService.Files.Get(fileId);

        await _retryPolicyWrapper
            .ExecuteAsync(async () =>
            {
                var status = await request.DownloadAsync(destinationStream, cancellationToken);

                if (status.Status == global::Google.Apis.Download.DownloadStatus.Failed)
                {
                    throw new Exception($"Google API Download Failed: {status.Exception?.Message}");
                }
            });
    }

    public async Task<IReadOnlyList<DriveFileItem>> GetAllItemsAsync(string? searchQuery = null, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator.GetDriveServiceAsync(cancellationToken);

        var result = new List<DriveFileItem>();
        string? pageToken = null;

        do
        {
            var request = driveService.Files.List();
            request.Fields = "nextPageToken, files(id, name, mimeType, size, md5Checksum, modifiedTime, parents)";
            request.PageToken = pageToken;

            var queryParts = new List<string> { "trashed = false" };
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                queryParts.Add($"name contains '{searchQuery}'");
            }
            request.Q = string.Join(" and ", queryParts);

            // Use the wrapper to safely fetch the response
            var response = await _retryPolicyWrapper.ExecuteAsync(async () =>
                await request.ExecuteAsync(cancellationToken)
            );

            if (response.Files != null)
            {
                foreach (var googleFile in response.Files)
                {
                    result.Add(MapToDomain(googleFile));
                }
            }

            pageToken = response.NextPageToken;

        } while (pageToken != null);

        return result;
    }

    public async Task<DriveFileItem> UploadFileAsync(Stream fileStream, string fileName, string? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator.GetDriveServiceAsync(cancellationToken);

        var fileMetadata = new GoogleFile()
        {
            Name = fileName
        };

        if (!string.IsNullOrWhiteSpace(parentFolderId))
        {
            fileMetadata.Parents = new List<string> { parentFolderId };
        }


        var request = driveService.Files.Create(fileMetadata, fileStream, "application/octet-stream");
        request.Fields = "id, name, mimeType, size, md5Checksum, modifiedTime, parents";

        var progress = await _retryPolicyWrapper
            .ExecuteAsync(async () =>
                await request.UploadAsync(cancellationToken)
            );

        if (progress.Status == global::Google.Apis.Upload.UploadStatus.Failed)
        {
            throw new Exception($"Google API Upload Failed: {progress.Exception?.Message}");
        }

        return MapToDomain(request.ResponseBody);
    }

    private DriveFileItem MapToDomain(GoogleFile googleFile)
    {
        bool isFolder = googleFile.MimeType == FolderMimeType;

        // Safely handle our Value Objects
        FileSize? fileSize = googleFile.Size.HasValue ? FileSize.Create(googleFile.Size.Value) : null;
        Checksum? checksum = !string.IsNullOrWhiteSpace(googleFile.Md5Checksum) ? Checksum.Create(googleFile.Md5Checksum) : null;

        return new DriveFileItem
        {
            Id = googleFile.Id ?? string.Empty,
            Name = googleFile.Name ?? string.Empty,
            IsFolder = isFolder,
            SizeBytes = fileSize,
            Md5Checksum = checksum,
            ModifiedTimeUtc = googleFile.ModifiedTime?.ToUniversalTime(),
            Parents = googleFile.Parents?.ToArray() ?? Array.Empty<string>()
        };
    }
}
