using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Gdpr.DTOs;

// ── Consent ──────────────────────────────────────────────────────────────────

public record ConsentEntryDto
{
    public ConsentType    ConsentType   { get; init; }
    public bool           IsGranted     { get; init; }
    public string         PolicyVersion { get; init; } = string.Empty;
    public string         Source        { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt    { get; init; }
    public Guid?          RecordedByUserId { get; init; }
}

public record ConsentStatusDto
{
    public Guid   ContactId { get; init; }

    /// <summary>Latest state per consent type (the "current" view).</summary>
    public IReadOnlyList<ConsentSummaryDto> Current { get; init; } = Array.Empty<ConsentSummaryDto>();

    /// <summary>Full history — all grant and withdrawal events, newest first.</summary>
    public IReadOnlyList<ConsentEntryDto> History { get; init; } = Array.Empty<ConsentEntryDto>();
}

public record ConsentSummaryDto
{
    public ConsentType    ConsentType { get; init; }
    public bool           IsGranted   { get; init; }
    public DateTimeOffset Since       { get; init; }
}

public record UpdateConsentRequest
{
    [Required]
    public ConsentType ConsentType   { get; init; }

    [Required]
    public bool        IsGranted     { get; init; }

    [Required]
    [MaxLength(50)]
    public string      PolicyVersion { get; init; } = string.Empty;

    [MaxLength(100)]
    public string      Source        { get; init; } = "admin-manual";

    [MaxLength(45)]
    public string?     IpAddress     { get; init; }
}

// ── DSR Export ───────────────────────────────────────────────────────────────

public record DsrExportJobDto
{
    public Guid    JobId            { get; init; }
    public string  Status           { get; init; } = string.Empty;
    public string? StatusUrl        { get; init; }
    public string? DownloadUrl      { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public int?    EstimatedSeconds { get; init; }
}

// ── Anonymization ─────────────────────────────────────────────────────────────

public record AnonymizeContactResult
{
    public Guid                    ContactId        { get; init; }
    public DateTimeOffset          AnonymizedAt     { get; init; }
    public IReadOnlyList<string>   FieldsAnonymized { get; init; } = Array.Empty<string>();
}

// ── DSR full payload (written to DsrExportJob.ExportPayload) ─────────────────

public record DsrContactExport
{
    public Guid   ContactId       { get; init; }
    public string ExportVersion   { get; init; } = "1.0";
    public DateTimeOffset ExportedAt { get; init; }

    public DsrContactProfile?      Profile      { get; init; }
    public IReadOnlyList<DsrDonation>    Donations    { get; init; } = Array.Empty<DsrDonation>();
    public IReadOnlyList<DsrInteraction> Interactions { get; init; } = Array.Empty<DsrInteraction>();
    public IReadOnlyList<DsrConsent>     Consents     { get; init; } = Array.Empty<DsrConsent>();
}

public record DsrContactProfile
{
    public string  FirstName           { get; init; } = string.Empty;
    public string  LastName            { get; init; } = string.Empty;
    public string? Email               { get; init; }
    public string? Phone               { get; init; }
    public string? AddressLine1        { get; init; }
    public string? City                { get; init; }
    public string? Country             { get; init; }
    public string? Tags                { get; init; }
    public bool    EmailOptOut         { get; init; }
    public DateTimeOffset? EmailOptOutAt { get; init; }
    public DateTimeOffset CreatedAt    { get; init; }
}

public record DsrDonation
{
    public Guid    Id            { get; init; }
    public decimal Amount        { get; init; }
    public DateOnly Date         { get; init; }
    public string  Status        { get; init; } = string.Empty;
    public string? CampaignName  { get; init; }
    public string? ProjectName   { get; init; }
    public string? PaymentMethod { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record DsrInteraction
{
    public Guid    Id          { get; init; }
    public string  Type        { get; init; } = string.Empty;
    public string? Subject     { get; init; }
    public string  Body        { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}

public record DsrConsent
{
    public ConsentType    ConsentType   { get; init; }
    public bool           IsGranted     { get; init; }
    public string         PolicyVersion { get; init; } = string.Empty;
    public string         Source        { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt    { get; init; }
}
