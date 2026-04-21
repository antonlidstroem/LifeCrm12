using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Events.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class EventsApiClient : ApiClientBase
{
    public EventsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<EventListDto>>> GetEventsAsync(
        PaginationParams p, CancellationToken ct = default)
    {
        var url = $"api/v1/events?page={p.Page}&pageSize={p.PageSize}" +
                  $"&search={Uri.EscapeDataString(p.Search ?? "")}" +
                  $"&sortAscending={p.SortAscending}";
        return await GetAsync<PagedResult<EventListDto>>(url);
    }

    public async Task<ApiResponse<EventDto>> GetEventAsync(Guid id)
        => await GetAsync<EventDto>($"api/v1/events/{id}");

    public async Task<ApiResponse<Guid>> CreateEventAsync(CreateEventRequest request)
        => await PostAsync<Guid>("api/v1/events", request);

    public async Task<ApiResponse> UpdateEventAsync(Guid id, UpdateEventRequest request)
        => await PutAsync($"api/v1/events/{id}", request);

    public async Task<ApiResponse> DeleteEventAsync(Guid id)
        => await DeleteAsync($"api/v1/events/{id}");

    public async Task<ApiResponse<CheckInResult>> CheckInAsync(Guid eventId, CheckInRequest request)
        => await PostAsync<CheckInResult>($"api/v1/events/{eventId}/checkin", request);

    public async Task<ApiResponse> RemoveAttendanceAsync(Guid eventId, Guid attendanceId)
        => await DeleteAsync($"api/v1/events/{eventId}/attendances/{attendanceId}");

    public async Task<ApiResponse<ContactEngagementDto>> GetContactEngagementAsync(Guid contactId)
        => await GetAsync<ContactEngagementDto>($"api/v1/events/contacts/{contactId}/engagement");
}
