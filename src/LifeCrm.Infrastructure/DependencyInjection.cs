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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Config abstraction — keeps IConfiguration out of Application layer
        services.AddSingleton<IAppSettings, AppSettingsService>();

        services.AddScoped<AuditSaveInterceptor>();
        services.AddDbContext<AppDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            opts.AddInterceptors(sp.GetRequiredService<AuditSaveInterceptor>());
        });

        services.AddScoped<ICurrentUserService,        CurrentUserService>();
        services.AddScoped<IUnitOfWork,                UnitOfWork>();
        services.AddScoped<IOrganizationReader,        OrganizationReader>();
        services.AddScoped<IAuditLogWriter,            AuditLogWriter>();
        services.AddScoped<ICsvService,                CsvService>();
        services.AddScoped<IPdfService,                PdfService>();
        services.AddScoped<IEmailService,              EmailService>();
        services.AddScoped<ISignalRSettings,           SignalRSettingsService>();
        services.AddScoped<IUnsubscribeTokenService,   UnsubscribeTokenService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
