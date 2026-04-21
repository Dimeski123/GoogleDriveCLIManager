namespace GoogleDriveCLIManager.Application.Handlers.SyncHandler;

public record SyncCommand(
    string? TargetDirectory = null,
    bool CreateEmptyFolders = false)
{
}
