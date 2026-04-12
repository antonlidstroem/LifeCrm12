#!/usr/bin/env python3
"""Validates required files exist and forbidden files are absent."""
import sys
from pathlib import Path

REQUIRED = [
    # Core
    "src/LifeCrm.Core/LifeCrm.Core.csproj",
    "src/LifeCrm.Core/Enums/CampaignStatus.cs",
    "src/LifeCrm.Core/Enums/UserRole.cs",
    "src/LifeCrm.Core/Interfaces/ISignalRSettings.cs",
    "src/LifeCrm.Core/Interfaces/IUnitOfWork.cs",
    "src/LifeCrm.Core/Entities/AppSettings.cs",
    # Application
    "src/LifeCrm.Application/DependencyInjection.cs",
    "src/LifeCrm.Application/Reports/Commands/CreateReportCommand.cs",
    "src/LifeCrm.Application/Reports/Commands/SubmitReportCommand.cs",
    # Infrastructure
    "src/LifeCrm.Infrastructure/Services/SignalRSettingsService.cs",
    "src/LifeCrm.Infrastructure/Persistence/Configuration/AppSettingsConfiguration.cs",
    "src/LifeCrm.Infrastructure/Migrations/20260412000000_AddAppSettings.cs",
    # API
    "src/LifeCrm.Api/Controllers/v1/SignalRSettingsController.cs",
    "src/LifeCrm.Api/Hubs/ActivityHub.cs",
    # Web
    "src/LifeCrm.Web/Program.cs",
    "src/LifeCrm.Web/Services/ApiClient.cs",
    "src/LifeCrm.Web/Services/SignalRSettingsApiClient.cs",
    "src/LifeCrm.Web/Pages/Admin/SignalRSettingsPage.razor",
    "src/LifeCrm.Web/Components/Shared/ReportStatusChip.razor",
]

FORBIDDEN = [
    "src/LifeCrm.Web/WebProgram.cs",
    "src/LifeCrm.Core/Enums/Enums.cs",
    "src/LifeCrm.Core/Interfaces/IServices.cs",
    "src/LifeCrm.Infrastructure/Persistence/Configuration/CrmEntityConfiguration.cs",
    "src/LifeCrm.Infrastructure/Persistence/Configuration/NewsletterEntityConfiguration.cs",
    "src/LifeCrm.Infrastructure/Persistence/Configuration/ReportEntityConfiguration.cs",
]

errors = []
for f in REQUIRED:
    if not Path(f).exists(): errors.append(f"MISSING: {f}")
for f in FORBIDDEN:
    if Path(f).exists(): errors.append(f"FORBIDDEN exists: {f}")

if errors:
    for e in errors: print(e)
    sys.exit(1)
else:
    print("OK - file tree validation passed")
