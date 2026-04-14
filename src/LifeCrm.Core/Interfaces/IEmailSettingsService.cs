namespace LifeCrm.Core.Interfaces;

/// <summary>DTO for outgoing email (SMTP) configuration.</summary>
public record EmailSettingsDto
{
    public string Host      { get; init; } = string.Empty;
    public int    Port      { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    /// <summary>
    /// Password stored in DB (plain text in DB — use secret manager or encrypted column in prod).
    /// When returned via the API this is masked as "••••••••".
    /// </summary>
    public string Password  { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName  { get; init; } = string.Empty;
    public bool   UseSsl    { get; init; } = true;
    public bool   DryRun    { get; init; } = false;
}

/// <summary>
/// Reads and writes outgoing email (SMTP) configuration from the AppSettings DB table.
/// Falls back to appsettings.json values when no DB override has been saved.
/// </summary>
public interface IEmailSettingsService
{
    Task<EmailSettingsDto> GetAsync(CancellationToken ct = default);
    Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default);
}
