using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class ReportsApiClient : ApiClientBase
{
    public ReportsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<MissionReportListDto>>> GetReportsAsync(PaginationParams p, ReportStatus? status = null, Guid? campaignId = null, Guid? projectId = null)
    {
        var url = $"api/v1/missionreports?page={p.Page}&pageSize={p.PageSize}";
        if (!string.IsNullOrWhiteSpace(p.Search)) url += $"&search={Uri.EscapeDataString(p.Search)}";
        if (status.HasValue)     url += $"&status={status.Value}";
        if (campaignId.HasValue) url += $"&campaignId={campaignId.Value}";
        if (projectId.HasValue)  url += $"&projectId={projectId.Value}";
        return await GetAsync<PagedResult<MissionReportListDto>>(url);
    }

    public async Task<ApiResponse<MissionReportDetailDto>> GetReportAsync(Guid id)       => await GetAsync<MissionReportDetailDto>($"api/v1/missionreports/{id}");
    public async Task<ApiResponse<Guid>>   CreateReportAsync(CreateReportRequest req)    => await PostAsync<Guid>("api/v1/missionreports", req);
    public async Task<ApiResponse>         UpdateReportAsync(Guid id, UpdateReportRequest req) => await PutAsync($"api/v1/missionreports/{id}", req);
    public async Task<ApiResponse>         DeleteReportAsync(Guid id)                    => await DeleteAsync($"api/v1/missionreports/{id}");
    public async Task<ApiResponse>         SubmitReportAsync(Guid id)                    => await PostVoidAsync($"api/v1/missionreports/{id}/submit");
    public async Task<ApiResponse>         ApproveReportAsync(Guid id)                   => await PostVoidAsync($"api/v1/missionreports/{id}/approve");
    public async Task<ApiResponse>         ReturnReportAsync(Guid id, ReturnForRevisionRequest req) => await PostVoidAsync($"api/v1/missionreports/{id}/return", req);
    public async Task<ApiResponse<DecisionCountDto>> UpsertDecisionCountAsync(Guid reportId, UpsertDecisionCountRequest req)
        => await PutAsync<DecisionCountDto>($"api/v1/missionreports/{reportId}/decisions", req);
    public async Task<ApiResponse<IReadOnlyList<PeopleGroupSearchDto>>> SearchPeopleGroupsAsync(string q)
        => await GetAsync<IReadOnlyList<PeopleGroupSearchDto>>($"api/v1/missionreports/people-groups/search?q={Uri.EscapeDataString(q)}");
    public async Task<ApiResponse<PeopleGroupReachedDto>> AddPeopleGroupAsync(Guid reportId, AddPeopleGroupRequest req)
        => await PostAsync<PeopleGroupReachedDto>($"api/v1/missionreports/{reportId}/people-groups", req);
    public async Task<ApiResponse> RemovePeopleGroupAsync(Guid reportId, Guid entryId)
        => await DeleteAsync($"api/v1/missionreports/{reportId}/people-groups/{entryId}");
    public async Task<ApiResponse<PrayerPointDto>> AddReportPrayerPointAsync(Guid reportId, CreatePrayerPointRequest req)
        => await PostAsync<PrayerPointDto>($"api/v1/missionreports/{reportId}/prayer-points", req);
    public async Task<ApiResponse<IReadOnlyList<AnsweredPrayerWidgetDto>>> GetAnsweredPrayersThisMonthAsync()
        => await GetAsync<IReadOnlyList<AnsweredPrayerWidgetDto>>("api/v1/missionreports/answered-prayers/this-month");
    public async Task<ApiResponse<IReadOnlyList<PrayerPointDto>>> GetActivePrayerPointsAsync()
        => await GetAsync<IReadOnlyList<PrayerPointDto>>("api/v1/prayerpoints/active");
    public async Task<ApiResponse<PrayerPointDto>> MarkPrayerAnsweredAsync(Guid id, MarkAnsweredRequest req)
        => await PostAsync<PrayerPointDto>($"api/v1/prayerpoints/{id}/mark-answered", req);
    public async Task<ApiResponse> DeletePrayerPointAsync(Guid id)
        => await DeleteAsync($"api/v1/prayerpoints/{id}");
}
