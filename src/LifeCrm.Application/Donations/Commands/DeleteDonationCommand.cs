using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Donations.Commands;

public class DeleteDonationCommand : IRequest<Unit>, IAuditableRequest
{
    public Guid DonationId { get; }
    public string AuditEntityName => "Donation";
    public Guid   AuditEntityId   => DonationId;
    public string AuditAction     => "Deleted";
    public DeleteDonationCommand(Guid id) { DonationId = id; }
}

public sealed class DeleteDonationHandler : IRequestHandler<DeleteDonationCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteDonationHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteDonationCommand cmd, CancellationToken ct)
    {
        var d = await _uow.Donations.GetByIdAsync(cmd.DonationId, ct) ?? throw new NotFoundException(nameof(Donation), cmd.DonationId);
        _uow.Donations.Delete(d);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
