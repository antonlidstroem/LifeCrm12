using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public sealed record ContactCreatedNotification(Guid ContactId, string ContactName) : INotification;
