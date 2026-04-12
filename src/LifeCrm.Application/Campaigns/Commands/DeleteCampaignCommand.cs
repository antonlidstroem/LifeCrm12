using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Campaigns.Commands;

public class DeleteCampaignCommand : IRequest<Unit>
{
    public Guid CampaignId { get; }
    public DeleteCampaignCommand(Guid id) { CampaignId = id; }
}

public sealed class DeleteCampaignHandler : IRequestHandler<DeleteCampaignCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteCampaignHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteCampaignCommand cmd, CancellationToken ct)
    {
        var c = await _uow.Campaigns.GetByIdAsync(cmd.CampaignId, ct)
            ?? throw new NotFoundException(nameof(Campaign), cmd.CampaignId);
        _uow.Campaigns.Delete(c);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
