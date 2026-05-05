using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

// ── Local request/response types (mirror what the API controller expects) ────

/// <summary>The email settings as returned by the API (password masked).</summary>
public record EmailSettingsViewModel
{
    public string Host      { get; init; } = string.Empty;
    public int    Port      { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    public string Password  { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName  { get; init; } = string.Empty;
    public bool   UseSsl    { get; init; } = true;
    public bool   DryRun    { get; init; } = false;
}

/// <summary>The save request sent to the API.</summary>
public record EmailSettingsSaveRequest
{
    public string Host      { get; init; } = string.Empty;
    public int    Port      { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    public string Password  { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName  { get; init; } = string.Empty;
    public bool   UseSsl    { get; init; } = true;
    public bool   DryRun    { get; init; } = false;
}

public class EmailSettingsApiClient : ApiClientBase
{
    public EmailSettingsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    public async Task<ApiResponse<EmailSettingsViewModel>> GetAsync(CancellationToken ct = default)
        => await GetAsync<EmailSettingsViewModel>("api/v1/emailsettings");

    public async Task<ApiResponse> SaveAsync(
        EmailSettingsSaveRequest settings, CancellationToken ct = default)
        => await PutAsync("api/v1/emailsettings", settings);

    public async Task<ApiResponse<string>> TestAsync(string toEmail, CancellationToken ct = default)
        => await PostAsync<string>("api/v1/emailsettings/test", new { toEmail });
}
