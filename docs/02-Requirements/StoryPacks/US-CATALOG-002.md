# StoryPack — US-CATALOG-002: Quản lý Ngành hàng (Admin)

## 6) API mapping (contract-first)
> Owner: Tech Lead (CATALOG). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/catalog/categories` — tag: CATALOG — schemas: `CatalogCategory[]` (response)
- `POST /api/v1/catalog/categories` — tag: CATALOG — schemas: `CatalogCategoryRequest` (request), `CatalogCategory` (response)
- `PATCH /api/v1/catalog/categories/{id}` — tag: CATALOG — schemas: `CatalogCategoryUpdateRequest` (request), `CatalogCategory` (response)

**Error codes used (planned)**
- `CATALOG_CATEGORY_CODE_DUPLICATE`
- `CATALOG_CATEGORY_IN_USE`

**Idempotency**
- Required? No explicit idempotency header; standard admin CRUD semantics.

**Pagination/filtering**
- Not paged in MVP; list size expected to be small. Filtering by active/inactive can be added later if needed.

