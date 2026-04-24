using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        // Kolla om vår admin redan finns
        var existingAdmin = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "admin@lifecrm.dev");

        if (existingAdmin != null) return; // Om admin finns, gör inget mer.

        _logger.LogInformation("No admin found, starting seeding...");

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Demo Organisation",
            Slug = "demo",
            IsActive = true
        };
        _db.Organizations.Add(org);

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id, // Här kopplas de ihop korrekt
            FullName = "Admin User",
            Email = "admin@lifecrm.dev",
            // Här skapas en RIKTIG hash som koden kan verifiera
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"),
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false
        };
        _db.Users.Add(admin);

        await _db.SaveChangesAsync();
    }
}