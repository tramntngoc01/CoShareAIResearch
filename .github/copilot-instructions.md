# Copilot Instructions for AI Agents

## Project Context
**Documentation-first .NET 9 + React + PostgreSQL** system. This is a comprehensive docs pack defining implementation patterns—source code follows these specs.

## Architecture at a Glance
- **8 modules**: AUTH, USERS, CATALOG, ORDERS, PAYMENTS, NOTIFICATIONS, ADMIN, REPORTING
- **Layering**: Controllers (thin, validation only) → Services (business logic + authz) → Repositories (data access)
- **Contract-first**: OpenAPI at `docs/03-Design/OpenAPI/openapi.yaml` is the single source of truth

## Before Writing Code (Mandatory)
1. **Check Implementation Plan is approved**: `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`
2. **Read requirements**: `docs/02-Requirements/SRS/SRS-<MODULE>.md` + `docs/02-Requirements/Stories/Stories-<MODULE>.md`
3. **Update OpenAPI FIRST** if changing any request/response contract
4. **Check business rules**: `docs/02-Requirements/Business-Rules.md` for cross-module constraints

## API Patterns (Enforce Strictly)
```
URL:      /api/v1/{module}/{resource}
Headers:  X-Correlation-Id (generate if missing), Idempotency-Key (payments/orders)
Pagination: ?page=1&pageSize=20 → { items, page, totalItems, totalPages }
Errors:   { "error": { "code": "MODULE_ERROR_CODE", "message": "...", "correlationId": "..." } }
```
- 400=validation, 401=unauthenticated, 403=unauthorized, 409=conflict/idempotency, 429=rate-limit

## Database Patterns
| Aspect | Convention |
|--------|------------|
| Naming | `snake_case` tables/columns |
| PK/FK | `id` (BIGSERIAL), `<table>_id` (BIGINT) |
| Audit | `is_deleted`, `created_at`, `created_by`, `updated_at`, `updated_by` |
| Money | `numeric(18,2)` |
| Time | `timestamptz` (UTC) |
| Indexes | `ix_<table>__<col>`, `ux_<table>__<col>` for unique |

All schema changes via EF Core migrations with rollback steps documented.

## Security Rules (Non-Negotiable)
- **Never log PII or secrets**; always include `correlationId`
- **Deny-by-default authz**; enforce policies in service layer, not controllers
- Validate all inputs server-side; parameterized queries only
- Hash passwords with BCrypt/Argon2; JWT access tokens short-lived

## Testing Requirements
- **Unit tests**: business rules, state transitions in `/src/api/tests/<Project>.UnitTests`
- **Integration tests**: API+DB behavior, authz negative cases
- **Map tests to stories** in `docs/02-Requirements/Traceability.md`

## Key Files Reference
| Purpose | Path |
|---------|------|
| All standards | `docs/00-Guardrails/Engineering-Standards-Index.md` |
| API rules | `docs/03-Design/API-Conventions.md` |
| Error codes | `docs/03-Design/Error-Conventions.md` |
| DB rules | `docs/03-Design/DB/DB-Design-Rules.md` |
| DoD | `docs/00-Guardrails/Definition-of-Done.md` |
| Module DB | `docs/03-Design/DB/DB-<MODULE>.md` |

## AI-Specific Constraints
- **RED (never use)**: Real PII, secrets, prod data, connection strings
- **GREEN (OK)**: Requirements, OpenAPI samples, fake test data, templates
- **Never guess field names**—always reference OpenAPI/DB schema
- AI outputs are drafts; human review + CI gates required before merge

## Workflow Checklist
When implementing a feature for module `<M>`:
1. ✅ `Implementation-Plan-<M>.md` is approved
2. ✅ OpenAPI endpoint exists (or add it first)
3. ✅ DB schema in `DB-<M>.md` matches implementation
4. ✅ Business rules from `Business-Rules.md` are enforced
5. ✅ Tests mapped in `Traceability.md`
6. ✅ No PII in logs, correlationId included
7. ✅ Migration + rollback documented
