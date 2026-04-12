#!/usr/bin/env python3
"""Validates the SignalR on/off feature is correctly wired."""
import sys
from pathlib import Path

checks = [
    ("src/LifeCrm.Core/Interfaces/ISignalRSettings.cs", "GetEnabledAsync"),
    ("src/LifeCrm.Infrastructure/Services/SignalRSettingsService.cs", "_cachedValue"),
    ("src/LifeCrm.Api/Controllers/v1/SignalRSettingsController.cs", "SetEnabledAsync"),
    ("src/LifeCrm.Api/Hubs/ActivityHub.cs", "GetEnabledAsync"),
    ("src/LifeCrm.Api/Hubs/ActivityHub.cs", "Context.Abort"),
    ("src/LifeCrm.Web/Services/SignalRService.cs", "signalREnabled"),
    ("src/LifeCrm.Web/Services/SignalRSettingsApiClient.cs", "api/v1/signalrsettings"),
    ("src/LifeCrm.Web/Pages/Admin/SignalRSettingsPage.razor", "SettingsApi"),
    ("src/LifeCrm.Core/Entities/AppSettings.cs", "AppSettings"),
]

errors = []
for filepath, keyword in checks:
    p = Path(filepath)
    if not p.exists():
        errors.append(f"MISSING file: {filepath}")
    elif keyword not in p.read_text(encoding='utf-8', errors='ignore'):
        errors.append(f"MISSING '{keyword}' in {filepath}")

if errors:
    for e in errors: print(e)
    sys.exit(1)
else:
    print("OK - SignalR feature wiring validated")
