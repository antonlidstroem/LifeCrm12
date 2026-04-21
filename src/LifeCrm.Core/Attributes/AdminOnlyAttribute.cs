namespace LifeCrm.Core.Attributes;

/// <summary>
/// Documents that this entity or DTO contains sensitive admin-only enrichment data.
/// NOT a functional security attribute — [Authorize(Policy="AdminOnly")] handles that.
/// This marker triggers a code-review gate: any type decorated [AdminOnly] must never
/// appear in public-facing queries, DTOs, or API responses without AdminOnly authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AdminOnlyAttribute : Attribute
{
    public string Reason { get; }
    public AdminOnlyAttribute(string reason = "Contains sensitive enrichment data — admin access only.")
        => Reason = reason;
}
