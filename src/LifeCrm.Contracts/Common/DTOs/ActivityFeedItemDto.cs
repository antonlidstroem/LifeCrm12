namespace LifeCrm.Contracts.Common.DTOs;
public record ActivityFeedItemDto
{
    public string ActivityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string ContactName { get; init; } = string.Empty;
    public Guid ContactId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
