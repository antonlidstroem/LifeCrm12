using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

/// <summary>
/// Client for the SignalR settings endpoint.
/// Allows super-admins to toggle the hub on/off from the UI.
/// </summary>
public class SignalRSettingsApiClient : ApiClientBase
{
    public SignalRSettingsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<bool>> GetAsync(CancellationToken ct = default)
        => await GetAsync<bool>("api/v1/signalrsettings");

    public async Task<ApiResponse<bool>> SetAsync(bool enabled, CancellationToken ct = default)
    {
        await AttachTokenAsync();
        try
        {
            var json    = JsonSerializer.Serialize(enabled, JsonOpts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await _http.PatchAsync("api/v1/signalrsettings", content);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>(JsonOpts)
                ?? ApiResponse<bool>.Fail("Empty response.");
        }
        catch (Exception ex) { return ApiResponse<bool>.Fail(ex.Message); }
    }
}
