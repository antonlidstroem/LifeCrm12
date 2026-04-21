using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public abstract class ApiClientBase
{
    protected readonly HttpClient _http;
    private readonly ILocalStorageService _storage;
    private readonly IJSRuntime _js;

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiClientBase(HttpClient http, ILocalStorageService storage, IJSRuntime js)
    {
        _http    = http;
        _storage = storage;
        _js      = js;
    }

    protected async Task AttachTokenAsync()
    {
        var token = await _storage.GetItemAsync<string>("auth_token");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.GetAsync(url);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts)
                    ?? ApiResponse<T>.Fail("Empty response.");
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse<T>.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PostAsJsonAsync(url, body);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts)
                    ?? ApiResponse<T>.Fail("Empty response.");
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse<T>.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> PostVoidAsync(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PostAsJsonAsync(url, body);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PutAsJsonAsync(url, body);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts)
                    ?? ApiResponse<T>.Fail("Empty response.");
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse<T>.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
        }
        catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> PutAsync(string url, object? body = null)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.PutAsJsonAsync(url, body);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
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
            var err = await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
            return ApiResponse.Fail(err?.Errors?.ToArray() ?? new[] { resp.ReasonPhrase ?? "Error" });
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    protected async Task<ApiResponse> DownloadFileAsync(string url, string fileName, string contentType)
    {
        await AttachTokenAsync();
        try
        {
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return ApiResponse.Fail("Download failed.");
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            await _js.InvokeVoidAsync("lifecrm.downloadFile", fileName, contentType, bytes);
            return ApiResponse.Ok();
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }
}
