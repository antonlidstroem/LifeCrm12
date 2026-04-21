using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

// ── Base ──────────────────────────────────────────────────────────────────────

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
}

public abstract class TenantEntity : BaseEntity
{
    public Guid OrganizationId { get; set; }
}

// ── Organisation & User ───────────────────────────────────────────────────────

public class Organization : BaseEntity
{
    public string Name    { get; set; } = string.Empty;
    public string? Slug   { get; set; }
    public bool IsActive  { get; set; } = true;
    public ICollection<ApplicationUser> Users    { get; set; } = new List<ApplicationUser>();
    public ICollection<Contact>         Contacts { get; set; } = new List<Contact>();
}

public class ApplicationUser : TenantEntity
{
    public string FullName     { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role       { get; set; } = UserRole.Viewer;
    public bool IsActive       { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public string CreatedBy    { get; set; } = "system";
    public Organization? Organization { get; set; }
}

// ── Financials ────────────────────────────────────────────────────────────────

public class Donation : TenantEntity
{
    public Guid    ContactId             { get; set; }
    public decimal Amount                { get; set; }
    public DateOnly Date                 { get; set; }
    public DonationStatus Status         { get; set; } = DonationStatus.Confirmed;
    public Guid?   CampaignId            { get; set; }
    public Guid?   ProjectId             { get; set; }
    public Guid?   RecurringDonationId   { get; set; }
    public string? PaymentMethod         { get; set; }
    public string? ReferenceNumber       { get; set; }
    public string? Notes                 { get; set; }
    public bool    ReceiptSent           { get; set; } = false;
    public DateTimeOffset? ReceiptSentAt { get; set; }
    public string  CreatedBy             { get; set; } = "system";
    public Contact?  Contact             { get; set; }
    public Campaign? Campaign            { get; set; }
    public Project?  Project             { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}

public class Campaign : TenantEntity
{
    public string Name             { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public CampaignStatus Status   { get; set; } = CampaignStatus.Draft;
    public decimal? BudgetGoal     { get; set; }
    public DateOnly? StartDate     { get; set; }
    public DateOnly? EndDate       { get; set; }
    public string? Notes           { get; set; }
    public Guid? ProjectId         { get; set; }
    public Project? Project        { get; set; }
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}

public class Project : TenantEntity
{
    public string Name              { get; set; } = string.Empty;
    public string? Description      { get; set; }
    public ProjectStatus Status     { get; set; } = ProjectStatus.Planning;
    public string? Location         { get; set; }
    public decimal? BudgetGoal      { get; set; }
    public DateOnly? StartDate      { get; set; }
    public DateOnly? EndDate        { get; set; }
    public string? Notes            { get; set; }
    public ICollection<Donation>    Donations    { get; set; } = new List<Donation>();
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
    public ICollection<Campaign>    Campaigns    { get; set; } = new List<Campaign>();
}

// ── Interactions & Documents ──────────────────────────────────────────────────

public class Interaction : TenantEntity
{
    public InteractionType Type      { get; set; } = InteractionType.Note;
    public string Body               { get; set; } = string.Empty;
    public string? Subject           { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? ContactId           { get; set; }
    public Guid? ProjectId           { get; set; }
    public DateOnly? DueDate         { get; set; }
    public bool IsCompleted          { get; set; } = false;
    public string CreatedBy          { get; set; } = "system";
    public Contact? Contact          { get; set; }
    public Project? Project          { get; set; }
}

public class Document : TenantEntity
{
    public DocumentType Type     { get; set; }
    public string FileName       { get; set; } = string.Empty;
    public byte[] PdfBytes       { get; set; } = Array.Empty<byte>();
    public Guid?  ContactId      { get; set; }
    public Guid?  DonationId     { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd   { get; set; }
    public Contact?  Contact     { get; set; }
    public Donation? Donation    { get; set; }
}

// ── Communications ────────────────────────────────────────────────────────────

public class Newsletter : TenantEntity
{
    public string Title               { get; set; } = string.Empty;
    public string Subject             { get; set; } = string.Empty;
    public string HtmlBody            { get; set; } = string.Empty;
    public NewsletterStatus Status    { get; set; } = NewsletterStatus.Draft;
    public string? TagFilter          { get; set; }
    public string? ContactTypeFilter  { get; set; }
    public DateTimeOffset? SentAt     { get; set; }
    public string? SentBy             { get; set; }
    public int  SentCount             { get; set; }
    public int  SkippedCount          { get; set; }
    public int  ErrorCount            { get; set; }
    public string CreatedBy           { get; set; } = "system";
    public ICollection<NewsletterAttachment> Attachments { get; set; } = new List<NewsletterAttachment>();
}

public class NewsletterAttachment : TenantEntity
{
    public Guid   NewsletterId    { get; set; }
    public string FileName        { get; set; } = string.Empty;
    public string ContentType     { get; set; } = string.Empty;
    public byte[] FileBytes       { get; set; } = Array.Empty<byte>();
    public long   FileSizeBytes   { get; set; }
    public Newsletter? Newsletter { get; set; }
}

// ── Mission / Field Reports ───────────────────────────────────────────────────

public class MissionReport : TenantEntity
{
    public string Title                     { get; set; } = string.Empty;
    public DateOnly ReportDate              { get; set; }
    public string? Location                 { get; set; }
    public ReportStatus Status              { get; set; } = ReportStatus.Draft;
    public ReportLanguage Language          { get; set; } = ReportLanguage.English;
    public Guid?   CampaignId               { get; set; }
    public Guid?   ProjectId                { get; set; }
    public Guid    AuthorUserId             { get; set; }
    public string  AuthorName               { get; set; } = string.Empty;
    public string  HtmlBody                 { get; set; } = string.Empty;
    public int?    EventsHeld               { get; set; }
    public int?    TotalAttendees           { get; set; }
    public int?    NewContacts              { get; set; }
    public int?    MaterialsDistributed     { get; set; }
    public DateTimeOffset? SubmittedAt      { get; set; }
    public DateTimeOffset? ApprovedAt       { get; set; }
    public string? ApprovedBy               { get; set; }
    public string? ReviewComment            { get; set; }
    public Campaign? Campaign               { get; set; }
    public Project?  Project                { get; set; }
    public ICollection<DecisionCount>      Decisions    { get; set; } = new List<DecisionCount>();
    public ICollection<PeopleGroupReached> PeopleGroups { get; set; } = new List<PeopleGroupReached>();
    public ICollection<PrayerPoint>        PrayerPoints { get; set; } = new List<PrayerPoint>();
}

public class PrayerPoint : TenantEntity
{
    public Guid?  ReportId       { get; set; }
    public string Title          { get; set; } = string.Empty;
    public string Detail         { get; set; } = string.Empty;
    public PrayerStatus Status   { get; set; } = PrayerStatus.Active;
    public DateTimeOffset? AnsweredAt { get; set; }
    public string? AnsweredNote  { get; set; }
    public string  CreatedBy     { get; set; } = "system";
    public MissionReport? Report { get; set; }
}

public class DecisionCount : TenantEntity
{
    public Guid         ReportId     { get; set; }
    public DecisionType DecisionType { get; set; }
    public int          Count        { get; set; }
    public MissionReport? Report     { get; set; }
}

public class PeopleGroupReached : TenantEntity
{
    public Guid    ReportId          { get; set; }
    public string  JpCode            { get; set; } = string.Empty;
    public string  PeopleGroupName   { get; set; } = string.Empty;
    public string  Country           { get; set; } = string.Empty;
    public string? Language          { get; set; }
    public int?    EstimatedReached  { get; set; }
    public string? Notes             { get; set; }
    public MissionReport? Report     { get; set; }
}

public class PeopleGroupSeed : BaseEntity
{
    public string  JpCode      { get; set; } = string.Empty;
    public string  Name        { get; set; } = string.Empty;
    public string  Country     { get; set; } = string.Empty;
    public string? Language    { get; set; }
    public int?    Population  { get; set; }
    public bool    IsUnreached { get; set; } = true;
    public string? Region      { get; set; }
}

// ── System / Audit ────────────────────────────────────────────────────────────

public class AppSettings : BaseEntity
{
    public string Key   { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class AuditLog
{
    public Guid   Id             { get; set; }
    public Guid   OrganizationId { get; set; }
    public string EntityName     { get; set; } = string.Empty;
    public Guid   EntityId       { get; set; }
    public string Action         { get; set; } = string.Empty;
    public string ChangedBy      { get; set; } = "system";
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? OldValues     { get; set; }
    public string? NewValues     { get; set; }
}
