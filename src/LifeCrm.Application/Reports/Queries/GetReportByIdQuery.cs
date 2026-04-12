using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Queries;

public class GetReportByIdQuery : IRequest<MissionReportDetailDto>
{
    public Guid ReportId { get; }
    public GetReportByIdQuery(Guid id) { ReportId = id; }
}

public sealed class GetReportByIdHandler : IRequestHandler<GetReportByIdQuery, MissionReportDetailDto>
{
    private readonly IUnitOfWork _uow;
    public GetReportByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<MissionReportDetailDto> Handle(GetReportByIdQuery q, CancellationToken ct)
    {
        var r = await _uow.Reports.Query()
            .Include(r => r.Campaign).Include(r => r.Project)
            .Include(r => r.Decisions.Where(d => !d.IsDeleted))
            .Include(r => r.PeopleGroups.Where(pg => !pg.IsDeleted))
            .Include(r => r.PrayerPoints.Where(pp => !pp.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == q.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), q.ReportId);
        return new MissionReportDetailDto
        {
            Id = r.Id, Title = r.Title, ReportDate = r.ReportDate, Location = r.Location,
            Status = r.Status, Language = r.Language, AuthorName = r.AuthorName,
            CampaignId = r.CampaignId, CampaignName = r.Campaign?.Name,
            ProjectId  = r.ProjectId,  ProjectName  = r.Project?.Name,
            HtmlBody = r.HtmlBody, EventsHeld = r.EventsHeld, TotalAttendees = r.TotalAttendees,
            NewContacts = r.NewContacts, MaterialsDistributed = r.MaterialsDistributed,
            ReviewComment = r.ReviewComment, ApprovedBy = r.ApprovedBy,
            DecisionTotal = r.Decisions.Sum(d => d.Count), PrayerCount = r.PrayerPoints.Count,
            CreatedAt = r.CreatedAt, SubmittedAt = r.SubmittedAt, ApprovedAt = r.ApprovedAt,
            Decisions = r.Decisions.Select(d => new DecisionCountDto { Id = d.Id, DecisionType = d.DecisionType, Count = d.Count }).ToList().AsReadOnly(),
            PeopleGroups = r.PeopleGroups.Select(p => new PeopleGroupReachedDto { Id = p.Id, JpCode = p.JpCode, PeopleGroupName = p.PeopleGroupName, Country = p.Country, Language = p.Language, EstimatedReached = p.EstimatedReached, Notes = p.Notes }).ToList().AsReadOnly(),
            PrayerPoints = r.PrayerPoints.Select(p => new PrayerPointDto { Id = p.Id, ReportId = p.ReportId, Title = p.Title, Detail = p.Detail, Status = p.Status, AnsweredAt = p.AnsweredAt, AnsweredNote = p.AnsweredNote, CreatedAt = p.CreatedAt }).ToList().AsReadOnly()
        };
    }
}
