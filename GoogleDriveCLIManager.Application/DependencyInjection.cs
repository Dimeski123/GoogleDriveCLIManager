using GoogleDriveCLIManager.Application.Configuration;
using GoogleDriveCLIManager.Application.Handlers.LogoutHandler;
using GoogleDriveCLIManager.Application.Handlers.SearchHandler;
using GoogleDriveCLIManager.Application.Handlers.SyncHandler;
using GoogleDriveCLIManager.Application.Handlers.UpdateHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleDriveCLIManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncOptions>(configuration.GetSection("SyncOptions"));

        services.AddScoped<SyncHandler>();
        services.AddScoped<SearchHandler>();
        services.AddScoped<UploadHandler>();
        services.AddScoped<LogoutHandler>();
        return services;
    }
}
