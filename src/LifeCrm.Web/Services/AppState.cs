using Blazored.LocalStorage;

namespace LifeCrm.Web.Services;

public class AppState
{
    public event Action? StateChanged;

    public bool   IsAuthenticated  { get; private set; }
    public Guid?  UserId           { get; private set; }
    public string UserFullName     { get; private set; } = string.Empty;
    public string UserEmail        { get; private set; } = string.Empty;
    public string UserRole         { get; private set; } = string.Empty;
    public Guid?  OrganizationId   { get; private set; }
    public string OrganizationName { get; private set; } = string.Empty;
    public string? Token           { get; private set; }
    public bool   SignalREnabled   { get; set; } = true;

    public bool IsAdmin          => UserRole == "Admin";
    public bool IsFinanceOrAdmin => UserRole is "Finance" or "Admin";
    public bool CanWrite         => UserRole is "Manager" or "Finance" or "Admin";

    public void SetUser(Guid userId, string fullName, string email, string role,
        Guid orgId, string orgName, string token)
    {
        IsAuthenticated  = true;
        UserId           = userId;
        UserFullName     = fullName;
        UserEmail        = email;
        UserRole         = role;
        OrganizationId   = orgId;
        OrganizationName = orgName;
        Token            = token;
        StateChanged?.Invoke();
    }

    public void ClearUser()
    {
        IsAuthenticated  = false;
        UserId           = null;
        UserFullName     = string.Empty;
        UserEmail        = string.Empty;
        UserRole         = string.Empty;
        OrganizationId   = null;
        OrganizationName = string.Empty;
        Token            = null;
        StateChanged?.Invoke();
    }

    public async Task TryRestoreAsync(ILocalStorageService storage)
    {
        try
        {
            var token = await storage.GetItemAsync<string>("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var userId  = await storage.GetItemAsync<Guid>("user_id");
            var name    = await storage.GetItemAsync<string>("user_name") ?? string.Empty;
            var email   = await storage.GetItemAsync<string>("user_email") ?? string.Empty;
            var role    = await storage.GetItemAsync<string>("user_role") ?? string.Empty;
            var orgId   = await storage.GetItemAsync<Guid>("org_id");
            var orgName = await storage.GetItemAsync<string>("org_name") ?? string.Empty;

            if (userId != Guid.Empty && orgId != Guid.Empty)
                SetUser(userId, name, email, role, orgId, orgName, token);
        }
        catch { /* storage unavailable */ }
    }

    public async Task PersistAsync(ILocalStorageService storage)
    {
        await storage.SetItemAsync("auth_token",  Token);
        await storage.SetItemAsync("user_id",     UserId);
        await storage.SetItemAsync("user_name",   UserFullName);
        await storage.SetItemAsync("user_email",  UserEmail);
        await storage.SetItemAsync("user_role",   UserRole);
        await storage.SetItemAsync("org_id",      OrganizationId);
        await storage.SetItemAsync("org_name",    OrganizationName);
    }

    public async Task ClearPersistedAsync(ILocalStorageService storage)
    {
        await storage.RemoveItemsAsync(new[]
        {
            "auth_token","user_id","user_name","user_email","user_role","org_id","org_name"
        });
    }
}
