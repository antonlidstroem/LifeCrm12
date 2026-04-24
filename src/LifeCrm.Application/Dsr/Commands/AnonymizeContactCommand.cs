using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Dsr.Commands;

public record AnonymizeContactCommand(
    Guid ContactId,
    string Reason,
    bool RetainFinancialRecords = true) : IRequest<Unit>;

public sealed class AnonymizeContactHandler : IRequestHandler<AnonymizeContactCommand, Unit>
{
    private readonly IDsrService _dsr;

    public AnonymizeContactHandler(IDsrService dsr) { _dsr = dsr; }

    public async Task<Unit> Handle(AnonymizeContactCommand cmd, CancellationToken ct)
    {
        await _dsr.AnonymizeAsync(cmd.ContactId, cmd.Reason, cmd.RetainFinancialRecords, ct);
        return Unit.Value;
    }
}