# StoryPack — US-USERS-003: Quản lý hồ sơ KYC User (Admin)

## 6) API mapping (contract-first)
> Owner: Tech Lead (USERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/users/{id}` — tag: USERS — schemas: `UserDetail` (response)
- `PATCH /api/v1/users/{id}/kyc` — tag: USERS — schemas: `UserKycUpdateRequest` (request), `UserDetail` (response)

**Error codes used (planned)**
- `USERS_USER_NOT_FOUND`
- `USERS_KYC_FIELD_NOT_ALLOWED`
- `USERS_KYC_VALIDATION_FAILED`

**Idempotency**
- Required? No explicit idempotency key; operation is idempotent by latest-write-wins semantics per current version.

**Pagination/filtering**
- Not applicable for the detail/update endpoints. Listing is covered by US-USERS-006.

