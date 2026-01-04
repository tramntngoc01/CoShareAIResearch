# StoryPack — US-CATALOG-005: Tính và cung cấp tồn kho khả dụng cho ORDERS

## 6) API mapping (contract-first)
> Owner: Tech Lead (CATALOG). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/catalog/products/{id}/stock` — tag: CATALOG — schemas: `CatalogAvailableStock` (response)

**Error codes used (planned)**
- `CATALOG_PRODUCT_NOT_FOUND`

**Idempotency**
- Required? No. GET is safe/idempotent; ORDERS handles reservation semantics separately.

**Pagination/filtering**
- Not applicable for this single-product stock endpoint. Bulk stock queries (if needed) would be a separate design.

