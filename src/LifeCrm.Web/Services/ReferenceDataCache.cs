// src/LifeCrm.Web/Services/ReferenceDataCache.cs
using LifeCrm.Contracts.Campaigns.DTOs;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Projects.DTOs;

namespace LifeCrm.Web.Services;

/// <summary>
/// Lightweight in-memory cache for reference data that is fetched repeatedly
/// during a single session — primarily project and campaign lists used to
/// populate dropdowns in forms.
///
/// FIX E: Without this cache every form that contains a project or campaign
/// picker issued a full paginated API request on render, even when the user
/// had already loaded the same list moments earlier on the same page.
///
/// TTL is intentionally short (60 s) so that data created in the current
/// session appears in pickers quickly without a full page refresh.
/// </summary>
public sealed class ReferenceDataCache
{
    private record CacheEntry<T>(T Value, DateTimeOffset ExpiresAt);

    private CacheEntry<IReadOnlyList<ProjectListDto>>?  _projects;
    private CacheEntry<IReadOnlyList<CampaignListDto>>? _campaigns;
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(60);

    // ── Projects ──────────────────────────────────────────────────────────────

    public IReadOnlyList<ProjectListDto>? GetProjects()
        => _projects is not null && DateTimeOffset.UtcNow < _projects.ExpiresAt
            ? _projects.Value
            : null;

    public void SetProjects(IReadOnlyList<ProjectListDto> value)
        => _projects = new(value, DateTimeOffset.UtcNow.Add(_ttl));

    public void InvalidateProjects() => _projects = null;

    // ── Campaigns ─────────────────────────────────────────────────────────────

    public IReadOnlyList<CampaignListDto>? GetCampaigns()
        => _campaigns is not null && DateTimeOffset.UtcNow < _campaigns.ExpiresAt
            ? _campaigns.Value
            : null;

    public void SetCampaigns(IReadOnlyList<CampaignListDto> value)
        => _campaigns = new(value, DateTimeOffset.UtcNow.Add(_ttl));

    public void InvalidateCampaigns() => _campaigns = null;

    /// <summary>Drop all cached entries — call on logout.</summary>
    public void Clear()
    {
        _projects  = null;
        _campaigns = null;
    }
}
