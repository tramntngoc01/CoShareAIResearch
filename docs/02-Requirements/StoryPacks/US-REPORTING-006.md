# StoryPack — US-REPORTING-006: Cung cấp số liệu cho Dashboard Admin

## 0) Metadata
- Story ID: **US-REPORTING-006**
- Module: **REPORTING**
- Priority: P0
- Status: Ready
- Owner (BA):
- Reviewers: Tech Lead / QA
- API mapping owner: Tech Lead
- Target sprint/release: MVP REPORTING
- Last updated: 2026-01-04

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/reporting/dashboard/orders-today` — tag: REPORTING — schemas: response: `ReportingDashboardOrdersToday`.
- `GET /api/v1/reporting/dashboard/cod-open` — tag: REPORTING — schemas: response: `ReportingDashboardCodOpen`.
- `GET /api/v1/reporting/dashboard/debt-overdue` — tag: REPORTING — schemas: response: `ReportingDashboardDebtOverdue`.
- `GET /api/v1/reporting/dashboard/top-category` — tag: REPORTING — schemas: response: `ReportingDashboardTopCategory`.

**Error codes used**
- `REPORTING_FORBIDDEN_SCOPE`

**Idempotency**
- Required? No
- Key header: N/A
- Conflict behavior: N/A (read-only)

**Pagination/filtering**
- No pagination; widgets use simple GETs.
- Optional filter: `companyId` for company-scoped dashboards.
