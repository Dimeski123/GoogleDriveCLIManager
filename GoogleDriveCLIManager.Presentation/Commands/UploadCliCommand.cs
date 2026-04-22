using GoogleDriveCLIManager.Presentation.CLI;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using GoogleDriveCLIManager.Application.Handlers.UpdateHandler;

namespace GoogleDriveCLIManager.Presentation.Commands;

public class UploadCliCommand : AsyncCommand<UploadCliCommand.Settings>
{
    private readonly UploadHandler _uploadHandler;
    private readonly ConsoleWriter _console;

    public UploadCliCommand(UploadHandler uploadHandler, ConsoleWriter console)
    {
        _uploadHandler = uploadHandler;
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<filepath>")]
        [Description("The full local path to the file you want to upload.")]
        public string LocalFilePath { get; init; } = string.Empty;

        [CommandOption("-d|--destination")]
        [Description("Optional Cloud Folder structure where the file should be placed. - If it doesn't exists, the folder/folders will be created")]
        public string? TargetCloudPath { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        _console.PrintHeader("Uploading File");

        var command = new UploadCommand(
            LocalFilePath: settings.LocalFilePath,
            settings.TargetCloudPath
        );

        var result = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow3)
            .SpinnerStyle(Style.Parse("yellow"))
            .StartAsync("Uploading securely to Google Drive...", async ctx =>
            {
                return await _uploadHandler.HandleAsync(command, cancellationToken);
            });

        _console.PrintUploadSummary(result);

        return result.IsSuccess ? 0 : 1;
    }
}
