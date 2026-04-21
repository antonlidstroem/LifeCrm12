using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Infrastructure.Services;

public class SignalRSettingsService : ISignalRSettings
{
    private readonly IServiceScopeFactory _scopeFactory;
    public SignalRSettingsService(IServiceScopeFactory f) => _scopeFactory = f;

    public async Task<bool> GetEnabledAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var val = await db.AppSettings.IgnoreQueryFilters()
            .Where(s => s.Key == "SignalR:Enabled")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);
        return val is null || val == "true";
    }

    public async Task SetEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.AppSettings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == "SignalR:Enabled", ct);
        if (row is null)
        {
            db.AppSettings.Add(new Core.Entities.AppSettings
            {
                Id = Guid.NewGuid(), Key = "SignalR:Enabled", Value = enabled.ToString().ToLower()
            });
        }
        else { row.Value = enabled.ToString().ToLower(); }
        await db.SaveChangesAsync(ct);
    }
}
