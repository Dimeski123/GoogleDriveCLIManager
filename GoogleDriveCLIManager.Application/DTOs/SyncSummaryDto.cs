namespace GoogleDriveCLIManager.Application.DTOs;

public record SyncSummaryDto(
    int TotalCloudFiles,
    int ScheduledForDownload,
    int SkippedUpToDate,
    int SuccessfulDownloads,
    int FailedDownloads,
    int DeletedLocalFiles,
    string DownloadedSizeFormatted,
    TimeSpan TotalTime,
    IReadOnlyCollection<string> Errors);
