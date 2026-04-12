using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IReadOnlyList<Campaign>> GetActiveAsync(CancellationToken ct = default);
}
