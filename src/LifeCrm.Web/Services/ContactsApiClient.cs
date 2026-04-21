using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Contacts.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class ContactsApiClient : ApiClientBase
{
    public ContactsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<ContactListDto>>> GetContactsAsync(
        PaginationParams p, Guid? contactId = null)
    {
        var url = $"api/v1/contacts?page={p.Page}&pageSize={p.PageSize}" +
                  $"&search={Uri.EscapeDataString(p.Search ?? "")}&sortAscending={p.SortAscending}";
        return await GetAsync<PagedResult<ContactListDto>>(url);
    }

    public async Task<ApiResponse<ContactDto>> GetContactAsync(Guid id)
        => await GetAsync<ContactDto>($"api/v1/contacts/{id}");

    public async Task<ApiResponse<Guid>> CreateContactAsync(CreateContactRequest request)
        => await PostAsync<Guid>("api/v1/contacts", request);

    public async Task<ApiResponse> UpdateContactAsync(Guid id, UpdateContactRequest request)
        => await PutAsync($"api/v1/contacts/{id}", request);

    public async Task<ApiResponse> DeleteContactAsync(Guid id)
        => await DeleteAsync($"api/v1/contacts/{id}");

    public async Task<ApiResponse> ImportCsvAsync(byte[] csvBytes)
        => await PostVoidAsync("api/v1/contacts/import", csvBytes);

    public async Task<ApiResponse> ExportCsvAsync()
        => await DownloadFileAsync("api/v1/contacts/export", "contacts.csv", "text/csv");
}
