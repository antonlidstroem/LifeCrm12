using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Dsr.Queries;

public record ExportContactDataQuery(Guid ContactId) : IRequest<ContactDataExport>;

public sealed class ExportContactDataHandler : IRequestHandler<ExportContactDataQuery, ContactDataExport>
{
    private readonly IDsrService _dsr;

    public ExportContactDataHandler(IDsrService dsr) { _dsr = dsr; }

    public Task<ContactDataExport> Handle(ExportContactDataQuery query, CancellationToken ct)
        => _dsr.ExportAsync(query.ContactId, ct);
}