using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
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
        => await PatchAsync<bool>("api/v1/signalrsettings", enabled);

    private async Task<ApiResponse<T>> PatchAsync<T>(string url, object body)
    {
        await AttachTokenAsync();
        try
        {
            var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(body, JsonOpts),
                System.Text.Encoding.UTF8, "application/json");
            var resp = await _http.PatchAsync(url, content);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts)
                ?? ApiResponse<T>.Fail("Empty response.");
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }
}
