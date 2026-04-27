// src/LifeCrm.Web/Services/ReferenceDataCache.cs
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Projects.DTOs;

namespace LifeCrm.Web.Services;

/// <summary>
/// FIX E: Lightweight in-process cache for reference data fetched repeatedly
/// during a session — primarily project/campaign lists used in dropdowns.
/// Scoped per circuit so each browser tab gets its own cache instance.
/// </summary>
public sealed class ReferenceDataCache
{
    private record CacheEntry<T>(T Value, DateTimeOffset ExpiresAt);

    private CacheEntry<IReadOnlyList<ProjectListDto>>?  _projects;
    private CacheEntry<IReadOnlyList<CampaignListDto>>? _campaigns;
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(60);

    public IReadOnlyList<ProjectListDto>? GetProjects()
        => _projects is not null && DateTimeOffset.UtcNow < _projects.ExpiresAt
            ? _projects.Value : null;

    public void SetProjects(IReadOnlyList<ProjectListDto> value)
        => _projects = new(value, DateTimeOffset.UtcNow.Add(_ttl));

    public void InvalidateProjects() => _projects = null;

    public IReadOnlyList<CampaignListDto>? GetCampaigns()
        => _campaigns is not null && DateTimeOffset.UtcNow < _campaigns.ExpiresAt
            ? _campaigns.Value : null;

    public void SetCampaigns(IReadOnlyList<CampaignListDto> value)
        => _campaigns = new(value, DateTimeOffset.UtcNow.Add(_ttl));

    public void InvalidateCampaigns() => _campaigns = null;

    public void Clear() { _projects = null; _campaigns = null; }
}
