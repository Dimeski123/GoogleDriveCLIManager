using GoogleDriveCLIManager.Application.Handlers.SyncHandler;
using GoogleDriveCLIManager.Presentation.CLI;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace GoogleDriveCLIManager.Presentation.Commands;

public class SyncCliCommand : AsyncCommand<SyncCliCommand.Settings>
{
    private readonly SyncHandler _syncHandler;
    private readonly ConsoleWriter _console;

    public SyncCliCommand(SyncHandler syncHandler, ConsoleWriter console)
    {
        _syncHandler = syncHandler;
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-e|--empty-folders")]
        [Description("Set to true to mirror empty folders from Google Drive.")]
        [DefaultValue(false)]
        public bool CreateEmptyFolders { get; init; }

        [CommandOption("-p|--path")]
        [Description("Optional override for the download directory.")]
        public string? TargetDirectoryOverride { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        _console.PrintHeader("Synchronizing Google Drive");

        var appCommand = new SyncCommand(
            settings.TargetDirectoryOverride,
            settings.CreateEmptyFolders
        );

        var result = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Analyzing cloud files and syncing...", async ctx =>
            {
                return await _syncHandler.HandleAsync(appCommand, cancellationToken);
            });

        _console.PrintSyncSummary(result);

        return 0;
    }
}