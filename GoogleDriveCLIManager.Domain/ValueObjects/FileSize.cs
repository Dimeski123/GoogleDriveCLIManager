using System.Text.Json.Serialization;

namespace GoogleDriveCLIManager.Domain.ValueObjects;

public record FileSize
{
    [JsonConstructor]
    private FileSize(long bytes)
    {
        Bytes = bytes;
    }

    public long Bytes { get; init; }

    public static FileSize Create(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("File size cannot be negative.");

        return new FileSize(bytes);
    }

    public double Kilobytes => Bytes / 1024.0;
    public double Megabytes => Kilobytes / 1024.0;
    public double Gigabytes => Megabytes / 1024.0;

    public override string ToString()
    {
        if (Gigabytes >= 1) return $"{Gigabytes:F2} GB";
        if (Megabytes >= 1) return $"{Megabytes:F2} MB";
        if (Kilobytes >= 1) return $"{Kilobytes:F2} KB";

        return $"{Bytes} B";
    }
}

