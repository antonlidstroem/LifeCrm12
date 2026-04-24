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

    // FIX: PATCH with empty body — endpoint has no [FromBody], but we still need to send
    // a PATCH with a valid content-type. Use an empty JSON object.
    public async Task<ApiResponse> DeactivateUserAsync(Guid id)
        => await PatchVoidAsync($"api/v1/users/{id}/deactivate");

    public async Task<ApiResponse> ActivateUserAsync(Guid id)
        => await PatchVoidAsync($"api/v1/users/{id}/activate");
}
