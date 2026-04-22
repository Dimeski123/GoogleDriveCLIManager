using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Infrastructure.Interfaces;

namespace GoogleDriveCLIManager.Infrastructure.Google;

public class GoogleAuthenticator : IGoogleAuthenticator, IGoogleInternalAuthenticator
{
    private DriveService? _cachedDriveService;

    private readonly string _credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Secrets", "client_secret.json");
    private readonly string _tokenDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GoogleCLITokens");

    public async Task<DriveService> GetDriveServiceAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_credentialsPath))
        {
            throw new FileNotFoundException(
                "Missing Google Credentials.\n" +
                "  [grey]You must provide your own OAuth client secret to use this tool.[/]\n" +
                "  [grey]1. Go to the Google Cloud Console.[/]\n" +
                "  [grey]2. Download your OAuth 2.0 Client ID as a JSON file.[/]\n" +
                $"  [grey]3. Rename it to[/] [bold white]client_secret.json[/] [grey]and place it here:[/]\n" +
                $"  [yellow]{_credentialsPath}[/]"
            );
        }

        if (_cachedDriveService != null)
            return _cachedDriveService;

        if (!File.Exists(_credentialsPath))
        {
            throw new FileNotFoundException(
                $"Google OAuth client secret file was not found at '{_credentialsPath}'. " +
                "Place your client_secret.json there before running the CLI.");
        }

        Directory.CreateDirectory(_tokenDirectory);

        UserCredential credential;

        using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                cancellationToken,
                new FileDataStore(_tokenDirectory, true));
        }

        _cachedDriveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GoogleDriveCLIManager"
        });

        return _cachedDriveService;
    }
    public async Task AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        await GetDriveServiceAsync(cancellationToken);
    }

    public async Task<string> GetCurrentUserEmailAsync(CancellationToken cancellationToken = default)
    {
        var service = await GetDriveServiceAsync(cancellationToken);

        var aboutRequest = service.About.Get();
        aboutRequest.Fields = "user/emailAddress";

        var aboutResponse = await aboutRequest
            .ExecuteAsync(cancellationToken);

        return aboutResponse.User.EmailAddress ?? "Unknown User";
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_tokenDirectory))
        {
            Directory.Delete(_tokenDirectory, true);
        }

        return Task.CompletedTask;
    }
}
