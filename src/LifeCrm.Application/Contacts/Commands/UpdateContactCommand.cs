using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public sealed class UpdateContactCommand : IRequest<Unit>, IAuditableRequest
{
    public UpdateContactRequest Request { get; }
    public string AuditEntityName => "Contact";
    public Guid   AuditEntityId   => Request.Id;
    public string AuditAction     => "Updated";

    public UpdateContactCommand(UpdateContactRequest request) { Request = request; }
}

public sealed class UpdateContactHandler : IRequestHandler<UpdateContactCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailHashService _emailHash;

    public UpdateContactHandler(IUnitOfWork uow, IEmailHashService emailHash)
    {
        _uow       = uow;
        _emailHash = emailHash;
    }

    public async Task<Unit> Handle(UpdateContactCommand command, CancellationToken ct)
    {
        var req     = command.Request;
        var contact = await _uow.Contacts.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(nameof(Contact), req.Id);

        var normalizedEmail = req.Email?.Trim().ToLowerInvariant();

        // ── Name split ────────────────────────────────────────────────────────
        contact.FirstName = req.FirstName.Trim();
        contact.LastName  = req.LastName?.Trim() ?? string.Empty;
#pragma warning disable CS0618
        contact.Name      = $"{contact.FirstName} {contact.LastName}".Trim();
#pragma warning restore CS0618

        contact.Type      = req.Type;
        contact.Email     = normalizedEmail;

        // Refresh the hash whenever the email changes
        contact.EmailHash = normalizedEmail != null
            ? _emailHash.Hash(normalizedEmail)
            : null;

        contact.Phone              = req.Phone?.Trim();
        contact.AddressLine1       = req.AddressLine1?.Trim();
        contact.AddressLine2       = req.AddressLine2?.Trim();
        contact.City               = req.City?.Trim();
        contact.StateProvince      = req.StateProvince?.Trim();
        contact.PostalCode         = req.PostalCode?.Trim();
        contact.Country            = req.Country?.Trim();
        contact.Tags               = req.Tags?.Trim();
        contact.Notes              = req.Notes?.Trim();
        contact.PrimaryContactName = req.PrimaryContactName?.Trim();
        contact.EmailOptOut        = req.EmailOptOut;

        _uow.Contacts.Update(contact);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
