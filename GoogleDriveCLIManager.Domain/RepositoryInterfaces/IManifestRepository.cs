using GoogleDriveCLIManager.Domain.Entities;

namespace GoogleDriveCLIManager.Domain.RepositoryInterfaces;

public interface IManifestRepository
{
    Task<IReadOnlyDictionary<string, Manifest>> LoadManifestAsync(CancellationToken cancellationToken = default);
    Task SaveManifestAsync(IEnumerable<Manifest> entries, CancellationToken cancellationToken = default);
}
