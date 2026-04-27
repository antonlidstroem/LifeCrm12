// src/LifeCrm.Web/Shared/BreadcrumbItem.cs
namespace LifeCrm.Web.Shared;

/// <summary>
/// A single breadcrumb entry used by <see cref="PageHeader"/>.
/// Defined in its own file so it can be referenced from any page
/// without relying on Razor component codegen ordering.
/// </summary>
public record BreadcrumbItem(string Text, string? Href = null);
