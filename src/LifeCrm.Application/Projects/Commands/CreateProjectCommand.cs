using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Projects.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Projects.Commands;

public class CreateProjectCommand : IRequest<Guid>
{
    public CreateProjectRequest Request { get; }
    public CreateProjectCommand(CreateProjectRequest r) { Request = r; }
}

public sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateProjectHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Guid> Handle(CreateProjectCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var p = new Project { Id = Guid.NewGuid(), OrganizationId = orgId, Name = cmd.Request.Name.Trim(), Description = cmd.Request.Description?.Trim(), Status = cmd.Request.Status, BudgetGoal = cmd.Request.BudgetGoal, StartDate = cmd.Request.StartDate, EndDate = cmd.Request.EndDate, Location = cmd.Request.Location?.Trim(), Notes = cmd.Request.Notes?.Trim() };
        await _uow.Projects.AddAsync(p, ct);
        await _uow.SaveChangesAsync(ct);
        return p.Id;
    }
}
