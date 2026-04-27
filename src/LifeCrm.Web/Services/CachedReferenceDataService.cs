// src/LifeCrm.Web/Services/CachedReferenceDataService.cs
using LifeCrm.Contracts.Campaigns.DTOs;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Projects.DTOs;

namespace LifeCrm.Web.Services;

/// <summary>
/// Service layer over ReferenceDataCache + the underlying API clients.
/// Components that need a full list of projects or campaigns for a dropdown
/// should call this instead of ProjectsApiClient/CampaignsApiClient directly.
///
/// Usage in a Blazor component:
///   @inject CachedReferenceDataService RefData
///   ...
///   var projects = await RefData.GetAllProjectsAsync();
/// </summary>
public sealed class CachedReferenceDataService
{
    private readonly ProjectsApiClient  _projectsClient;
    private readonly CampaignsApiClient _campaignsClient;
    private readonly ReferenceDataCache _cache;

    // Large page size to fetch all reference records in one call.
    // Reference lists (projects, campaigns) are typically small (< 200 items).
    private static readonly PaginationParams AllItems = new() { Page = 1, PageSize = 100 };

    public CachedReferenceDataService(
        ProjectsApiClient  projectsClient,
        CampaignsApiClient campaignsClient,
        ReferenceDataCache cache)
    {
        _projectsClient  = projectsClient;
        _campaignsClient = campaignsClient;
        _cache           = cache;
    }

    // ── Projects ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all projects. Result is served from in-process cache if still
    /// fresh, otherwise fetches from the API and caches the response.
    /// </summary>
    public async Task<IReadOnlyList<ProjectListDto>> GetAllProjectsAsync(
        CancellationToken ct = default)
    {
        var cached = _cache.GetProjects();
        if (cached is not null) return cached;

        var resp = await _projectsClient.GetProjectsAsync(AllItems);
        if (!resp.Success || resp.Data is null)
            return Array.Empty<ProjectListDto>();

        var items = resp.Data.Items;
        _cache.SetProjects(items);
        return items;
    }

    /// <summary>Force-refresh the project cache (call after create/update/delete).</summary>
    public async Task<IReadOnlyList<ProjectListDto>> RefreshProjectsAsync(
        CancellationToken ct = default)
    {
        _cache.InvalidateProjects();
        return await GetAllProjectsAsync(ct);
    }

    // ── Campaigns ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all campaigns, optionally filtered by project.
    /// Note: project-scoped campaign lists are not cached separately to keep
    /// the implementation simple; only the unfiltered list is cached.
    /// </summary>
    public async Task<IReadOnlyList<CampaignListDto>> GetAllCampaignsAsync(
        Guid? projectId = null, CancellationToken ct = default)
    {
        // Only use the cache when no project filter is applied
        if (projectId is null)
        {
            var cached = _cache.GetCampaigns();
            if (cached is not null) return cached;
        }

        var resp = await _campaignsClient.GetCampaignsAsync(AllItems, projectId);
        if (!resp.Success || resp.Data is null)
            return Array.Empty<CampaignListDto>();

        var items = resp.Data.Items;

        if (projectId is null)
            _cache.SetCampaigns(items);

        return items;
    }

    /// <summary>Force-refresh the campaign cache (call after create/update/delete).</summary>
    public async Task<IReadOnlyList<CampaignListDto>> RefreshCampaignsAsync(
        CancellationToken ct = default)
    {
        _cache.InvalidateCampaigns();
        return await GetAllCampaignsAsync(ct: ct);
    }

    /// <summary>Clear all caches — call on logout.</summary>
    public void Clear() => _cache.Clear();
}
