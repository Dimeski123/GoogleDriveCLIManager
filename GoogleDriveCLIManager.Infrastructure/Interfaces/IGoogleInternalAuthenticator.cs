using Google.Apis.Drive.v3;

namespace GoogleDriveCLIManager.Infrastructure.Interfaces;

public interface IGoogleInternalAuthenticator
{
    Task<DriveService> GetDriveServiceAsync(CancellationToken cancellationToken = default);
}
