using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Interactions.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class InteractionsApiClient : ApiClientBase
{
    public InteractionsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<InteractionDto>> GetInteractionAsync(Guid id)
        => await GetAsync<InteractionDto>($"api/v1/interactions/{id}");
    public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetInteractionsAsync(Guid contactId)
        => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/contacts/{contactId}/interactions");
    public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetProjectInteractionsAsync(Guid projectId)
        => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/projects/{projectId}/interactions");
    public async Task<ApiResponse<Guid>> CreateInteractionAsync(CreateInteractionRequest req)
        => await PostAsync<Guid>("api/v1/interactions", req);
    public async Task<ApiResponse> UpdateInteractionAsync(Guid id, UpdateInteractionRequest req)
        => await PutAsync($"api/v1/interactions/{id}", req);
    public async Task<ApiResponse> DeleteInteractionAsync(Guid id)
        => await DeleteAsync($"api/v1/interactions/{id}");
}
