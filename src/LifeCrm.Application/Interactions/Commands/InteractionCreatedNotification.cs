using MediatR;

namespace LifeCrm.Application.Interactions.Commands;

public sealed record InteractionCreatedNotification(Guid InteractionId, Guid? ContactId) : INotification;
