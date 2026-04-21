using System.Text.Json;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Persists outgoing email (SMTP) configuration as a JSON blob in the AppSettings table
/// under the key "Email:Settings".
///
/// The SMTP password is encrypted at rest using the ASP.NET Core Data Protection API
/// before being written to the DB. It is decrypted transparently on read.
/// This closes the plaintext-password-in-DB vulnerability identified in the GDPR audit.
/// </summary>
public class EmailSettingsService : IEmailSettingsService
{
    private const string SettingsKey = "Email:Settings";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppSettings         _appConfig;
    private readonly IDataProtector       _protector;
    private readonly ILogger<EmailSettingsService> _logger;

    private volatile EmailSettingsDto? _cached;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;
    private readonly object _cacheLock  = new();

    public EmailSettingsService(
        IServiceScopeFactory scopeFactory,
        IAppSettings appConfig,
        IDataProtectionProvider dataProtection,
        ILogger<EmailSettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _appConfig    = appConfig;
        _logger       = logger;
        // Separate purpose string from PII field encryption — keys are independent
        _protector    = dataProtection.CreateProtector("LifeCrm.EmailSettings.Password.v1");
    }

    public async Task<EmailSettingsDto> GetAsync(CancellationToken ct = default)
    {
        lock (_cacheLock)
        {
            if (_cached is not null && DateTimeOffset.UtcNow < _cacheExpiry)
                return _cached;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == SettingsKey, ct);

            if (row is not null && !string.IsNullOrWhiteSpace(row.Value))
            {
                var fromDb = JsonSerializer.Deserialize<EmailSettingsDto>(row.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fromDb is not null)
                {
                    // Decrypt the password
                    var decrypted = DecryptPassword(fromDb.Password);
                    var settings  = fromDb with { Password = decrypted };

                    lock (_cacheLock)
                    {
                        _cached      = settings;
                        _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
                    }
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not read email settings from DB. Falling back to appsettings.json.");
        }

        return _appConfig.DefaultEmailSettings;
    }

    public async Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default)
    {
        // Encrypt the password before persistence
        var encrypted = EncryptPassword(settings.Password);
        var toStore   = settings with { Password = encrypted };
        var json      = JsonSerializer.Serialize(toStore);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == SettingsKey, ct);

            if (row is null)
            {
                row = new Core.Entities.AppSettings
                {
                    Id    = Guid.NewGuid(),
                    Key   = SettingsKey,
                    Value = json
                };
                db.AppSettings.Add(row);
            }
            else
            {
                row.Value = json;
                db.Entry(row).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            await db.SaveChangesAsync(ct);

            // Immediately update cache with the plaintext version (callers get plaintext)
            lock (_cacheLock)
            {
                _cached      = settings;
                _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
            }

            _logger.LogInformation("Email settings updated and cached (password encrypted).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email settings to DB.");
            throw;
        }
    }

    private string EncryptPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return password;
        try { return _protector.Protect(password); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt SMTP password — storing empty string.");
            return string.Empty;
        }
    }

    private string DecryptPassword(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return ciphertext;
        try { return _protector.Unprotect(ciphertext); }
        catch
        {
            // Value may have been stored before encryption was introduced.
            // Return as-is and log — a re-save via the admin UI will encrypt it.
            _logger.LogWarning(
                "Failed to decrypt SMTP password — value may be a legacy plaintext entry. " +
                "Re-save the email settings via Admin → E-postinställningar to encrypt it.");
            return ciphertext;
        }
    }
}
