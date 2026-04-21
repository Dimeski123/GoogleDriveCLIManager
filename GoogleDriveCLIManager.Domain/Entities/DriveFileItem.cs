using GoogleDriveCLIManager.Domain.ValueObjects;

namespace GoogleDriveCLIManager.Domain.Entities;

public class DriveFileItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public FileSize? SizeBytes { get; init; }
    public Checksum? Md5Checksum { get; init; }
    public DateTime? ModifiedTimeUtc { get; init; }
    public string[] Parents { get; init; } = Array.Empty<string>();
    public string FullCloudPath { get; set; } = string.Empty;
}
