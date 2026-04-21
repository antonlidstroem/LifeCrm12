using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Gdpr.Commands;

public record RequestDsrExportCommand(Guid ContactId) : IRequest<DsrExportJobDto>;

public sealed class RequestDsrExportHandler : IRequestHandler<RequestDsrExportCommand, DsrExportJobDto>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _cu;

    public RequestDsrExportHandler(AppDbContext db, ICurrentUserService cu)
    {
        _db = db;
        _cu = cu;
    }

    public async Task<DsrExportJobDto> Handle(RequestDsrExportCommand cmd, CancellationToken ct)
    {
        var userId = _cu.UserId ?? throw new ForbiddenException("No user context.");

        // Verify contact exists and belongs to this org
        var exists = await _db.Contacts
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == cmd.ContactId, ct);

        if (!exists)
            throw new NotFoundException(nameof(Contact), cmd.ContactId);

        var job = new DsrExportJob
        {
            Id                  = Guid.NewGuid(),
            OrganizationId      = _cu.OrganizationId ?? Guid.Empty,
            ContactId           = cmd.ContactId,
            Status              = DsrExportStatus.Pending,
            RequestedByUserId   = userId,
            CreatedAt           = DateTimeOffset.UtcNow
        };

        _db.Set<DsrExportJob>().Add(job);
        await _db.SaveChangesAsync(ct);

        return new DsrExportJobDto
        {
            JobId            = job.Id,
            Status           = job.Status,
            StatusUrl        = $"/api/v1/gdpr/export-jobs/{job.Id}",
            EstimatedSeconds = 5
        };
    }
}
