using Blazored.LocalStorage;
using LifeCrm.Contracts.Campaigns.DTOs;
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Contracts.Contacts.DTOs;
using LifeCrm.Contracts.Documents.DTOs;
using LifeCrm.Contracts.Donations.DTOs;
using LifeCrm.Contracts.Interactions.DTOs;
using LifeCrm.Contracts.Newsletters.DTOs;
using LifeCrm.Contracts.Projects.DTOs;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Contracts.Users.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services;

/// <summary>
/// Façade that aggregates all domain API clients.
/// Pages that need multiple domains inject this instead of several individual clients.
/// </summary>
public class ApiClient : ApiClientBase
{
    private readonly ContactsApiClient     _contacts;
    private readonly DonationsApiClient    _donations;
    private readonly CampaignsApiClient    _campaigns;
    private readonly ProjectsApiClient     _projects;
    private readonly InteractionsApiClient _interactions;
    private readonly UsersApiClient        _users;
    private readonly DashboardApiClient    _dashboard;
    private readonly ReportsApiClient      _reports;
    private readonly NewslettersApiClient  _newsletters;

    public ApiClient(
        HttpClient http, ILocalStorageService storage, IJSRuntime js,
        ContactsApiClient contacts, DonationsApiClient donations,
        CampaignsApiClient campaigns, ProjectsApiClient projects,
        InteractionsApiClient interactions, UsersApiClient users,
        DashboardApiClient dashboard, ReportsApiClient reports,
        NewslettersApiClient newsletters)
        : base(http, storage, js)
    {
        _contacts = contacts; _donations = donations; _campaigns = campaigns;
        _projects = projects; _interactions = interactions; _users = users;
        _dashboard = dashboard; _reports = reports; _newsletters = newsletters;
    }

    // ── Contacts ─────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<ContactListDto>>> GetContactsAsync(PaginationParams p)
        => _contacts.GetContactsAsync(p);
    public Task<ApiResponse<ContactDto>> GetContactAsync(Guid id) => _contacts.GetContactAsync(id);
    public Task<ApiResponse<Guid>>       CreateContactAsync(CreateContactRequest req) => _contacts.CreateContactAsync(req);
    public Task<ApiResponse>             UpdateContactAsync(Guid id, UpdateContactRequest req) => _contacts.UpdateContactAsync(id, req);
    public Task<ApiResponse>             DeleteContactAsync(Guid id) => _contacts.DeleteContactAsync(id);
    public Task<ApiResponse>             ExportContactsCsvAsync() => _contacts.ExportContactsCsvAsync();
    public Task<ApiResponse<ImportContactsResult>> ImportContactsCsvAsync(Stream s, string name) => _contacts.ImportContactsCsvAsync(s, name);

    // ── Donations ─────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<DonationListDto>>> GetDonationsAsync(
        PaginationParams p, Guid? contactId = null, DateOnly? fromDate = null,
        DateOnly? toDate = null, Guid? campaignId = null, Guid? projectId = null)
        => _donations.GetDonationsAsync(p, contactId, fromDate, toDate, campaignId, projectId);
    public Task<ApiResponse<DonationDto>> GetDonationAsync(Guid id) => _donations.GetDonationAsync(id);
    public Task<ApiResponse<Guid>>        CreateDonationAsync(CreateDonationRequest req) => _donations.CreateDonationAsync(req);
    public Task<ApiResponse>              UpdateDonationAsync(Guid id, UpdateDonationRequest req) => _donations.UpdateDonationAsync(id, req);
    public Task<ApiResponse>              DeleteDonationAsync(Guid id) => _donations.DeleteDonationAsync(id);
    public Task<ApiResponse<DocumentDto>> GenerateReceiptAsync(Guid id, bool sendByEmail) => _donations.GenerateReceiptAsync(id, sendByEmail);
    public Task<ApiResponse>              DownloadReceiptAsync(Guid donationId, Guid documentId) => _donations.DownloadReceiptAsync(donationId, documentId);
    public Task<ApiResponse>              DownloadLatestReceiptAsync(Guid donationId) => _donations.DownloadLatestReceiptAsync(donationId);

    // ── Campaigns ─────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<CampaignListDto>>> GetCampaignsAsync(PaginationParams p, Guid? projectId = null)
        => _campaigns.GetCampaignsAsync(p, projectId);
    public Task<ApiResponse<CampaignDto>> GetCampaignAsync(Guid id) => _campaigns.GetCampaignAsync(id);
    public Task<ApiResponse<Guid>>        CreateCampaignAsync(CreateCampaignRequest req) => _campaigns.CreateCampaignAsync(req);
    public Task<ApiResponse>              UpdateCampaignAsync(Guid id, UpdateCampaignRequest req) => _campaigns.UpdateCampaignAsync(id, req);
    public Task<ApiResponse>              DeleteCampaignAsync(Guid id) => _campaigns.DeleteCampaignAsync(id);

    public Task<ApiResponse<NewsletterPreviewDto>> PreviewCampaignNewsletterAsync(Guid campaignId, string? tagFilter)
        => GetAsync<NewsletterPreviewDto>($"api/v1/campaigns/{campaignId}/newsletter/preview" +
           (tagFilter is not null ? $"?tagFilter={Uri.EscapeDataString(tagFilter)}" : ""));
    public Task<ApiResponse<NewsletterSendResultDto>> SendCampaignNewsletterAsync(
        Guid campaignId, CampaignSendNewsletterRequest req)
        => PostAsync<NewsletterSendResultDto>($"api/v1/campaigns/{campaignId}/newsletter/send", req);

    // ── Projects ─────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<ProjectListDto>>> GetProjectsAsync(PaginationParams p) => _projects.GetProjectsAsync(p);
    public Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id) => _projects.GetProjectAsync(id);
    public Task<ApiResponse<Guid>>       CreateProjectAsync(CreateProjectRequest req) => _projects.CreateProjectAsync(req);
    public Task<ApiResponse>             UpdateProjectAsync(Guid id, UpdateProjectRequest req) => _projects.UpdateProjectAsync(id, req);
    public Task<ApiResponse>             DeleteProjectAsync(Guid id) => _projects.DeleteProjectAsync(id);

    // ── Interactions ─────────────────────────────────────────────────────────
    public Task<ApiResponse<InteractionDto>> GetInteractionAsync(Guid id) => _interactions.GetInteractionAsync(id);
    public Task<ApiResponse<Guid>>           CreateInteractionAsync(CreateInteractionRequest req) => _interactions.CreateInteractionAsync(req);
    public Task<ApiResponse>                 UpdateInteractionAsync(Guid id, UpdateInteractionRequest req) => _interactions.UpdateInteractionAsync(id, req);
    public Task<ApiResponse>                 DeleteInteractionAsync(Guid id) => _interactions.DeleteInteractionAsync(id);

    // Replace the Users section in src/LifeCrm.Web/Services/ApiClient.cs
    // ── Users ─────────────────────────────────────────────────────────────────
    public Task<ApiResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync() => _users.GetUsersAsync();
    public Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest req) => _users.CreateUserAsync(req);
    public Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest req) => _users.UpdateUserAsync(id, req);
    public Task<ApiResponse> DeleteUserAsync(Guid id) => _users.DeleteUserAsync(id);
    public Task<ApiResponse> ChangeUserRoleAsync(Guid id, UserRole role) => _users.ChangeUserRoleAsync(id, role);
    public Task<ApiResponse> DeactivateUserAsync(Guid id) => _users.DeactivateUserAsync(id);
    public Task<ApiResponse> ActivateUserAsync(Guid id) => _users.ActivateUserAsync(id);



    // ── Newsletters ───────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<NewsletterListDto>>> GetNewslettersAsync(PaginationParams p, NewsletterStatus? status = null)
        => _newsletters.GetNewslettersAsync(p, status);
    public Task<ApiResponse<NewsletterDetailDto>> GetNewsletterAsync(Guid id) => _newsletters.GetNewsletterAsync(id);
    public Task<ApiResponse<Guid>>   CreateNewsletterAsync(CreateNewsletterRequest req) => _newsletters.CreateNewsletterAsync(req);
    public Task<ApiResponse>         UpdateNewsletterAsync(Guid id, UpdateNewsletterRequest req) => _newsletters.UpdateNewsletterAsync(id, req);
    public Task<ApiResponse>         DeleteNewsletterAsync(Guid id) => _newsletters.DeleteNewsletterAsync(id);
    public Task<ApiResponse<NewsletterPreviewDto>> PreviewNewsletterRecipientsAsync(
        string? tagFilter, string? contactTypeFilter)
        => _newsletters.PreviewNewsletterRecipientsAsync(tagFilter, contactTypeFilter);
    public Task<ApiResponse<NewsletterSendResultDto>> SendNewsletterAsync(Guid id, SendNewsletterRequest req)
        => _newsletters.SendNewsletterAsync(id, req);
    public Task<ApiResponse<AttachmentDto>> UploadNewsletterAttachmentAsync(
        Guid id, Stream stream, string fileName, string contentType)
        => _newsletters.UploadNewsletterAttachmentAsync(id, stream, fileName, contentType);
    public Task<ApiResponse> DeleteNewsletterAttachmentAsync(Guid id, Guid attachmentId)
        => _newsletters.DeleteNewsletterAttachmentAsync(id, attachmentId);

    // ── Reports ───────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<MissionReportListDto>>> GetReportsAsync(
        PaginationParams p, ReportStatus? status = null, Guid? campaignId = null, Guid? projectId = null)
        => _reports.GetReportsAsync(p, status, campaignId, projectId);
    public Task<ApiResponse<MissionReportDetailDto>> GetReportAsync(Guid id) => _reports.GetReportAsync(id);
    public Task<ApiResponse<Guid>>   CreateReportAsync(CreateReportRequest req) => _reports.CreateReportAsync(req);
    public Task<ApiResponse>         UpdateReportAsync(Guid id, UpdateReportRequest req) => _reports.UpdateReportAsync(id, req);
    public Task<ApiResponse>         DeleteReportAsync(Guid id) => _reports.DeleteReportAsync(id);
    public Task<ApiResponse>         SubmitReportAsync(Guid id) => _reports.SubmitReportAsync(id);
    public Task<ApiResponse>         ApproveReportAsync(Guid id) => _reports.ApproveReportAsync(id);
    public Task<ApiResponse>         ReturnReportAsync(Guid id, ReturnForRevisionRequest req) => _reports.ReturnReportAsync(id, req);
    public Task<ApiResponse<DecisionCountDto>> UpsertDecisionCountAsync(Guid reportId, UpsertDecisionCountRequest req)
        => _reports.UpsertDecisionCountAsync(reportId, req);
    public Task<ApiResponse<IReadOnlyList<PeopleGroupSearchDto>>> SearchPeopleGroupsAsync(string q)
        => _reports.SearchPeopleGroupsAsync(q);
    public Task<ApiResponse<PeopleGroupReachedDto>> AddPeopleGroupAsync(Guid reportId, AddPeopleGroupRequest req)
        => _reports.AddPeopleGroupAsync(reportId, req);
    public Task<ApiResponse> RemovePeopleGroupAsync(Guid reportId, Guid entryId)
        => _reports.RemovePeopleGroupAsync(reportId, entryId);
    public Task<ApiResponse<PrayerPointDto>> AddReportPrayerPointAsync(Guid reportId, CreatePrayerPointRequest req)
        => _reports.AddReportPrayerPointAsync(reportId, req);
    public Task<ApiResponse<IReadOnlyList<AnsweredPrayerWidgetDto>>> GetAnsweredPrayersThisMonthAsync()
        => _reports.GetAnsweredPrayersThisMonthAsync();
    public Task<ApiResponse<IReadOnlyList<PrayerPointDto>>> GetActivePrayerPointsAsync()
        => _reports.GetActivePrayerPointsAsync();
    public Task<ApiResponse<PrayerPointDto>> CreateStandalonePrayerPointAsync(CreatePrayerPointRequest req)
        => _reports.CreateStandalonePrayerPointAsync(req);
    public Task<ApiResponse<PrayerPointDto>> MarkPrayerAnsweredAsync(Guid id, MarkAnsweredRequest req)
        => _reports.MarkPrayerAnsweredAsync(id, req);
    public Task<ApiResponse> DeletePrayerPointAsync(Guid id) => _reports.DeletePrayerPointAsync(id);

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public Task<ApiResponse<DashboardDto>> GetDashboardAsync() => _dashboard.GetDashboardAsync();
    public Task<ApiResponse<DocumentDto>>  GenerateDonationSummaryAsync(GenerateDonationSummaryRequest req)
        => _dashboard.GenerateDonationSummaryAsync(req);
    public Task<ApiResponse> DownloadDocumentAsync(Guid id) => _dashboard.DownloadDocumentAsync(id);

    // ── Direct email (Finance/Admin) ──────────────────────────────────────────
    public Task<ApiResponse<string>> SendDirectEmailAsync(
        string toEmail, string toName, string subject, string htmlBody)
        => PostAsync<string>("api/v1/directemail/send",
            new { toEmail, toName, subject, htmlBody });
}
