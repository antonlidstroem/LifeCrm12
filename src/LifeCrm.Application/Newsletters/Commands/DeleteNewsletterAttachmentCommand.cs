using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands;

public class DeleteNewsletterAttachmentCommand : IRequest<Unit>
{
    public Guid NewsletterId { get; }
    public Guid AttachmentId { get; }
    public DeleteNewsletterAttachmentCommand(Guid newslId, Guid attId) { NewsletterId = newslId; AttachmentId = attId; }
}

public sealed class DeleteNewsletterAttachmentHandler : IRequestHandler<DeleteNewsletterAttachmentCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteNewsletterAttachmentHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteNewsletterAttachmentCommand cmd, CancellationToken ct)
    {
        var att = await _uow.NewsletterAttachments.GetByIdAsync(cmd.AttachmentId, ct) ?? throw new NotFoundException(nameof(NewsletterAttachment), cmd.AttachmentId);
        if (att.NewsletterId != cmd.NewsletterId) throw new ForbiddenException("Attachment does not belong to this newsletter.");
        var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct);
        if (nl?.Status == NewsletterStatus.Sent) throw new ConflictException("Cannot remove attachments from a sent newsletter.");
        _uow.NewsletterAttachments.Delete(att);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
