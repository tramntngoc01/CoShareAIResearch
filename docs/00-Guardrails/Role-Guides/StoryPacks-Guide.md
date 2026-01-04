
# StoryPacks Guide (AI-readable)

Date: 2026-01-03

## Location in repo
- `docs/02-Requirements/StoryPacks/US-XXX.md`

## Template
- `docs/02-Requirements/StoryPacks/US-XXX-TEMPLATE.md`

## Ownership
- Owner (StoryPack overall): BA
- **Owner (API mapping section): Tech Lead**
- Contributor: QA
- Users: Dev BE/FE, DevOps, any AI agent

## Purpose

Enable short prompt:
> "Implement US-XXX end-to-end"
without guessing contract or data.

## Rules
- Link to OpenAPI paths and schema names; never guess fields.
- Include authz, error codes, idempotency, migration notes, test IDs.
- Keep Traceability updated.
