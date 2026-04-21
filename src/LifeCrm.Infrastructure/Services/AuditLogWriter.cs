using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;

namespace LifeCrm.Infrastructure.Services;

public interface IAuditLogWriter
{
    Task WriteAsync(Guid orgId, string entityName, Guid entityId, string action,
        string changedBy, string? newValues = null, CancellationToken ct = default);
}

public class AuditLogWriter : IAuditLogWriter
{
    private readonly AppDbContext _db;
    public AuditLogWriter(AppDbContext db) => _db = db;

    public async Task WriteAsync(Guid orgId, string entityName, Guid entityId, string action,
        string changedBy, string? newValues = null, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            EntityName     = entityName,
            EntityId       = entityId,
            Action         = action,
            ChangedBy      = changedBy,
            ChangedAt      = DateTimeOffset.UtcNow,
            NewValues      = newValues
        });
        await _db.SaveChangesAsync(ct);
    }
}
