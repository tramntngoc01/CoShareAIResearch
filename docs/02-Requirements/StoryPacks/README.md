
# StoryPacks (Single-entry context for AI + Humans)

Date: 2026-01-03

## What is a StoryPack?
A **StoryPack** is a **one-page, single entry point** for a user story (`US-XXX`) that lets any AI agent or teammate implement end‑to‑end without guessing.

It **does not replace** SRS/Stories/OpenAPI/DB/TestCases.  
It **links** them and adds only the missing glue: mapping, constraints, and proof checklist.

## Why do we need it?
- Enables very short prompts: “Implement US-XXX end-to-end”
- Prevents API/DB guessing by pointing to exact sources
- Makes traceability effortless (one place to maintain links)

## Where it lives
- `docs/02-Requirements/StoryPacks/US-XXX.md`

## Ownership (RACI)
- **Owner (Responsible): BA**
  - Creates the StoryPack when story becomes Ready
  - Ensures AC, sample data, edge cases, and mappings exist or marked TBD
  - **Does NOT list API paths** (Tech Lead owns API list)
- **Owner for API mapping: Tech Lead**
  - Adds and maintains the **API mapping section** (OpenAPI paths, operations, schema names)
  - Validates technical constraints: error codes, idempotency, pagination
- **Contributor: QA**
  - Adds/validates Test IDs and proof steps
- **Contributors: Dev BE/FE**
  - Add implementation notes only after plan approved (optional)

## When to create/update

- Create when story is “Ready”
- Update whenever:
  - OpenAPI changes
  - DB changes
  - AC changes
  - Test IDs added/changed

## Minimum required sections
- Metadata
- Links to SRS/Story/OpenAPI/DB/Implementation Plan
- AC summary + edge cases
- AuthZ rules
- Test IDs (UT/IT/E2E/SEC)
- Done evidence checklist

## Rule for API section
- The **API mapping** section in each StoryPack is owned/maintained by **Tech Lead**. BA must not guess API paths.
