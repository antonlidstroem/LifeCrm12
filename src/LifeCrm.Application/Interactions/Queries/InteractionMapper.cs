using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Core.Entities;

namespace LifeCrm.Application.Interactions.Queries;

internal static class InteractionMapper
{
    internal static InteractionDto Map(Interaction i) => new()
    {
        Id = i.Id, Type = i.Type, Body = i.Body, Subject = i.Subject,
        OccurredAt = i.OccurredAt, ContactId = i.ContactId, ContactName = i.Contact?.Name,
        ProjectId = i.ProjectId, ProjectName = i.Project?.Name,
        DueDate = i.DueDate, IsCompleted = i.IsCompleted, CreatedByName = i.CreatedBy, CreatedAt = i.CreatedAt
    };
}
