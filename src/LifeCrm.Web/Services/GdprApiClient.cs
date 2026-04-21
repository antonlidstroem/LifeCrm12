using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class GdprApiClient : ApiClientBase
{
    public GdprApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js)
        : base(h, s, js) { }

    // ── Consent ───────────────────────────────────────────────────────────────

    public async Task<ApiResponse<ConsentStatusDto>> GetConsentAsync(
        Guid contactId, CancellationToken ct = default)
        => await GetAsync<ConsentStatusDto>($"api/v1/gdpr/contacts/{contactId}/consent");

    public async Task<ApiResponse<ConsentEntryDto>> UpdateConsentAsync(
        Guid contactId, UpdateConsentRequest request, CancellationToken ct = default)
        => await PostAsync<ConsentEntryDto>($"api/v1/gdpr/contacts/{contactId}/consent", request);

    // ── DSR Export ────────────────────────────────────────────────────────────

    public async Task<ApiResponse<DsrExportJobDto>> RequestExportAsync(
        Guid contactId, CancellationToken ct = default)
        => await PostAsync<DsrExportJobDto>($"api/v1/gdpr/contacts/{contactId}/export");

    public async Task<ApiResponse<DsrExportJobDto>> GetExportJobStatusAsync(
        Guid jobId, CancellationToken ct = default)
        => await GetAsync<DsrExportJobDto>($"api/v1/gdpr/export-jobs/{jobId}");

    public async Task<ApiResponse> DownloadExportAsync(Guid jobId, Guid contactId)
        => await DownloadFileAsync(
            $"api/v1/gdpr/export-jobs/{jobId}/download",
            $"dsr-export-{contactId:N}.json",
            "application/json");

    // ── Anonymization ─────────────────────────────────────────────────────────

    public async Task<ApiResponse<AnonymizeContactResult>> AnonymizeAsync(
        Guid contactId, CancellationToken ct = default)
        => await PostAsync<AnonymizeContactResult>($"api/v1/gdpr/contacts/{contactId}/anonymize");

    // ── Hard Delete ───────────────────────────────────────────────────────────

    public async Task<ApiResponse> HardDeleteAsync(Guid contactId, CancellationToken ct = default)
    {
        await AttachTokenAsync();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"api/v1/gdpr/contacts/{contactId}");
            request.Headers.Add("X-Gdpr-Confirm", "HARD-DELETE");

            var resp = await _http.SendAsync(request);
            if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
            return await resp.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts)
                ?? ApiResponse.Fail("Unknown error.");
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }
}
