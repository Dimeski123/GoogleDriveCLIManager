using GoogleDriveCLIManager.Presentation.CLI;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleDriveCLIManager.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<ConsoleWriter>();
        return services;
    }
}