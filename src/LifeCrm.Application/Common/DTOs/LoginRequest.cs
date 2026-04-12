using System.ComponentModel.DataAnnotations;
namespace LifeCrm.Application.Common.DTOs;
public record LoginRequest
{
    [Required][EmailAddress][MaxLength(320)] public string Email { get; init; } = string.Empty;
    [Required][MinLength(1)][MaxLength(128)] public string Password { get; init; } = string.Empty;
}
