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

    public async Task DownloadFileAsStreamAsync(DriveFileItem file, Stream destinationStream, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator
            .GetDriveServiceAsync(cancellationToken);

        await _retryPolicyWrapper.ExecuteAsync(async () =>
        {
            global::Google.Apis.Download.IDownloadProgress status;

            if (file.IsGoogleWorkspaceFile)
            {
                var exportRequest = driveService.Files.Export(file.Id, file.GetExportMimeType());
                status = await exportRequest.DownloadAsync(destinationStream, cancellationToken);
            }
            else
            {
                var getRequest = driveService.Files.Get(file.Id);
                status = await getRequest.DownloadAsync(destinationStream, cancellationToken);
            }

            if (status.Status == global::Google.Apis.Download.DownloadStatus.Failed)
            {
                throw new Exception($"Google API Download/Export Failed: {status.Exception?.Message}");
            }
        });
    }

    public async Task<IReadOnlyList<DriveFileItem>> GetAllItemsAsync(string? searchQuery = null, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator
            .GetDriveServiceAsync(cancellationToken);

        var allRawFiles = new List<GoogleFile>();
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

            var response = await _retryPolicyWrapper.ExecuteAsync(async () =>
                await request.ExecuteAsync(cancellationToken)
            );

            if (response.Files != null)
            {
                allRawFiles.AddRange(response.Files);
            }

            pageToken = response.NextPageToken;

        } while (pageToken != null);

        var fileLookup = allRawFiles.ToDictionary(f => f.Id, f => f);
        var result = new List<DriveFileItem>();

        foreach (var rawFile in allRawFiles)
        {
            string fullPath = await BuildFullPathAsync(rawFile, fileLookup, driveService, cancellationToken);
            result.Add(MapToDomain(rawFile, fullPath));
        }

        return result;
    }

    public async Task<DriveFileItem> UploadFileAsync(Stream fileStream, string fileName, string? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        var driveService = await _googleInternalAuthenticator
            .GetDriveServiceAsync(cancellationToken);

        var fileMetadata = new GoogleFile
        {
            Name = fileName,
            Parents = !string.IsNullOrWhiteSpace(parentFolderId)
                  ? new List<string> { parentFolderId }
                  : null
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

        return MapToDomain(request.ResponseBody, fileName);
    }
    public async Task<string> GetOrCreateFolderByPathAsync(string folderPath, CancellationToken token)
    {
        var driveService = await _googleInternalAuthenticator
            .GetDriveServiceAsync(token);

        var parts = folderPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        string parentId = "root";

        foreach (var part in parts)
        {
            var query = $"name = '{part}' and mimeType = 'application/vnd.google-apps.folder' and '{parentId}' in parents and trashed = false";
            var request = driveService.Files.List();
            request.Q = query;
            request.Fields = "files(id, name)";

            var response = await request.ExecuteAsync(token);
            var folder = response.Files.FirstOrDefault();

            if (folder != null)
            {
                parentId = folder.Id;
            }
            else
            {
                var newFolderMetadata = new GoogleFile
                {
                    Name = part,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { parentId }
                };

                var createRequest = driveService.Files.Create(newFolderMetadata);
                createRequest.Fields = "id";
                var newFolder = await createRequest.ExecuteAsync(token);

                parentId = newFolder.Id;
            }
        }

        return parentId;
    }

    private DriveFileItem MapToDomain(GoogleFile googleFile, string fullCloudPath)
    {
        bool isFolder = googleFile.MimeType == FolderMimeType;
        FileSize? fileSize = googleFile.Size.HasValue ? FileSize.Create(googleFile.Size.Value) : null;
        Checksum? checksum = !string.IsNullOrWhiteSpace(googleFile.Md5Checksum) ? Checksum.Create(googleFile.Md5Checksum) : null;

        return new DriveFileItem
        {
            Id = googleFile.Id ?? string.Empty,
            Name = googleFile.Name ?? string.Empty,
            MimeType = googleFile.MimeType ?? string.Empty, // <--- Add this line
            IsFolder = isFolder,
            SizeBytes = fileSize,
            Md5Checksum = checksum,
            ModifiedTimeUtc = googleFile.ModifiedTime?.ToUniversalTime(),
            Parents = googleFile.Parents?.ToArray() ?? Array.Empty<string>(),
            FullCloudPath = fullCloudPath
        };
    }

    private async Task<string> BuildFullPathAsync(GoogleFile currentFile, Dictionary<string, GoogleFile> lookup, DriveService driveService, CancellationToken cancellationToken)
    {
        var pathParts = new List<string> { currentFile.Name };
        var current = currentFile;

        while (current.Parents != null && current.Parents.Count > 0)
        {
            string parentId = current.Parents[0];

            if (lookup.TryGetValue(parentId, out var parentFile))
            {
                pathParts.Insert(0, parentFile.Name);
                current = parentFile;
            }
            else
            {
                try
                {
                    var request = driveService.Files.Get(parentId);
                    request.Fields = "id, name, parents";

                    var fallbackParent = await _retryPolicyWrapper.ExecuteAsync(async () =>
                        await request.ExecuteAsync(cancellationToken)
                    );

                    if (fallbackParent.Parents == null || fallbackParent.Parents.Count == 0)
                    {
                        break;
                    }

                    pathParts.Insert(0, fallbackParent.Name);
                    current = fallbackParent;
                    lookup[parentId] = fallbackParent;
                }
                catch
                {
                    break;
                }
            }
        }

        return string.Join("/", pathParts);
    }
}
