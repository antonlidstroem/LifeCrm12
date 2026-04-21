using System.IO.Compression;
using System.Text;
using System.Text.Json;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.BackgroundServices;

/// <summary>
/// Processes pending DsrExportJob rows by building a full JSON export.
/// Includes EventAttendance data (Human Growth layer addition).
/// </summary>
public class DsrExportProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DsrExportProcessor> _logger;
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DownloadTtl  = TimeSpan.FromHours(24);
    private const int BatchSize = 10;

    public DsrExportProcessor(IServiceScopeFactory scopeFactory, ILogger<DsrExportProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessPendingJobsAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DsrExportProcessor error.");
            }
            await Task.Delay(TickInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingJobsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var jobs = await db.Set<DsrExportJob>()
            .Where(j => j.Status == DsrExportStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var job in jobs)
            await ProcessJobAsync(db, job, ct);
    }

    private async Task ProcessJobAsync(AppDbContext db, DsrExportJob job, CancellationToken ct)
    {
        job.Status = DsrExportStatus.Running;
        await db.SaveChangesAsync(ct);
        try
        {
            var payload    = await BuildExportAsync(db, job.ContactId, ct);
            job.Status     = DsrExportStatus.Completed;
            job.ExportPayload = Compress(payload);
            job.CompletedAt   = DateTimeOffset.UtcNow;
            job.ExpiresAt     = DateTimeOffset.UtcNow.Add(DownloadTtl);
            _logger.LogInformation("DSR export completed for job {JobId}.", job.Id);
        }
        catch (Exception ex)
        {
            job.Status       = DsrExportStatus.Failed;
            job.ErrorMessage = ex.Message;
            _logger.LogError(ex, "DSR export failed for job {JobId}.", job.Id);
        }
        finally { await db.SaveChangesAsync(ct); }
    }

    private static async Task<string> BuildExportAsync(
        AppDbContext db, Guid contactId, CancellationToken ct)
    {
        var contact = await db.Contacts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == contactId, ct);

        var donations = await db.Donations.IgnoreQueryFilters()
            .Where(d => d.ContactId == contactId)
            .Include(d => d.Campaign).Include(d => d.Project)
            .OrderByDescending(d => d.Date).ToListAsync(ct);

        var interactions = await db.Interactions.IgnoreQueryFilters()
            .Where(i => i.ContactId == contactId)
            .OrderByDescending(i => i.OccurredAt).ToListAsync(ct);

        var consents = await db.ConsentRecords.IgnoreQueryFilters()
            .Where(r => r.ContactId == contactId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

        // ── Human Growth addition: EventAttendance ─────────────────────────
        var attendances = await db.Set<EventAttendance>()
            .IgnoreQueryFilters()
            .Where(a => a.ContactId == contactId)
            .Include(a => a.Event)
            .OrderByDescending(a => a.Event!.StartsAt)
            .ToListAsync(ct);

        var export = new
        {
            ContactId   = contactId,
            ExportVersion = "2.0",
            ExportedAt  = DateTimeOffset.UtcNow,
            Profile     = contact == null ? null : new
            {
                contact.FirstName, contact.LastName,
                Email       = contact.Email,
                Phone       = contact.Phone,
                City        = contact.City,
                Country     = contact.Country,
                Tags        = (string?)null, // tags now in ContactTags — omit raw field
                contact.EmailOptOut,
                contact.EmailOptOutAt,
                contact.CreatedAt,
                contact.Source
            },
            Donations = donations.Select(d => new
            {
                d.Id, d.Amount, d.Date, Status = d.Status.ToString(),
                CampaignName = d.Campaign?.Name, ProjectName = d.Project?.Name,
                d.PaymentMethod, d.CreatedAt
            }),
            Interactions = interactions.Select(i => new
            {
                i.Id, Type = i.Type.ToString(), i.Subject, i.Body, i.OccurredAt
            }),
            // ── NEW: Event attendance history ──────────────────────────────
            EventAttendances = attendances.Select(a => new
            {
                a.Id,
                EventId    = a.EventId,
                EventTitle = a.Event?.Title,
                EventType  = a.Event?.Type.ToString(),
                StartsAt   = a.Event?.StartsAt,
                Role       = a.Role.ToString(),
                a.Source,
                a.CreatedAt
            }),
            Consents = consents.Select(r => new
            {
                ConsentType   = r.ConsentType.ToString(),
                r.IsGranted, r.PolicyVersion, r.Source,
                RecordedAt    = r.CreatedAt
            })
        };

        return JsonSerializer.Serialize(export, new JsonSerializerOptions
        {
            WriteIndented        = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static byte[] Compress(string json)
    {
        using var output = new MemoryStream();
        using (var gz = new GZipStream(output, CompressionLevel.Optimal))
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            gz.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }
}
