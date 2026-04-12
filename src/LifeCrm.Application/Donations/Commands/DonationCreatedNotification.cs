using MediatR;

namespace LifeCrm.Application.Donations.Commands;

public record DonationCreatedNotification(Guid DonationId, string? ContactEmail, bool EmailOptOut) : INotification;
