# StoryPack — US-CATALOG-003: Tạo và kiểm duyệt Lô hàng từ NCC

## 6) API mapping (contract-first)
> Owner: Tech Lead (CATALOG). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/catalog/batches` — tag: CATALOG — schemas: `PagedResult` + `CatalogBatchSummary[]` (response `items`)
- `POST /api/v1/catalog/batches` — tag: CATALOG — schemas: `CatalogBatchCreateRequest` (request), `CatalogBatchDetail` (response)
- `GET /api/v1/catalog/batches/{id}` — tag: CATALOG — schemas: `CatalogBatchDetail` (response)
- `PATCH /api/v1/catalog/batches/{id}/status` — tag: CATALOG — schemas: `CatalogBatchStatusChangeRequest` (request), `CatalogBatchDetail` (response)

**Error codes used (planned)**
- `CATALOG_BATCH_INVALID_CATEGORY_MIX`
- `CATALOG_BATCH_MISSING_DOCUMENTS`
- `CATALOG_BATCH_INVALID_STATE`

**Idempotency**
- Required? No explicit idempotency header; batch creation and status changes are admin/QC operations with clear state machine rules.

**Pagination/filtering**
- `GET /api/v1/catalog/batches` uses standard `page` / `pageSize` with optional `status` and `supplierId` filters.

