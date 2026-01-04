# StoryPack — US-USERS-005: Quản lý trạng thái User (Active/Inactive/Locked)

## 6) API mapping (contract-first)
> Owner: Tech Lead (USERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/users/{id}` — tag: USERS — schemas: `UserDetail` (response)
- `PATCH /api/v1/users/{id}/status` — tag: USERS — schemas: `UserStatusChangeRequest` (request), `UserDetail` (response)

**Error codes used (planned)**
- `USERS_USER_NOT_FOUND`
- `USERS_STATUS_INVALID_TRANSITION`

**Idempotency**
- Required? No explicit idempotency key; changing to the current status is treated as a no-op.

**Pagination/filtering**
- Not directly applicable; listing/filtering of users by status is handled in US-USERS-006.

