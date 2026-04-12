using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

public class DecisionCount : TenantEntity
{
    public Guid ReportId          { get; set; }
    public DecisionType DecisionType { get; set; }
    public int Count              { get; set; }
    public MissionReport? Report  { get; set; }
}
