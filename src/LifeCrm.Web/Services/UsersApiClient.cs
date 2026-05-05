// src/LifeCrm.Web/Services/UsersApiClient.cs
using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Users.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class UsersApiClient : ApiClientBase
{
    public UsersApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    public async Task<ApiResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync()
        => await GetAsync<IReadOnlyList<UserSummaryDto>>("api/v1/users");

    public async Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest req)
        => await PostAsync<Guid>("api/v1/users", req);

    public async Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest req)
        => await PutAsync($"api/v1/users/{id}", req);

    public async Task<ApiResponse> DeleteUserAsync(Guid id)
        => await DeleteAsync($"api/v1/users/{id}");

    public async Task<ApiResponse> ChangeUserRoleAsync(Guid id, UserRole newRole)
        => await PatchAsync($"api/v1/users/{id}/role", newRole);

    public async Task<ApiResponse> DeactivateUserAsync(Guid id)
        => await PatchVoidAsync($"api/v1/users/{id}/deactivate");

    public async Task<ApiResponse> ActivateUserAsync(Guid id)
        => await PatchVoidAsync($"api/v1/users/{id}/activate");
}