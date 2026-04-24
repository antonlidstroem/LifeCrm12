using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Documents.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class DashboardApiClient : ApiClientBase
{
    public DashboardApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync()
        => await GetAsync<DashboardDto>("api/v1/dashboard");

    /// <summary>Generates a donation summary PDF for a contact over a given period.</summary>
    public async Task<ApiResponse<DocumentDto>> GenerateDonationSummaryAsync(GenerateDonationSummaryRequest req)
        => await PostAsync<DocumentDto>("api/v1/documents/summary", req);

    /// <summary>Downloads a previously generated document (receipt or summary) as a PDF.</summary>
    public async Task<ApiResponse> DownloadDocumentAsync(Guid id)
        => await DownloadFileAsync($"api/v1/documents/{id}/download", $"document-{id:N}.pdf", "application/pdf");
}
