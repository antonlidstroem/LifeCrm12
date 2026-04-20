// src/LifeCrm.Application/Gdpr/Commands/AnonymizeContactCommand.cs
using System.Linq.Expressions;
using System.Reflection;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Gdpr.Commands;

public record AnonymizeContactCommand(Guid ContactId) : IRequest<AnonymizeContactResult>;

public record AnonymizeContactResult(
    Guid ContactId,
    DateTimeOffset AnonymizedAt,
    IReadOnlyList<string> FieldsAnonymized);

public sealed class AnonymizeContactHandler
    : IRequestHandler<AnonymizeContactCommand, AnonymizeContactResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    private readonly AppDbContext _db; 

    public AnonymizeContactHandler(IUnitOfWork uow, ICurrentUserService cu, AppDbContext db)
    {
        _uow = uow; _cu = cu; _db = db;
    }

    public async Task<AnonymizeContactResult> Handle(
        AnonymizeContactCommand cmd, CancellationToken ct)
    {
        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Documents)
            .Include(c => c.Interactions)
            .FirstOrDefaultAsync(c => c.Id == cmd.ContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), cmd.ContactId);

        if (contact.IsAnonymized)
            throw new ConflictException("Contact is already anonymized.");

        var anonymizedAt = DateTimeOffset.UtcNow;
        var fieldsAnonymized = new List<string>();

        // Anonymize contact PII
        void Anonymize<T>(
            Expression<Func<Contact, T>> prop,
            T replacement,
            string fieldName)
        {
            // Use reflection to set — keeps this handler change-safe
            var memberExpr = (MemberExpression)prop.Body;
            var propInfo = (PropertyInfo)memberExpr.Member;
            var current = propInfo.GetValue(contact);
            if (Equals(current, replacement)) return;
            propInfo.SetValue(contact, replacement);
            fieldsAnonymized.Add(fieldName);
        }

        Anonymize(c => c.FirstName, "[Removed]", nameof(Contact.FirstName));
        Anonymize(c => c.LastName, "[Removed]", nameof(Contact.LastName));
        Anonymize(c => c.Name, "[Removed]", nameof(Contact.Name));
        Anonymize(c => c.Email, null, nameof(Contact.Email));
        Anonymize(c => c.EmailHash, null, nameof(Contact.EmailHash));
        Anonymize(c => c.Phone, null, nameof(Contact.Phone));
        Anonymize(c => c.AddressLine1, null, nameof(Contact.AddressLine1));
        Anonymize(c => c.AddressLine2, null, nameof(Contact.AddressLine2));
        Anonymize(c => c.City, null, nameof(Contact.City));
        Anonymize(c => c.StateProvince, null, nameof(Contact.StateProvince));
        Anonymize(c => c.PostalCode, null, nameof(Contact.PostalCode));
        Anonymize(c => c.Country, null, nameof(Contact.Country));
        Anonymize(c => c.Notes, null, nameof(Contact.Notes));
        Anonymize(c => c.Tags, null, nameof(Contact.Tags));
        Anonymize(c => c.PrimaryContactName, null, nameof(Contact.PrimaryContactName));

        contact.EmailOptOut = true;
        contact.IsAnonymized = true;
        contact.AnonymizedAt = anonymizedAt;
        contact.LastModifiedAt = anonymizedAt;
        contact.LastModifiedBy = _cu.UserId?.ToString() ?? "gdpr-system";

        // Anonymize free-text in Interactions
        foreach (var interaction in contact.Interactions.Where(i => !i.IsDeleted))
        {
            interaction.Body = "[Anonymized]";
            interaction.Subject = null;
        }

        // Hard-delete PDF Documents (contain rendered PII — cannot be anonymized)
        _db.Documents.RemoveRange(contact.Documents);

        await _db.SaveChangesAsync(ct);

        return new AnonymizeContactResult(
            contact.Id, anonymizedAt, fieldsAnonymized.AsReadOnly());
    }
}