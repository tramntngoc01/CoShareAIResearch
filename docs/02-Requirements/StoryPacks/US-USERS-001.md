# StoryPack — US-USERS-001: Import danh sách nhân sự từ Công ty

## 6) API mapping (contract-first)
> Owner: Tech Lead (USERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/users/import` — tag: USERS — schemas: `UsersImportJobCreated` (response)
- `GET /api/v1/users/import/{importId}` — tag: USERS — schemas: `UsersImportJobResult` (response)

**Error codes used (planned)**
- `USERS_IMPORT_STRUCTURE_INVALID` — file missing required columns or invalid structure.
- `USERS_IMPORT_TOO_LARGE` — file exceeds configured size/row limits.

**Idempotency**
- Required? No explicit idempotency key. Each accepted upload is a new import job, identified by `importId`.

**Pagination/filtering**
- Not applicable for these endpoints. Import job list/reporting (if needed) will be handled in ADMIN/REPORTING.

