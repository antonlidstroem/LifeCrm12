using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.BackgroundServices;
using LifeCrm.Infrastructure.Encryption;
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
        // ── Encryption ────────────────────────────────────────────────────
        services.AddFieldEncryption(config);

        // ── Config bridge ─────────────────────────────────────────────────
        services.AddSingleton<IAppSettings, AppSettingsService>();

        // ── Database ──────────────────────────────────────────────────────
        services.AddScoped<AuditSaveInterceptor>();
        services.AddScoped<PropertyAuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            opts.AddInterceptors(sp.GetRequiredService<AuditSaveInterceptor>());
            opts.AddInterceptors(sp.GetRequiredService<PropertyAuditInterceptor>());
        });

        // ── Core services ─────────────────────────────────────────────────
        services.AddScoped<ICurrentUserService,    CurrentUserService>();
        services.AddScoped<IUnitOfWork,            UnitOfWork>();
        services.AddScoped<IOrganizationReader,    OrganizationReader>();
        services.AddScoped<IAuditLogWriter,        AuditLogWriter>();
        services.AddScoped<ICsvService,            CsvService>();
        services.AddScoped<IPdfService,            PdfService>();

        // ── GDPR services ─────────────────────────────────────────────────
        services.AddScoped<IConsentService,        ConsentService>();
        services.AddSingleton<IEmailHashService,   EmailHashService>();

        // ── Human Growth services (new) ───────────────────────────────────
        services.AddScoped<IEngagementService,     EngagementService>();

        // ── Email ─────────────────────────────────────────────────────────
        services.AddSingleton<IEmailSettingsService, EmailSettingsService>();
        services.AddScoped<IEmailService,            EmailService>();

        // ── Real-time / Token ─────────────────────────────────────────────
        services.AddSingleton<ISignalRSettings,          SignalRSettingsService>();
        services.AddScoped<IUnsubscribeTokenService,     UnsubscribeTokenService>();

        // ── Background services ───────────────────────────────────────────
        services.AddHostedService<AuditOutboxProcessor>();
        services.AddHostedService<DsrExportProcessor>();
        services.AddHostedService<RetentionCleanupJob>();

        // ── Seeder ────────────────────────────────────────────────────────
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
