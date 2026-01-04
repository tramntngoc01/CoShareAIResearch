You are a Senior BA. Create/update:
docs/02-Requirements/Traceability.md

INPUTS:
- All Stories files in docs/02-Requirements/Stories/
- All SRS files in docs/02-Requirements/SRS/
- Modules list in docs/01-Discovery/Modules.md

RULES:
- Create a mapping table with columns:
  Story ID | Module | Screen(s) | API (TBD - Tech Lead) | DB (TBD/Conceptual) | Test IDs (placeholders)
- Fill Screen if known, else TBD
- Do NOT guess API paths
- Ensure every P0 story exists in the table

OUTPUT:
- Traceability.md
