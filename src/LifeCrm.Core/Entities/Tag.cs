// src/LifeCrm.Core/Entities/Tag.cs
// Admin-defined taxonomy — not user-defined, prevents tag sprawl
using LifeCrm.Core.Entities;

public class Tag : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }   // UI display only
    public string? Category { get; set; } // "segment", "interest", "status"

    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
}

// src/LifeCrm.Core/Entities/ContactTag.cs
public class ContactTag : TenantEntity
{
    public Guid TagId { get; set; }
    public Guid ContactId { get; set; }
    public Guid AddedByUserId { get; set; }  // Who assigned this tag

    public Tag? Tag { get; set; }
    public Contact? Contact { get; set; }
}