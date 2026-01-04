# StoryPack — US-REPORTING-005: Xem báo cáo công nợ trả chậm theo Công ty

## 0) Metadata
- Story ID: **US-REPORTING-005**
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
- `GET /api/v1/reporting/debts/companies` — tag: REPORTING — schemas: request via query params; response items: `ReportingDebtCompanyRow` wrapped in `PagedResult`.
- `GET /api/v1/reporting/debts/companies/{companyId}/users` — tag: REPORTING — schemas: request via path+query params; response items: `ReportingDebtUserRow` wrapped in `PagedResult`.

**Error codes used**
- `REPORTING_INVALID_DATE_RANGE`
- `REPORTING_RANGE_TOO_WIDE`
- `REPORTING_FORBIDDEN_SCOPE`

**Idempotency**
- Required? No
- Key header: N/A
- Conflict behavior: N/A (read-only)

**Pagination/filtering**
- Pagination: `page`/`pageSize`.
- Filters: `asOfDate` (required) and optional `companyId` for company-level; `asOfDate` (required) and `companyId` path for user-level.
