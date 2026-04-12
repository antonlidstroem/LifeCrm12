using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Newsletters.DTOs;

public record NewsletterListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public NewsletterStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SentAt { get; init; }
    public string? SentBy { get; init; }
    public int SentCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public string? TagFilter { get; init; }
    public string? ContactTypeFilter { get; init; }
    public int AttachmentCount { get; init; }
}

public record NewsletterDetailDto : NewsletterListDto
{
    public string HtmlBody { get; init; } = string.Empty;
    public IReadOnlyList<AttachmentDto> Attachments { get; init; } = Array.Empty<AttachmentDto>();
}

public record AttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
}

public record CreateNewsletterRequest
{
    [Required][MaxLength(300)] public string Title { get; init; } = string.Empty;
    [Required][MaxLength(500)] public string Subject { get; init; } = string.Empty;
    [MaxLength(200_000)] public string HtmlBody { get; init; } = string.Empty;
}

public record UpdateNewsletterRequest
{
    [Required] public Guid Id { get; init; }
    [Required][MaxLength(300)] public string Title { get; init; } = string.Empty;
    [Required][MaxLength(500)] public string Subject { get; init; } = string.Empty;
    [MaxLength(200_000)] public string HtmlBody { get; init; } = string.Empty;
}

public record SendNewsletterRequest
{
    [MaxLength(500)] public string? TagFilter { get; init; }
    [MaxLength(200)] public string? ContactTypeFilter { get; init; }
}

public record NewsletterSendResultDto
{
    public int SentCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

public record NewsletterPreviewDto
{
    public int EligibleCount { get; init; }
    public int OptedOutCount { get; init; }
    public int NoEmailCount  { get; init; }
}
