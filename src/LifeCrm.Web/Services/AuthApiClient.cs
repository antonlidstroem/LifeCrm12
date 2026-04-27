using System.Net.Http.Json;
using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class AuthApiClient : ApiClientBase
{
    public AuthApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/v1/auth/login", request, JsonOpts);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOpts) ?? ApiResponse<LoginResponse>.Fail("Connection error.");
    }

    public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request)
        => await PostVoidAsync("api/v1/auth/change-password", request);
}
