using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Commands;

public sealed class CreateInteractionCommand : IRequest<Guid>
{
    public CreateInteractionRequest Request { get; }
    public CreateInteractionCommand(CreateInteractionRequest r) { Request = r; }
}

public sealed class CreateInteractionHandler : IRequestHandler<CreateInteractionCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    private readonly IMediator _mediator;
    public CreateInteractionHandler(IUnitOfWork uow, ICurrentUserService cu, IMediator mediator) { _uow = uow; _cu = cu; _mediator = mediator; }

    public async Task<Guid> Handle(CreateInteractionCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var r = cmd.Request;
        if (!r.ContactId.HasValue && !r.ProjectId.HasValue)
            throw new ValidationException("ContactId", "An interaction must be linked to a contact or a project.");
        var interaction = new Interaction { Id = Guid.NewGuid(), OrganizationId = orgId, Type = r.Type, Body = r.Body.Trim(), Subject = r.Subject?.Trim(), OccurredAt = r.OccurredAt, ContactId = r.ContactId, ProjectId = r.ProjectId, DueDate = r.DueDate, IsCompleted = r.IsCompleted };
        await _uow.Interactions.AddAsync(interaction, ct);
        await _uow.SaveChangesAsync(ct);
        await _mediator.Publish(new InteractionCreatedNotification(interaction.Id, interaction.ContactId), ct);
        return interaction.Id;
    }
}
