// src/LifeCrm.Application/Projects/Queries/GetProjectByIdQuery.cs
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Projects.DTOs;
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
        // FIX D: Consolidate 4 sequential round-trips into a single projection query.
        // Previous: GetByIdAsync + SumAsync(funded) + CountAsync(donations) + CountAsync(interactions)
        // Now: one SELECT with three subquery aggregations translated by EF Core.

        var result = await _uow.Projects.Query()
            .Where(p => p.Id == q.ProjectId)
            .Select(p => new
            {
                p.Id, p.Name, p.Description, p.Status,
                p.Location, p.BudgetGoal, p.StartDate, p.EndDate,
                p.Notes, p.CreatedAt, p.LastModifiedAt,
                TotalFunded      = p.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                DonationCount    = p.Donations.Count(d => !d.IsDeleted),
                InteractionCount = p.Interactions.Count(i => !i.IsDeleted)
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Project), q.ProjectId);

        return new ProjectDto
        {
            Id               = result.Id,
            Name             = result.Name,
            Description      = result.Description,
            Status           = result.Status,
            Location         = result.Location,
            BudgetGoal       = result.BudgetGoal,
            TotalFunded      = result.TotalFunded,
            StartDate        = result.StartDate,
            EndDate          = result.EndDate,
            Notes            = result.Notes,
            DonationCount    = result.DonationCount,
            InteractionCount = result.InteractionCount,
            CreatedAt        = result.CreatedAt,
            LastModifiedAt   = result.LastModifiedAt
        };
    }
}
