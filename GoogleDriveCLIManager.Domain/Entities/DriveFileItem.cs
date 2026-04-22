using GoogleDriveCLIManager.Domain.ValueObjects;

namespace GoogleDriveCLIManager.Domain.Entities;

public class DriveFileItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public FileSize? SizeBytes { get; init; }
    public Checksum? Md5Checksum { get; init; }
    public DateTime? ModifiedTimeUtc { get; init; }
    public string[] Parents { get; init; } = Array.Empty<string>();
    public string FullCloudPath { get; set; } = string.Empty;

    public bool IsGoogleWorkspaceFile =>
        !string.IsNullOrEmpty(MimeType) &&
        MimeType.StartsWith("application/vnd.google-apps.") &&
        !IsFolder;

    public string GetExportMimeType()
    {
        return MimeType switch
        {
            "application/vnd.google-apps.document" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.google-apps.spreadsheet" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.google-apps.presentation" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "application/vnd.google-apps.drawing" => "application/pdf",
            "application/vnd.google-apps.script" => "application/vnd.google-apps.script+json",
            _ => "application/pdf"
        };
    }
    public string GetExportExtension()
    {
        return MimeType switch
        {
            "application/vnd.google-apps.document" => ".docx",
            "application/vnd.google-apps.spreadsheet" => ".xlsx",
            "application/vnd.google-apps.presentation" => ".pptx",
            "application/vnd.google-apps.drawing" => ".pdf",
            "application/vnd.google-apps.script" => ".json",
            _ => ".pdf"
        };
    }
}
