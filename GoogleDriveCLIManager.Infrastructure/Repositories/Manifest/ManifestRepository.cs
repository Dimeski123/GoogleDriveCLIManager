using GoogleDriveCLIManager.Application.Configuration;
using GoogleDriveCLIManager.Application.Interfaces;
using GoogleDriveCLIManager.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GoogleDriveCLIManager.Infrastructure.Repositories.Manifest;

public class ManifestRepository : IManifestRepository
{
    private readonly ILocalFileSystem _localFileSystem;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly string _manifestFilePath;

    public ManifestRepository(ILocalFileSystem localFileSystem, IOptions<SyncOptions> options)
    {
        _localFileSystem = localFileSystem;

        _manifestFilePath = Path.Combine(options.Value.DefaultDownloadPath, ".drive_manifest.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyDictionary<string, Domain.Entities.Manifest>> LoadManifestAsync(CancellationToken cancellationToken = default)
    {
        if (!_localFileSystem.FileExists(_manifestFilePath))
        {
            return new Dictionary<string, Domain.Entities.Manifest>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var stream = _localFileSystem.OpenReadStream(_manifestFilePath);

            var entries = await JsonSerializer.DeserializeAsync<Dictionary<string, Domain.Entities.Manifest>>(stream, _jsonOptions, cancellationToken);

            return entries ?? new Dictionary<string, Domain.Entities.Manifest>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, Domain.Entities.Manifest>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task SaveManifestAsync(IEnumerable<Domain.Entities.Manifest> entries, CancellationToken cancellationToken = default)
    {
        var dictionaryToSave = entries.ToDictionary(e => e.DriveFileId, e => e);

        using var stream = _localFileSystem.CreateFileStream(_manifestFilePath);

        await JsonSerializer.SerializeAsync(stream, dictionaryToSave, _jsonOptions, cancellationToken);
    }
}
