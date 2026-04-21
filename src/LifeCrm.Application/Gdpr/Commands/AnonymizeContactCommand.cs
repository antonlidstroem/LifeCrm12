using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Gdpr.Commands;

public record AnonymizeContactCommand(Guid ContactId) : IRequest<AnonymizeContactResult>;

public sealed class AnonymizeContactHandler
    : IRequestHandler<AnonymizeContactCommand, AnonymizeContactResult>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _cu;

    public AnonymizeContactHandler(AppDbContext db, ICurrentUserService cu)
    {
        _db = db;
        _cu = cu;
    }

    public async Task<AnonymizeContactResult> Handle(
        AnonymizeContactCommand cmd, CancellationToken ct)
    {
        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Documents)
            .Include(c => c.Interactions)
            .Include(c => c.ContactTags)       // NEW — Human Growth layer
            .FirstOrDefaultAsync(c => c.Id == cmd.ContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), cmd.ContactId);

        if (contact.IsAnonymized)
            throw new ConflictException("Contact is already anonymized.");

        var anonymizedAt  = DateTimeOffset.UtcNow;
        var fieldsChanged = new List<string>();

        void Wipe(string field, Action setter) { setter(); fieldsChanged.Add(field); }

        // ── Anonymize PII fields ──────────────────────────────────────────
        Wipe(nameof(Contact.FirstName),          () => contact.FirstName          = "[Removed]");
        Wipe(nameof(Contact.LastName),           () => contact.LastName           = "[Removed]");
#pragma warning disable CS0618
        Wipe(nameof(Contact.Name),               () => contact.Name               = "[Removed]");
#pragma warning restore CS0618
        Wipe(nameof(Contact.Email),              () => contact.Email              = null);
        Wipe(nameof(Contact.EmailHash),          () => contact.EmailHash          = null);
        Wipe(nameof(Contact.Phone),              () => contact.Phone              = null);
        Wipe(nameof(Contact.Address),            () => contact.Address            = null);
        Wipe(nameof(Contact.City),               () => contact.City               = null);
        Wipe(nameof(Contact.Country),            () => contact.Country            = null);
        Wipe(nameof(Contact.Notes),              () => contact.Notes              = null);
        Wipe(nameof(Contact.Source),             () => contact.Source             = null);

        contact.EmailOptOut       = true;
        contact.EmailOptOutAt     = anonymizedAt;
        contact.EmailOptOutSource = "gdpr-erasure";
        contact.IsAnonymized      = true;
        contact.AnonymizedAt      = anonymizedAt;
        contact.LastModifiedAt    = anonymizedAt;
        contact.LastModifiedBy    = _cu.UserId?.ToString() ?? "gdpr-system";

        // ── Anonymize Interaction free-text ───────────────────────────────
        foreach (var i in contact.Interactions.Where(i => !i.IsDeleted))
        {
            i.Body    = "[Anonymized]";
            i.Subject = null;
        }

        // ── Hard-delete PDF Documents (cannot anonymize binary) ───────────
        if (contact.Documents.Count > 0)
            _db.Documents.RemoveRange(contact.Documents);

        // ── Hard-delete ContactProfile (separate table — one row delete) ──
        // NEW: Human Growth layer
        var profile = await _db.Set<ContactProfile>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ContactId == cmd.ContactId, ct);
        if (profile is not null)
            _db.Set<ContactProfile>().Remove(profile);

        // ── Hard-delete ContactTags (no PII value after contact is wiped) ─
        // NEW: Human Growth layer
        if (contact.ContactTags.Count > 0)
            _db.Set<ContactTag>().RemoveRange(contact.ContactTags);

        // ── Hard-delete MentorRelationships ───────────────────────────────
        // NEW: Human Growth layer — both sides (as mentor and as mentee)
        var mentoring = await _db.Set<MentorRelationship>()
            .IgnoreQueryFilters()
            .Where(r => r.MentorContactId == cmd.ContactId || r.MenteeContactId == cmd.ContactId)
            .ToListAsync(ct);
        if (mentoring.Count > 0)
            _db.Set<MentorRelationship>().RemoveRange(mentoring);

        // EventAttendance rows are RETAINED with anonymized ContactId reference.
        // Financial records (Donations) are also RETAINED for legal obligation.

        await _db.SaveChangesAsync(ct);

        return new AnonymizeContactResult
        {
            ContactId        = contact.Id,
            AnonymizedAt     = anonymizedAt,
            FieldsAnonymized = fieldsChanged.AsReadOnly()
        };
    }
}
