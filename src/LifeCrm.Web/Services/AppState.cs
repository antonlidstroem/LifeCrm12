using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Constants;

namespace LifeCrm.Web.Services;

public class AppState
{
    private LoginResponse? _login;
    private const string StorageKey = "lifecrm_user";

    public string? UserId           => _login?.UserId;
    public string? UserFullName     => _login?.FullName;
    public string? UserEmail        => _login?.Email;
    public string? UserRole         => _login?.Role;
    public string? OrganizationId   => _login?.OrganizationId;
    public string? OrganizationName => _login?.OrganizationName;
    public string? Token            => _login?.Token;
    public bool    IsAuthenticated  => _login is not null && !IsTokenExpired(_login.Token);

    public bool IsAdmin          => UserRole == Roles.Admin;
    public bool IsFinanceOrAdmin => UserRole is Roles.Admin or Roles.Finance;
    public bool CanWrite         => UserRole is Roles.Admin or Roles.Finance or Roles.Manager;

    /// <summary>
    /// Whether the SignalR real-time hub is enabled.
    /// Loaded after login from the API. Defaults to true so the client
    /// attempts to connect; if the server rejects, it gracefully skips.
    /// </summary>
    public bool SignalREnabled { get; set; } = true;

    public event Action? StateChanged;

    public void SetUser(LoginResponse login) { _login = login; StateChanged?.Invoke(); }
    public void ClearUser()                  { _login = null;  StateChanged?.Invoke(); }

    public async Task TryRestoreAsync(ILocalStorageService storage)
    {
        if (_login is not null) return;
        try
        {
            var stored = await storage.GetItemAsync<LoginResponse>(StorageKey);
            if (stored is not null && !IsTokenExpired(stored.Token)) { _login = stored; StateChanged?.Invoke(); }
            else if (stored is not null) await storage.RemoveItemAsync(StorageKey);
        }
        catch { }
    }

    public async Task PersistAsync(ILocalStorageService storage)
    {
        if (_login is null) return;
        try { await storage.SetItemAsync(StorageKey, _login); } catch { }
    }

    public async Task ClearPersistedAsync(ILocalStorageService storage)
    {
        try { await storage.RemoveItemAsync(StorageKey); } catch { }
    }

    private static bool IsTokenExpired(string token)
    {
        try { var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token); return jwt.ValidTo.ToUniversalTime() < DateTime.UtcNow.AddSeconds(30); }
        catch { return true; }
    }
}
