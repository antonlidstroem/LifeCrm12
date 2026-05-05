using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Donations.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Donations.Queries;

public sealed class GetDonationByIdQuery : IRequest<DonationDto>
{
    public Guid DonationId { get; }
    public GetDonationByIdQuery(Guid id) { DonationId = id; }
}

public sealed class GetDonationByIdHandler : IRequestHandler<GetDonationByIdQuery, DonationDto>
{
    private readonly IUnitOfWork _uow;
    public GetDonationByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<DonationDto> Handle(GetDonationByIdQuery query, CancellationToken cancellationToken)
    {
        var d = await _uow.Donations.GetByIdAsync(query.DonationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Donation), query.DonationId);
        var contact = d.Contact ?? await _uow.Contacts.GetByIdAsync(d.ContactId, cancellationToken);
        return new DonationDto { Id = d.Id, ContactId = d.ContactId, ContactName = contact?.Name ?? string.Empty, Amount = d.Amount, Date = d.Date, Status = d.Status, CampaignId = d.CampaignId, CampaignName = d.Campaign?.Name, ProjectId = d.ProjectId, ProjectName = d.Project?.Name, RecurringDonationId = d.RecurringDonationId, PaymentMethod = d.PaymentMethod, ReferenceNumber = d.ReferenceNumber, Notes = d.Notes, ReceiptSent = d.ReceiptSent, ReceiptSentAt = d.ReceiptSentAt, CreatedAt = d.CreatedAt, LastModifiedAt = d.LastModifiedAt };
    }
}
