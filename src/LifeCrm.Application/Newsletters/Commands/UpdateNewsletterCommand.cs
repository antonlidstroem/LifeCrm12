using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands;

public class UpdateNewsletterCommand : IRequest<Unit>
{
    public UpdateNewsletterRequest Request { get; }
    public UpdateNewsletterCommand(UpdateNewsletterRequest r) { Request = r; }
}

public sealed class UpdateNewsletterHandler : IRequestHandler<UpdateNewsletterCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateNewsletterHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateNewsletterCommand cmd, CancellationToken ct)
    {
        var nl = await _uow.Newsletters.GetByIdAsync(cmd.Request.Id, ct) ?? throw new NotFoundException(nameof(Newsletter), cmd.Request.Id);
        if (nl.Status == NewsletterStatus.Sent) throw new ConflictException("Cannot edit a newsletter that has already been sent.");
        nl.Title = cmd.Request.Title.Trim(); nl.Subject = cmd.Request.Subject.Trim(); nl.HtmlBody = cmd.Request.HtmlBody;
        _uow.Newsletters.Update(nl);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
