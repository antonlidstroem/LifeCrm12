using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Projects.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Projects.Commands;

public class UpdateProjectCommand : IRequest<Unit>
{
    public UpdateProjectRequest Request { get; }
    public UpdateProjectCommand(UpdateProjectRequest r) { Request = r; }
}

public sealed class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateProjectHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateProjectCommand cmd, CancellationToken ct)
    {
        var p = await _uow.Projects.GetByIdAsync(cmd.Request.Id, ct) ?? throw new NotFoundException(nameof(Project), cmd.Request.Id);
        p.Name = cmd.Request.Name.Trim(); p.Description = cmd.Request.Description?.Trim();
        p.Status = cmd.Request.Status; p.BudgetGoal = cmd.Request.BudgetGoal;
        p.StartDate = cmd.Request.StartDate; p.EndDate = cmd.Request.EndDate;
        p.Location = cmd.Request.Location?.Trim(); p.Notes = cmd.Request.Notes?.Trim();
        _uow.Projects.Update(p);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
