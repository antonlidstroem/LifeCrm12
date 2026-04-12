using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Projects.DTOs;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

public class ProjectsApiClient : ApiClientBase
{
    public ProjectsApiClient(HttpClient h, ILocalStorageService s, IJSRuntime js) : base(h, s, js) { }

    public async Task<ApiResponse<PagedResult<ProjectListDto>>> GetProjectsAsync(PaginationParams p)
        => await GetAsync<PagedResult<ProjectListDto>>($"api/v1/projects?page={p.Page}&pageSize={p.PageSize}&search={Uri.EscapeDataString(p.Search ?? "")}");
    public async Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id)         => await GetAsync<ProjectDto>($"api/v1/projects/{id}");
    public async Task<ApiResponse<Guid>>       CreateProjectAsync(CreateProjectRequest req)  => await PostAsync<Guid>("api/v1/projects", req);
    public async Task<ApiResponse>             UpdateProjectAsync(Guid id, UpdateProjectRequest req) => await PutAsync($"api/v1/projects/{id}", req);
    public async Task<ApiResponse>             DeleteProjectAsync(Guid id)      => await DeleteAsync($"api/v1/projects/{id}");
}
