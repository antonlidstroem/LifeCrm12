using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands;

public class AddNewsletterAttachmentCommand : IRequest<AttachmentDto>
{
    public Guid NewsletterId { get; }
    public string FileName   { get; }
    public string ContentType { get; }
    public byte[] FileBytes  { get; }
    public AddNewsletterAttachmentCommand(Guid newslId, string fileName, string contentType, byte[] fileBytes)
    { NewsletterId = newslId; FileName = fileName; ContentType = contentType; FileBytes = fileBytes; }
}

public sealed class AddNewsletterAttachmentHandler : IRequestHandler<AddNewsletterAttachmentCommand, AttachmentDto>
{
    private static readonly Dictionary<string, long> AllowedTypes = new()
    {
        ["application/pdf"] = 10 * 1024 * 1024, ["image/jpeg"] = 3 * 1024 * 1024,
        ["image/png"] = 3 * 1024 * 1024, ["image/gif"] = 3 * 1024 * 1024, ["image/webp"] = 3 * 1024 * 1024,
    };
    private const int MaxImagesPerNewsletter = 3;

    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public AddNewsletterAttachmentHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<AttachmentDto> Handle(AddNewsletterAttachmentCommand cmd, CancellationToken ct)
    {
        var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct) ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);
        if (nl.Status == NewsletterStatus.Sent) throw new ConflictException("Cannot add attachments to a sent newsletter.");
        if (!AllowedTypes.TryGetValue(cmd.ContentType.ToLower(), out var maxBytes)) throw new ValidationException("ContentType", "Only PDF and image files are allowed.");
        if (cmd.FileBytes.Length > maxBytes) throw new ValidationException("FileSize", $"File exceeds the {maxBytes / 1024 / 1024} MB limit.");
        if (cmd.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var existingImages = await _uow.NewsletterAttachments.CountAsync(a => a.NewsletterId == cmd.NewsletterId && a.ContentType.StartsWith("image/"), ct);
            if (existingImages >= MaxImagesPerNewsletter) throw new ConflictException($"Maximum {MaxImagesPerNewsletter} images allowed per newsletter.");
        }
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException();
        var att = new NewsletterAttachment { Id = Guid.NewGuid(), OrganizationId = orgId, NewsletterId = cmd.NewsletterId, FileName = cmd.FileName, ContentType = cmd.ContentType.ToLower(), FileBytes = cmd.FileBytes, FileSizeBytes = cmd.FileBytes.Length };
        await _uow.NewsletterAttachments.AddAsync(att, ct);
        await _uow.SaveChangesAsync(ct);
        return new AttachmentDto { Id = att.Id, FileName = att.FileName, ContentType = att.ContentType, FileSizeBytes = att.FileSizeBytes };
    }
}
