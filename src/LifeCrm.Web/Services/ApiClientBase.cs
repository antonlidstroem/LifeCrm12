using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public abstract class ApiClientBase
{
    protected readonly HttpClient           _http;
    protected readonly ILocalStorageService _storage;
    protected readonly IJSRuntime           _js;
    private   const    string TokenKey = "lifecrm_token";

    protected static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected ApiClientBase(HttpClient http, ILocalStorageService storage, IJSRuntime js)
    { _http = http; _storage = storage; _js = js; }

    public async Task SaveTokenAsync(string token)  => await _storage.SetItemAsStringAsync(TokenKey, token);
    public async Task ClearTokenAsync()             => await _storage.RemoveItemAsync(TokenKey);

    protected async Task AttachTokenAsync()
    {
        var token = await _storage.GetItemAsStringAsync(TokenKey);
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        await AttachTokenAsync();
        try { return await _http.GetFromJsonAsync<ApiResponse<T>>(url, JsonOpts) ?? ApiResponse<T>.Fail("Empty response."); }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = body is null ? await _http.PostAsync(url, null) : await _http.PostAsJsonAsync(url, body, JsonOpts);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts) ?? ApiResponse<T>.Fail("Empty response.");
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> PostVoidAsync(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = body is null ? await _http.PostAsync(url, null) : await _http.PostAsJsonAsync(url, body, JsonOpts);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return err ?? ApiResponse.Fail($"HTTP {(int)resp.StatusCode}");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> PutAsync(string url, object body)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PutAsJsonAsync(url, body, JsonOpts);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            return await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts) ?? ApiResponse.Fail("Unknown error.");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse<T>> PutAsync<T>(string url, object body)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PutAsJsonAsync(url, body, JsonOpts);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts) ?? ApiResponse<T>.Fail("Empty response.");
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> PatchAsync(string url, object body)
    {
        await AttachTokenAsync();
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");
            var resp = await _http.PatchAsync(url, content);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            return await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts) ?? ApiResponse.Fail("Unknown error.");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    /// <summary>
    /// FIX: PATCH with an empty JSON body ("{}") for endpoints that take no body params.
    /// Sending a completely bodyless PATCH can cause 415 on some middleware configurations.
    /// </summary>
    protected async Task<ApiResponse> PatchVoidAsync(string url)
    {
        await AttachTokenAsync();
        try
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var resp = await _http.PatchAsync(url, content);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            return await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts) ?? ApiResponse.Fail("Unknown error.");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> DeleteAsync(string url)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.DeleteAsync(url);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            return await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts) ?? ApiResponse.Fail("Unknown error.");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> DownloadFileAsync(string url, string filename, string mimeType = "application/octet-stream")
    {
        // FIX: Always attach token before file downloads — was missing in original
        await AttachTokenAsync();
        try
        {
            var bytes = await _http.GetByteArrayAsync(url);
            await _js.InvokeVoidAsync("lifecrm.downloadFile", filename, Convert.ToBase64String(bytes), mimeType);
            return ApiResponse.Ok();
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }
}
