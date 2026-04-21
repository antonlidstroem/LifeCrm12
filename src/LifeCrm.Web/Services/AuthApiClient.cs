using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Api.Controllers.v1;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class AuthApiClient : ApiClientBase
{
    public AuthApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password)
        => await PostAsync<LoginResponse>("api/v1/auth/login", new LoginRequest { Email = email, Password = password });

    public async Task ClearTokenAsync()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private record LoginRequest { public string Email { get; init; } = ""; public string Password { get; init; } = ""; }
}
