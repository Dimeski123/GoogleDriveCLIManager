namespace GoogleDriveCLIManager.Application.DTOs;

public record SearchResultDto(
    string Id,
    string Name,
    bool IsFolder,
    string FormattedSize,
    DateTime? ModifiedTimeUtc,
    string FullCloudPath,
    string SyncStatus);

