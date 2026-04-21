using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LifeCrm.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Sets CreatedAt, LastModifiedAt, LastModifiedBy on save.
/// Works alongside PropertyAuditInterceptor — this runs first (entity timestamps),
/// then the property interceptor captures the diff for the outbox.
/// </summary>
public class AuditSaveInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditSaveInterceptor(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is null) return new(result);
        var now    = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId?.ToString() ?? "system";

        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = now;
                entry.Entity.LastModifiedBy = userId;
            }
        }
        return new(result);
    }
}
