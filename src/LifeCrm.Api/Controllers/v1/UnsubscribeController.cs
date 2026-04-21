using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Api.Controllers.v1;

[AllowAnonymous]
public class UnsubscribeController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAppSettings _appSettings;
    private readonly IConsentService _consent;
    private readonly ILogger<UnsubscribeController> _logger;

    public UnsubscribeController(
        AppDbContext db,
        IAppSettings appSettings,
        IConsentService consent,
        ILogger<UnsubscribeController> logger)
    {
        _db          = db;
        _appSettings = appSettings;
        _consent     = consent;
        _logger      = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Unsubscribe(
        [FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return HtmlPage("Ogiltig länk", "Avregistreringslänken saknar en token.", ok: false);

        if (!TryDecodeToken(token, out var contactId, out var orgId))
            return HtmlPage("Ogiltig länk", "Länken är ogiltig eller har löpt ut.", ok: false);

        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                c => c.Id == contactId && c.OrganizationId == orgId && !c.IsDeleted, ct);

        if (contact is null)
            return HtmlPage("Kontakt hittades inte", "Vi kunde inte hitta dina uppgifter.", ok: false);

        if (contact.EmailOptOut)
            return HtmlPage("Redan avregistrerad", "E-postadressen är redan avregistrerad.", ok: true);

        var now = DateTimeOffset.UtcNow;

        // ── Update contact ────────────────────────────────────────────────────
        contact.EmailOptOut       = true;
        contact.EmailOptOutAt     = now;          // GDPR: durable timestamp
        contact.EmailOptOutSource = "unsubscribe-link";
        contact.LastModifiedAt    = now;
        contact.LastModifiedBy    = "unsubscribe-link";

        // ── Write durable AuditLog record (existing table — no new infrastructure) ──
        // This is the "first move" from the design document: a tamper-evident,
        // structured record that a regulator can inspect as proof of opt-out.
        _db.AuditLogs.Add(new AuditLog
        {
            Id             = Guid.NewGuid(),
            OrganizationId = contact.OrganizationId,
            EntityName     = nameof(Contact),
            EntityId       = contact.Id,
            Action         = "EmailOptOut",
            ChangedBy      = "unsubscribe-link",
            ChangedAt      = now,
            NewValues      = JsonSerializer.Serialize(new
            {
                EmailOptOut       = true,
                EmailOptOutAt     = now,
                EmailOptOutSource = "unsubscribe-link"
            })
        });

        await _db.SaveChangesAsync(ct);

        // ── Write ConsentRecord (withdrawal of MarketingEmail consent) ─────────
        // Done after SaveChanges so a ConsentService failure doesn't block opt-out.
        try
        {
            await _consent.WithdrawAsync(
                contactId:         contact.Id,
                consentType:       ConsentType.MarketingEmail,
                source:            "unsubscribe-link",
                recordedByUserId:  null,
                ct:                ct);
        }
        catch (Exception ex)
        {
            // Consent withdrawal failure must not prevent the opt-out from completing.
            _logger.LogError(ex,
                "ConsentRecord withdrawal failed for contact {ContactId} via unsubscribe link. " +
                "EmailOptOut flag and AuditLog are already committed.",
                contactId);
        }

        _logger.LogInformation(
            "Contact {ContactId} in org {OrgId} unsubscribed via email link at {Time}.",
            contactId, orgId, now);

        return HtmlPage("Avregistrerad", "Du har avregistrerats från våra e-postutskick.", ok: true);
    }

    private bool TryDecodeToken(string token, out Guid contactId, out Guid orgId)
    {
        contactId = Guid.Empty;
        orgId     = Guid.Empty;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return false;

            static byte[] FromBase64Url(string s)
            {
                s  = s.Replace('-', '+').Replace('_', '/');
                s += new string('=', (4 - s.Length % 4) % 4);
                return Convert.FromBase64String(s);
            }

            var payloadBytes = FromBase64Url(parts[0]);
            var sigBytes     = FromBase64Url(parts[1]);

            using var hmac      = new HMACSHA256(Encoding.UTF8.GetBytes(_appSettings.JwtSecretKey));
            var expectedSig     = hmac.ComputeHash(payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(sigBytes, expectedSig)) return false;

            var payload  = Encoding.UTF8.GetString(payloadBytes);
            var segments = payload.Split(':');
            if (segments.Length != 2) return false;

            contactId = Guid.Parse(segments[0]);
            orgId     = Guid.Parse(segments[1]);
            return true;
        }
        catch { return false; }
    }

    private ContentResult HtmlPage(string title, string message, bool ok) =>
        Content($@"<!DOCTYPE html>
<html lang=""sv"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>{title} — LifeCrm</title>
  <style>
    body {{ font-family: sans-serif; display:flex; justify-content:center;
           align-items:center; min-height:100vh; margin:0; background:#f5f5f5; }}
    .card {{ background:#fff; border-radius:8px; padding:40px 48px;
             box-shadow:0 2px 12px rgba(0,0,0,.1); max-width:480px; text-align:center; }}
    h1 {{ font-size:1.5rem; margin-bottom:12px; color:{(ok ? "#2e7d32" : "#c62828")}; }}
    p {{ color:#555; line-height:1.6; }}
  </style>
</head>
<body>
  <div class=""card"">
    <h1>{title}</h1>
    <p>{message}</p>
  </div>
</body>
</html>", "text/html");
}
