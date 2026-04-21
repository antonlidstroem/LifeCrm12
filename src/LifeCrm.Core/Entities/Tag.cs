namespace LifeCrm.Core.Entities;

/// <summary>
/// Admin-defined tag taxonomy. Created by admins, assigned to contacts.
/// Replaces the free-text Contact.Tags string — prevents tag sprawl.
/// Category examples: "segment" | "interest" | "status" | "ministry"
/// </summary>
public class Tag : TenantEntity
{
    public string  Name      { get; set; } = string.Empty;
    public string? Color     { get; set; }     // Hex colour for UI display, e.g. "#4CAF50"
    public string? Category  { get; set; }

    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
}

/// <summary>
/// Join: a tag assigned to a contact. Tracks who assigned it and when.
/// </summary>
public class ContactTag : TenantEntity
{
    public Guid  TagId          { get; set; }
    public Guid  ContactId      { get; set; }
    public Guid? AddedByUserId  { get; set; }

    public Tag?     Tag     { get; set; }
    public Contact? Contact { get; set; }
}
