using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces;

public interface IOrganizationReader
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveDocumentAsync(Document document, CancellationToken ct = default);
}
