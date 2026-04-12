#!/usr/bin/env python3
"""
One-type-per-file validator — enforces no god classes.
Documented exceptions (Phase 2, Decision 2 and 6):
  - MediatR Command+Handler pairs in same file (MediatR convention)
  - DTO files grouping related request/response records
  - Validator pairs (tightly coupled Create/Update validators)
  - BaseEntity+TenantEntity (inheritance hierarchy, always used together)
  - IEmailService+EmailAttachment / ICsvService+CsvParseError (record owned by interface)
  - ActivityHub+ActivityNotifier (hub implementation pair)
  - SignalRService+ActivityEvent (event record owned by service)
"""
import re, sys
from pathlib import Path

MEDIATOR_PAT = re.compile(r'IRequest|IRequestHandler|INotificationHandler|INotification\b')
DTO_PAT = re.compile(r'record\s+\w+(?:Dto|Request|Response|Result|Row|Event)\b')
VALIDATOR_PAT = re.compile(r'AbstractValidator<')
IFACE_OWNED_RECORD = {'ICsvService.cs', 'IEmailService.cs'}
KNOWN_PAIRS = {'BaseEntity.cs', 'ActivityHub.cs', 'SignalRService.cs'}

type_pat = re.compile(
    r'^\s*(?:public|internal|sealed|abstract|static)[\s\w]*(?:class|record|struct|interface|enum)\s+(\w+)',
    re.MULTILINE
)

violations = []
for f in Path('src').rglob('*.cs'):
    content = f.read_text(encoding='utf-8', errors='ignore')
    names = set(type_pat.findall(content))
    if len(names) <= 1: continue
    if MEDIATOR_PAT.search(content): continue
    if DTO_PAT.search(content): continue
    if VALIDATOR_PAT.search(content): continue
    if f.name in IFACE_OWNED_RECORD: continue
    if f.name in KNOWN_PAIRS: continue
    violations.append(f"{f}: {sorted(names)}")

if violations:
    print("GOD-CLASS VIOLATIONS (unrelated types in one file):")
    for v in violations: print(f"  {v}")
    sys.exit(1)
else:
    cs = len(list(Path('src').rglob('*.cs')))
    print(f"OK - god-class check passed ({cs} .cs files scanned, all exceptions documented)")
