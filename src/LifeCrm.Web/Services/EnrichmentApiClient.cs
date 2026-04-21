using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Enrichment.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class EnrichmentApiClient : ApiClientBase
{
    public EnrichmentApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    // ── Tags ──────────────────────────────────────────────────────────────

    public async Task<ApiResponse<IReadOnlyList<TagDto>>> GetTagsAsync()
        => await GetAsync<IReadOnlyList<TagDto>>("api/v1/enrichment/tags");

    public async Task<ApiResponse<TagDto>> CreateTagAsync(CreateTagRequest request)
        => await PostAsync<TagDto>("api/v1/enrichment/tags", request);

    public async Task<ApiResponse> DeleteTagAsync(Guid tagId)
        => await DeleteAsync($"api/v1/enrichment/tags/{tagId}");

    public async Task<ApiResponse<IReadOnlyList<ContactTagDto>>> GetContactTagsAsync(Guid contactId)
        => await GetAsync<IReadOnlyList<ContactTagDto>>($"api/v1/enrichment/contacts/{contactId}/tags");

    public async Task<ApiResponse> AssignTagAsync(Guid contactId, Guid tagId)
        => await PostVoidAsync($"api/v1/enrichment/contacts/{contactId}/tags",
            new AssignTagRequest { TagId = tagId });

    public async Task<ApiResponse> RemoveTagAsync(Guid contactId, Guid tagId)
        => await DeleteAsync($"api/v1/enrichment/contacts/{contactId}/tags/{tagId}");

    // ── Profile ───────────────────────────────────────────────────────────

    public async Task<ApiResponse<ContactProfileDto>> GetProfileAsync(Guid contactId)
        => await GetAsync<ContactProfileDto>($"api/v1/enrichment/contacts/{contactId}/profile");

    public async Task<ApiResponse<ContactProfileDto>> UpsertProfileAsync(
        Guid contactId, UpsertContactProfileRequest request)
        => await PutAsync<ContactProfileDto>(
            $"api/v1/enrichment/contacts/{contactId}/profile", request);

    public async Task<ApiResponse> DeleteProfileAsync(Guid contactId)
        => await DeleteAsync($"api/v1/enrichment/contacts/{contactId}/profile");

    public async Task<ApiResponse<AdminContactDetailDto>> GetAdminContactDetailAsync(Guid contactId)
        => await GetAsync<AdminContactDetailDto>(
            $"api/v1/enrichment/contacts/{contactId}/full");

    // ── Mentoring ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<IReadOnlyList<MentorRelationshipDto>>> GetMentoringAsync(Guid contactId)
        => await GetAsync<IReadOnlyList<MentorRelationshipDto>>(
            $"api/v1/enrichment/contacts/{contactId}/mentoring");

    public async Task<ApiResponse<MentorRelationshipDto>> CreateMentoringAsync(
        CreateMentorRelationshipRequest request)
        => await PostAsync<MentorRelationshipDto>("api/v1/enrichment/mentoring", request);

    public async Task<ApiResponse> DeleteMentoringAsync(Guid id)
        => await DeleteAsync($"api/v1/enrichment/mentoring/{id}");
}
