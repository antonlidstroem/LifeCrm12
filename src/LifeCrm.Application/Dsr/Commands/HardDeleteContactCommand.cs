using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Dsr.Commands;

public record HardDeleteContactCommand(Guid ContactId) : IRequest<Unit>;

public sealed class HardDeleteContactHandler : IRequestHandler<HardDeleteContactCommand, Unit>
{
    private readonly IDsrService _dsr;

    public HardDeleteContactHandler(IDsrService dsr) { _dsr = dsr; }

    public async Task<Unit> Handle(HardDeleteContactCommand cmd, CancellationToken ct)
    {
        await _dsr.HardDeleteAsync(cmd.ContactId, ct);
        return Unit.Value;
    }
}