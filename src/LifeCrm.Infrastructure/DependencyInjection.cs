using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using LifeCrm.Infrastructure.Persistence.Interceptors;
using LifeCrm.Infrastructure.Persistence.Repositories;
using LifeCrm.Infrastructure.Persistence.Seeders;
using LifeCrm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // ── Config bridge (Singleton — stateless, reads IConfiguration) ──────────
        services.AddSingleton<IAppSettings, AppSettingsService>();

        // ── Database ──────────────────────────────────────────────────────────────
        services.AddScoped<AuditSaveInterceptor>();
        services.AddDbContext<AppDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            opts.AddInterceptors(sp.GetRequiredService<AuditSaveInterceptor>());
        });

        // ── Core services ─────────────────────────────────────────────────────────
        services.AddScoped<ICurrentUserService,    CurrentUserService>();
        services.AddScoped<IUnitOfWork,            UnitOfWork>();
        services.AddScoped<IOrganizationReader,    OrganizationReader>();
        services.AddScoped<IAuditLogWriter,        AuditLogWriter>();
        services.AddScoped<ICsvService,            CsvService>();
        services.AddScoped<IPdfService,            PdfService>();

        // ── Email ─────────────────────────────────────────────────────────────────
        // EmailSettingsService is Singleton: holds in-memory SMTP config cache.
        // It uses IServiceScopeFactory to access the Scoped AppDbContext safely.
        services.AddSingleton<IEmailSettingsService, EmailSettingsService>();
        // EmailService is Scoped: calls IEmailSettingsService per request.
        services.AddScoped<IEmailService, EmailService>();

        // ── Real-time / Token ─────────────────────────────────────────────────────
        services.AddSingleton<ISignalRSettings,          SignalRSettingsService>();
        services.AddScoped<IUnsubscribeTokenService,     UnsubscribeTokenService>();

        // ── Seeder ────────────────────────────────────────────────────────────────
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
