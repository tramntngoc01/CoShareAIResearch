
# Implementation Plan — ADMIN: Admin Console

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead (ADMIN)
- Reviewers: BA / Tech Lead / QA / DevOps
- Target sprint/release: MVP ADMIN

---

## 1) Scope
### In scope
- ADMIN-SCOPE-01: P0 stories US-ADMIN-001 (manage Company & PickupPoint) and US-ADMIN-002 (manage Admin users & roles).
- ADMIN-SCOPE-02: Supporting system configuration and audit log read APIs required by FR-ADMIN-003 and FR-ADMIN-005.

### Out of scope
- ADMIN-OOS-01: Detailed business logic for reporting metrics and dashboard data (belongs to REPORTING, ORDERS, PAYMENTS, CATALOG).
- ADMIN-OOS-02: Low-level authentication flows (login, password reset) which are implemented in AUTH; ADMIN only manages roles and profile.

### Success criteria
- SC1: Admin Portal users can reliably manage companies, pickup points, and admin roles without breaking cross-module references or RBAC rules.
- SC2: Super Admin can configure system-level branding/legal info and search audit logs for sensitive actions without exposing PII or degrading performance.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-ADMIN.md`
- Stories: `docs/02-Requirements/Stories/Stories-ADMIN.md`
- OpenAPI paths:
  - GET /api/v1/admin/companies
  - POST /api/v1/admin/companies
  - GET /api/v1/admin/companies/{id}
  - PATCH /api/v1/admin/companies/{id}
  - GET /api/v1/admin/companies/{companyId}/pickup-points
  - POST /api/v1/admin/companies/{companyId}/pickup-points
  - GET /api/v1/admin/companies/{companyId}/pickup-points/{id}
  - PATCH /api/v1/admin/companies/{companyId}/pickup-points/{id}
  - GET /api/v1/admin/roles
  - POST /api/v1/admin/roles
  - GET /api/v1/admin/roles/{id}
  - PATCH /api/v1/admin/roles/{id}
  - GET /api/v1/admin/admin-users
  - POST /api/v1/admin/admin-users
  - GET /api/v1/admin/admin-users/{id}
  - PATCH /api/v1/admin/admin-users/{id}
  - GET /api/v1/admin/system-config
  - PUT /api/v1/admin/system-config
  - GET /api/v1/admin/audit-logs
- DB doc: `docs/03-Design/DB/DB-ADMIN.md`
- Business rules:
  - BR-ADMIN-001 — Phân quyền dạng deny-by-default.
  - BR-ADMIN-002 — Quản lý Công ty & Điểm nhận (no physical delete for referenced records).
  - BR-ADMIN-003 — Audit Log bắt buộc cho cấu hình và phân quyền.

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - Implement ADMIN API controllers for the paths listed above, following API-Conventions (paging, error envelope, correlationId).
- Service layer:
  - CompanyService for create/list/detail/update companies.
  - PickupPointService for managing pickup points and default flags per company.
  - AdminUserService for listing, creating, and updating admin users and assigned roles (integrating with AUTH for credentials).
  - AdminRoleService for managing role catalog (base roles + possible extensions).
  - SystemConfigService for reading/updating global configuration.
  - AuditLogQueryService for filtered search over `admin_audit_log`.
- Validation:
  - Enforce unique `company_code` for active companies and prevent deactivation when references exist.
  - Enforce single default pickup point per company and validate status transitions.
  - Validate that every admin user has at least one role and that role codes exist.
  - Validate system configuration fields (required vs optional, URL formats where applicable).
- Error codes:
  - Define ADMIN-specific codes, e.g. `ADMIN_COMPANY_CODE_DUPLICATE`, `ADMIN_COMPANY_IN_USE`, `ADMIN_PICKUPPOINT_IN_USE`, `ADMIN_ROLE_CODE_DUPLICATE`, `ADMIN_ADMINUSER_LOGIN_DUPLICATE`, `ADMIN_ADMINUSER_ROLE_EMPTY`, `ADMIN_AUDITLOG_RANGE_TOO_WIDE`.
  - Ensure they are documented in Error-Conventions using the shared `ErrorResponse` envelope.
- AuthZ policies:
  - Restrict company/pickup and config endpoints to appropriate roles (Super Admin/Ops for companies, Super Admin for system config).
  - Restrict admin-user and role management endpoints to Super Admin only.
  - Enforce deny-by-default semantics; all ADMIN controllers must explicitly declare required policies.
- Logging/CorrelationId:
  - Use centralized logging with correlationId; integrate with ADMIN audit logging for sensitive actions.

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Create `admin_company`, `admin_pickup_point`, `admin_admin_user`, `admin_role`, `admin_admin_user_role`, `admin_system_config`, and `admin_audit_log` as per DB-ADMIN.
  - Add indexes and unique constraints to enforce BRs and support typical queries.
- Migration steps:
  - Introduce ADMIN tables in a dedicated migration, then wire AUTH FKs (`auth_admin_account`, `auth_refresh_token.admin_user_id`) if not already present.
- Rollback steps:
  - In early environments, drop ADMIN tables in reverse dependency order if feature is rolled back.
  - In production, prefer feature toggles and additive migrations; do not drop `admin_audit_log`.
- Data seeding (if any):
  - Seed base roles (SUPER_ADMIN, OPS, QC, FINANCE, SUPPORT) into `admin_role`.
  - Optionally seed an initial `admin_system_config` row with default branding.

### Frontend (React Web) / Mobile (if any)
- Screens/components:
  - Admin Portal: Company list & detail with embedded pickup point management.
  - Admin Portal: Admin user list & detail with role assignment UI.
  - Admin Portal: System configuration screen (brand/legal/terms).
  - Admin Portal: Audit Log search screen.
- State management:
  - Keep paged lists for companies, pickup points, admin users, and audit logs with filters in URL state.
- Form validation + error mapping:
  - Map ADMIN error codes to user-friendly messages (e.g. duplicate company code, admin without roles, audit range too wide).
- UI evidence expectations:
  - Capture screenshots for core flows (create company, change default pickup point, create admin user, update roles, search audit logs) to attach to test cases.

### Integrations / Jobs (if any)
- External APIs:
  - None for core ADMIN; other modules consume ADMIN data (companies, pickup points, roles, audit log) via internal services.
- Webhooks:
  - N/A for MVP ADMIN.
- Background jobs (idempotency, retries):
  - Optional archival/rotation job for `admin_audit_log` depending on retention decisions.

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - ADMIN paths and schemas added to `openapi.yaml` for companies, pickup points, roles, admin users, system config, and audit logs.
  - ADMIN-specific error codes defined and aligned with Error-Conventions.
- Backward compatibility considerations:
  - ADMIN APIs are new for MVP; no external clients depend on older versions.
  - Future changes should be additive (new optional fields/filters) or versioned when breaking.
- Idempotency requirements:
  - ADMIN endpoints follow standard admin CRUD semantics; no idempotency header is required, but controllers must be safe against double-submits via unique constraints.
- Pagination/filtering rules:
  - Use `page`/`pageSize` for list endpoints (`/admin/companies`, `/admin/admin-users`, `/admin/audit-logs`).
  - Enforce global max `pageSize` and validate filter parameters.

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - SUPER_ADMIN, OPS, QC, FINANCE, SUPPORT (from `admin_role`).
- Permissions matrix (action -> role):
  - Manage companies & pickup points: SUPER_ADMIN, OPS (with possible company scoping).
  - Manage admin users & roles: SUPER_ADMIN only.
  - Read system configuration: all admin roles; update system configuration: SUPER_ADMIN only.
  - Search audit logs: SUPER_ADMIN; restricted views for SUPPORT and other roles per security policy.
- Sensitive operations requiring audit trail:
  - All changes to companies, pickup points, roles, admin users, and system configuration must write to `admin_audit_log`.
  - Cross-module sensitive actions (e.g., order cancel approval) must also log into `admin_audit_log` via their respective services.

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-ADMIN-001: Company and pickup point rules (unique company_code, single default per company, inactive/in-use constraints).
- UT-ADMIN-002: Admin user and role assignments (no empty role sets, valid role codes, status transitions).
- UT-ADMIN-003: System configuration validation and update behavior.

### Integration tests
- IT-ADMIN-001: End-to-end company + pickup point management, including effects on USERS/ORDERS (via shared company/pickup references).
- IT-ADMIN-002: Admin user creation and role assignment, verifying RBAC behavior on representative endpoints.
- IT-ADMIN-003: Audit log write + search across modules (e.g. ORDERS cancel approval writing to ADMIN audit log and visible via `/admin/audit-logs`).

### E2E / Smoke (staging)
- E2E-ADMIN-001: Super Admin sets up a new company with pickup points and sees them available in USERS/ORDERS flows.
- E2E-ADMIN-002: Super Admin creates an admin user with specific roles, verifies access to appropriate menus and denies others.

### Security sanity
- SEC-ADMIN-001 (authz negative): Verify that unauthorized roles cannot access ADMIN configuration endpoints.
- SEC-ADMIN-002 (input validation/injection basic): Validate inputs for companies, pickup points, admin users, and audit-log queries.

---

## 7) Observability Plan
- Logs (no PII):
  - Centralize logging for all ADMIN endpoints with correlationId.
  - Ensure audit log metadata is redacted and does not store PII/OTP/CCCD.
- Metrics (key counters/timers):
  - Count of companies, pickup points, and admin users created/updated.
  - Volume and latency of `/admin/audit-logs` queries.
- Alerts (if needed):
  - Alert on spikes in audit-log query errors or unusually large ranges requested.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Optional feature flag for ADMIN advanced features (audit log search, system config) if rollout needs staging.
- Migration order:
  - Deploy DB migration for ADMIN tables.
  - Deploy backend with ADMIN APIs.
  - Enable ADMIN features in Admin Portal.
- Staging verification steps:
  - Run integration and E2E tests for ADMIN flows.
  - Manually verify company/pickup and admin-role flows, plus audit-log search.
- Production checklist items:
  - Confirm base roles and at least one Super Admin account configured.
  - Confirm retention and backup strategy for `admin_audit_log`.
- Rollback procedure:
  - Disable ADMIN features in Admin Portal and API routing if necessary.
  - Leave DB schema intact; apply forward-only fixes for any issues.

---

## 9) Risks & Mitigations
- R1: Misconfigured RBAC allowing over-privileged access to ADMIN functions.
  - Mitigation: Thorough authz negative testing (SEC-ADMIN-001) and review of policy mappings.
- R2: Audit log growth impacting query performance.
  - Mitigation: Apply indexes, enforce maximum query ranges, and design archival strategy.
- R3: Company/pickup changes breaking downstream modules.
  - Mitigation: Validate references before deactivation and include ADMIN -> USERS/ORDERS integration tests.

---

## 10) Estimates & Owners
| Work item | Owner | Estimate | Notes |
|----------|-------|----------|------|
| BE | TBD | TBD | ADMIN APIs and services |
| FE | TBD | TBD | Admin Portal screens for companies, admin users, config, audit logs |
| DB | TBD | TBD | ADMIN tables, indexes, migrations |
| QA | TBD | TBD | Test design and automation for ADMIN |
| DevOps | TBD | TBD | Monitoring, alerts, deployment pipeline updates |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
