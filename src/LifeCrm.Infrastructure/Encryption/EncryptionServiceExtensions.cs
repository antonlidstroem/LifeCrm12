// src/LifeCrm.Infrastructure/Encryption/EncryptionServiceExtensions.cs
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeCrm.Infrastructure.Encryption;

public static class EncryptionServiceExtensions
{
    public static IServiceCollection AddFieldEncryption(
        this IServiceCollection services, IConfiguration config)
    {
        // Keys stored in a dedicated DB table — survives app restarts and scale-out.
        // In production, configure Azure Key Vault or similar HSM here.
        services.AddDataProtection()
            .SetApplicationName("LifeCrm")
            .PersistKeysToDbContext<AppDbContext>() // EF Core key store
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        services.AddSingleton<EncryptedStringConverter>(sp =>
        {
            var protector = sp.GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("LifeCrm.PiiFields.v1");
            return new EncryptedStringConverter(protector);
        });

        return services;
    }
}