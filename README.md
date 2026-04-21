# LifeCrm

ASP.NET Core 8 + Blazor WASM CRM for churches, NGOs, and community organisations.

## Architecture

```
LifeCrm.sln
└── src/
    ├── LifeCrm.Core          — Entities, enums, interfaces (no dependencies)
    ├── LifeCrm.Application   — MediatR commands/queries, DTOs, validators
    ├── LifeCrm.Infrastructure — EF Core, services, background workers
    ├── LifeCrm.Api           — ASP.NET Core Web API (hosts Blazor WASM)
    └── LifeCrm.Web           — Blazor WebAssembly frontend
```

## Layers

| Layer | Contents |
|---|---|
| **Core** | Contact, Event, Tag, ContactProfile, MentorRelationship, Donation, Campaign, Project, Interaction, Newsletter, MissionReport, PrayerPoint, ConsentRecord, PropertyAuditLog |
| **Application** | CQRS handlers (MediatR), DTOs, FluentValidation, GDPR commands, Event commands, Enrichment commands |
| **Infrastructure** | AppDbContext, EF Core configs, 3 migrations, 3 background services, all domain services |
| **API** | Auth, Contacts, Events, Enrichment, GDPR, Donations, Newsletters, DirectEmail, Unsubscribe controllers |
| **Web** | Login, Dashboard, Contacts, Events, Enrichment (admin), GDPR admin, MudBlazor UI |

## Getting started

### Prerequisites
- .NET 8 SDK
- SQL Server (or LocalDB)

### 1. Configure

Edit `src/LifeCrm.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LifeCrmDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "your-strong-secret-min-32-characters-here"
  }
}
```

### 2. Run

```bash
cd src/LifeCrm.Api
dotnet run
```

The API auto-applies all migrations on startup and seeds a default admin account.

### 3. Login

- URL: `https://localhost:7001`
- Email: `admin@lifecrm.dev`
- Password: `Admin123!@#`

**Change this immediately after first login.**

## Database migrations (run order)

| Order | File | Contents |
|---|---|---|
| 1 | `20260416071240_Init` | All original tables (Contacts, Donations, Campaigns, Projects, etc.) |
| 2 | `20260418000000_AddGdprLayer` | ConsentRecords, PropertyAuditLogs, AuditOutbox, DsrExportJobs, DataProtectionKeys, Contact GDPR columns |
| 3 | `20260420000000_AddHumanGrowthLayer` | Events, EventAttendances, Tags, ContactTags, ContactProfiles, MentorRelationships |

## Post-deploy checklist

- [ ] Set `Jwt:SecretKey` to a strong secret (min 32 chars) in user secrets / environment
- [ ] Configure SMTP via Admin → E-postinställningar
- [ ] Change default admin password
- [ ] Set `Email:DryRun` to `false` when email is configured
- [ ] Re-save all contacts to generate correct HMAC-SHA256 EmailHash values (migration uses SQL SHA2_256 as bootstrap approximation)
- [ ] Review retention settings at Admin → GDPR & Retention

## Key design decisions

### Simplicity-first contact model
Only **FirstName + Email** are required. Everything else is optional. A contact can be created from a Sunday check-in in under 5 seconds.

### GDPR layer
- Email, Phone, Address, Notes encrypted at rest (ASP.NET Core Data Protection API)
- EmailHash (HMAC-SHA256) for indexed lookup without decrypting
- ConsentRecord table (append-only) tracks every grant and withdrawal
- PropertyAuditInterceptor + AuditOutbox → PropertyAuditLog (zero hot-path latency)
- AnonymizeContact wipes all PII + deletes Profile/Tags/MentorRelationships
- DSR export includes EventAttendance history

### Human Growth layer
- EventAttendance is the primary engagement data point
- EngagementSegment (New/Growing/Core/AtRisk/Lapsed) computed at query time, never stored
- ContactProfile is in a **separate table** — impossible to accidentally expose in Contact queries
- [AdminOnly] attribute marks enrichment entities for code-review enforcement
- MentorRelationship is directional and admin-only — contacts are never shown this data
