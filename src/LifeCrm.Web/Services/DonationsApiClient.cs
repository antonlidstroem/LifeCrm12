using Blazored.LocalStorage;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Documents.DTOs;
using LifeCrm.Contracts.Donations.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class DonationsApiClient : ApiClientBase
{
    public DonationsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<DonationListDto>>> GetDonationsAsync(
        PaginationParams p, Guid? contactId = null, DateOnly? fromDate = null,
        DateOnly? toDate = null, Guid? campaignId = null, Guid? projectId = null)
    {
        var url = $"api/v1/donations?page={p.Page}&pageSize={p.PageSize}";
        if (contactId.HasValue)  url += $"&contactId={contactId.Value}";
        if (fromDate.HasValue)   url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue)     url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        if (campaignId.HasValue) url += $"&campaignId={campaignId.Value}";
        if (projectId.HasValue)  url += $"&projectId={projectId.Value}";
        if (!string.IsNullOrWhiteSpace(p.SortBy)) url += $"&sortBy={p.SortBy}";
        url += $"&sortAscending={p.SortAscending}";
        return await GetAsync<PagedResult<DonationListDto>>(url);
    }

    public async Task<ApiResponse<DonationDto>> GetDonationAsync(Guid id)
        => await GetAsync<DonationDto>($"api/v1/donations/{id}");

    public async Task<ApiResponse<Guid>> CreateDonationAsync(CreateDonationRequest req)
        => await PostAsync<Guid>("api/v1/donations", req);

    public async Task<ApiResponse> UpdateDonationAsync(Guid id, UpdateDonationRequest req)
        => await PutAsync($"api/v1/donations/{id}", req);

    public async Task<ApiResponse> DeleteDonationAsync(Guid id)
        => await DeleteAsync($"api/v1/donations/{id}");

    public async Task<ApiResponse<DocumentDto>> GenerateReceiptAsync(Guid id, bool sendByEmail)
        => await PostAsync<DocumentDto>($"api/v1/donations/{id}/receipt?sendByEmail={sendByEmail}");

    // FIX: use DownloadFileAsync which calls AttachTokenAsync internally
    public async Task<ApiResponse> DownloadReceiptAsync(Guid donationId, Guid documentId)
        => await DownloadFileAsync(
            $"api/v1/donations/{donationId}/receipt/{documentId}/download",
            $"receipt-{donationId:N}.pdf",
            "application/pdf");

    // FIX: use DownloadFileAsync which calls AttachTokenAsync internally
    public async Task<ApiResponse> DownloadLatestReceiptAsync(Guid donationId)
        => await DownloadFileAsync(
            $"api/v1/documents/donation/{donationId}/receipt/latest",
            $"receipt-{donationId:N}.pdf",
            "application/pdf");
}
