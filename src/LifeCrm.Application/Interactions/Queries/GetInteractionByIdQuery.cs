using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Interactions.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries;

public class GetInteractionByIdQuery : IRequest<InteractionDto>
{
    public Guid InteractionId { get; }
    public GetInteractionByIdQuery(Guid id) { InteractionId = id; }
}

public sealed class GetInteractionByIdHandler : IRequestHandler<GetInteractionByIdQuery, InteractionDto>
{
    private readonly IUnitOfWork _uow;
    public GetInteractionByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<InteractionDto> Handle(GetInteractionByIdQuery q, CancellationToken ct)
    {
        var i = await _uow.Interactions.GetByIdAsync(q.InteractionId, ct)
            ?? throw new NotFoundException(nameof(Interaction), q.InteractionId);
        return InteractionMapper.Map(i);
    }
}
