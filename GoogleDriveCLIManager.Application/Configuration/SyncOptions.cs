namespace GoogleDriveCLIManager.Application.Configuration;

public class SyncOptions
{
    public const string SectionName = "SyncSettings";
    public int MaxParallelDownloads { get; init; } = 4;
    public string DefaultDownloadPath { get; init; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads", "GoogleDriveRoot");
    public int RetryCount { get; init; } = 3;
    public int RetryDelaySeconds { get; init; } = 2;
}
