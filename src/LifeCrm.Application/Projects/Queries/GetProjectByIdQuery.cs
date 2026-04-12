using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Projects.Queries;

public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public Guid ProjectId { get; }
    public GetProjectByIdQuery(Guid id) { ProjectId = id; }
}

public sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IUnitOfWork _uow;
    public GetProjectByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery q, CancellationToken ct)
    {
        var p = await _uow.Projects.GetByIdAsync(q.ProjectId, ct) ?? throw new NotFoundException(nameof(Project), q.ProjectId);
        var funded = await _uow.Donations.Query().Where(d => d.ProjectId == p.Id).SumAsync(d => (decimal?)d.Amount, ct) ?? 0;
        var dc = await _uow.Donations.CountAsync(d => d.ProjectId == p.Id, ct);
        var ic = await _uow.Interactions.CountAsync(i => i.ProjectId == p.Id, ct);
        return new ProjectDto { Id = p.Id, Name = p.Name, Description = p.Description, Status = p.Status, Location = p.Location, BudgetGoal = p.BudgetGoal, TotalFunded = funded, StartDate = p.StartDate, EndDate = p.EndDate, Notes = p.Notes, DonationCount = dc, InteractionCount = ic, CreatedAt = p.CreatedAt, LastModifiedAt = p.LastModifiedAt };
    }
}
