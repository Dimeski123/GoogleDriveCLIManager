namespace GoogleDriveCLIManager.Application.Handlers.UpdateHandler;

public record UploadCommand(
    string LocalFilePath,
    string? TargetCloudFolderId = null)
{
}
