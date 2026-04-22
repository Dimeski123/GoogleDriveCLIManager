using GoogleDriveCLIManager.Presentation.CLI;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using GoogleDriveCLIManager.Application.Handlers.SearchHandler;

namespace GoogleDriveCLIManager.Presentation.Commands;

public class SearchCliCommand : AsyncCommand<SearchCliCommand.Settings>
{
    private readonly SearchHandler _searchHandler;
    private readonly ConsoleWriter _console;

    public SearchCliCommand(SearchHandler searchHandler, ConsoleWriter console)
    {
        _searchHandler = searchHandler;
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[query]")]
        [Description("The text to search for. Leave empty to list all files.")]
        public string? SearchTerm { get; init; }

        [CommandOption("-p|--path")]
        [Description("Optional override to check sync status against a different local folder.")]
        public string? TargetDirectoryOverride { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        string title = string.IsNullOrWhiteSpace(settings.SearchTerm)
            ? "Listing All Cloud Files"
            : $"Searching for: '{settings.SearchTerm}'";

        _console.PrintHeader(title);

        var query = new SearchCommand(
            SearchTerm: settings.SearchTerm,
            settings.TargetDirectoryOverride
        );

        var results = await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Querying Google Drive...", async ctx =>
            {
                return await _searchHandler.HandleAsync(query, cancellationToken);
            });

        _console.PrintSearchResults(results);

        return 0;
    }
}