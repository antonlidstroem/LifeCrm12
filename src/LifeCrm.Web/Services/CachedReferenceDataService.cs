// src/LifeCrm.Web/Services/CachedReferenceDataService.cs
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Projects.DTOs;

namespace LifeCrm.Web.Services;

/// <summary>
/// FIX E: Service that wraps ReferenceDataCache + the raw API clients.
/// Components that need projects or campaigns for dropdowns inject this
/// instead of calling the API clients directly on every render.
/// </summary>
public sealed class CachedReferenceDataService
{
    private readonly ProjectsApiClient  _projectsClient;
    private readonly CampaignsApiClient _campaignsClient;
    private readonly ReferenceDataCache _cache;

    private static readonly PaginationParams AllItems =
        new() { Page = 1, PageSize = 100 };

    public CachedReferenceDataService(
        ProjectsApiClient  projectsClient,
        CampaignsApiClient campaignsClient,
        ReferenceDataCache cache)
    {
        _projectsClient  = projectsClient;
        _campaignsClient = campaignsClient;
        _cache           = cache;
    }

    public async Task<IReadOnlyList<ProjectListDto>> GetAllProjectsAsync(
        CancellationToken ct = default)
    {
        var cached = _cache.GetProjects();
        if (cached is not null) return cached;

        var resp = await _projectsClient.GetProjectsAsync(AllItems);
        if (!resp.Success || resp.Data is null) return Array.Empty<ProjectListDto>();

        _cache.SetProjects(resp.Data.Items);
        return resp.Data.Items;
    }

    public async Task<IReadOnlyList<ProjectListDto>> RefreshProjectsAsync(
        CancellationToken ct = default)
    {
        _cache.InvalidateProjects();
        return await GetAllProjectsAsync(ct);
    }

    public async Task<IReadOnlyList<CampaignListDto>> GetAllCampaignsAsync(
        Guid? projectId = null, CancellationToken ct = default)
    {
        if (projectId is null)
        {
            var cached = _cache.GetCampaigns();
            if (cached is not null) return cached;
        }

        var resp = await _campaignsClient.GetCampaignsAsync(AllItems, projectId);
        if (!resp.Success || resp.Data is null) return Array.Empty<CampaignListDto>();

        if (projectId is null) _cache.SetCampaigns(resp.Data.Items);
        return resp.Data.Items;
    }

    public async Task<IReadOnlyList<CampaignListDto>> RefreshCampaignsAsync(
        CancellationToken ct = default)
    {
        _cache.InvalidateCampaigns();
        return await GetAllCampaignsAsync(ct: ct);
    }

    public void Clear() => _cache.Clear();
}
