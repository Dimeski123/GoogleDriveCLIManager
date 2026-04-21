using GoogleDriveCLIManager.Domain.RepositoryInterfaces;

namespace GoogleDriveCLIManager.Infrastructure.Repositories.Manifest;

public class ManifestRepository : IManifestRepository
{
    public Task<IReadOnlyDictionary<string, Domain.Entities.Manifest>> LoadManifestAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveManifestAsync(IEnumerable<Domain.Entities.Manifest> entries, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
