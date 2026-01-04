
# Threat Model (baseline)

## Assets
- User accounts
- Orders/payments
- Admin operations
- PII

## Entry points
- Web/Mobile clients
- Public APIs
- Admin APIs
- Webhooks (payments)

## Common risks (OWASP-focused)
- Broken access control (IDOR)
- Injection (SQL/NoSQL)
- Auth/session weaknesses
- Sensitive data exposure
- SSRF (if any outbound fetch)
- Supply chain risks (deps)

## Controls (minimum)
- RBAC/ABAC checks in service layer
- Input validation + output encoding
- Rate limiting for auth endpoints
- Secrets management + rotation
- No PII in logs
- Dependency scanning

## Security test ideas
- AuthZ negative tests
- Injection probes
- Session/cookie settings

---

## AUTH module — specific threats & controls

### A. End User OTP flows (registration & first-time login)

**Threats**
- T-AUTH-001: OTP brute force (online guessing on `/auth/end-user/*/verify-otp`).
- T-AUTH-002: OTP abuse / spam (attackers trigger大量 OTP sends to a victim's phone).
- T-AUTH-003: OTP leakage through logs or client storage.
- T-AUTH-004: SIM-swap / stolen phone leading to unauthorized account creation.

**Controls**
- C-AUTH-001: Enforce server-side limits per phone + purpose:
	- Max N OTP sends per window; respond with 429 and error code `AUTH_OTP_RATE_LIMITED`.
	- Max M failed verification attempts per OTP; then expire OTP early.
- C-AUTH-002: Store only `otp_code_hash` in `auth_otp_request` (no raw OTP).
- C-AUTH-003: Never log full OTP or unmasked phone numbers; only masked values and correlationId.
- C-AUTH-004: Use short OTP TTL (e.g., 2–5 minutes — **Open Question**) and clear expired OTPs via background job.
- C-AUTH-005: Optionally add business-side monitoring for unusual OTP activity per company/phone.

**Open Questions (AUTH–OTP)**
- Q-TM-AUTH-OTP-01: Exact OTP TTL and resend/attempt limits.
- Q-TM-AUTH-OTP-02: Whether to introduce additional KYC/friction when a phone number is used to register multiple times or across companies.

---

### B. Token handling & auto-login

**Threats**
- T-AUTH-005: Theft of access/refresh tokens (XSS, device compromise, leaked storage).
- T-AUTH-006: Token replay from untrusted origins.
- T-AUTH-007: Lack of proper revocation (logout not actually terminating sessions).

**Controls**
- C-AUTH-006: Use short-lived access tokens (minutes) and longer-lived refresh tokens stored server-side as hashes in `auth_refresh_token`.
- C-AUTH-007: Bind tokens to device context where feasible (`device_id`, `user_agent`, `platform`) and validate on refresh.
- C-AUTH-008: Implement `/api/v1/auth/logout` to revoke current refresh token and reject further use.
- C-AUTH-009: Consider a global logout endpoint (all sessions) for security incidents — **Open Question**.
- C-AUTH-010: Use HttpOnly + Secure cookies when tokens are stored in cookies; avoid exposing tokens to JS unless unavoidable.

**Open Questions (AUTH–tokens)**
- Q-TM-AUTH-TOKEN-01: Exact TTLs for access vs refresh tokens per persona (Admin vs End User).
- Q-TM-AUTH-TOKEN-02: Whether to support "logout all devices" in MVP or later.

---

### C. Admin login (credentials & brute force)

**Threats**
- T-AUTH-008: Credential stuffing / brute-force attacks on `/api/v1/auth/login`.
- T-AUTH-009: Weak or reused admin passwords.
- T-AUTH-010: Privilege escalation if non-admin accounts can access Admin login.

**Controls**
- C-AUTH-011: Hash admin passwords using BCrypt/Argon2 and store only hashes in `auth_admin_account.password_hash`.
- C-AUTH-012: Implement rate limiting and lockout policies for failed admin logins (per `login_id`).
- C-AUTH-013: Enforce deny-by-default: only principals mapped to ADMIN roles can sign in successfully.
- C-AUTH-014: Consider optional 2FA (e.g., OTP ZNS or TOTP) for Super Admin accounts — **Open Question**.

**Open Questions (AUTH–admin)**
- Q-TM-AUTH-ADMIN-01: Exact lockout thresholds (failed attempts, lockout duration).
- Q-TM-AUTH-ADMIN-02: Whether 2FA is required for some or all admin roles in MVP.

---

### D. Logging & privacy

**Threats**
- T-AUTH-011: Leakage of PII (phone, email) or secrets (OTP, tokens) via logs.

**Controls**
- C-AUTH-015: Centralized logging helpers that:
	- Always include correlationId.
	- Mask phone numbers and emails.
	- Prohibit logging OTPs and raw tokens.
- C-AUTH-016: Security reviews of log queries and dashboards to ensure PII minimization.

These AUTH-specific items complement the global Security-Baseline and should be revisited before go-live.

---

## USERS module — specific threats & controls

### U1. HR import (US-USERS-001)

**Threats**
- T-USERS-001: Malicious or malformed HR file causes partial import with silent data corruption (wrong company/pickup, wrong tier).
- T-USERS-002: Oversized files or excessive imports used as a DoS vector on `/api/v1/users/import`.
- T-USERS-003: Leakage of sensitive HR data via `users_import_row.raw_payload` or logs.

**Controls**
- C-USERS-001: Strict structural validation of the uploaded file before enqueuing the import; reject entire file with clear error (e.g., `USERS_IMPORT_STRUCTURE_INVALID`) when required columns are missing.
- C-USERS-002: Enforce file size and row-count limits; on violation, return 413/400 with error code `USERS_IMPORT_TOO_LARGE` and do not start the job.
- C-USERS-003: Store only the minimum required information in `users_import_row.raw_payload` and apply retention (e.g., 90 days) — full content never exposed directly via API.
- C-USERS-004: Use `import_uuid` (not internal PKs) when referencing jobs in logs or APIs, always with correlationId.

**Open Questions (USERS–import)**
- Q-TM-USERS-IMPORT-01: Exact maximum file size and row count per import.
- Q-TM-USERS-IMPORT-02: Whether to encrypt `raw_payload` at rest or rely on environment-level protection + retention.

### U2. Ref tier manipulation (US-USERS-002)

**Threats**
- T-USERS-004: Unauthorized change of ref tier hierarchy to divert commissions (e.g., moving a Tier 3 under a colluding Tier 2).
- T-USERS-005: Inconsistent or cyclic hierarchies created via `/api/v1/users/{id}/ref-tier` leading to incorrect reporting or infinite traversals.

**Controls**
- C-USERS-005: Restrict `PUT /api/v1/users/{id}/ref-tier` to Super Admin roles; enforce company and tier compatibility rules in the service layer.
- C-USERS-006: Validate that new parent does not create cycles and follows single-parent constraints per FR-USERS-002.
- C-USERS-007: Persist every change in `users_ref_tier_history` with `old_*`/`new_*` and actor info; expose only aggregated views to reporting.

**Open Questions (USERS–ref tier)**
- Q-TM-USERS-REFTIER-01: Whether cross-company ref tiers are ever allowed; if yes, under what governance controls.

### U3. KYC & profile data (US-USERS-003/004)

**Threats**
- T-USERS-006: Over-privileged admins or end users modifying fields that should be company-owned (e.g., company, pickup point, employee code).
- T-USERS-007: Exposure of sensitive KYC data (e.g., CCCD, birth date) via APIs, logs, or analytics.

**Controls**
- C-USERS-008: Backend validates allowed fields for `/api/v1/users/{id}/kyc` and any end-user profile endpoint; silently ignore or reject attempts to change company-owned fields.
- C-USERS-009: Store only hashed/tokenized CCCD (`cccd_hash`) and masked representation (`cccdMasked`) per Data-Classification; never return full CCCD over APIs.
- C-USERS-010: Capture KYC changes in an audit mechanism (ADMIN module) with minimized old/new values to avoid PII overexposure.

**Open Questions (USERS–KYC)**
- Q-TM-USERS-KYC-01: Exact list of fields that are Admin-editable vs. HR-only vs. End-User-editable.
- Q-TM-USERS-KYC-02: Masking rules for displaying KYC on Admin vs End User UI.

### U4. User status & cross-module impact (US-USERS-005)

**Threats**
- T-USERS-008: Inconsistent enforcement of Locked/Inactive status across AUTH/ORDERS/PAYMENTS, allowing blocked users to continue operations.
- T-USERS-009: Abuse of status change API (`/api/v1/users/{id}/status`) by over-privileged admins to disrupt operations.

**Controls**
- C-USERS-011: Centralize status transitions in the USERS service with explicit state machine rules, writing to `users_status_history` for every change.
- C-USERS-012: Enforce RBAC so only authorized roles can lock/unlock or set Inactive; include mandatory `reason` for auditability.
- C-USERS-013: Require all consuming modules (AUTH, ORDERS, PAYMENTS) to check `users_user.status` in their critical flows (login, order placement, payout).

**Open Questions (USERS–status)**
- Q-TM-USERS-STATUS-01: Whether any emergency override exists to allow temporary access for Locked users (e.g., for reconciliation).

### U5. User search & enumeration (US-USERS-006)

**Threats**
- T-USERS-010: User enumeration via broad search filters (e.g., by phone) on `/api/v1/users` by unauthorized or over-broadly authorized actors.
- T-USERS-011: Bulk export of PII through high `pageSize` or unbounded pagination.

**Controls**
- C-USERS-014: Restrict `/api/v1/users` to Admin/Ops roles; scope results by company where applicable.
- C-USERS-015: Enforce sane maximum `pageSize` (per API-Conventions) and rate limiting for frequent searches.
- C-USERS-016: Avoid exposing highly sensitive fields (e.g., full CCCD) in list views; only masked/summary data in `UserSummary`.

**Open Questions (USERS–search)**
- Q-TM-USERS-SEARCH-01: Whether cross-company global search is required for any role or all queries must be company-scoped.

These USERS-specific items complement the baseline and AUTH threats and should be revisited together before go-live.

---

## CATALOG module — specific threats & controls

### C1. Product & category integrity (US-CATALOG-001/002)

**Threats**
- T-CATALOG-001: Unauthorized or accidental changes to product price/category leading to incorrect billing or commissions.
- T-CATALOG-002: Duplicate or conflicting product/category codes (`product_code`, `category_code`) causing ambiguity across modules.

**Controls**
- C-CATALOG-001: Enforce uniqueness at DB level for `catalog_product.product_code` and `catalog_category.category_code` with clear error codes (e.g., `CATALOG_PRODUCT_CODE_DUPLICATE`, `CATALOG_CATEGORY_CODE_DUPLICATE`).
- C-CATALOG-002: Restrict product and category management APIs (`/api/v1/catalog/admin/*`) to Admin roles; log all create/update operations with correlationId.

**Open Questions (CATALOG–product/category)**
- Q-TM-CATALOG-PROD-01: Whether product codes must be unique per supplier or globally; recommended default is global uniqueness for MVP.

### C2. Batch QC & stock integrity (US-CATALOG-003/005)

**Threats**
- T-CATALOG-003: QC bypass — marking a batch as APPROVED without required documents, exposing unverified goods.
- T-CATALOG-004: Stock miscalculation (e.g., double-applying batch quantities or failing to reverse on REJECTED) leading to overselling.

**Controls**
- C-CATALOG-003: Enforce state machine for `catalog_batch.status` in services; validate required documents before allowing APPROVED.
- C-CATALOG-004: Implement atomic updates between `catalog_batch`, `catalog_batch_item`, and `catalog_inventory` when batch status changes; prohibit negative `available_quantity`.

**Open Questions (CATALOG–batches)**
- Q-TM-CATALOG-BATCH-01: Whether status changes from APPROVED back to another status are allowed; recommended default is to disallow downgrades in MVP to simplify stock integrity.

### C3. Product listing & enumeration (US-CATALOG-004)

**Threats**
- T-CATALOG-005: Abuse of `/api/v1/catalog/products` to scrape large volumes of product data.

**Controls**
- C-CATALOG-005: Enforce reasonable `pageSize` limits and rate limiting for product listing; expose only non-sensitive data.

**Open Questions (CATALOG–listing)**
- Q-TM-CATALOG-LIST-01: Whether anonymous access is allowed for product listing or all calls must be authenticated End User traffic.

### C4. Inventory exposure to ORDERS (US-CATALOG-005)

**Threats**
- T-CATALOG-006: Race conditions between CATALOG inventory updates and ORDERS reservations allowing oversell.

**Controls**
- C-CATALOG-006: Treat `Catalog_GetProductStock` as a read-only view and enforce non-negative inventory at DB level; rely on ORDERS for final reservation/locking logic.

**Open Questions (CATALOG–inventory)**
- Q-TM-CATALOG-STOCK-01: Whether bulk stock queries (multiple products at once) are needed; if yes, design an efficient, rate-limited API.

These CATALOG-specific items should be revisited together with USERS/ORDERS before go-live to validate end-to-end stock and catalog integrity.

---

## ORDERS module — specific threats & controls

### O1. Order access control & data leakage (US-ORDERS-001/002/005)

**Threats**
- T-ORDERS-001: End Users or lower-privileged users access orders that do not belong to them (IDOR on `/api/v1/orders/{id}` or `/api/v1/orders/my`).
- T-ORDERS-002: Over-broad Ops/Shipper queries on `/api/v1/orders` leaking cross-company or out-of-scope orders.

**Controls**
- C-ORDERS-001: Always enforce ownership checks for End User calls (`/api/v1/orders/my`, `/api/v1/orders/{id}`) by comparing `userId` from token vs order.
- C-ORDERS-002: Scope `/api/v1/orders` results by company, pickup point, and assignment based on the caller's roles and claims.
- C-ORDERS-003: Apply pagination and sane `pageSize` limits per API-Conventions; do not expose unnecessary PII (e.g. receiver names) in list views.

**Open Questions (ORDERS–access)**
- Q-TM-ORDERS-ACCESS-01: Whether any role (e.g. Super Admin) is allowed global search across companies, and how this is audited.

### O2. Stock reservation and race conditions (US-ORDERS-001/002)

**Threats**
- T-ORDERS-003: Race conditions between order creation and stock updates (multiple orders consuming the same available stock) leading to overselling.
- T-ORDERS-004: Partial failures between ORDERS and PAYMENTS/ CATALOG causing inconsistent order vs payment vs stock states.

**Controls**
- C-ORDERS-004: Implement atomic reservation logic when creating orders (e.g. transaction around stock check + reservation) to avoid negative stock.
- C-ORDERS-005: Require `Idempotency-Key` for `POST /api/v1/orders` to prevent duplicate orders from client retries.
- C-ORDERS-006: Design reconciliation flows with PAYMENTS and CATALOG for failure scenarios (e.g. compensating transactions or scheduled repair jobs).

**Open Questions (ORDERS–stock)**
- Q-TM-ORDERS-STOCK-01: Exact mechanism for stock reservation (hard lock in CATALOG vs soft reservation view) and which module is source of truth for available quantity.

### O3. Status tampering & cancel abuse (US-ORDERS-002/003)

**Threats**
- T-ORDERS-005: Unauthorized status changes via `/api/v1/orders/{id}/status` (e.g. Shipper updating orders not assigned to them).
- T-ORDERS-006: Abuse of cancel flow to hide fraud or mis-shipments (Ops creating and approving cancels without proper oversight).

**Controls**
- C-ORDERS-007: Implement a strict state machine in the service layer for status transitions and enforce role-based permissions per transition.
- C-ORDERS-008: Restrict cancel approval endpoint (`/api/v1/orders/{id}/cancel-request/decision`) to Super Admin roles and log all actions into ADMIN audit log with correlationId.
- C-ORDERS-009: Allow at most one active cancel request per order (`orders_order_cancel_request.is_active`) and ensure conflicting requests return clear conflicts.

**Open Questions (ORDERS–cancel/status)**
- Q-TM-ORDERS-CANCEL-01: Final list of statuses where cancel is allowed vs where only system auto-cancel is permitted.

### O4. POD & returns integrity (US-ORDERS-004)

**Threats**
- T-ORDERS-007: Fake or tampered POD images used to falsely mark orders as delivered.
- T-ORDERS-008: Abuse of return APIs to repeatedly request returns or manipulate quantities, impacting stock and financials.

**Controls**
- C-ORDERS-010: Validate POD uploads at the edge (size, type, basic malware checks) and store only URLs in `orders_order_delivery_proof.pod_image_urls`.
- C-ORDERS-011: Require at least one POD image for transitions to "Completed" status, unless explicit business override is configured.
- C-ORDERS-012: Limit return creation (`/api/v1/orders/{id}/returns`) to eligible statuses and enforce quantity checks against original ordered quantities.

**Open Questions (ORDERS–POD/returns)**
- Q-TM-ORDERS-POD-01: Whether any flows allow marking orders as completed without POD (e.g. after a timeout or manual override) and how these are audited.
- Q-TM-ORDERS-RET-01: Maximum number of return attempts allowed per order and whether only one active return request is permitted.

These ORDERS-specific items complement the baseline, AUTH, USERS, and CATALOG threats and should be reviewed end-to-end before go-live, especially around stock integrity and financial reconciliation.

---

## ADMIN module — specific threats & controls

### AD1. Company & pickup point integrity (US-ADMIN-001)

**Threats**
- T-ADMIN-001: Unauthorized or erroneous changes to `admin_company` or `admin_pickup_point` (e.g., setting company inactive or changing default pickup point) leading to misrouted orders, broken reporting, or cross-company data leakage.
- T-ADMIN-002: Inconsistent defaults (multiple default pickup points per company) due to race conditions during updates.

**Controls**
- C-ADMIN-001: Restrict `/api/v1/admin/companies*` and `/api/v1/admin/companies/{companyId}/pickup-points*` endpoints to Super Admin/Ops roles with deny-by-default RBAC per BR-ADMIN-001.
- C-ADMIN-002: Enforce unique default pickup point per company at DB level via partial unique index and in service layer when toggling `isDefault`.
- C-ADMIN-003: Prevent deactivation of companies or pickup points that are still referenced by active users/orders; surface clear error codes (e.g. `ADMIN_COMPANY_IN_USE`, `ADMIN_PICKUPPOINT_IN_USE`).
- C-ADMIN-004: Log all create/update operations on companies and pickup points to `admin_audit_log` with correlationId and actor identity.

**Open Questions (ADMIN–company/pickup)**
- Q-TM-ADMIN-COMP-01: Whether companies can ever be merged or split and how historical references should be handled; recommended default is to avoid merge/split in MVP and rely on inactivation plus new records.

### AD2. Admin roles & privilege escalation (US-ADMIN-002)

**Threats**
- T-ADMIN-003: Privilege escalation by granting high-privilege roles (e.g. Super Admin) to unauthorized accounts via `/api/v1/admin/admin-users*` or `/api/v1/admin/roles*`.
- T-ADMIN-004: Over-privileged or orphaned roles that are never reviewed, leading to long-lived excessive access.

**Controls**
- C-ADMIN-005: Restrict role and admin-user management endpoints to Super Admin only; enforce this in the service layer, not just the UI.
- C-ADMIN-006: Require at least one role for every admin user and block removal of all roles (`ADMIN_ADMINUSER_ROLE_EMPTY`), ensuring that access changes are explicit and auditable.
- C-ADMIN-007: Record all role and admin-user changes in `admin_audit_log` (actor, target, roles before/after summarized in metadata) while avoiding PII in metadata.
- C-ADMIN-008: Implement periodic review processes (procedural) for admin roles and assignments, supported by queries on `admin_admin_user_role` and audit logs.

**Open Questions (ADMIN–roles)**
- Q-TM-ADMIN-ROLES-01: Whether an admin user is ever allowed to modify their own roles; recommended default is that only a different Super Admin may change roles for a given admin account.

### AD3. System configuration tampering (US-ADMIN-003/004)

**Threats**
- T-ADMIN-005: Unauthorized or accidental changes to system branding or legal/terms configuration leading to legal/compliance issues or user confusion.
- T-ADMIN-006: Misconfigured dashboard widgets exposing data to roles that should not see specific KPIs (e.g., financial metrics visible to Ops-only users).

**Controls**
- C-ADMIN-009: Restrict `/api/v1/admin/system-config` (and any dashboard widget config endpoints) to Super Admin; other roles may only read effective configuration.
- C-ADMIN-010: Store system configuration centrally in `admin_system_config` with full audit trail; every update writes an entry to `admin_audit_log` including changed keys (not full content when sensitive).
- C-ADMIN-011: Apply role-based filtering on dashboard widgets in frontends and backend REPORTING APIs so that each widget is only visible to authorized roles.

**Open Questions (ADMIN–config/dashboard)**
- Q-TM-ADMIN-CONFIG-01: Whether any configuration values are considered sensitive (e.g., URLs pointing to internal systems) and require additional protection beyond standard DB controls.

### AD4. Audit log misuse & PII exposure (US-ADMIN-005)

**Threats**
- T-ADMIN-007: Audit logs leaking PII or secrets (e.g., full CCCD, phone numbers, OTPs, tokens) via `metadata` or search APIs.
- T-ADMIN-008: Abuse of `/api/v1/admin/audit-logs` for bulk enumeration of users, orders, or other sensitive identifiers.

**Controls**
- C-ADMIN-012: Enforce strict contracts for `admin_audit_log.metadata` (no raw PII or secrets); only store hashed/masked identifiers or business codes per Data-Classification.
- C-ADMIN-013: Scope `/api/v1/admin/audit-logs` access primarily to Super Admin; Support or other roles may have restricted views (subset of modules/actionTypes) configured in RBAC policies.
- C-ADMIN-014: Apply pagination and enforce a maximum time range per query, returning `ADMIN_AUDITLOG_RANGE_TOO_WIDE` for overly broad requests; consider rate limiting high-frequency queries.

**Open Questions (ADMIN–audit log)**
- Q-TM-ADMIN-AUDIT-01: Exact retention period and archival mechanism for `admin_audit_log` in production environments.

These ADMIN-specific items complement the baseline, AUTH, USERS, CATALOG, ORDERS, and NOTIFICATIONS threats and should be validated as part of an end-to-end RBAC and audit review before go-live.

---

## REPORTING module — specific threats & controls

### R1. Over-broad access to reporting data (US-REPORTING-001/003/004/005/006)

**Threats**
- T-REPORT-001: Finance/Ops or other roles query reports for companies, tiers, or users outside their authorized scope, leading to cross-company data leakage.
- T-REPORT-002: Tier 1/Tier 2 users are accidentally granted access to system-wide or other-company reports (e.g. via misconfigured roles).

**Controls**
- C-REPORT-001: Enforce RBAC and company scoping for all `/api/v1/reporting/*` endpoints in the service layer, not just via UI; rely on ADMIN roles and USERS tier/company mapping.
- C-REPORT-002: For Tier 1/Tier 2 views, restrict queries to their own network or company based on USERS ref-tier relationships and ADMIN company assignments.

**Open Questions (REPORTING–scope)**
- Q-TM-REPORT-ACCESS-01: Exact matrix of which roles (Super Admin, Finance, Ops, Tier 1, Tier 2) can view which REPORTING endpoints and scopes (global vs company vs own network).

### R2. Heavy queries & DoS via large time ranges (US-REPORTING-001/003/004)

**Threats**
- T-REPORT-003: Attackers or misconfigured clients run very wide date range queries on `/api/v1/reporting/*` (e.g. multiple years), causing excessive load on DB and impacting other modules.

**Controls**
- C-REPORT-003: Enforce maximum allowed time window on reporting endpoints; reject overly broad requests with 400 and `REPORTING_RANGE_TOO_WIDE` per API-Conventions.
- C-REPORT-004: Apply pagination and reasonable pageSize caps consistently for list-style report APIs; consider additional rate limiting for expensive endpoints.

**Open Questions (REPORTING–performance)**
- Q-TM-REPORT-PERF-01: Exact maximum time window (e.g. N months) for each report type and whether this is configurable per environment.

### R3. PII exposure through reports & exports (US-REPORTING-001/003/004/005/007)

**Threats**
- T-REPORT-004: Reports or exports leak unnecessary PII (e.g. phone numbers, CCCD, exact addresses) beyond what is needed for operational decisions.
- T-REPORT-005: CSV/Excel exports (if in scope) are used to exfiltrate large volumes of sensitive data outside the system.

**Controls**
- C-REPORT-005: Design REPORTING schemas and APIs to use IDs and aggregated metrics; only join to USERS/ORDERS for display when absolutely necessary and apply masking per Data-Classification.
- C-REPORT-006: For any export features (e.g. US-REPORTING-007, if approved), restrict to appropriate roles, enforce file size/row limits, and omit highly sensitive fields.

**Open Questions (REPORTING–PII/export)**
- Q-TM-REPORT-PII-01: Precise list of fields allowed in each report and export, per persona.
- Q-TM-REPORT-EXPORT-01: Whether CSV/Excel export is in scope for MVP and, if yes, what size limits and retention policies apply to generated files.

### R4. Inconsistent or misleading KPIs on Dashboard (US-REPORTING-006)

**Threats**
- T-REPORT-006: Dashboard KPIs (orders today, COD open, overdue debt, top category) show inconsistent values compared to detailed reports due to timing or calculation differences, eroding trust.
- T-REPORT-007: Dashboard widgets silently fail or hang, preventing admins from seeing any overview.

**Controls**
- C-REPORT-007: Derive Dashboard KPIs from the same aggregation logic as underlying reports (sales, COD, debt), with clear documentation of time windows and calculation rules.
- C-REPORT-008: Expose dedicated, small-footprint APIs per widget (e.g. `/api/v1/reporting/dashboard/*`) so that failures are isolated per widget and do not block the entire Dashboard.

**Open Questions (REPORTING–dashboard)**
- Q-TM-REPORT-DASH-01: Exact refresh frequency and cache strategy for Dashboard KPIs (real-time vs near-real-time vs batch).

These REPORTING-specific items complement the baseline and other module threat models and should be reviewed together with PAYMENTS and ORDERS, given heavy reliance on financial and operational data.
