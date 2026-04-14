using Blazored.LocalStorage;
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class CampaignsApiClient : ApiClientBase
{
    public CampaignsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<CampaignListDto>>> GetCampaignsAsync(
        PaginationParams p, Guid? projectId = null)
    {
        var url = $"api/v1/campaigns?page={p.Page}&pageSize={p.PageSize}" +
                  $"&search={Uri.EscapeDataString(p.Search ?? "")}";
        if (projectId.HasValue) url += $"&projectId={projectId.Value}";
        return await GetAsync<PagedResult<CampaignListDto>>(url);
    }

    public async Task<ApiResponse<CampaignDto>> GetCampaignAsync(Guid id)
        => await GetAsync<CampaignDto>($"api/v1/campaigns/{id}");

    public async Task<ApiResponse<Guid>> CreateCampaignAsync(CreateCampaignRequest req)
        => await PostAsync<Guid>("api/v1/campaigns", req);

    public async Task<ApiResponse> UpdateCampaignAsync(Guid id, UpdateCampaignRequest req)
        => await PutAsync($"api/v1/campaigns/{id}", req);

    public async Task<ApiResponse> DeleteCampaignAsync(Guid id)
        => await DeleteAsync($"api/v1/campaigns/{id}");
}
