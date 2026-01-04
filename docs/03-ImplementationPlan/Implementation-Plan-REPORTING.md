
# Implementation Plan — REPORTING: Reporting & Exports

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead (REPORTING)
- Reviewers: BA / Tech Lead / QA / DevOps (if needed)
- Target sprint/release: MVP REPORTING

---

## 1) Scope
### In scope
- REPORTING-SCOPE-01: P0 stories US-REPORTING-001, US-REPORTING-003, US-REPORTING-004, US-REPORTING-005, US-REPORTING-006.
- REPORTING-SCOPE-02: Backend reporting APIs and DB structures needed for sales, commission (Tier 2), COD shift, debt, and Dashboard KPIs, without changing source business logic in ORDERS/PAYMENTS/USERS.

### Out of scope
- REPORTING-OOS-01: Advanced analytics/BI, financial accounting reports beyond operational MVP scope.
- REPORTING-OOS-02: Full-featured export engine and large offline report generation; CSV export (US-REPORTING-007) is P2 and depends on security decisions.

### Success criteria
- SC1: Admin roles (Super Admin/Ops/Finance) can retrieve accurate, performant reports for sales, commission, COD shifts, and debt aligned with source modules.
- SC2: Dashboard widgets (orders today, COD open, overdue debt, top category) display consistent KPIs with underlying reports and degrade gracefully on partial failures.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-REPORTING.md`
- Stories: `docs/02-Requirements/Stories/Stories-REPORTING.md`
- OpenAPI paths:
  - GET /api/v1/reporting/sales/company
  - GET /api/v1/reporting/commissions/t2
  - GET /api/v1/reporting/cod/shifts
  - GET /api/v1/reporting/debts/companies
  - GET /api/v1/reporting/debts/companies/{companyId}/users
  - GET /api/v1/reporting/dashboard/orders-today
  - GET /api/v1/reporting/dashboard/cod-open
  - GET /api/v1/reporting/dashboard/debt-overdue
  - GET /api/v1/reporting/dashboard/top-category
- DB doc: `docs/03-Design/DB/DB-REPORTING.md`
- Business rules:
  - BR-REPORTING-001 — REPORTING does not define its own business logic; it must follow ORDERS/PAYMENTS rules.
  - BR-REPORTING-002 — Avoid double counting revenue and commission.
  - BR-REPORTING-003 — Data visibility scopes follow ADMIN/USERS RBAC.

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - Implement REPORTING controllers exposing the OpenAPI paths above under tag REPORTING.
- Service layer:
  - ReportingSalesService: aggregates sales metrics from ORDERS and CATALOG, optionally using `reporting_sales_summary`.
  - ReportingCommissionService: reads commission data from PAYMENTS and populates/queries `reporting_commission_t2`.
  - ReportingCodShiftService: reads from PAYMENTS shifts/receipts and exposes `/reporting/cod/shifts` using `PaymentShiftSummary`.
  - ReportingDebtService: reads from PAYMENTS debt balances and populates/queries `reporting_debt_company` and `reporting_debt_user`.
  - ReportingDashboardService: exposes small, widget-focused endpoints using either live aggregations or `reporting_dashboard_snapshot`.
- Validation:
  - Validate all date ranges (`fromDate`, `toDate`, `asOfDate`, `periodStart`, `periodEnd`) and enforce from <= to.
  - Enforce maximum allowed range per endpoint; reject with `REPORTING_RANGE_TOO_WIDE` when exceeded.
  - Validate that filter IDs (companyId, shipperId, userId) exist and are within caller's scope.
- Error codes:
  - Define `REPORTING_INVALID_DATE_RANGE`, `REPORTING_RANGE_TOO_WIDE`, and `REPORTING_FORBIDDEN_SCOPE` in Error-Conventions.
  - Use standard `ErrorResponse` envelope with correlationId.
- AuthZ policies:
  - Restrict REPORTING endpoints to appropriate ADMIN roles (Super Admin/Finance/Ops), with future extension for Tier 1/Tier 2 views.
  - Enforce company-scoped queries based on caller claims (companyId, allowedCompanies, tier network).
- Logging/CorrelationId:
  - Log all REPORTING calls with correlationId, role, filters, and status (success/error), without logging aggregates or PII rows.

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Create `reporting_sales_summary`, `reporting_commission_t2`, `reporting_cod_shift`, `reporting_debt_company`, `reporting_debt_user`, `reporting_dashboard_snapshot` per DB-REPORTING.
  - Add recommended indexes for typical filters (date ranges, company, user/shipper).
- Migration steps:
  - Introduce REPORTING tables in a dedicated migration after core ORDERS/PAYMENTS/USERS/ADMIN tables exist.
  - Optionally backfill recent historical data via one-off jobs or scripts.
- Rollback steps:
  - In non-prod, drop REPORTING tables in reverse dependency order if needed.
  - In prod, disable REPORTING endpoints and keep schema/data for later adjustments.
- Data seeding (if any):
  - None for business data; only backfill from existing ORDERS/PAYMENTS if required.

### Frontend (React Web) / Mobile (if any)
- Screens/components:
  - Admin Portal: "Báo cáo doanh số" screen (US-REPORTING-001).
  - Admin Portal: "Báo cáo hoa hồng" screen (US-REPORTING-003).
  - Admin Portal: "Báo cáo COD theo Ca" screen (US-REPORTING-004).
  - Admin Portal: "Báo cáo công nợ" screen (US-REPORTING-005).
  - Admin Portal: Dashboard widgets wired to `/reporting/dashboard/*` (US-REPORTING-006).
- State management:
  - Keep filters (date ranges, company, shipper, status) in URL/query params for all reporting screens.
- Form validation + error mapping:
  - Map REPORTING error codes (`REPORTING_INVALID_DATE_RANGE`, `REPORTING_RANGE_TOO_WIDE`, `REPORTING_FORBIDDEN_SCOPE`) to clear UI messages.
- UI evidence expectations:
  - Capture example screenshots for each report screen and Dashboard state (normal, empty, error widget).

### Integrations / Jobs (if any)
- External APIs:
  - None; all data comes from internal modules.
- Webhooks:
  - N/A.
- Background jobs (idempotency, retries):
  - Optional ETL jobs to populate/refresh reporting tables or materialized views nightly/hourly.

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - REPORTING endpoints and schemas added under tag REPORTING in `openapi.yaml`.
  - Document error codes `REPORTING_INVALID_DATE_RANGE`, `REPORTING_RANGE_TOO_WIDE`, `REPORTING_FORBIDDEN_SCOPE`.
- Backward compatibility considerations:
  - REPORTING APIs are new in MVP; no legacy clients.
  - Future evolution should be additive or versioned (`/api/v2/reporting/...`) when breaking.
- Idempotency requirements:
  - All REPORTING endpoints are read-only GETs; no Idempotency-Key required.
- Pagination/filtering rules:
  - Use `page`/`pageSize` for list-like report endpoints; enforce global max `pageSize`.
  - Validate and cap date ranges per endpoint.

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - Super Admin, Finance, Ops (ADMIN roles); potential future Tier 1/Tier 2 read-only roles.
- Permissions matrix (action -> role):
  - Sales, commission, COD, debt reports: Super Admin, Finance; Ops may see limited subsets (e.g. no debt details) per ADMIN RBAC.
  - Dashboard widgets: all Admin roles; values may differ depending on role (e.g. Finance sees financial KPIs, Ops sees operational ones).
- Sensitive operations requiring audit trail:
  - Large or export-like report queries should be logged with actor, filters, and correlationId in `admin_audit_log`.

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-REPORT-001: Sales aggregation logic (date range validation, totalOrders/totalRevenue/totalItems consistency with mocked ORDERS data).
- UT-REPORT-002: Commission aggregation logic (per-user totals, rate and amount calculations from mocked PAYMENTS data).
- UT-REPORT-003: Debt snapshot classification (in-term vs overdue vs negative) based on mocked PAYMENTS debt balances.

### Integration tests
- IT-REPORT-001: `/api/v1/reporting/sales/company` end-to-end against seeded ORDERS/CATALOG data.
- IT-REPORT-002: `/api/v1/reporting/cod/shifts` against seeded PAYMENTS shifts/receipts.
- IT-REPORT-003: `/api/v1/reporting/debts/companies` and `/users` against seeded PAYMENTS debt balances and ADMIN companies.

### E2E / Smoke (staging)
- E2E-REPORT-001: Finance retrieves sales, commission, COD, and debt reports for a company and verifies numbers align with manual calculations.
- E2E-REPORT-002: Admin Dashboard loads all widgets; numbers match corresponding detailed reports for the same filters.

### Security sanity
- SEC-REPORT-001 (authz negative): Verify unauthorized roles cannot access REPORTING APIs or see out-of-scope companies/users.
- SEC-REPORT-002 (input validation/injection basic): Validate date filters and IDs; ensure queries use parameterized access without injection.

---

## 7) Observability Plan
- Logs (no PII):
  - Log REPORTING calls with correlationId, actor role, and high-level filters; avoid logging aggregates or user-level data.
- Metrics (key counters/timers):
  - Track p95 latency and error rates for each REPORTING endpoint.
  - Track count of rejected wide-range queries (`REPORTING_RANGE_TOO_WIDE`).
- Alerts (if needed):
  - Alert on sustained high latency or error rates for key endpoints and Dashboard widgets.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Optional feature flag for REPORTING and individual Dashboard widgets.
- Migration order:
  - Deploy DB migration for REPORTING tables.
  - Deploy services and endpoints.
  - Wire Admin Portal screens and Dashboard widgets.
- Staging verification steps:
  - Run integration and E2E tests for REPORTING flows.
  - Validate performance with typical and worst-case report queries.
- Production checklist items:
  - Confirm RBAC mappings for REPORTING endpoints.
  - Confirm monitoring dashboards and alerts for REPORTING APIs.
- Rollback procedure:
  - Disable REPORTING feature flags and/or routes;
  - Keep DB schema; apply forward fixes rather than dropping tables.

---

## 9) Risks & Mitigations
- R1: Over-broad report access leaking cross-company or user-level data.
  - Mitigation: Strong RBAC scoping (C-REPORT-001/002) and negative authz tests (SEC-REPORT-001).
- R2: Slow or failing reports impacting user experience.
  - Mitigation: Max range enforcement, indexes, and pre-aggregation; performance testing in staging.
- R3: Inconsistent KPIs between Dashboard and detailed reports.
  - Mitigation: Reuse the same aggregation logic and include automated tests comparing KPIs vs detail queries.

---

## 10) Estimates & Owners
| Work item | Owner | Estimate | Notes |
|----------|-------|----------|------|
| BE | TBD | TBD | REPORTING APIs and aggregation services |
| FE | TBD | TBD | Admin Portal reporting screens and Dashboard widgets |
| DB | TBD | TBD | Reporting tables/materialized views, migrations |
| QA | TBD | TBD | Test design and automation for REPORTING |
| DevOps | TBD | TBD | Monitoring, alerts, capacity planning for REPORTING |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
