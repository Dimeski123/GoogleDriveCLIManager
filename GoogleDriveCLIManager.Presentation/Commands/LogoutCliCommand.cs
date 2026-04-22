using GoogleDriveCLIManager.Application.Handlers.LogoutHandler;
using Spectre.Console.Cli;
using Spectre.Console;

namespace GoogleDriveCLIManager.Presentation.Commands;

public class LogoutCliCommand : AsyncCommand<EmptyCommandSettings>
{
    private readonly LogoutHandler _handler;

    public LogoutCliCommand(LogoutHandler handler)
    {
        _handler = handler;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("red"))
            .StartAsync("Revoking Google credentials and clearing local cache...", async ctx =>
            {
                await _handler.HandleAsync(cancellationToken);
            });

        AnsiConsole.MarkupLine("[bold green]Successfully logged out.[/] The next command you run will prompt for a new Google Login.");

        return 0;
    }
}