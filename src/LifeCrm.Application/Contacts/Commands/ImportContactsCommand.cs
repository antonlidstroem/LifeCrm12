using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public record ImportContactsResult
{
    public int SuccessCount { get; init; }
    public int ErrorCount   { get; init; }
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();
}

public record ImportRowError(int RowNumber, string Reason, string RawRow);

public sealed class ImportContactsCommand : IRequest<ImportContactsResult>
{
    public byte[] CsvBytes { get; }
    public ImportContactsCommand(byte[] csvBytes) { CsvBytes = csvBytes; }
}

public sealed class ImportContactsHandler : IRequestHandler<ImportContactsCommand, ImportContactsResult>
{
    private readonly ICsvService _csv;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    public ImportContactsHandler(ICsvService csv, IUnitOfWork uow, ICurrentUserService currentUser)
    { _csv = csv; _uow = uow; _currentUser = currentUser; }

    public async Task<ImportContactsResult> Handle(ImportContactsCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var (rows, parseErrors) = await _csv.ImportAsync<ContactCsvRow>(command.CsvBytes);
        int successCount = 0;
        var importErrors = parseErrors.Select(e => new ImportRowError(e.RowNumber, e.Reason, e.RawRow)).ToList();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Name)) { importErrors.Add(new ImportRowError(0, "Name is required.", string.Empty)); continue; }
            var contactType = ContactType.Individual;
            if (!string.IsNullOrWhiteSpace(row.Type) && !Enum.TryParse<ContactType>(row.Type.Trim(), ignoreCase: true, out contactType))
                contactType = ContactType.Individual;
            var contact = new Contact
            {
                Id = Guid.NewGuid(), OrganizationId = orgId, Name = row.Name.Trim(), Type = contactType,
                Email = row.Email?.Trim().ToLowerInvariant(), Phone = row.Phone?.Trim(), Tags = row.Tags?.Trim(),
                AddressLine1 = row.AddressLine1?.Trim(), City = row.City?.Trim(),
                StateProvince = row.StateProvince?.Trim(), PostalCode = row.PostalCode?.Trim(),
                Country = row.Country?.Trim(), Notes = row.Notes?.Trim()
            };
            await _uow.Contacts.AddAsync(contact, cancellationToken);
            successCount++;
        }
        await _uow.SaveChangesAsync(cancellationToken);
        return new ImportContactsResult { SuccessCount = successCount, ErrorCount = importErrors.Count, ValidationErrors = importErrors.Select(e => e.Reason).ToList() };
    }
}
