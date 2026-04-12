using System.Net.Http.Json;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Application.Interactions.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class ContactsApiClient : ApiClientBase
{
    public ContactsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<ContactListDto>>> GetContactsAsync(PaginationParams p)
        => await GetAsync<PagedResult<ContactListDto>>($"api/v1/contacts?page={p.Page}&pageSize={p.PageSize}&search={Uri.EscapeDataString(p.Search ?? "")}&sortBy={p.SortBy ?? ""}&sortAscending={p.SortAscending}");

    public async Task<ApiResponse<ContactDto>> GetContactAsync(Guid id)    => await GetAsync<ContactDto>($"api/v1/contacts/{id}");
    public async Task<ApiResponse<Guid>>       CreateContactAsync(CreateContactRequest req) => await PostAsync<Guid>("api/v1/contacts", req);
    public async Task<ApiResponse>             UpdateContactAsync(Guid id, UpdateContactRequest req) => await PutAsync($"api/v1/contacts/{id}", req);
    public async Task<ApiResponse>             DeleteContactAsync(Guid id) => await DeleteAsync($"api/v1/contacts/{id}");

    public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetInteractionsAsync(Guid contactId)
        => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/contacts/{contactId}/interactions");

    public async Task<ApiResponse> ExportContactsCsvAsync()
    {
        await AttachTokenAsync();
        try
        {
            var bytes = await _http.GetByteArrayAsync("api/v1/contacts/export");
            await _js.InvokeVoidAsync("lifecrm.downloadFile", $"contacts-{DateTime.Today:yyyy-MM-dd}.csv", Convert.ToBase64String(bytes), "text/csv");
            return ApiResponse.Ok();
        }
        catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
    }

    public async Task<ApiResponse<ImportContactsResult>> ImportContactsCsvAsync(Stream fileStream, string fileName)
    {
        await AttachTokenAsync();
        try
        {
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(ms.ToArray()), "file", fileName);
            var resp = await _http.PostAsync("api/v1/contacts/import", content);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<ImportContactsResult>>(JsonOpts) ?? ApiResponse<ImportContactsResult>.Fail("Import failed.");
        }
        catch (Exception ex) { return ApiResponse<ImportContactsResult>.Fail(ex.Message); }
    }
}
