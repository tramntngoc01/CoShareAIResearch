
# Implementation Plan — AUTH: Authentication & Authorization

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead (AUTH)
- Reviewers: BA / QA / DevOps (if needed)
- Target sprint/release: MVP AUTH

---

## 1) Scope
### In scope
- AUTH-SCOPE-01: End User authentication flows per SRS-AUTH (FR-AUTH-001..004):
  - Registration via phone + OTP ZNS.
  - First-time login via OTP.
  - Auto-login via access/refresh tokens.
  - Logout + token revocation on current device/browser.
- AUTH-SCOPE-02: Admin Portal login (FR-AUTH-005) using username (email/phone) + password and roles from ADMIN.
- AUTH-SCOPE-03: AUTH DB schema for OTP requests, refresh tokens, and admin credentials (DB-AUTH).
- AUTH-SCOPE-04: OpenAPI contracts for AUTH endpoints, including error handling and rate-limit semantics.

### Out of scope
- AUTH-OOS-01: Detailed UI implementation (React) beyond contract and basic integration requirements.
- AUTH-OOS-02: Full device management UI (list/revoke sessions on all devices).
- AUTH-OOS-03: 2FA for Admin Portal (OTP, TOTP) — design placeholder only, no implementation in MVP.

### Success criteria
- SC1: All P0 AUTH stories (US-AUTH-001..005) pass defined UT/IT/E2E/SEC tests.
- SC2: No known critical OWASP-auth related issues (broken auth, token leakage, missing rate limits) at go-live.
- SC3: OpenAPI AUTH section is the single source of truth for all AUTH endpoints and is kept in sync with implementation.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-AUTH.md`
- Stories: `docs/02-Requirements/Stories/Stories-AUTH.md`
- OpenAPI paths (AUTH):
  - `POST /api/v1/auth/end-user/register/request-otp`
  - `POST /api/v1/auth/end-user/register/verify-otp`
  - `POST /api/v1/auth/end-user/login/request-otp`
  - `POST /api/v1/auth/end-user/login/verify-otp`
  - `POST /api/v1/auth/login` (Admin Portal)
  - `POST /api/v1/auth/refresh`
  - `POST /api/v1/auth/logout`
- DB doc: `docs/03-Design/DB/DB-AUTH.md`
- Business rules (central):
  - `docs/02-Requirements/Business-Rules.md` — BR-006..BR-008 (AUTH) plus global rules (BR-001..BR-005).

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - `AuthEndUserController` for end-user OTP/register/login/logout flows.
  - `AuthAdminController` (or shared `AuthController`) for Admin Portal login.
- Service layer:
  - `IAuthEndUserService` for FR-AUTH-001..004 (OTP orchestration, token issuance, logout).
  - `IAuthAdminService` for FR-AUTH-005 (admin credential validation, lockout, token issuance).
  - `ITokenService` for JWT generation/validation and refresh handling.
- Validation:
  - Implement fluent validators for all AUTH request DTOs (phone format, required fields, password non-empty).
  - Enforce phone pattern via centralized config (Open Question for exact pattern).
- Error codes:
  - Define AUTH error codes per Error-Conventions, e.g.:
    - `AUTH_INVALID_PHONE_FORMAT`, `AUTH_OTP_INVALID_OR_EXPIRED`, `AUTH_OTP_RATE_LIMITED`.
    - `AUTH_USER_NOT_FOUND`, `AUTH_USER_LOCKED`.
    - `AUTH_INVALID_CREDENTIALS`, `AUTH_ADMIN_LOCKED`.
    - `AUTH_TOKEN_INVALID`, `AUTH_TOKEN_EXPIRED`, `AUTH_REFRESH_TOKEN_REVOKED`.
  - Map to `400/401/403/409/429` consistently with API-Conventions.
- AuthZ policies:
  - Rely on existing `bearerAuth` scheme; ensure all non-AUTH module endpoints enforce role-based policies.
  - For `/api/v1/auth/logout`, require an authenticated principal and revoke only that principal's session.
- Logging/CorrelationId:
  - Ensure all AUTH endpoints accept/generate `X-Correlation-Id` and propagate to NOTIFICATIONS.
  - Implement structured logging that masks PII (phone/email) and never logs OTP/tokens.

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Create `auth_otp_request`, `auth_refresh_token`, `auth_admin_account` per DB-AUTH.
- Migration steps:
  - Add initial migration for AUTH module creating these tables and indexes.
  - Wire EF Core entities and DbContext configuration to match DB-AUTH.
- Rollback steps:
  - In lower envs: drop AUTH tables if feature is reverted.
  - In shared envs: disable endpoints and mark data obsolete; avoid destructive rollback unless explicitly approved.
- Data seeding (if any):
  - No sample data; only deterministic reference data if we later add auth-related enums.

### Frontend (React Web)
- Screens/components (AUTH-specific contracts):
  - End User: Registration, OTP verification, Login (OTP), basic "logged-in shell" triggers auto-login.
  - Admin: Login screen posting to `/api/v1/auth/login`.
- State management:
  - Store access/refresh tokens per Security-Baseline (prefer HttpOnly cookies where feasible).
  - Handle logout by clearing client storage and calling `/api/v1/auth/logout`.
- Form validation + error mapping:
  - Client-side validation mirrors server (phone format, required fields) but server remains source of truth.
- UI evidence expectations:
  - Screenshots or short videos for P0 stories attached to PRs.

### Integrations / Jobs (if any)
- External APIs:
  - Integrate with NOTIFICATIONS module for OTP ZNS via internal API gateway.
- Background jobs:
  - Periodic job to clean up expired OTPs and refresh tokens (no functional change, housekeeping only).

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - AUTH endpoints as listed in section 2 are already added to `openapi.yaml`.
  - Shared `LoginResponse` used for both end-user and admin logins; include `user`/`admin` profile and `roles`.
  - Schemas for OTP requests/verification and device info are defined and must be used by implementation.
- Backward compatibility considerations:
  - Current OpenAPI version is pre-MVP (`0.x`); no external consumers yet, but we will treat AUTH contracts as stable once merged.
- Idempotency requirements:
  - Use `Idempotency-Key` header on OTP and registration/login verification endpoints to avoid duplicate side-effects (duplicate ZNS sends or duplicate user creation).
  - For `/api/v1/auth/refresh` and `/api/v1/auth/logout`, operations are idempotent by design without explicit keys.
- Pagination/filtering rules:
  - Not applicable to AUTH endpoints (no list endpoints in MVP).

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - End Users (Tầng 1/2/3) — OTP and token-based access.
  - Admin roles from ADMIN (Super Admin, Ops, QC, Finance, Support).
- Permissions matrix (action -> role):
  - End User registration/login/logout: anonymous → authenticated End User.
  - Admin login: only principals with matching `admin_admin_user` + `auth_admin_account` rows.
  - Other modules consume `roles` claim to enforce RBAC in their own services.
- Sensitive operations requiring audit trail:
  - All login attempts (success/failure) for both End Users and Admins.
  - OTP sends and verifications.
  - Token refresh and logout events (including device context where available).

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-AUTH-001: Validate OTP TTL and resend/attempt limits (US-AUTH-001).
- UT-AUTH-002: Validate first-time login OTP checks and token issuance (US-AUTH-002).
- UT-AUTH-003: Validate refresh-token flow and auto-login logic (US-AUTH-003).
- UT-AUTH-004: Validate logout/token revocation behavior (US-AUTH-004).
- UT-AUTH-005: Validate admin credential checks and lockout thresholds (US-AUTH-005).

### Integration tests
- IT-AUTH-001: End User registration via AUTH + NOTIFICATIONS + USERS.
- IT-AUTH-002: First-time End User login via AUTH + NOTIFICATIONS + USERS.
- IT-AUTH-003: Token refresh with `auth_refresh_token` persistence.
- IT-AUTH-004: Logout flow invalidating subsequent calls to protected APIs.
- IT-AUTH-005: Admin login integrating AUTH with ADMIN roles.

### E2E / Smoke (staging)
- E2E-AUTH-001: New End User registers and logs in to Portal successfully.
- E2E-AUTH-002: Existing End User logs in again via auto-login (no OTP required while tokens valid).
- E2E-AUTH-003: End User logs out; further access requires fresh OTP.
- E2E-AUTH-004: Admin logs in to Admin Portal and sees correct dashboard.

### Security sanity
- SEC-AUTH-001: Negative OTP tests (wrong/exhausted OTP, expired OTP, rate-limit violations).
- SEC-AUTH-002: Input validation and injection probes on AUTH endpoints.
- SEC-AUTH-003: Token-theft simulations (replay, corrupted tokens) and ensure proper 401/403 behavior.
- SEC-AUTH-004: Brute-force attempts on admin login and verification of lockout.

---

## 7) Observability Plan
- Logs (no PII):
  - Log at INFO for successful login/logout/refresh events (with masked identifiers).
  - Log at WARN for repeated OTP failures and admin login lockouts.
- Metrics (key counters/timers):
  - Count of OTP requests and verification success/failure by purpose.
  - Latency for AUTH endpoints.
  - Count of admin login failures and lockouts.
- Alerts (if needed):
  - Alert on spikes in OTP failures or admin login failures over baseline.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Optional: gate End User OTP flows and Admin login separately if needed.
- Migration order:
  1. Deploy DB migrations for AUTH (`auth_*` tables).
  2. Deploy backend changes implementing AUTH endpoints.
  3. Deploy frontend changes (End User + Admin login flows).
- Staging verification steps:
  - Run all IT/E2E AUTH tests.
  - Manual smoke tests: register, login, auto-login, logout, admin login.
- Production checklist items:
  - Confirm NOTIFICATIONS/ZNS configuration for OTP templates.
  - Confirm Security-Baseline (TLS, rate-limits, logging) enabled for AUTH endpoints.
- Rollback procedure:
  - Disable AUTH endpoints at ingress/gateway.
  - Rollback backend deployment; retain DB schema unless a dedicated backward migration is approved.

---

## 9) Risks & Mitigations
- R1: Misconfigured OTP TTL or rate limits leading to poor UX or security gaps.
  - M1: Parameterize values via config, validate in staging with BA/Security.
- R2: Token storage on FE introducing XSS risk.
  - M2: Prefer HttpOnly cookies; conduct basic security review of FE storage mechanisms.
- R3: Admin lockout policy too strict/too lax.
  - M3: Start with conservative defaults and adjust based on real usage; ensure monitoring.

---

## 10) Estimates & Owners
| Work item | Owner | Estimate | Notes |
|----------|-------|----------|-------|
| BE       | TBD   | TBD      | AUTH controllers/services, token service, integrations |
| FE       | TBD   | TBD      | End User + Admin login flows, error handling UI        |
| DB       | TBD   | TBD      | Migrations for `auth_*` tables                         |
| QA       | TBD   | TBD      | Test design + automation for all AUTH test IDs         |
| DevOps   | TBD   | TBD      | Rate-limits, TLS, monitoring/alerts for AUTH           |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
