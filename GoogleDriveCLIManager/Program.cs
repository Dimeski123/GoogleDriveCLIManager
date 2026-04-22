using GoogleDriveCLIManager.Application;
using GoogleDriveCLIManager.Infrastructure;
using GoogleDriveCLIManager.Presentation;
using GoogleDriveCLIManager.Presentation.CLI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Threading.Tasks;

namespace GoogleDriveCLIManager
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var services = new ServiceCollection();

                services.AddApplication(configuration);
                services.AddInfrastructure();
                services.AddPresentation();

                var registrar = new TypeRegistrar(services);
                var engine = new AppEngine(registrar);

                return await engine.RunAsync(args);
            }
            catch (Exception ex)
            { 

                AnsiConsole.MarkupLine($"\n[bold white on red] FATAL SYSTEM ERROR [/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return 1;
            }
        }
    }
}