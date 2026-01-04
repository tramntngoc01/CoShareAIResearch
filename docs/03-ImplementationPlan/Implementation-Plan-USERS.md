
# Implementation Plan — USERS: User & Profile Management

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead — USERS
- Reviewers: BA / Tech Lead / QA / DevOps (if needed)
- Target sprint/release: MVP P0 — USERS

---

## 1) Scope
### In scope
- USERS-SCOPE-01: Implement USERS backend APIs for HR import, ref tier management, KYC/admin profile editing, user status changes, and search/list users as per SRS-USERS P0 stories (US-USERS-001,002,003,005,006).
- USERS-SCOPE-02: Implement PostgreSQL schema for USERS (`users_user`, `users_ref_tier_history`, `users_status_history`, `users_import_job`, `users_import_row`) and integrate with ADMIN-owned `admin_company`, `admin_pickup_point`, `admin_admin_user`.

### Out of scope
- USERS-OOS-01: End User self-service profile editing (US-USERS-004) beyond what is already defined for future phases.
- USERS-OOS-02: Downstream reporting/commission calculations (REPORTING, PAYMENTS); they will consume USERS data but are implemented in their own modules.

### Success criteria
- SC1: All P0 USERS APIs are implemented according to OpenAPI, pass integration tests against a real database, and are documented for frontend consumption.
- SC2: AUTH/ORDERS/PAYMENTS can reliably query `users_user` for status/tier/company and enforce business rules without direct HR file parsing.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-USERS.md`
- Stories: `docs/02-Requirements/Stories/Stories-USERS.md`
- OpenAPI paths (USERS):
  - GET /api/v1/users
  - GET /api/v1/users/{id}
  - PATCH /api/v1/users/{id}/kyc
  - PATCH /api/v1/users/{id}/status
  - GET /api/v1/users/{id}/ref-tier
  - PUT /api/v1/users/{id}/ref-tier
  - POST /api/v1/users/import
  - GET /api/v1/users/import/{importId}
- DB doc: `docs/03-Design/DB/DB-USERS.md`
- Business rules (from Business-Rules.md & SRS-USERS):
  - BR-USERS-001: HR is the source of truth for company-owned attributes.
  - BR-USERS-002: Single-parent ref tier hierarchy (T1/T2/T3) per company.
  - BR-USERS-003: KYC edit permissions and masking rules.
  - BR-USERS-004: User status lifecycle and cross-module impact.

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - `UsersController` for admin/ops operations: search, detail, KYC update, status change, ref tier operations.
  - `UserImportsController` (or sub-controller) for HR import upload and status retrieval.
- Service layer:
  - `IUserService` / `UserService` implementing search, load, updateKyc, changeStatus, getRefTier, changeRefTier with full BR enforcement.
  - `IUserImportService` / `UserImportService` to validate file structure, enqueue jobs, and process rows in background.
- Validation:
  - FluentValidation (or equivalent) for request DTOs (`UserStatusChangeRequest`, `UserKycUpdateRequest`, ref tier change, search filters).
  - Server-side validation of allowed transitions and tier rules; map validation failures to standardized error codes (e.g., `USERS_STATUS_INVALID_TRANSITION`).
- Error codes:
  - Define USERS_* codes in Error-Conventions (e.g., `USERS_IMPORT_STRUCTURE_INVALID`, `USERS_IMPORT_TOO_LARGE`, `USERS_USER_NOT_FOUND`, `USERS_REF_TIER_INVALID_PARENT`, `USERS_STATUS_INVALID_TRANSITION`).
- AuthZ policies:
  - Policy for Admin/Ops to access `/api/v1/users` and detail.
  - Policy for Super Admin to change ref tiers and perform sensitive status changes.
  - Deny-by-default; all public endpoints require explicit role checks in the service layer.
- Logging/CorrelationId:
  - Ensure all endpoints include and propagate `X-Correlation-Id`.
  - Do not log PII or raw HR row payload; use IDs and masked values.

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Create `users_user`, `users_ref_tier_history`, `users_status_history`, `users_import_job`, `users_import_row` as per DB-USERS.
  - Add necessary FKs to `admin_company`, `admin_pickup_point`, `admin_admin_user`.
- Migration steps:
  - Add EF Core migration for USERS schema; ensure default values for status/tier and audit columns.
  - Backfill any reference data needed for tiers (e.g., enum mapping) if implemented via lookup tables.
- Rollback steps:
  - Drop USERS tables in reverse dependency order (rows → jobs/history → user master) if migration must be reverted before go-live.
- Data seeding (if any):
  - Seed minimal reference data for tiers if represented as lookup (T1/T2/T3/SHIPPER) or align with enum-only approach.

### Frontend (React Web) / Mobile (if any)
- Screens/components:
  - Admin Portal: HR import upload + result view; User list with filters; User detail; Ref tier edit dialog; Status change dialog.
- State management:
  - Use shared API client for USERS endpoints, reusing pagination models from other modules.
- Form validation + error mapping:
  - Map USERS_* error codes to user-friendly messages (e.g., invalid file, invalid ref tier, invalid status transition).
- UI evidence expectations:
  - Screenshots or Loom recordings of critical flows for UAT sign-off.

### Integrations / Jobs (if any)
- External APIs:
  - None for USERS in MVP; HR integration is via file upload only.
- Webhooks:
  - None in MVP.
- Background jobs (idempotency, retries):
  - Background worker to process `users_import_job` records; idempotent per `import_uuid`.
  - Job writes aggregated stats and row-level results; safe to retry on failure.

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - USERS paths and schemas already added in `openapi.yaml` for search, detail, KYC update, status change, ref tier operations, and import.
  - Define USERS-specific error codes in Error-Conventions, reusing global error envelope.
- Backward compatibility considerations:
  - USERS is new for MVP; no legacy APIs to support. Future changes must be additive (new fields, new endpoints) to avoid breaking consumers.
- Idempotency requirements:
  - HR import: rely on `import_uuid` generated in backend; repeated uploads of the same file are treated as separate jobs unless business rules specify otherwise (Open Question in SRS).
  - Status/ref tier changes: treat as idempotent per current state (no-op if requested state/parent already applied).
- Pagination/filtering rules:
  - `/api/v1/users` follows global `page`/`pageSize` conventions and returns a `PagedResult` with `items` = `UserSummary[]`.
  - Enforce maximum `pageSize` as per API-Conventions.

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - Admin/Ops, Super Admin, (future) Company-level admin if introduced.
- Permissions matrix (action -> role):
  - Search/list users, view detail: Admin/Ops, Super Admin (scoped by company where applicable).
  - Update KYC/profile (admin endpoint): Admin (within allowed fields), Super Admin.
  - Change status: Admin (for standard transitions), Super Admin (for more sensitive transitions, e.g., Locked).
  - Change ref tier: Super Admin only in MVP.
- Sensitive operations requiring audit trail:
  - Status changes (write to `users_status_history` and ADMIN audit log).
  - Ref tier changes (write to `users_ref_tier_history` and ADMIN audit log).
  - KYC updates for high-sensitivity fields (logged via ADMIN audit with masked values).

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-USERS-001: User status state machine (valid/invalid transitions, Locked behavior).
- UT-USERS-002: Ref tier change rules (cycle prevention, tier compatibility, company constraints).
- UT-USERS-003: KYC update field-ownership rules (admin vs HR vs end user).

### Integration tests
- IT-USERS-001: `/api/v1/users` search with filters and pagination against seeded data.
- IT-USERS-002: `/api/v1/users/{id}/status` and cross-module enforcement stub (e.g., AUTH/ORDERS honoring status from DB).
- IT-USERS-003: `/api/v1/users/import` + `/api/v1/users/import/{importId}` full flow with sample HR file and row-level results.

### E2E / Smoke (staging)
- E2E-USERS-001: Admin performs HR import, then sees users in list and details.
- E2E-USERS-002: Admin locks a user, and that user is prevented from creating new orders (via ORDERS flow in staging).

### Security sanity
- SEC-USERS-001 (authz negative): Verify unauthorized roles cannot access `/api/v1/users` or mutate KYC/status/ref-tier.
- SEC-USERS-002 (input validation/injection basic): Validate that search filters and import processing are safe from injection and robust to malformed input.

---

## 7) Observability Plan
- Logs (no PII):
  - Log import job lifecycle (submitted/started/completed/failed) with `import_uuid`, not file contents.
  - Log status and ref tier changes with `user_id`, actor, and masked identifiers only.
- Metrics (key counters/timers):
  - Number of import jobs by status; rows processed per job; error rate.
  - Latency percentiles for `/api/v1/users` search and key mutations.
- Alerts (if needed):
  - High failure rate on HR imports.
  - Unusually high rate of status or ref tier changes.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Optional flag to hide USERS admin UI until backend is validated in staging.
- Migration order:
  - Deploy DB migrations for USERS and ADMIN (if needed) first.
  - Deploy backend with new USERS services and endpoints.
  - Deploy frontend changes to Admin Portal consuming the USERS APIs.
- Staging verification steps:
  - Run IT/E2E test suite for USERS.
  - Simulate at least one real-like HR import and validate downstream module behavior.
- Production checklist items:
  - Confirm role mappings for Admin/Ops/Super Admin.
  - Confirm monitoring and alerts are enabled for USERS metrics.
- Rollback procedure:
  - If critical issues arise, disable USERS admin features via feature flag and roll back backend deployment; consider leaving DB schema in place if already used by other modules.

---

## 9) Risks & Mitigations
- R1: HR file format or business rules change late.
  - M1: Keep import parser isolated and configurable; document assumptions and open questions in SRS/Threat-Model.
- R2: Performance issues on large user lists or imports.
  - M2: Use appropriate indexing, background jobs, and pagination; load test critical flows.

---

## 10) Estimates & Owners
| Work item | Owner         | Estimate | Notes |
|----------|--------------|----------|------|
| BE       | USERS BE dev | TBD      | Controllers, services, tests |
| FE       | USERS FE dev | TBD      | Admin Portal screens & flows |
| DB       | DB engineer  | TBD      | Migrations, performance tuning |
| QA       | QA engineer  | TBD      | Test design & execution |
| DevOps   | DevOps       | TBD      | CI/CD, monitoring, alerts |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
