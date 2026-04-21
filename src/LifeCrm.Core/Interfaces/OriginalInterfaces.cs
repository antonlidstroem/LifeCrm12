using System.Linq.Expressions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Interfaces;

// ── Repository interfaces ─────────────────────────────────────────────────────

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IReadOnlyList<Campaign>> GetActiveAsync(CancellationToken ct = default);
}

public interface IInteractionRepository : IRepository<Interaction>
{
    Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default);
    Task<IReadOnlyList<Interaction>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
}

// ── Service interfaces ────────────────────────────────────────────────────────

public interface ICurrentUserService
{
    Guid?   UserId         { get; }
    Guid?   OrganizationId { get; }
    string? UserRole       { get; }
    bool    IsAuthenticated { get; }
}

public interface IActivityNotifier
{
    Task NotifyAsync(string orgId, string eventType, object payload, CancellationToken ct = default);
}

public interface IAppSettings
{
    string AppBaseUrl            { get; }
    string JwtSecretKey          { get; }
    EmailSettingsDto DefaultEmailSettings { get; }
}

public interface IEmailSettingsService
{
    Task<EmailSettingsDto> GetAsync(CancellationToken ct = default);
    Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default);
}

public record EmailSettingsDto
{
    public string Host      { get; init; } = string.Empty;
    public int    Port      { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    public string Password  { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName  { get; init; } = string.Empty;
    public bool   UseSsl    { get; init; } = true;
    public bool   DryRun    { get; init; } = false;
}

public record EmailAttachment(byte[] Bytes, string FileName, string ContentType);

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null, CancellationToken ct = default);
}

public interface ICsvService
{
    Task<byte[]> ExportAsync<T>(IEnumerable<T> rows);
    Task<(List<T> Rows, List<CsvParseError> Errors)> ImportAsync<T>(byte[] csvBytes);
}

public record CsvParseError(int RowNumber, string Reason, string RawRow);

public interface IPdfService
{
    Task<byte[]> GenerateDonationReceiptAsync(Donation donation, Organization org, string receiptNumber);
    Task<byte[]> GenerateDonationSummaryAsync(Contact contact, IEnumerable<Donation> donations,
        Organization org, DateOnly from, DateOnly to);
}

public interface IOrganizationReader
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveDocumentAsync(Document document, CancellationToken ct = default);
}

public interface ISignalRSettings
{
    Task<bool> GetEnabledAsync(CancellationToken ct = default);
    Task SetEnabledAsync(bool enabled, CancellationToken ct = default);
}

public interface IUnsubscribeTokenService
{
    string GenerateToken(Guid contactId, Guid organizationId);
}
