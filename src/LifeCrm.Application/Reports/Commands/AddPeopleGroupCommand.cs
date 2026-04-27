using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class AddPeopleGroupCommand : IRequest<PeopleGroupReachedDto>
{
    public Guid ReportId { get; }
    public AddPeopleGroupRequest Request { get; }
    public AddPeopleGroupCommand(Guid reportId, AddPeopleGroupRequest r) { ReportId = reportId; Request = r; }
}

public sealed class AddPeopleGroupHandler : IRequestHandler<AddPeopleGroupCommand, PeopleGroupReachedDto>
{
    private readonly IUnitOfWork _uow;
    public AddPeopleGroupHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PeopleGroupReachedDto> Handle(AddPeopleGroupCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        var r  = cmd.Request;
        var pg = new PeopleGroupReached
        {
            Id = Guid.NewGuid(), OrganizationId = rpt.OrganizationId, ReportId = cmd.ReportId,
            JpCode = r.JpCode, PeopleGroupName = r.PeopleGroupName.Trim(), Country = r.Country.Trim(),
            Language = r.Language?.Trim(), EstimatedReached = r.EstimatedReached, Notes = r.Notes?.Trim()
        };
        await _uow.PeopleGroupsReached.AddAsync(pg, ct);
        await _uow.SaveChangesAsync(ct);
        return new PeopleGroupReachedDto { Id = pg.Id, JpCode = pg.JpCode, PeopleGroupName = pg.PeopleGroupName, Country = pg.Country, Language = pg.Language, EstimatedReached = pg.EstimatedReached, Notes = pg.Notes };
    }
}
