using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

public class Campaign : TenantEntity
{
    public string Name           { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public decimal? BudgetGoal   { get; set; }
    public DateOnly? StartDate   { get; set; }
    public DateOnly? EndDate     { get; set; }
    public string? Notes         { get; set; }

    /// <summary>
    /// Every campaign must belong to a project.
    /// Nullable in the DB/entity for backward compat with existing data,
    /// but required by the application layer validators.
    /// </summary>
    public Guid? ProjectId   { get; set; }
    public Project? Project  { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
