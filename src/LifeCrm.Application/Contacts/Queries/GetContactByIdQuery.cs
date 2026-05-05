// src/LifeCrm.Application/Contacts/Queries/GetContactByIdQuery.cs
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Contacts.DTOs;
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
        // FIX D: Previous implementation made 3 separate round-trips:
        //   1. GetByIdAsync (loads full entity)
        //   2. SumAsync on Donations
        //   3. CountAsync on Donations
        //   4. CountAsync on Interactions
        //
        // Replace with a single projection query that computes all aggregates
        // in one SQL statement, the same pattern already used in GetCampaignByIdQuery.

        var result = await _uow.Contacts.Query()
            .Where(c => c.Id == q.ContactId)
            .Select(c => new
            {
                c.Id, c.Name, c.Type, c.Email, c.Phone,
                c.AddressLine1, c.AddressLine2, c.City,
                c.StateProvince, c.PostalCode, c.Country,
                c.Tags, c.Notes, c.PrimaryContactName,
                c.EmailOptOut, c.CreatedAt, c.LastModifiedAt,
                TotalDonated    = c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                DonationCount   = c.Donations.Count(d => !d.IsDeleted),
                InteractionCount = c.Interactions.Count(i => !i.IsDeleted)
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Contact), q.ContactId);

        return new ContactDto
        {
            Id                 = result.Id,
            Name               = result.Name,
            Type               = result.Type,
            Email              = result.Email,
            Phone              = result.Phone,
            AddressLine1       = result.AddressLine1,
            AddressLine2       = result.AddressLine2,
            City               = result.City,
            StateProvince      = result.StateProvince,
            PostalCode         = result.PostalCode,
            Country            = result.Country,
            Tags               = result.Tags,
            Notes              = result.Notes,
            PrimaryContactName = result.PrimaryContactName,
            EmailOptOut        = result.EmailOptOut,
            CreatedAt          = result.CreatedAt,
            LastModifiedAt     = result.LastModifiedAt,
            TotalDonated       = result.TotalDonated,
            DonationCount      = result.DonationCount,
            InteractionCount   = result.InteractionCount
        };
    }
}
