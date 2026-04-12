# LifeCrm — Refactored Solution

## What changed in this refactor
- **One file per type** across all projects (MediatR Command+Handler pairs are the documented exception)
- All enums moved to `LifeCrm.Core/Enums/` — one file each
- All entity configurations split to individual files in `Infrastructure/Persistence/Configuration/`
- `MudColorExtensions.cs` split into four files (one per enum type)
- `CommonDtos.cs` split into individual DTO files
- `WebProgram.cs` deleted (was a duplicate `Program` class)
- All bugs from the previous analysis fixed (enum file, missing `projectId`/`campaignId` params, `_reports` field, `ReportStatusChip`, chip binding, raw string CSS braces, `ApiClient` façade missing, etc.)

## New feature: SignalR on/off toggle

Super-admins can toggle the SignalR real-time hub on or off to reduce CPU when real-time updates are not needed. The application is fully functional in both states.

### How it works
1. Setting is persisted in the `AppSettings` database table (`Key = "SignalR:Enabled"`)
2. `SignalRSettingsService` caches the value for 30 seconds (zero DB hits per hub message when stable)
3. When disabled: `ActivityNotifier` returns immediately without sending, `ActivityHub.OnConnected` aborts the connection
4. Web client reads the setting after login and skips the SignalR connection if disabled

### Admin UI
Navigate to **Admin → SignalR** (`/admin/signalr`). Toggle the switch and press Save.

### API
```
GET  /api/v1/signalrsettings       → returns { "data": true/false }
PATCH /api/v1/signalrsettings      → body: true or false
```
Both require `AdminOnly` policy.

## Getting started

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (or change the connection string)

### First run
```bash
cd src/LifeCrm.Api
dotnet run
```
The seeder runs automatically and creates:
- Organization: Demo Organisation
- Admin user: `admin@lifecrm.dev` / `Admin123!@#`
- SignalR setting: enabled (default)

### Apply migrations manually (if needed)
```bash
cd src/LifeCrm.Infrastructure
dotnet ef database update --startup-project ../LifeCrm.Api
```

### Run validation scripts
```bash
cd /path/to/solution
python3 validate/validate_one_class_per_file.py
python3 validate/validate_file_tree.py
python3 validate/validate_signalr_feature.py
```

## Project structure
```
src/
├── LifeCrm.Core/          — Entities, Enums (1 file each), Interfaces (1 file each)
├── LifeCrm.Application/   — MediatR handlers, DTOs, validators
├── LifeCrm.Infrastructure/— EF Core, repositories, services
├── LifeCrm.Api/           — ASP.NET Core controllers, SignalR hub
└── LifeCrm.Web/           — Blazor WASM, pages, components
```
