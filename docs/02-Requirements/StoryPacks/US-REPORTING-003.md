# StoryPack — US-REPORTING-003: Xem báo cáo hoa hồng theo Tầng 2

## 0) Metadata
- Story ID: **US-REPORTING-003**
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
- `GET /api/v1/reporting/commissions/t2` — tag: REPORTING — schemas: request via query params; response items: `ReportingCommissionT2Row` wrapped in `PagedResult`.

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
- Filters: `periodStart`, `periodEnd` (required), optional `companyId`, `userId`.
