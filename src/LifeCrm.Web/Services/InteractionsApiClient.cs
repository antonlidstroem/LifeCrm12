// src/LifeCrm.Web/Services/InteractionsApiClient.cs
// UPDATED: Added GetInteractionsListAsync for the standalone /interactions page
using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Interactions.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class InteractionsApiClient : ApiClientBase
{
    public InteractionsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    /// <summary>Get a single interaction by ID.</summary>
    public async Task<ApiResponse<InteractionDto>> GetInteractionAsync(Guid id)
        => await GetAsync<InteractionDto>($"api/v1/interactions/{id}");

    /// <summary>
    /// Paginated list of all interactions in the organisation.
    /// Used by the standalone /interactions page.
    /// Requires GET /api/v1/interactions?page=&pageSize=&search=&type= on the backend.
    /// </summary>
    public async Task<ApiResponse<PagedResult<InteractionDto>>> GetInteractionsListAsync(
        PaginationParams p, string? typeFilter = null)
    {
        var url = $"api/v1/interactions?page={p.Page}&pageSize={p.PageSize}";
        if (!string.IsNullOrWhiteSpace(p.Search))
            url += $"&search={Uri.EscapeDataString(p.Search)}";
        if (!string.IsNullOrWhiteSpace(typeFilter))
            url += $"&type={Uri.EscapeDataString(typeFilter)}";
        return await GetAsync<PagedResult<InteractionDto>>(url);
    }

    /// <summary>Get all interactions for a specific contact.</summary>
    public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetInteractionsAsync(Guid contactId)
        => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/contacts/{contactId}/interactions");

    /// <summary>Get all interactions for a specific project.</summary>
    public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetProjectInteractionsAsync(Guid projectId)
        => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/projects/{projectId}/interactions");

    /// <summary>Create a new interaction.</summary>
    public async Task<ApiResponse<Guid>> CreateInteractionAsync(CreateInteractionRequest req)
        => await PostAsync<Guid>("api/v1/interactions", req);

    /// <summary>Update an existing interaction.</summary>
    public async Task<ApiResponse> UpdateInteractionAsync(Guid id, UpdateInteractionRequest req)
        => await PutAsync($"api/v1/interactions/{id}", req);

    /// <summary>Delete an interaction.</summary>
    public async Task<ApiResponse> DeleteInteractionAsync(Guid id)
        => await DeleteAsync($"api/v1/interactions/{id}");
}
