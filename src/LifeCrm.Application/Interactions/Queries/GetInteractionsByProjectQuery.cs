// src/LifeCrm.Application/Interactions/Queries/GetInteractionsByProjectQuery.cs
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries;

public class GetInteractionsByProjectQuery : IRequest<IReadOnlyList<InteractionDto>>
{
    public Guid ProjectId { get; }
    public GetInteractionsByProjectQuery(Guid id) { ProjectId = id; }
}

public sealed class GetInteractionsByProjectHandler
    : IRequestHandler<GetInteractionsByProjectQuery, IReadOnlyList<InteractionDto>>
{
    private readonly IUnitOfWork _uow;
    public GetInteractionsByProjectHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<InteractionDto>> Handle(
        GetInteractionsByProjectQuery q, CancellationToken ct)
    {
        // FIX F: Same as contact handler — Take is in the repository SQL query.
        var list = await _uow.InteractionRepo.GetByProjectAsync(q.ProjectId, ct);
        return list.Select(InteractionMapper.Map).ToList().AsReadOnly();
    }
}
