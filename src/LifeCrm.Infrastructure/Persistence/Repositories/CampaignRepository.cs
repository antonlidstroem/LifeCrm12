using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence.Repositories;

public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Campaign>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(c => c.Status == CampaignStatus.Active)
                     .OrderBy(c => c.Name)
                     .ToListAsync(ct);
}
