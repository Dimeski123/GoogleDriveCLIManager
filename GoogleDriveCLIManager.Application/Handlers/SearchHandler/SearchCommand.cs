namespace GoogleDriveCLIManager.Application.Handlers.SearchHandler;

public record SearchCommand (
    string? SearchTerm,
    string? TargetDirectory = null)
{
}
