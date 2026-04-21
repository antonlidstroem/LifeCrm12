using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence.Repositories;

public class InteractionRepository : GenericRepository<Interaction>, IInteractionRepository
{
    public InteractionRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default)
        => await _set.Where(i => i.ContactId == contactId)
                     .OrderByDescending(i => i.OccurredAt)
                     .ToListAsync(ct);

    public async Task<IReadOnlyList<Interaction>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await _set.Where(i => i.ProjectId == projectId)
                     .OrderByDescending(i => i.OccurredAt)
                     .ToListAsync(ct);
}
