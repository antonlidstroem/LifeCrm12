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
        // Keys stored in the DataProtectionKeys DB table — survives app restarts and
        // scale-out (all instances share the same key ring from the DB).
        //
        // Production recommendation: layer Azure Key Vault or AWS KMS on top for
        // HSM-backed key protection. That is a one-line addition:
        //   .ProtectKeysWithAzureKeyVault(...)
        // and does not change anything below.
        services.AddDataProtection()
            .SetApplicationName("LifeCrm")
            .PersistKeysToDbContext<AppDbContext>()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        // Register the converter as a singleton — one instance shared across all
        // DbContext instances, which is safe because IDataProtector is thread-safe.
        services.AddSingleton<EncryptedStringConverter>(sp =>
        {
            var provider  = sp.GetRequiredService<IDataProtectionProvider>();
            var protector = provider.CreateProtector("LifeCrm.PiiFields.v1");
            return new EncryptedStringConverter(protector);
        });

        return services;
    }
}
