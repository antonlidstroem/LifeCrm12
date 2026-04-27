using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands;

public class CreateNewsletterCommand : IRequest<Guid>
{
    public CreateNewsletterRequest Request { get; }
    public CreateNewsletterCommand(CreateNewsletterRequest r) { Request = r; }
}

public sealed class CreateNewsletterHandler : IRequestHandler<CreateNewsletterCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateNewsletterHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Guid> Handle(CreateNewsletterCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var nl = new Newsletter { Id = Guid.NewGuid(), OrganizationId = orgId, Title = cmd.Request.Title.Trim(), Subject = cmd.Request.Subject.Trim(), HtmlBody = cmd.Request.HtmlBody, Status = NewsletterStatus.Draft, CreatedBy = _cu.UserId?.ToString() ?? "system" };
        await _uow.Newsletters.AddAsync(nl, ct);
        await _uow.SaveChangesAsync(ct);
        return nl.Id;
    }
}
