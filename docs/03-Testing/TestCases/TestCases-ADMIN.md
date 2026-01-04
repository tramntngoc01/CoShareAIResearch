# Test Cases — ADMIN: Admin Console

## Section 1: Scope & assumptions
- Scope: Entire ADMIN module across OpenAPI tag `ADMIN`, Stories US-ADMIN-001 → 005, SRS-ADMIN, and related business rules. No ScreenSpecs `SC-ADMIN-*` exist; UI checks rely on Admin Portal behavior inferred from SRS/Stories and API responses.
- Sources of truth: SRS-ADMIN.md, Stories-ADMIN.md, OpenAPI `/api/v1/admin/*`, Error-Conventions.md, Testing-Strategy.md, Test-Plan.md. No StoryPacks for ADMIN.
- Assumptions / constraints:
  - AuthN/AuthZ enforced by AUTH; ADMIN tests use valid JWT for Super Admin unless stated otherwise (BR-ADMIN-001 deny-by-default).
  - Company/PickupPoint field set follows SRS samples; full field list TBD (Q-ADMIN-001) → validations beyond provided samples are out of scope and marked blocked where relevant.
  - Dashboard widgets and exact metrics come from REPORTING/ORDERS/PAYMENTS; widget list still pending (US-ADMIN-004 DoR) → tests marked blocked until confirmed.
  - Audit Log retention limits/time-range cap TBD (US-ADMIN-005 DoR); tests note expected `ADMIN_AUDITLOG_RANGE_TOO_WIDE` per OpenAPI.
  - No PII used; all data below is fake.

## Section 2: Test data set (fake)
- Admin identities:
  - `super_admin_01` (JWT with role SUPER_ADMIN, correlationId header generated if missing).
  - `ops_admin_01` (role OPS), `support_admin_01` (role SUPPORT) for authz negatives.
- Company & pickup:
  - Company A: `{companyCode: "CTY_MAY_A", companyName: "Công ty May KCN A", zone: "KCN A"}`
  - Company B (existing for duplicate test): `{companyCode: "CTY_EXIST", companyName: "Công ty Thép", zone: "KCN B"}`
  - Pickup points: `pp_gate_main` (default=true), `pp_gate_alt` (default=false).
- Admin users:
  - New admin to create: `{loginId: "ops_02@example.com", roles: ["OPS"]}`
  - Duplicate login check: `"ops_02@example.com"` reused.
- System config (before/after):
  - Before: `{systemName: "CoShare", logoUrl: "https://cdn.example.com/logo-old.png", termsUrl: "https://coshare.example.com/terms"}`
  - After: `{systemName: "CoShare KCN", logoUrl: "https://cdn.example.com/logo-new.png", termsUrl: "https://coshare.example.com/terms-v2"}`
- Audit Log filter: `{from: "2026-01-01T00:00:00Z", to: "2026-01-04T23:59:59Z", module: "ORDERS", actionType: "CANCEL_ORDER_APPROVED", correlationId: "corr-order-0001"}`

## Section 3: E2E testcases (P0 then P1)

### E2E-ADMIN-001 — Manage company & default pickup (happy path)
- Priority: P0
- Story IDs: US-ADMIN-001
- Screen IDs: N/A (no SC-ADMIN provided; Admin Portal company management)
- Preconditions: `super_admin_01` authenticated.
- Test data (fake): Company A; pickup points `pp_gate_main`, `pp_gate_alt`.
- Steps:
  1. Call `POST /api/v1/admin/companies` with Company A payload.
  2. Call `POST /api/v1/admin/companies/{companyId}/pickup-points` twice to create main and alt pickup points; set `isDefault=true` on first.
  3. Call `PATCH /api/v1/admin/companies/{companyId}/pickup-points/{id}` to set alt as default.
  4. Call `GET /api/v1/admin/companies/{companyId}/pickup-points` to verify state.
- Expected results:
  - Step 1 returns 201 with company detail; correlationId present.
  - Step 2 creates pickup points; first marked default.
  - Step 3 succeeds with 200; after change exactly one pickup point has `isDefault=true`.
  - Step 4 list shows both pickup points, only alt marked default; statuses remain ACTIVE.
- Evidence to capture: API responses (create/update/list), correlationIds.
- Notes: Validates AC1/AC2 of US-ADMIN-001 and BR-ADMIN-002 (single default, no delete).

### E2E-ADMIN-002 — Create admin user and assign roles
- Priority: P0
- Story IDs: US-ADMIN-002
- Screen IDs: N/A (Admin Portal: Admin & role management)
- Preconditions: `super_admin_01` authenticated; roles exist (SUPER_ADMIN, OPS, QC, FINANCE, SUPPORT).
- Test data (fake): New admin `{loginId: "ops_02@example.com", roles: ["OPS"]}`.
- Steps:
  1. Call `GET /api/v1/admin/roles` to confirm base roles are available.
  2. Call `POST /api/v1/admin/admin-users` with new admin payload.
  3. Call `PATCH /api/v1/admin/admin-users/{id}` to add `SUPPORT` role.
  4. Call `GET /api/v1/admin/admin-users/{id}` to verify roles persisted.
- Expected results:
  - Step 1 returns 200 with role list including OPS/SUPPORT.
  - Step 2 returns 201 with admin detail; roles include OPS; correlationId present.
  - Step 3 returns 200; roles now include OPS and SUPPORT.
  - Step 4 shows updated roles; status ACTIVE.
- Evidence to capture: Role list, create/update/detail responses, correlationIds.
- Notes: Covers AC1/AC2/AC3 of US-ADMIN-002; assumes AUTH picks up new roles on next login (out of scope to validate cache refresh).

### E2E-ADMIN-003 — Update system configuration and verify propagation
- Priority: P1
- Story IDs: US-ADMIN-003
- Screen IDs: N/A (Admin Portal: System config)
- Preconditions: `super_admin_01` authenticated; existing config baseline available.
- Test data (fake): After-config values in Section 2.
- Steps:
  1. Call `GET /api/v1/admin/system-config` to snapshot current values.
  2. Call `PUT /api/v1/admin/system-config` with updated logo/systemName/termsUrl.
  3. Call `GET /api/v1/admin/system-config` again to verify persisted values.
- Expected results:
  - Step 1 returns 200 with current config.
  - Step 2 returns 200 with updated config; correlationId present.
  - Step 3 returns updated values matching payload; no redeploy required (functional expectation).
- Evidence to capture: Before/after config responses, correlationIds.
- Notes: AC1–AC3 of US-ADMIN-003; PII not present.

### E2E-ADMIN-004 — Dashboard widgets load with fallback
- Priority: P1
- Story IDs: US-ADMIN-004
- Screen IDs: N/A (Admin Portal: Dashboard)
- Preconditions: Authenticated Admin with role OPS or FINANCE; downstream REPORTING services reachable or stubbed.
- Test data (fake): Query params `companyId=100` (optional).
- Steps:
  1. Call dashboard widget APIs (`GET /api/v1/reporting/dashboard/orders-today`, `/cod-open`, `/debt-overdue`) sequentially.
  2. Simulate one widget failure (e.g., mock 500 from `/cod-open`) and load Dashboard page/API aggregator.
- Expected results:
  - Normal calls return 200 with KPI schemas per OpenAPI; correlationId present.
  - On failure of one widget, Dashboard still renders remaining widgets; failing widget shows friendly error state (per AC2) and does not break page load.
- Evidence to capture: Widget API responses, UI/API aggregator response showing partial load with error state, correlationIds.
- Notes: Blocked by requirement until widget list/layout finalized (DoR for US-ADMIN-004). Mark as "Blocked by requirement".

### E2E-ADMIN-005 — Search Audit Log by correlationId
- Priority: P1
- Story IDs: US-ADMIN-005
- Screen IDs: N/A (Admin Portal: Audit Log)
- Preconditions: `super_admin_01` authenticated; at least one audit log entry exists with `correlationId="corr-order-0001"`.
- Test data (fake): Filter payload from Section 2.
- Steps:
  1. Call `GET /api/v1/admin/audit-logs` with filters including correlationId.
  2. Repeat without correlationId but with module/actionType to verify pagination.
- Expected results:
  - Step 1 returns 200 paged result containing matching entry with fields timestamp, actorType, actorId, actionType, module, correlationId, result; no PII.
  - Step 2 returns paged list; when no records, message "Không có dữ liệu" or empty list (per AC3); correlationId header present.
- Evidence to capture: Filtered responses, pagination metadata, correlationIds.
- Notes: Time-range limit still TBD; see IT/SEC cases for range too wide handling.

## Section 4: Integration/API testcases

### IT-ADMIN-001 — Reject duplicate company code
- Priority: P0
- Story IDs: US-ADMIN-001
- Screen IDs: N/A
- Preconditions: Company B with `companyCode="CTY_EXIST"` already exists.
- Test data (fake): Create request with same companyCode.
- Steps:
  1. Call `POST /api/v1/admin/companies` with duplicate code.
- Expected results:
  - 409 with `error.code=ADMIN_COMPANY_CODE_DUPLICATE`; correlationId present.
  - No new company created.
- Evidence to capture: Error response, correlationId.
- Notes: Data validation for unique companyCode (AC1).

### IT-ADMIN-002 — Enforce single default pickup per company
- Priority: P0
- Story IDs: US-ADMIN-001
- Screen IDs: N/A
- Preconditions: Company A exists with two pickup points; first is default.
- Test data (fake): `PATCH` payload `{isDefault: true}` on second pickup point.
- Steps:
-  1. Call `PATCH /api/v1/admin/companies/{companyId}/pickup-points/{id}` to set second pickup point default.
-  2. Call list endpoint to verify flags.
- Expected results:
  - Patch returns 200.
  - List shows exactly one `isDefault=true` (second), first set to false automatically.
- Evidence to capture: Patch response, list response, correlationId.
- Notes: Covers AC2 and BR-ADMIN-002.

### IT-ADMIN-003 — Prevent deactivating pickup point in use
- Priority: P0
- Story IDs: US-ADMIN-001
- Screen IDs: N/A
- Preconditions: Pickup point linked to active users/orders (mock or fixture).
- Test data (fake): `PATCH` payload `{status: "INACTIVE"}`.
- Steps:
  1. Call `PATCH /api/v1/admin/companies/{companyId}/pickup-points/{id}` to deactivate.
- Expected results:
  - 400 with `error.code=ADMIN_PICKUPPOINT_IN_USE`; pickup point remains active.
- Evidence to capture: Error response, follow-up list/detail, correlationId.
- Notes: Blocked by requirement until reference-check rule is finalized across modules (Q-ADMIN-001/Q-ADMIN-002).

### IT-ADMIN-004 — Reject admin user without roles
- Priority: P0
- Story IDs: US-ADMIN-002
- Screen IDs: N/A
- Preconditions: `super_admin_01` authenticated.
- Test data (fake): `POST /api/v1/admin/admin-users` with `roles=[]`.
- Steps:
  1. Submit create request with empty roles.
- Expected results:
  - 400 with `error.code=ADMIN_ADMINUSER_ROLE_EMPTY`; no admin user created.
- Evidence to capture: Error response, correlationId.
- Notes: AC2 of US-ADMIN-002.

### IT-ADMIN-005 — Non-Super Admin cannot assign roles
- Priority: P0
- Story IDs: US-ADMIN-002
- Screen IDs: N/A
- Preconditions: `support_admin_01` authenticated (no SUPER_ADMIN role); target admin exists.
- Test data (fake): `PATCH /api/v1/admin/admin-users/{id}` to add FINANCE role.
- Steps:
  1. Attempt patch with insufficient privileges.
- Expected results:
  - 403 (or standard error response) denying operation; roles unchanged.
- Evidence to capture: Error response, before/after role list, correlationId.
- Notes: Authz negative per BR-ADMIN-001 (deny-by-default).

### IT-ADMIN-006 — Validate system config required fields
- Priority: P1
- Story IDs: US-ADMIN-003
- Screen IDs: N/A
- Preconditions: `super_admin_01` authenticated.
- Test data (fake): `PUT /api/v1/admin/system-config` missing `systemName` or with empty `logoUrl`.
- Steps:
  1. Submit invalid config payload.
- Expected results:
  - 400 with validation error in standard error shape; no config change persisted.
- Evidence to capture: Error response, follow-up GET result, correlationId.
- Notes: Basic validation; specific required fields depend on finalized key list (DoR item).

### IT-ADMIN-007 — Reject audit log query with excessive date range
- Priority: P1
- Story IDs: US-ADMIN-005
- Screen IDs: N/A
- Preconditions: `super_admin_01` authenticated.
- Test data (fake): Query `from=2024-01-01T00:00:00Z`, `to=2026-12-31T23:59:59Z`.
- Steps:
  1. Call `GET /api/v1/admin/audit-logs` with wide range and default page/pageSize.
- Expected results:
  - 400 with `error.code=ADMIN_AUDITLOG_RANGE_TOO_WIDE`; no data returned.
- Evidence to capture: Error response, correlationId.
- Notes: Validates rate-limit/protection described in OpenAPI.

## Section 5: Security sanity testcases

### SEC-ADMIN-001 — Authz bypass attempt on role assignment
- Priority: P0
- Story IDs: US-ADMIN-002
- Screen IDs: N/A
- Preconditions: `support_admin_01` authenticated; target admin exists.
- Test data (fake): Role update payload `{roles:["OPS"]}`.
- Steps:
  1. Call `PATCH /api/v1/admin/admin-users/{id}` as Support role.
- Expected results:
  - 403 with standard error shape; no role changes applied.
  - CorrelationId present; no sensitive info in message.
- Evidence to capture: Error response, before/after role list.
- Notes: Authz negative (ADMINz bypass) per requirement #1.

### SEC-ADMIN-002 — Injection attempt in company search
- Priority: P1
- Story IDs: US-ADMIN-001
- Screen IDs: N/A
- Preconditions: Authenticated admin.
- Test data (fake): `GET /api/v1/admin/companies?searchText=%27%3Bdrop%20table%20admin_company--`
- Steps:
  1. Execute search with injection-like input.
- Expected results:
  - 200 with empty/filtered result or 400 validation; no server error/stack trace; correlationId present.
- Evidence to capture: Response body, logs (sanitized), correlationId.
- Notes: Ensures input sanitization and no SQL/JSON injection.

### SEC-ADMIN-003 — Audit Log range abuse protection (DoS guard)
- Priority: P1
- Story IDs: US-ADMIN-005
- Screen IDs: N/A
- Preconditions: `super_admin_01` authenticated.
- Test data (fake): Same as IT-ADMIN-007 (very wide range).
- Steps:
  1. Call `GET /api/v1/admin/audit-logs` with extreme date range repeatedly (e.g., 3 attempts).
- Expected results:
  - Each call returns 400 `ADMIN_AUDITLOG_RANGE_TOO_WIDE`; system remains responsive; correlationIds unique.
  - No degradation in subsequent valid queries.
- Evidence to capture: Error responses, subsequent successful query response times.
- Notes: Covers brute-force/rate-limit behavior for heavy queries.

### SEC-ADMIN-004 — Expired/invalid token on protected endpoint
- Priority: P1
- Story IDs: US-ADMIN-001, US-ADMIN-002
- Screen IDs: N/A
- Preconditions: Expired or tampered JWT.
- Test data (fake): `GET /api/v1/admin/companies` with expired token.
- Steps:
  1. Send request with invalid token.
- Expected results:
  - 401/403 per AUTH policy; no company data returned; correlationId present or generated.
- Evidence to capture: Error response headers/body.
- Notes: Session/token handling sanity.

### SEC-ADMIN-005 — Duplicate admin loginId does not leak PII
- Priority: P1
- Story IDs: US-ADMIN-002
- Screen IDs: N/A
- Preconditions: Admin user with `loginId="ops_02@example.com"` already exists.
- Test data (fake): Repeat `POST /api/v1/admin/admin-users` with same loginId.
- Steps:
  1. Submit duplicate create request.
- Expected results:
  - 409 `ADMIN_ADMINUSER_LOGIN_DUPLICATE`; message generic without echoing email; correlationId present.
- Evidence to capture: Error response, correlationId, logs showing no PII.
- Notes: Ensures sensitive data not exposed in errors/logs.

### SEC-ADMIN-006 — Unauthorized config change attempt
- Priority: P1
- Story IDs: US-ADMIN-003
- Screen IDs: N/A
- Preconditions: `ops_admin_01` authenticated (no SUPER_ADMIN role).
- Test data (fake): `PUT /api/v1/admin/system-config` with any payload.
- Steps:
  1. Submit config update as non-Super Admin.
- Expected results:
  - 403 with standard error; no config changed.
- Evidence to capture: Error response, before/after config snapshot.
- Notes: RBAC enforcement on high-sensitivity settings.

## Section 6: Regression Suite
- REG-ADMIN-001 → E2E-ADMIN-001 (company & pickup default happy path)
- REG-ADMIN-002 → E2E-ADMIN-002 (admin user create & role update)
- REG-ADMIN-003 → E2E-ADMIN-005 (audit log search by correlationId)
- REG-ADMIN-004 → IT-ADMIN-002 (single default pickup enforcement)
- REG-ADMIN-005 → IT-ADMIN-004 (reject admin user without roles)
- REG-ADMIN-006 → SEC-ADMIN-001 (authz bypass on role assignment)

## Section 7: Open Questions / blockers
- Q-ADMIN-001 (field list for Company/PickupPoint) and Q-ADMIN-002 (approval workflow for sensitive changes) remain open → IT-ADMIN-003 blocked until reference-handling rule is finalized.
- US-ADMIN-004 DoR: dashboard widget list/layout and role-based visibility not finalized → E2E-ADMIN-004 marked "Blocked by requirement".
- Audit Log retention/time-range cap not finalized → behavior for borderline ranges (near limit) may change; retest IT-ADMIN-007/SEC-ADMIN-003 once decided.
- No ScreenSpecs `SC-ADMIN-*`; UI validation limited to API responses and generic Admin Portal expectations. Provide screen IDs once defined to tighten coverage.
