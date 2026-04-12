using System.ComponentModel.DataAnnotations;
namespace LifeCrm.Application.Common.DTOs;
public record ChangePasswordRequest
{
    [Required][MaxLength(128)] public string CurrentPassword { get; init; } = string.Empty;
    [Required][MinLength(10)][MaxLength(128)] public string NewPassword { get; init; } = string.Empty;
}
