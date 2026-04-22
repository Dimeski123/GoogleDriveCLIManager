using GoogleDriveCLIManager.Presentation.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text;

namespace GoogleDriveCLIManager.Presentation.CLI;

public class AppEngine
{
    private readonly ITypeRegistrar _registrar;

    public AppEngine(ITypeRegistrar registrar)
    {
        _registrar = registrar;
    }

    public async Task<int> RunAsync(string[] args)
    {
        var app = new CommandApp(_registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("gdrive-cli");
            config.SetApplicationVersion("1.0.0");

            config.AddCommand<SyncCliCommand>("sync")
                  .WithDescription("Synchronize Google Drive with local folder.");

            config.AddCommand<SearchCliCommand>("search")
                  .WithDescription("Search files in Google Drive.");

            config.AddCommand<UploadCliCommand>("upload")
                  .WithDescription("Upload a file to Google Drive.");

            config.AddCommand<LogoutCliCommand>("logout")
                      .WithDescription("Sign out of the current Google account.");
        });

        if (args.Length > 0)
        {
            return await app.RunAsync(args);
        }

        AnsiConsole.Clear();
        PrintHeader();

        while (true)
        {
            var input = AnsiConsole.Ask<string>("[bold yellow]gdrive>[/]");

            if (string.IsNullOrWhiteSpace(input)) continue;

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[grey]Goodbye![/]");
                break;
            }

            if (input.Trim().Equals("clear", StringComparison.OrdinalIgnoreCase) ||
                    input.Trim().Equals("cls", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.Clear();
                PrintHeader();
                continue;
            }

            if (input.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await app.RunAsync(new[] { "-h" });
                AnsiConsole.WriteLine();
                continue;
            }

            var parsedArgs = ParseArguments(input);

            try
            {
                await app.RunAsync(parsedArgs);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"\n[bold red]Command Error:[/] {ex.Message}");
            }

            AnsiConsole.WriteLine(); 
        }

        return 0;
    }

    private string[] ParseArguments(string commandLine)
    {
        var args = new List<string>();
        var inQuotes = false;
        var currentArg = new StringBuilder();

        foreach (var c in commandLine)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }
    private void PrintHeader()
    {
        AnsiConsole.Write(
            new FigletText("GDrive CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[grey]Welcome to the Google Drive CLI Manager.[/]");
        AnsiConsole.MarkupLine("Type [green]help[/] to see commands, or [red]exit[/] to quit.\n");
    }
}