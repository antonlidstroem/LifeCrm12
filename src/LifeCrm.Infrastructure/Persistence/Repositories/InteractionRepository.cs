// src/LifeCrm.Infrastructure/Persistence/Repositories/InteractionRepository.cs
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence.Repositories;

public class InteractionRepository : GenericRepository<Interaction>, IInteractionRepository
{
    public InteractionRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Interaction>> GetByContactAsync(
        Guid contactId, CancellationToken ct = default)
        // FIX F: .Take(200) moved into EF query — SQL applies TOP 200 before
        // transferring rows. Previously the handler called .Take(200) on the
        // already-materialised list, loading all rows first.
        => await _set
            .Include(i => i.Contact)
            .Include(i => i.Project)
            .Where(i => i.ContactId == contactId)
            .OrderByDescending(i => i.OccurredAt)
            .Take(200)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Interaction>> GetByProjectAsync(
        Guid projectId, CancellationToken ct = default)
        // FIX F: Same fix applied to project interaction queries.
        => await _set
            .Include(i => i.Contact)
            .Include(i => i.Project)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.OccurredAt)
            .Take(200)
            .ToListAsync(ct);
}
