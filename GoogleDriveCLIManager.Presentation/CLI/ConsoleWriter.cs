using GoogleDriveCLIManager.Application.DTOs;
using Spectre.Console;

namespace GoogleDriveCLIManager.Presentation.CLI;

public class ConsoleWriter
{
    public void PrintHeader(string title)
    {
        AnsiConsole.Write(new Rule($"[bold blue]{title}[/]").RuleStyle("grey").LeftJustified());
    }

    public void PrintError(string message)
    {
        AnsiConsole.MarkupLine($"[bold red]Error:[/] {message}");
    }

    public void PrintSyncSummary(SyncSummaryDto summary)
    {
        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("Metric");
        table.AddColumn("Value");

        table.AddRow("Cloud Files Found", summary.TotalCloudFiles.ToString());
        table.AddRow("Skipped (Up To Date)", $"[green]{summary.SkippedUpToDate}[/]");
        table.AddRow("Successfully Downloaded", $"[blue]{summary.SuccessfulDownloads}[/]");
        table.AddRow("Failed Downloads", summary.FailedDownloads > 0 ? $"[red]{summary.FailedDownloads}[/]" : "0");
        table.AddRow("Local Files Deleted", summary.DeletedLocalFiles > 0 ? $"[yellow]{summary.DeletedLocalFiles}[/]" : "0");
        table.AddRow("Total Download Size", summary.DownloadedSizeFormatted);
        table.AddRow("Time Elapsed", summary.TotalTime.ToString(@"hh\:mm\:ss"));

        AnsiConsole.Write(table);

        if (summary.Errors.Count > 0)
        {
            var errorTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Red)
                .Title("[bold red]⚠️Sync Failed Files[/]");

            errorTable.AddColumn("[red]File Name[/]");
            errorTable.AddColumn("[red]Error Reason[/]");

            foreach (var error in summary.Errors)
            {
                var parts = error.Split(':', 2);

                string fileName = parts.Length > 0 ? parts[0].Trim() : "Unknown";
                string errorMessage = parts.Length > 1 ? parts[1].Trim() : error;

                errorTable.AddRow($"[white]{fileName}[/]", $"[grey]{errorMessage}[/]");
            }
        }
    }

    public void PrintSearchResults(IReadOnlyList<SearchResultDto> results)
    {
        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No files found matching your search criteria.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("Name");
        table.AddColumn("Type");
        table.AddColumn("Size");
        table.AddColumn("Sync Status");
        table.AddColumn("Cloud Path");

        foreach (var file in results)
        {
            string typeIcon = file.IsFolder ? "[blue]Folder[/]" : "[grey]File[/]";

            string statusColored = file.SyncStatus switch
            {
                "Up To Date" => $"[green]{file.SyncStatus}[/]",
                "Not Downloaded" => $"[red]{file.SyncStatus}[/]",
                "Needs Sync (Outdated)" => $"[yellow]{file.SyncStatus}[/]",
                _ => file.SyncStatus // Default fallback
            };

            table.AddRow(
                $"[bold]{file.Name}[/]",
                typeIcon,
                file.FormattedSize,
                statusColored,
                file.FullCloudPath
            );
        }

        AnsiConsole.Write(table);
    }

    public void PrintUploadSummary(UploadResultDto result)
    {
        var panel = new Panel(
            new Markup(
                $"File: [bold]{result.FileName}[/]\n" +
                $"Size: {result.FormattedSize}\n" +
                $"Time: {result.UploadTime:hh\\:mm\\:ss}\n" +
                $"Status: {(result.IsSuccess ? "[green]Success[/]" : "[red]Failed[/]")}\n" +
                $"Message: {result.Message}"
            )
        );

        panel.Header = new PanelHeader(result.IsSuccess ? "Upload Complete" : "Upload Error");
        panel.BorderColor(result.IsSuccess ? Color.Green : Color.Red);
        panel.Padding(1, 1, 1, 1);

        AnsiConsole.Write(panel);
    }
}
