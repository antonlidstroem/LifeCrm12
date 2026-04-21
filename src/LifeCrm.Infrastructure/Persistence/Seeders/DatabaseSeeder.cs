using BCrypt.Net;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.Organizations.AnyAsync(ct))
        {
            _logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        _logger.LogInformation("Seeding default organisation and admin user…");

        var orgId = Guid.NewGuid();
        var org   = new Organization
        {
            Id       = orgId,
            Name     = "Demo Organisation",
            Slug     = "demo",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var admin = new ApplicationUser
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            FullName       = "Admin",
            Email          = "admin@lifecrm.dev",
            PasswordHash   = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"),
            Role           = UserRole.Admin,
            IsActive       = true,
            CreatedAt      = DateTimeOffset.UtcNow,
            CreatedBy      = "seeder"
        };

        _db.Organizations.Add(org);
        _db.Users.Add(admin);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Seed complete. Login: admin@lifecrm.dev / Admin123!@#");
    }
}
