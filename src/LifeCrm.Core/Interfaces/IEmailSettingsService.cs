namespace LifeCrm.Core.Interfaces;

public record EmailSettingsDto
{
    public string Host        { get; init; } = string.Empty;
    public int    Port        { get; init; } = 587;
    public string Username    { get; init; } = string.Empty;
    public string Password    { get; init; } = string.Empty;
    public string FromEmail   { get; init; } = string.Empty;
    public string FromName    { get; init; } = string.Empty;
    public bool   UseSsl      { get; init; } = true;
    public bool   DryRun      { get; init; } = false;
}

/// <summary>
/// Reads and writes outgoing email configuration from the database AppSettings table.
/// This allows admins to configure SMTP without touching appsettings.json.
/// </summary>
public interface IEmailSettingsService
{
    Task<EmailSettingsDto> GetAsync(CancellationToken ct = default);
    Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default);
}
