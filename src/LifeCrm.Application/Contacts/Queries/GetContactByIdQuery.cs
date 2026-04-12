using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Contacts.Queries;

public class GetContactByIdQuery : IRequest<ContactDto>
{
    public Guid ContactId { get; }
    public GetContactByIdQuery(Guid id) { ContactId = id; }
}

public sealed class GetContactByIdHandler : IRequestHandler<GetContactByIdQuery, ContactDto>
{
    private readonly IUnitOfWork _uow;
    public GetContactByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<ContactDto> Handle(GetContactByIdQuery q, CancellationToken ct)
    {
        var c = await _uow.Contacts.GetByIdAsync(q.ContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), q.ContactId);
        var donated       = await _uow.Donations.Query().Where(d => d.ContactId == q.ContactId).SumAsync(d => (decimal?)d.Amount, ct) ?? 0;
        var donationCount = await _uow.Donations.CountAsync(d => d.ContactId == q.ContactId, ct);
        var interactions  = await _uow.Interactions.CountAsync(i => i.ContactId == q.ContactId, ct);
        return new ContactDto
        {
            Id = c.Id, Name = c.Name, Type = c.Type, Email = c.Email, Phone = c.Phone,
            AddressLine1 = c.AddressLine1, AddressLine2 = c.AddressLine2, City = c.City,
            StateProvince = c.StateProvince, PostalCode = c.PostalCode, Country = c.Country,
            Tags = c.Tags, Notes = c.Notes, PrimaryContactName = c.PrimaryContactName,
            EmailOptOut = c.EmailOptOut, CreatedAt = c.CreatedAt, LastModifiedAt = c.LastModifiedAt,
            DonationCount = donationCount, TotalDonated = donated, InteractionCount = interactions
        };
    }
}
