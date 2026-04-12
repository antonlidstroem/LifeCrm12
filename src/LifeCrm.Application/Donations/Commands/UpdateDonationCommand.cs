using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Donations.Commands;

public class UpdateDonationCommand : IRequest<Unit>, IAuditableRequest
{
    public UpdateDonationRequest Request { get; }
    public string AuditEntityName => "Donation";
    public Guid   AuditEntityId   => Request.Id;
    public string AuditAction     => "Updated";
    public UpdateDonationCommand(UpdateDonationRequest r) { Request = r; }
}

public sealed class UpdateDonationHandler : IRequestHandler<UpdateDonationCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateDonationHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateDonationCommand cmd, CancellationToken ct)
    {
        var r = cmd.Request;
        var d = await _uow.Donations.GetByIdAsync(r.Id, ct) ?? throw new NotFoundException(nameof(Donation), r.Id);
        d.Amount = r.Amount; d.Date = r.Date; d.Status = r.Status;
        d.CampaignId = r.CampaignId; d.ProjectId = r.ProjectId;
        d.PaymentMethod = r.PaymentMethod?.Trim(); d.ReferenceNumber = r.ReferenceNumber?.Trim(); d.Notes = r.Notes?.Trim();
        _uow.Donations.Update(d);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
