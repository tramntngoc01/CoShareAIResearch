# StoryPack — US-USERS-002: Ánh xạ và quản lý Ref Tier (cấu trúc Tầng 1/2/3)

## 6) API mapping (contract-first)
> Owner: Tech Lead (USERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/users/{id}/ref-tier` — tag: USERS — schemas: `UserRefTierInfo` (response)
- `PUT /api/v1/users/{id}/ref-tier` — tag: USERS — schemas: `UserRefTierChangeRequest` (request), `UserRefTierInfo` (response)

**Error codes used (planned)**
- `USERS_USER_NOT_FOUND`
- `USERS_REF_TIER_INVALID_PARENT`

**Idempotency**
- Required? No explicit idempotency key; operation is idempotent by design (no-op if requested parent is already applied).

**Pagination/filtering**
- Not applicable. Any network/tree views are implemented via separate read models (TBD) or consuming modules.

