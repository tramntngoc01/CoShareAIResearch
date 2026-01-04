# StoryPack — US-CATALOG-004: Hiển thị danh sách sản phẩm cho End User

## 6) API mapping (contract-first)
> Owner: Tech Lead (CATALOG). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/catalog/products` — tag: CATALOG — schemas: `PagedResult` + `CatalogProductPublicSummary[]` (response `items`)
- `GET /api/v1/catalog/products/{id}` — tag: CATALOG — schemas: `CatalogProductPublicDetail` (response)

**Error codes used (planned)**
- `CATALOG_PRODUCT_NOT_FOUND`

**Idempotency**
- Required? No. GET is safe/idempotent by HTTP semantics.

**Pagination/filtering**
- `GET /api/v1/catalog/products` uses standard `page` / `pageSize` with filters: `categoryId`, `searchText`.

