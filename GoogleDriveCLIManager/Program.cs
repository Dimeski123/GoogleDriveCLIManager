using GoogleDriveCLIManager.Application;
using GoogleDriveCLIManager.Infrastructure;
using GoogleDriveCLIManager.Presentation;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace GoogleDriveCLIManager
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);
                    services.AddPresentation();
                })
                .Build();

            await host.RunAsync();
        }
    }
}