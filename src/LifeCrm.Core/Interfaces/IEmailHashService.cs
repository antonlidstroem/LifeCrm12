namespace LifeCrm.Core.Interfaces;

/// <summary>
/// Produces a deterministic HMAC-SHA256 hash of an email address for indexed lookup.
/// The hash is stored in Contact.EmailHash alongside the encrypted Email column,
/// enabling WHERE EmailHash = @hash queries without decrypting every row.
/// </summary>
public interface IEmailHashService
{
    string Hash(string email);
}
