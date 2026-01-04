
# Business Rules — Central Catalogue

> Single source for cross-module business rules. Module SRS files (SRS-*.md) reference these IDs.

## Format
- **BR-### — Title**  
  - Statement: (clear, testable rule)  
  - Applies to modules: ...  
  - Related stories (examples): ...  
  - Notes / Open questions: ...

IDs are unique and reused across modules. Where SRS-<MODULE> uses local codes (e.g., BR-AUTH-001), they are mapped into the BR-### catalogue below.

---

## 1. Global rules (cross-module)

**BR-001 — OTP ZNS only for first-time login/registration**  
- Statement: OTP over ZNS is required only for the first registration / first login of an End User; subsequent logins should prefer Session Token / Refresh Token bound to the device/browser, unless a security risk requires re-challenging with OTP.  
- Applies to modules: AUTH, NOTIFICATIONS.  
- Related stories (examples): US-AUTH-001, US-AUTH-002, US-AUTH-003, US-NOTIFICATIONS-002.  
- Notes / Open questions: Exact conditions for re-forcing OTP on later logins (device change, suspicious activity, long inactivity) to be defined in Security-Baseline and SRS-AUTH open questions.

**BR-002 — Pay-later by company with credit limit and lock on overdue**  
- Statement: Pay-later "trả chậm đến ngày nhận lương" may only be enabled per Company and must be tied to a credit limit (max orders / total amount). When a user is overdue beyond this limit, the system must block new purchases until Finance/Admin confirms the debt is settled.  
- Applies to modules: PAYMENTS, USERS, ORDERS, REPORTING.  
- Related stories (examples): US-PAYMENTS-005, US-ORDERS-XXX (đặt đơn trả chậm), US-REPORTING-005.  
- Notes / Open questions: Exact limit formulas and overdue grace periods come from commercial policy and may be refined; see SRS-00-Overview and SRS-PAYMENTS open questions.

**BR-003 — Each lot belongs to a single category and must be QC-approved before sale**  
- Statement: In MVP, each Lot belongs to exactly one Category and must have sufficient QC documentation (documents/images) recorded before any product from that lot may be sold.  
- Applies to modules: CATALOG, ORDERS, REPORTING.  
- Related stories (examples): US-CATALOG-XXX (quản lý Lô & ngành hàng), US-ORDERS-001 (tạo đơn với sản phẩm khả dụng).  
- Notes / Open questions: Detailed QC evidence requirements (types of documents, retention) to be confirmed with business stakeholders.

**BR-004 — Order cancelation with automatic cascading adjustments**  
- Statement: Any order cancelation initiated by Ops and approved by Super Admin must automatically: update order status, restore inventory, adjust commissions, adjust COD/pay-later debt, and write an audit log.  
- Applies to modules: ORDERS, PAYMENTS, REPORTING, ADMIN.  
- Related stories (examples): US-ORDERS-XXX (hủy đơn), US-PAYMENTS-00X (điều chỉnh COD/công nợ), US-ADMIN-005 (xem audit log).  
- Notes / Open questions: The exact states from which cancelation is allowed and the adjustment timing are described at high level in SRS-ORDERS and SRS-PAYMENTS and require detailed design.

**BR-005 — Shipper/Tầng 2 must close and reconcile COD shifts**  
- Statement: Any Shipper/Tầng 2 who collects COD must close their COD shift and be reconciled; all discrepancies (difference between expected and collected COD) must be recorded and reported.  
- Applies to modules: PAYMENTS, REPORTING.  
- Related stories (examples): US-PAYMENTS-003, US-PAYMENTS-004, US-REPORTING-004.  
- Notes / Open questions: Tolerance thresholds for small differences and escalation flows are to be agreed with Finance.

---

## 2. AUTH rules

Mapped from SRS-AUTH BR-AUTH-### and global rules.

**BR-006 — Login tokens bound to device / browser**  
- Statement: Login tokens (Session/Refresh) must be logically bound to a device/browser context to mitigate abuse; when a user switches device/browser, the system may require OTP re-verification according to security policy.  
- Applies to modules: AUTH.  
- Related stories (examples): US-AUTH-002, US-AUTH-003, US-AUTH-004.  
- Notes / Open questions: Exact device fingerprinting and when to force OTP again are left to Security-Baseline and Tech Lead design (see SRS-AUTH BR-AUTH-002).

**BR-007 — Valid HR/user master record required for tier assignment**  
- Statement: Only users with a valid record in the HR/user master data (USERS import) may be automatically assigned to Tier 1/2/3; records that do not match the import keys must follow a clearly defined exception policy (reject or route to manual review).  
- Applies to modules: AUTH, USERS.  
- Related stories (examples): US-AUTH-001, US-USERS-001 (import nhân sự).  
- Notes / Open questions: Exact unique key for matching (employee code + company vs phone + company) is TBD (see BR-USERS-001 in SRS-USERS).

**BR-008 — All registration/login attempts must be audit-logged**  
- Statement: Every registration/login attempt (successful or failed) must be written to an audit log with at least timestamp, identifier (phone/email masked per classification), and outcome.  
- Applies to modules: AUTH, ADMIN (audit UI), REPORTING (if reporting on security events).  
- Related stories (examples): US-AUTH-001..005, US-ADMIN-005.  
- Notes / Open questions: Exact log retention period and masking rules follow Security-Baseline and Data-Classification.

---

## 3. USERS rules

Mapped from SRS-USERS BR-USERS-###.

**BR-009 — User identity key for import and updates**  
- Statement: A configurable identity key (e.g., employeeCode + company, or phone + company) defines uniqueness of a User record. On import, if an incoming row matches an existing user by that key, the system must update the existing record instead of creating a duplicate.  
- Applies to modules: USERS.  
- Related stories (examples): US-USERS-001 (import nhân sự), US-USERS-00X (cập nhật hồ sơ).  
- Notes / Open questions: Exact field combination, overwrite precedence per field, and conflict resolution must be agreed with each company (TBD in SRS-USERS BR-USERS-001).

**BR-010 — Validity of Ref Tier hierarchy**  
- Statement: For Tier 2 users, Ref Tier must point to a valid Tier 1; for Tier 3 users, Ref Tier must point to a valid Tier 2. If the referenced parent does not exist at import time, the row must be flagged as error and no user is created/updated (unless a later-approved exception policy is applied).  
- Applies to modules: USERS, REPORTING (network-based reporting), PAYMENTS (commission).  
- Related stories (examples): US-USERS-002 (import Ref Tier), US-REPORTING-003.  
- Notes / Open questions: Any allowed temporary "dangling" refs or deferred resolution must be explicitly documented; default is to reject.

**BR-011 — Company-controlled fields**  
- Statement: Certain fields (e.g., Company, PickupPoint, Tier, employee code) are controlled by company-provided data or admin flows and must not be freely editable by End Users.  
- Applies to modules: USERS, ADMIN.  
- Related stories (examples): US-USERS-00X (chỉnh sửa hồ sơ hạn chế), US-ADMIN-001 (quản lý Công ty/Điểm nhận).  
- Notes / Open questions: Concrete field whitelist/blacklist to be finalized with each customer.

**BR-012 — Locked users cannot perform critical business actions**  
- Statement: When a User is in Locked state, all critical business operations (login, placing new orders, new payments) must be denied across modules.  
- Applies to modules: USERS, AUTH, ORDERS, PAYMENTS.  
- Related stories (examples): US-AUTH-002 (login bị chặn nếu user khoá), US-ORDERS-XXX, US-PAYMENTS-005.  
- Notes / Open questions: Who can change Locked → Active, and audit requirements, are detailed in SRS-USERS.

---

## 4. CATALOG rules

Mapped from SRS-CATALOG BR-CATALOG-###.

**BR-013 — Lot has single category (implementation of BR-003)**  
- Statement: A Lot cannot contain products from multiple categories. Import or creation operations that would mix categories into one Lot must be rejected or transformed according to a later-defined splitting policy.  
- Applies to modules: CATALOG.  
- Related stories (examples): US-CATALOG-001 (quản lý Lô & ngành hàng).  
- Notes / Open questions: Automatic splitting vs blocking is TBD; default assumption is to reject invalid input.

**BR-014 — No hard delete for products/categories**  
- Statement: Products and Categories may not be physically deleted while in use; instead they are marked inactive / is_deleted so historical reports remain consistent.  
- Applies to modules: CATALOG, ORDERS, REPORTING.  
- Related stories (examples): US-CATALOG-00X (ẩn sản phẩm/ngành hàng), US-REPORTING-001.  
- Notes / Open questions: Exact semantics of Inactive vs is_deleted and their impact on existing orders.

**BR-015 — Only QC-approved lots can be sold**  
- Statement: Products belonging to lots that have not passed QC must not be visible to End Users and cannot be added to orders.  
- Applies to modules: CATALOG, ORDERS.  
- Related stories (examples): US-CATALOG-003, US-ORDERS-001.  
- Notes / Open questions: QC workflow and evidence requirements are defined in SRS-CATALOG and related Discovery docs.

**BR-016 — Inventory must never go negative**  
- Statement: Any operation that adjusts stock must ensure the resulting inventory is non-negative; if an operation would push stock below zero, it must be rejected or handled by a defined backorder/exception flow.  
- Applies to modules: CATALOG, ORDERS.  
- Related stories (examples): US-ORDERS-001 (đặt hàng), US-CATALOG-00X (điều chỉnh tồn kho).  
- Notes / Open questions: Whether temporary negative inventory is permitted in rare operational scenarios is TBD (default: not allowed).

---

## 5. ORDERS rules

Mapped from SRS-ORDERS BR-ORDERS-### and global BR-004.

**BR-017 — Orders may only contain available and QC-approved products**  
- Statement: Orders must only contain products that have available stock and belong to QC-approved lots, respecting inventory and CATALOG rules.  
- Applies to modules: ORDERS, CATALOG.  
- Related stories (examples): US-ORDERS-001, US-CATALOG-003..004.  
- Notes / Open questions: Timing of stock reservation vs confirmation to be refined in design.

**BR-018 — Enforce valid order status transitions**  
- Statement: Orders may transition only through allowed states in the configured lifecycle; illegal transitions must be blocked.  
- Applies to modules: ORDERS, REPORTING, PAYMENTS.  
- Related stories (examples): US-ORDERS-00X (quản lý trạng thái đơn), US-REPORTING-001.  
- Notes / Open questions: Full state machine is defined in SRS-00-Overview and SRS-ORDERS; any changes must be reflected here.

**BR-019 — Cancelation requires appropriate approval**  
- Statement: For orders beyond certain states, cancelation must be requested by Ops and approved by Super Admin (except auto-cancel flows based on timeouts, if later defined).  
- Applies to modules: ORDERS, ADMIN, PAYMENTS.  
- Related stories (examples): US-ORDERS-00X (hủy đơn), US-ADMIN-005 (xem audit log).  
- Notes / Open questions: Exact states where cancelation requires approval and who can request are TBD.

**BR-020 — Returns/exchanges must capture evidence**  
- Statement: All return/exchange requests must record reason, responsible actor, and supporting evidence (e.g., images) when required by QC.  
- Applies to modules: ORDERS, CATALOG, QC/ADMIN.  
- Related stories (examples): US-ORDERS-00X (đổi trả), US-ADMIN-005.  
- Notes / Open questions: Evidence retention period and storage are defined in non-functional requirements.

---

## 6. PAYMENTS rules

Mapped from SRS-PAYMENTS BR-PAYMENTS-### and global BR-002, BR-005.

**BR-021 — Enforce user/company credit limits for pay-later**  
- Statement: Each Tier 3 user in a company with pay-later enabled must have an effective credit limit; current debt plus new order amount must not exceed this limit.  
- Applies to modules: PAYMENTS, ORDERS, USERS, REPORTING.  
- Related stories (examples): US-PAYMENTS-005, US-ORDERS-00X (đặt đơn trả chậm), US-REPORTING-005.  
- Notes / Open questions: How limits are calculated and maintained per company/user is described at high level in SRS-PAYMENTS.

**BR-022 — Mandatory COD reconciliation per shift**  
- Statement: Every shift that involves COD must be reconciled; a shift is not considered complete until its reconciliation status is RECONCILED or a documented difference status.  
- Applies to modules: PAYMENTS, REPORTING.  
- Related stories (examples): US-PAYMENTS-003, US-PAYMENTS-004, US-REPORTING-004.  
- Notes / Open questions: Escalation when reconciliation is overdue and accepted difference thresholds are TBD.

**BR-023 — COD receipts linked to orders and shifts, immutable after close**  
- Statement: Each COD receipt must be linked to a specific order and shift; once the shift is closed, receipts cannot be edited or deleted except through dedicated adjustment flows.  
- Applies to modules: PAYMENTS, ORDERS, REPORTING.  
- Related stories (examples): US-PAYMENTS-002, US-PAYMENTS-003, US-REPORTING-004.  
- Notes / Open questions: Design of adjustment flows where receipts need correction is pending.

**BR-024 — Payment data is authoritative source for financial reporting**  
- Statement: Changes to payment records (COD, bank transfer, pay-later) must remain consistent with how revenue, commission, and debt are reported; REPORTING should treat PAYMENTS as the single source of truth for monetary values.  
- Applies to modules: PAYMENTS, REPORTING.  
- Related stories (examples): US-PAYMENTS-001..005, US-REPORTING-001..005.  
- Notes / Open questions: Alignment between accounting requirements and operational reporting to be validated with Finance.

---

## 7. NOTIFICATIONS rules

Mapped from SRS-NOTIFICATIONS BR-NOTIFICATIONS-### and global BR-001.

**BR-025 — OTP ZNS usage aligned to cost-control policy**  
- Statement: NOTIFICATIONS sends OTP ZNS only when requested by AUTH according to BR-001; it must not independently increase OTP frequency beyond requests from AUTH.  
- Applies to modules: NOTIFICATIONS, AUTH.  
- Related stories (examples): US-NOTIFICATIONS-002, US-AUTH-001, US-AUTH-002.  
- Notes / Open questions: Detailed OTP cost thresholds and rate limits are defined in Security-Baseline and vendor contracts.

**BR-026 — Configurable ZNS operational reminders**  
- Statement: Operational reminders (pickup reminders, debt reminders, commission changes) sent via ZNS must be configurable per company and per notification group to balance user experience and ZNS cost.  
- Applies to modules: NOTIFICATIONS, ADMIN, PAYMENTS, ORDERS.  
- Related stories (examples): US-NOTIFICATIONS-003, US-ADMIN-001 (cấu hình theo Công ty).  
- Notes / Open questions: Default configurations and per-company override policies are TBD.

**BR-027 — Do not log sensitive notification content**  
- Statement: Sensitive content (e.g., OTP codes) must not be stored in full in logs; only minimal metadata (type, template, masked recipient, status, timestamps) may be recorded.  
- Applies to modules: NOTIFICATIONS, AUTH, SECURITY/LOGGING.  
- Related stories (examples): US-NOTIFICATIONS-001, US-NOTIFICATIONS-002, US-AUTH-001..003.  
- Notes / Open questions: Exact masking/obfuscation rules follow Data-Classification and Security-Baseline.

---

## 8. ADMIN rules

Mapped from SRS-ADMIN BR-ADMIN-###.

**BR-028 — Deny-by-default authorization on Admin Portal**  
- Statement: All Admin Portal features are denied by default; access is granted only when the Admin account has the appropriate role/permission assigned. Only Super Admin can grant or revoke roles for other Admin accounts.  
- Applies to modules: ADMIN, AUTH (Admin authz integration).  
- Related stories (examples): US-AUTH-005, US-ADMIN-002.  
- Notes / Open questions: Fine-grained permission model (per-screen/per-action) to be finalized in SRS-ADMIN and implementation plan.

**BR-029 — Company and PickupPoint lifecycle constraints**  
- Statement: Each Tier 3 user belongs to exactly one Company and has one default PickupPoint; Companies/PickupPoints referenced by users or orders must not be physically deleted, only deactivated/inactivated.  
- Applies to modules: ADMIN, USERS, ORDERS.  
- Related stories (examples): US-ADMIN-001, US-USERS-001, US-ORDERS-001.  
- Notes / Open questions: Detailed behavior when changing user’s company or default pickup point is described in SRS-ADMIN and SRS-USERS.

**BR-030 — Mandatory audit log for sensitive admin actions**  
- Statement: All configuration, permission, and sensitive business actions (including cancelation approval) must produce audit log entries with correlationId; ADMIN provides search UI while business modules are responsible for writing logs.  
- Applies to modules: ADMIN, AUTH, ORDERS, PAYMENTS, NOTIFICATIONS, REPORTING.  
- Related stories (examples): US-ADMIN-005, US-AUTH-001..005, US-ORDERS-00X, US-PAYMENTS-00X.  
- Notes / Open questions: Which operations are considered "sensitive" requires a curated list maintained in Security-Baseline.

---

## 9. REPORTING rules

Mapped from SRS-REPORTING BR-REPORTING-### and global BR-002, BR-004, BR-005.

**BR-031 — Reporting must not redefine source business logic**  
- Statement: REPORTING must not introduce its own business logic for revenue, commission, or debt; it must strictly follow rules defined in ORDERS, PAYMENTS, USERS, and CATALOG.  
- Applies to modules: REPORTING, ORDERS, PAYMENTS, USERS, CATALOG.  
- Related stories (examples): US-REPORTING-001..006.  
- Notes / Open questions: Any derived metrics must clearly document which source rules they aggregate.

**BR-032 — Avoid double-counting in revenue/commission**  
- Statement: Canceled or returned orders must be handled according to global cancel/return rules to avoid double-counting revenue or commission (e.g., adjusting prior periods as needed).  
- Applies to modules: REPORTING, ORDERS, PAYMENTS.  
- Related stories (examples): US-REPORTING-001, US-REPORTING-003, US-PAYMENTS-001..005.  
- Notes / Open questions: Accounting treatment for adjustments (same period vs later period) must be agreed with Finance.

**BR-033 — Data access scope in reporting follows role and network hierarchy**  
- Statement: Reporting visibility must respect role and ref-tier scope: Tier 1/Tier 2 see only their own network/commission scope, while Finance/Ops/Super Admin may see broader or full system data as configured in ADMIN.  
- Applies to modules: REPORTING, ADMIN, USERS, AUTH.  
- Related stories (examples): US-REPORTING-003, US-REPORTING-005, US-ADMIN-002.  
- Notes / Open questions: Exact RBAC matrix per persona and report is part of SRS-REPORTING and SRS-ADMIN.

---

## 10. Traceability notes

- Each module SRS (SRS-<MODULE>.md) continues to reference local BR-<MODULE>-### codes; they should be kept in sync with the BR-### mapping above.
- Stories files (Stories-<MODULE>.md) should reference BR-### IDs where applicable; gaps can be filled during refinement.
- When adding new rules, update this file with the next available BR-###, then reference it from SRS and stories.

