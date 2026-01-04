
# Dev Backend (.NET 9) Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Implement backend đúng theo OpenAPI + business rules, có tests cho rule quan trọng, không log PII, không hardcode secrets.

## Inputs
- StoryPack entry point: `docs/02-Requirements/StoryPacks/US-XXX.md`

- Stories module: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- SRS module: `docs/02-Requirements/SRS/SRS-<MODULE>.md`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- Error conventions: `docs/03-Design/Error-Conventions.md`
- API conventions: `docs/03-Design/API-Conventions.md`
- DB module doc: `docs/03-Design/DB/DB-<MODULE>.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`

## Outputs
- Working API endpoints aligned with OpenAPI
- Unit tests + integration tests for critical rules
- Migrations + rollback notes
- PR with checklist + CI pass

## Daily workflow (per story)
1. Confirm story is **Ready** and Implementation Plan is **Approved**
2. Verify endpoint exists in OpenAPI (add/update contract first if needed)
3. Implement:
   - Controller/endpoint
   - Service layer business logic + AuthZ checks
   - Validation + error codes
   - DB access + migrations
4. Add tests:
   - Unit for core rules/state transitions
   - Integration for API+DB critical paths
5. Update Traceability with test IDs if needed
6. PR using template, ensure CI passes, deploy staging smoke

## Backend Definition of Done
- OpenAPI contract satisfied (no extra/ missing fields)
- AuthZ enforced (403 tests for negative cases)
- Idempotency used where required
- No secrets, no PII logs; correlationId included
- Tests exist for critical business rules

## What NOT to do
- Don’t change contract silently without updating OpenAPI
- Don’t log request bodies containing sensitive data

## AI Prompt (Short)
```
You are a senior .NET 9 engineer. Implement story <US-XXX> end-to-end on backend:
- Follow OpenAPI (do not guess fields)
- Apply business rules and authz from docs
- Implement migrations if required + rollback notes
- Add unit + integration tests for critical rules
- Ensure error codes and correlationId conventions
Return: files to change + code outline + tests outline + PR checklist items.
```
