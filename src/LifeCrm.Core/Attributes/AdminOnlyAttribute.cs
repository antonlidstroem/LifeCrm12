// src/LifeCrm.Core/Attributes/AdminOnlyAttribute.cs
//
// Not a functional security attribute (AuthorizeAttribute handles that).
// This is a code-review marker — anyone reading an entity or DTO decorated
// with [AdminOnly] knows immediately it must never appear in public-facing
// queries, DTOs, or API responses.
[AttributeUsage(AttributeTargets.Class)]
public class AdminOnlyAttribute : Attribute
{
    public string Reason { get; }
    public AdminOnlyAttribute(string reason = "Contains sensitive enrichment data.")
        => Reason = reason;
}