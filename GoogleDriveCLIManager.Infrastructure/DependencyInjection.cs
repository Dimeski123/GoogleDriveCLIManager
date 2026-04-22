using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.RepositoryInterfaces;
using GoogleDriveCLIManager.Infrastructure.FileSystem;
using GoogleDriveCLIManager.Infrastructure.Google;
using GoogleDriveCLIManager.Infrastructure.Interfaces;
using GoogleDriveCLIManager.Infrastructure.Repositories.Manifest;
using GoogleDriveCLIManager.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleDriveCLIManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILocalFileSystem, LocalFileSystem>();
        services.AddSingleton<IManifestRepository, ManifestRepository>();
        services.AddSingleton<IRetryPolicyWrapper, RetryPolicyWrapper>();
        services.AddSingleton<IGoogleDriveClient, GoogleDriveClient>();

        services.AddSingleton<GoogleAuthenticator>();
        services.AddSingleton<IGoogleAuthenticator>(sp => sp.GetRequiredService<GoogleAuthenticator>());
        services.AddSingleton<IGoogleInternalAuthenticator>(sp => sp.GetRequiredService<GoogleAuthenticator>());

        return services;
    }
}