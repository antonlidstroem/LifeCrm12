using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeCrm.Infrastructure.Encryption;

/// <summary>
/// Passthrough converter used at design time (e.g. during dotnet ef migrations add)
/// when no IDataProtectionProvider is registered in the design-time DbContext factory.
/// Never used at runtime — the real EncryptedStringConverter is always injected.
/// </summary>
internal class NullEncryptedStringConverter : ValueConverter<string?, string?>
{
    public NullEncryptedStringConverter()
        : base(v => v, v => v)
    {
    }
}
