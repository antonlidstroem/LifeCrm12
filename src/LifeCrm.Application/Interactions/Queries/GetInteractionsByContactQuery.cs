using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries;

public class GetInteractionsByContactQuery : IRequest<IReadOnlyList<InteractionDto>>
{
    public Guid ContactId { get; }
    public GetInteractionsByContactQuery(Guid id) { ContactId = id; }
}

public sealed class GetInteractionsByContactHandler : IRequestHandler<GetInteractionsByContactQuery, IReadOnlyList<InteractionDto>>
{
    private readonly IUnitOfWork _uow;
    public GetInteractionsByContactHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<InteractionDto>> Handle(GetInteractionsByContactQuery q, CancellationToken ct)
    {
        var list = await _uow.InteractionRepo.GetByContactAsync(q.ContactId, ct);
        return list.Take(200).Select(InteractionMapper.Map).ToList().AsReadOnly();
    }
}
