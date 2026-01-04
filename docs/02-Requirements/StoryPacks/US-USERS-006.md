# StoryPack — US-USERS-006: Tra cứu danh sách User theo nhiều tiêu chí

## 6) API mapping (contract-first)
> Owner: Tech Lead (USERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/users` — tag: USERS — schemas: `PagedResult` + `UserSummary[]` (response `items`)

**Error codes used (planned)**
- `USERS_SEARCH_FILTER_INVALID`

**Idempotency**
- Required? No. GET is safe/idempotent by HTTP semantics.

**Pagination/filtering**
- Standard `page` / `pageSize` query parameters as per API-Conventions.
- Optional filters: `companyId`, `pickupPointId`, `tier`, `status`, `employeeCode`, `phone` as query parameters.

