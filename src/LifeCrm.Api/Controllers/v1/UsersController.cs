// src/LifeCrm.Api/Controllers/v1/UsersController.cs
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Users.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1;

[Authorize(Policy = "AdminOnly")]
public class UsersController : ApiControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UsersController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        var users = await _context.Users.IgnoreQueryFilters()
            .Where(u => u.OrganizationId == orgId && !u.IsDeleted)
            .OrderBy(u => u.FullName)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);
        return OkResponse<IReadOnlyList<UserSummaryDto>>(users);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ValidationException("FullName", "Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email", "Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 10)
            throw new ValidationException("Password", "Password must be at least 10 characters.");

        var exists = await _context.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()
                        && u.OrganizationId == orgId && !u.IsDeleted, ct);
        if (exists)
            throw new ConflictException("A user with that email already exists in your organisation.");

        var user = new Core.Entities.ApplicationUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedBy = _currentUser.UserId?.ToString() ?? "admin"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return CreatedResponse(nameof(GetAll), null, user.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
            ?? throw new NotFoundException("User", id);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (request.NewPassword.Length < 10)
                throw new ValidationException("NewPassword", "Password must be at least 10 characters.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        }
        user.Role = request.Role;
        await _context.SaveChangesAsync(ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        if (id == _currentUser.UserId)
            throw new ValidationException("Id", "You cannot delete your own account.");

        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
            ?? throw new NotFoundException("User", id);

        user.IsDeleted = true;
        user.DeletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(ct);
        return NoContentResponse();
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid id, [FromBody] UserRole newRole, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        if (id == _currentUser.UserId && newRole != UserRole.Admin)
            throw new ValidationException("Role", "You cannot remove your own Admin role.");
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
            ?? throw new NotFoundException("User", id);
        user.Role = newRole;
        await _context.SaveChangesAsync(ct);
        return NoContentResponse();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (id == _currentUser.UserId)
            throw new ValidationException("Id", "You cannot deactivate your own account.");
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
            ?? throw new NotFoundException("User", id);
        user.IsActive = false;
        await _context.SaveChangesAsync(ct);
        return NoContentResponse();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException();
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct)
            ?? throw new NotFoundException("User", id);
        user.IsActive = true;
        await _context.SaveChangesAsync(ct);
        return NoContentResponse();
    }
}