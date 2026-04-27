// src/LifeCrm.Application/Interactions/Queries/GetInteractionsByContactQuery.cs
using LifeCrm.Application.Interactions.Queries;
using LifeCrm.Contracts.Interactions.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Queries
{

    public class GetInteractionsByContactQuery : IRequest<IReadOnlyList<InteractionDto>>
    {
        public Guid ContactId { get; }
        public GetInteractionsByContactQuery(Guid id) { ContactId = id; }
    }

    public sealed class GetInteractionsByContactHandler
        : IRequestHandler<GetInteractionsByContactQuery, IReadOnlyList<InteractionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetInteractionsByContactHandler(IUnitOfWork uow) { _uow = uow; }

        public async Task<IReadOnlyList<InteractionDto>> Handle(
            GetInteractionsByContactQuery q, CancellationToken ct)
        {
            // FIX F: Removed .Take(200) here — the limit is now applied inside
            // InteractionRepository.GetByContactAsync at the SQL level, so we no
            // longer load a potentially huge list into memory just to slice it.
            var list = await _uow.InteractionRepo.GetByContactAsync(q.ContactId, ct);
            return list.Select(InteractionMapper.Map).ToList().AsReadOnly();
        }
    }
}


// src/LifeCrm.Application/Interactions/Queries/GetInteractionsByProjectQuery.cs
namespace LifeCrm.Contracts.Interactions.DTOs { 

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
            // FIX F: Same as above — limit is now in the repository.
            var list = await _uow.InteractionRepo.GetByProjectAsync(q.ProjectId, ct);
            return list.Select(InteractionMapper.Map).ToList().AsReadOnly();
        }
    }
}
