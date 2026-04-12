using System.ComponentModel.DataAnnotations;

namespace LifeCrm.Application.Campaigns.DTOs;

public record SendNewsletterRequest
{
    [Required][MaxLength(500)] public string Subject { get; init; } = string.Empty;
    [Required][MaxLength(100_000)] public string HtmlBody { get; init; } = string.Empty;
    [MaxLength(500)] public string? TagFilter { get; init; }
}

public record NewsletterResultDto
{
    public int SentCount    { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount   { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

public record NewsletterPreviewDto
{
    public int EligibleCount { get; init; }
    public int OptedOutCount { get; init; }
    public int NoEmailCount  { get; init; }
}
