using System.Net.Http.Json;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class NewslettersApiClient : ApiClientBase
{
    public NewslettersApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<NewsletterListDto>>> GetNewslettersAsync(PaginationParams p, NewsletterStatus? status = null)
    {
        var url = $"api/v1/newsletters?page={p.Page}&pageSize={p.PageSize}&search={Uri.EscapeDataString(p.Search ?? "")}";
        if (status.HasValue) url += $"&status={status.Value}";
        return await GetAsync<PagedResult<NewsletterListDto>>(url);
    }

    public async Task<ApiResponse<NewsletterDetailDto>> GetNewsletterAsync(Guid id)
        => await GetAsync<NewsletterDetailDto>($"api/v1/newsletters/{id}");

    public async Task<ApiResponse<Guid>> CreateNewsletterAsync(CreateNewsletterRequest req)
        => await PostAsync<Guid>("api/v1/newsletters", req);

    public async Task<ApiResponse> UpdateNewsletterAsync(Guid id, UpdateNewsletterRequest req)
        => await PutAsync($"api/v1/newsletters/{id}", req);

    public async Task<ApiResponse> DeleteNewsletterAsync(Guid id)
        => await DeleteAsync($"api/v1/newsletters/{id}");

    public async Task<ApiResponse<NewsletterPreviewDto>> PreviewNewsletterRecipientsAsync(string? tagFilter, string? contactTypeFilter)
    {
        var url = "api/v1/newsletters/recipients/preview";
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(tagFilter)) queryParams.Add($"tagFilter={Uri.EscapeDataString(tagFilter)}");
        if (!string.IsNullOrWhiteSpace(contactTypeFilter)) queryParams.Add($"contactTypeFilter={Uri.EscapeDataString(contactTypeFilter)}");
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

        return await GetAsync<NewsletterPreviewDto>(url);
    }

    public async Task<ApiResponse<NewsletterSendResultDto>> SendNewsletterAsync(Guid id, SendNewsletterRequest req)
        => await PostAsync<NewsletterSendResultDto>($"api/v1/newsletters/{id}/send", req);

    public async Task<ApiResponse<AttachmentDto>> UploadNewsletterAttachmentAsync(Guid newsletterId, Stream fileStream, string fileName, string contentType)
    {
        await AttachTokenAsync();
        try
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var resp = await _http.PostAsync($"api/v1/newsletters/{newsletterId}/attachments", content);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<AttachmentDto>>(JsonOpts)
                   ?? ApiResponse<AttachmentDto>.Fail("Upload failed.");
        }
        catch (Exception ex) { return ApiResponse<AttachmentDto>.Fail(ex.Message); }
    }

    public async Task<ApiResponse> DeleteNewsletterAttachmentAsync(Guid newsletterId, Guid attachmentId)
        => await DeleteAsync($"api/v1/newsletters/{newsletterId}/attachments/{attachmentId}");

    public async Task<ApiResponse> DownloadNewsletterAttachmentAsync(Guid newsletterId, Guid attachmentId, string fileName)
        => await DownloadFileAsync($"api/v1/newsletters/{newsletterId}/attachments/{attachmentId}/download", fileName);
}