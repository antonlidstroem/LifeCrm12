using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Interfaces;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public record EmailSettingsRequest
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

public record TestEmailRequest
{
    public string ToEmail { get; init; } = string.Empty;
}

public class EmailSettingsApiClient : ApiClientBase
{
    public EmailSettingsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<EmailSettingsDto>> GetAsync(CancellationToken ct = default)
        => await GetAsync<EmailSettingsDto>("api/v1/emailsettings");

    public async Task<ApiResponse> SaveAsync(EmailSettingsRequest settings, CancellationToken ct = default)
        => await PutAsync("api/v1/emailsettings", settings);

    public async Task<ApiResponse<string>> TestAsync(string toEmail, CancellationToken ct = default)
        => await PostAsync<string>("api/v1/emailsettings/test", new TestEmailRequest { ToEmail = toEmail });
}
