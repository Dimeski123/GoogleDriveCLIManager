using GoogleDriveCLIManager.Domain.ValueObjects;

namespace GoogleDriveCLIManager.Domain.Entities;

public class Manifest
{
    public string DriveFileId { get; init; } = string.Empty;
    public string LocalPath { get; init; } = string.Empty;
    public Checksum? Md5Checksum { get; init; }
    public DateTime? ModifiedTimeUtc { get; init; }

    public bool IsUpToDate(DriveFileItem cloudFile, string currentExpectedLocalPath, bool localFileExistsOnDisk)
    {
        if (!localFileExistsOnDisk)
            return false;

        if (!string.Equals(this.LocalPath, currentExpectedLocalPath, StringComparison.OrdinalIgnoreCase))
            return false;

        if (this.ModifiedTimeUtc.HasValue && cloudFile.ModifiedTimeUtc.HasValue)
        {
            if (this.ModifiedTimeUtc.Value != cloudFile.ModifiedTimeUtc.Value)
                return false;
        }

        if (this.Md5Checksum != null && cloudFile.Md5Checksum != null)
        {
            if (this.Md5Checksum != cloudFile.Md5Checksum)
                return false;
        }

        return true;
    }

        public static Manifest Create(DriveFileItem file, string localPath)
        {
            return new Manifest
            {
                DriveFileId = file.Id,
                LocalPath = localPath,
                Md5Checksum = file.Md5Checksum,
                ModifiedTimeUtc = file.ModifiedTimeUtc
            };
        }
}
