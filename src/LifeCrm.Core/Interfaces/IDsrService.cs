namespace LifeCrm.Core.Interfaces;

public interface IDsrService
{
    Task<ContactDataExport> ExportAsync(Guid contactId, CancellationToken ct = default);
    Task AnonymizeAsync(Guid contactId, string reason, bool retainFinancialRecords = true, CancellationToken ct = default);
    Task HardDeleteAsync(Guid contactId, CancellationToken ct = default);
}

public record ContactDataExport
{
    public DateTimeOffset ExportedAt { get; init; }
    public string RequestedBy { get; init; } = string.Empty;
    public ContactExportDto Subject { get; init; } = null!;
}

public record ContactExportDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? StateProvince { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Tags { get; init; }
    public string? Notes { get; init; }
    public bool EmailOptOut { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<ConsentExportDto> ConsentHistory { get; init; } = [];
    public IReadOnlyList<DonationExportDto> Donations { get; init; } = [];
    public IReadOnlyList<InteractionExportDto> Interactions { get; init; } = [];
    public IReadOnlyList<DocumentExportDto> Documents { get; init; } = [];
    public IReadOnlyList<AuditExportDto> AuditTrail { get; init; } = [];
}

public record ConsentExportDto
{
    public string ConsentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PolicyVersion { get; init; } = string.Empty;
    public string LegalBasis { get; init; } = string.Empty;
    public string? Channel { get; init; }
    public string RecordedBy { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

public record DonationExportDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DateOnly Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? PaymentMethod { get; init; }
    public string? ReferenceNumber { get; init; }
}

public record InteractionExportDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}

public record DocumentExportDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public bool Available { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; }
}

public record AuditExportDto
{
    public string Action { get; init; } = string.Empty;
    public string? Property { get; init; }
    public string ChangedBy { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
}