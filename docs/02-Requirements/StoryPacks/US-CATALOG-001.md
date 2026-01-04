# StoryPack — US-CATALOG-001: Quản lý danh sách Sản phẩm (Admin)

## 6) API mapping (contract-first)
> Owner: Tech Lead (CATALOG). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/catalog/admin/products` — tag: CATALOG — schemas: `PagedResult` + `CatalogProductAdminSummary[]` (response `items`)
- `POST /api/v1/catalog/admin/products` — tag: CATALOG — schemas: `CatalogProductAdminRequest` (request), `CatalogProductAdminDetail` (response)
- `GET /api/v1/catalog/admin/products/{id}` — tag: CATALOG — schemas: `CatalogProductAdminDetail` (response)
- `PATCH /api/v1/catalog/admin/products/{id}` — tag: CATALOG — schemas: `CatalogProductAdminUpdateRequest` (request), `CatalogProductAdminDetail` (response)

**Error codes used (planned)**
- `CATALOG_PRODUCT_CODE_DUPLICATE`
- `CATALOG_PRODUCT_VALIDATION_FAILED`

**Idempotency**
- Required? No explicit idempotency header; standard admin CRUD semantics, last write wins.

**Pagination/filtering**
- `GET /api/v1/catalog/admin/products` uses standard `page` / `pageSize` with filters: `categoryId`, `supplierId`, `status`, `searchText`.

