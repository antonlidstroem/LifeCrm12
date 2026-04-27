// src/LifeCrm.Application/Users/DTOs/UserDtos.cs
using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Contracts.Users.DTOs;

public record UserSummaryDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateUserRequest
{
    [Required][MaxLength(200)] public string FullName { get; init; } = string.Empty;
    [Required][EmailAddress][MaxLength(320)] public string Email { get; init; } = string.Empty;
    [Required][MinLength(10)][MaxLength(128)] public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.Manager;
}

public record UpdateUserRequest
{
    [MaxLength(200)] public string? FullName { get; init; }
    [MaxLength(128)] public string? NewPassword { get; init; }
    public UserRole Role { get; init; } = UserRole.Manager;
}