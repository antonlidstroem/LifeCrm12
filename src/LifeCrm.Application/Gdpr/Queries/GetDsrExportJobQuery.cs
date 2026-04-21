using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Gdpr.Queries;

public record GetDsrExportJobQuery(Guid JobId) : IRequest<DsrExportJobDto>;

public sealed class GetDsrExportJobHandler : IRequestHandler<GetDsrExportJobQuery, DsrExportJobDto>
{
    private readonly AppDbContext _db;

    public GetDsrExportJobHandler(AppDbContext db) => _db = db;

    public async Task<DsrExportJobDto> Handle(GetDsrExportJobQuery query, CancellationToken ct)
    {
        var job = await _db.Set<DsrExportJob>()
            .FirstOrDefaultAsync(j => j.Id == query.JobId, ct)
            ?? throw new NotFoundException(nameof(DsrExportJob), query.JobId);

        // Check expiry
        if (job.Status == DsrExportStatus.Completed
            && job.ExpiresAt.HasValue
            && job.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            job.Status = DsrExportStatus.Expired;
            await _db.SaveChangesAsync(ct);
        }

        return new DsrExportJobDto
        {
            JobId       = job.Id,
            Status      = job.Status,
            StatusUrl   = $"/api/v1/gdpr/export-jobs/{job.Id}",
            DownloadUrl = job.Status == DsrExportStatus.Completed
                ? $"/api/v1/gdpr/export-jobs/{job.Id}/download"
                : null,
            ExpiresAt   = job.ExpiresAt
        };
    }
}
