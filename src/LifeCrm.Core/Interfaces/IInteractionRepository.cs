using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces;

public interface IInteractionRepository : IRepository<Interaction>
{
    Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default);
    Task<IReadOnlyList<Interaction>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
}
