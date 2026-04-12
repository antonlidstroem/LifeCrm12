using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Users.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class UsersApiClient : ApiClientBase
{
    public UsersApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync()
        => await GetAsync<IReadOnlyList<UserSummaryDto>>("api/v1/users");
    public async Task<ApiResponse> ChangeUserRoleAsync(Guid id, UserRole newRole)
        => await PatchAsync($"api/v1/users/{id}/role", newRole);
    public async Task<ApiResponse> DeactivateUserAsync(Guid id)
        => await PatchAsync($"api/v1/users/{id}/deactivate", new { });
    public async Task<ApiResponse> ActivateUserAsync(Guid id)
        => await PatchAsync($"api/v1/users/{id}/activate", new { });
}
