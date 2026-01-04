
# Implementation Plan — CATALOG: Catalog / Content

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead — CATALOG
- Reviewers: BA / Tech Lead / QA / DevOps (if needed)
- Target sprint/release: MVP P0 — CATALOG

---

## 1) Scope
### In scope
- CATALOG-SCOPE-01: Implement CATALOG backend APIs for product management, category management, batch creation/QC, end-user product listing, and available stock for ORDERS (US-CATALOG-001..005).
- CATALOG-SCOPE-02: Implement PostgreSQL schema for CATALOG (`catalog_supplier`, `catalog_category`, `catalog_product`, `catalog_batch`, `catalog_batch_item`, `catalog_inventory`) and expose stable contracts for ORDERS/PAYMENTS/REPORTING.

### Out of scope
- CATALOG-OOS-01: Advanced marketing content (banners, promos) and complex multi-warehouse logistics.
- CATALOG-OOS-02: Detailed commission calculation per order (belongs to PAYMENTS/REPORTING).

### Success criteria
- SC1: All P0 CATALOG APIs are implemented per OpenAPI, covered by integration tests, and consumed by Admin Portal and ORDERS.
- SC2: Inventory values returned by CATALOG never go negative and ORDERS can rely on them to enforce order placement rules.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-CATALOG.md`
- Stories: `docs/02-Requirements/Stories/Stories-CATALOG.md`
- OpenAPI paths (CATALOG):
  - GET /api/v1/catalog/admin/products
  - POST /api/v1/catalog/admin/products
  - GET /api/v1/catalog/admin/products/{id}
  - PATCH /api/v1/catalog/admin/products/{id}
  - GET /api/v1/catalog/categories
  - POST /api/v1/catalog/categories
  - PATCH /api/v1/catalog/categories/{id}
  - GET /api/v1/catalog/batches
  - POST /api/v1/catalog/batches
  - GET /api/v1/catalog/batches/{id}
  - PATCH /api/v1/catalog/batches/{id}/status
  - GET /api/v1/catalog/products
  - GET /api/v1/catalog/products/{id}
  - GET /api/v1/catalog/products/{id}/stock
- DB doc: `docs/03-Design/DB/DB-CATALOG.md`
- Business rules (from Business-Rules.md & SRS-CATALOG):
  - BR-CATALOG-001: One category per batch in MVP.
  - BR-CATALOG-002: No hard deletes for product/category.
  - BR-CATALOG-003: Only products from approved batches can be sold.
  - BR-CATALOG-004: Stock must never go negative.

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - `CatalogAdminProductsController` for `/api/v1/catalog/admin/products*`.
  - `CatalogCategoriesController` for `/api/v1/catalog/categories*`.
  - `CatalogBatchesController` for `/api/v1/catalog/batches*`.
  - `CatalogProductsController` for `/api/v1/catalog/products*` (End User view + stock endpoint).
- Service layer:
  - `ICatalogProductService` for product admin and public operations.
  - `ICatalogCategoryService` for category CRUD.
  - `ICatalogBatchService` for batch lifecycle and stock impact.
  - `ICatalogInventoryService` for available stock calculation and exposure to ORDERS.
- Validation:
  - Validate uniqueness constraints at service layer with mapping to stable error codes (e.g., `CATALOG_PRODUCT_CODE_DUPLICATE`).
  - Enforce BR-CATALOG-001 (single category per batch) and quantity > 0 for batch items.
- Error codes:
  - Define CATALOG_* codes in Error-Conventions (e.g., `CATALOG_PRODUCT_CODE_DUPLICATE`, `CATALOG_CATEGORY_CODE_DUPLICATE`, `CATALOG_BATCH_INVALID_STATE`, `CATALOG_BATCH_MISSING_DOCUMENTS`).
- AuthZ policies:
  - Restrict `/api/v1/catalog/admin/*` to Admin/QC roles.
  - End User listing `/api/v1/catalog/products*` requires authenticated End User (recommended default; Open Question in Threat-Model).
- Logging/CorrelationId:
  - Ensure all endpoints read/generate `X-Correlation-Id` and pass to logs.
  - Audit product/category/batch state changes via ADMIN audit log patterns.

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Implement schema in DB-CATALOG with suppliers, categories, products, batches, batch items, inventory.
- Migration steps:
  - Add EF Core migration for CATALOG module; include unique indexes and FKs as specified.
- Rollback steps:
  - Document safe rollback by dropping new tables if no dependent data is yet in use; otherwise prefer forward-only migrations.
- Data seeding (if any):
  - Optional seeding for demo suppliers/categories; no mandatory seed for core logic.

### Frontend (React Web) / Mobile (if any)
- Screens/components:
  - Admin Portal: product list/detail, category list/detail, batch list/detail/QC workflows.
  - End User Portal: product list + detail views.
- State management:
  - Use shared pagination models and filter patterns from API-Conventions.
- Form validation + error mapping:
  - Map CATALOG_* error codes to localized messages (duplicate codes, invalid state, missing documents, etc.).
- UI evidence expectations:
  - Capture screenshots/recordings of main flows for UAT.

### Integrations / Jobs (if any)
- External APIs:
  - None in MVP; suppliers are configured internally.
- Webhooks:
  - None.
- Background jobs (idempotency, retries):
  - Optional: periodic job to reconcile `catalog_inventory` from batches and ORDERS snapshots if needed.

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - CATALOG paths and schemas have been added to `openapi.yaml` for admin products/categories/batches, end-user listing, and stock.
  - Define CATALOG_* error codes in Error-Conventions with clear mapping to HTTP status codes.
- Backward compatibility considerations:
  - New module for MVP; no legacy contracts to preserve. Future changes must be additive where possible.
- Idempotency requirements:
  - Product/category/batch mutations are standard admin operations; no explicit Idempotency-Key header required in MVP.
- Pagination/filtering rules:
  - `/api/v1/catalog/admin/products`, `/api/v1/catalog/batches`, and `/api/v1/catalog/products` use standard offset pagination (`page`/`pageSize`) and simple filters (category, supplier, status, searchText).

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - Admin, QC, End User, (future) T1/T2 readers.
- Permissions matrix (action -> role):
  - Manage products/categories/batches: Admin/QC.
  - Approve/reject batches: QC (or specific role subset).
  - View product catalog (end user): End User; T2/T1 may have read-only for advisory.
- Sensitive operations requiring audit trail:
  - Price changes.
  - Category changes affecting commissions.
  - Batch status transitions affecting stock.

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-CATALOG-001: Product/category validation and uniqueness rules.
- UT-CATALOG-002: Batch status state machine and BR-CATALOG-001 enforcement (single category).
- UT-CATALOG-003: Inventory aggregation logic ensuring no negative stock.

### Integration tests
- IT-CATALOG-001: Admin product/category CRUD via `/api/v1/catalog/admin/products*` and `/api/v1/catalog/categories*`.
- IT-CATALOG-002: Batch create + approve/reject flow updating `catalog_inventory` correctly.
- IT-CATALOG-003: End-user product listing honors approved batches and positive stock.

### E2E / Smoke (staging)
- E2E-CATALOG-001: Admin creates products, categories, batches → End User sees products and can start order flow.
- E2E-CATALOG-002: Stock updates after batch approval and order completion/rollback (with ORDERS in staging).

### Security sanity
- SEC-CATALOG-001 (authz negative): Verify unauthenticated/unauthorized principals cannot access `/api/v1/catalog/admin/*`.
- SEC-CATALOG-002 (input validation/injection basic): Validate filters, searchText and batch payloads are safe from injection.

---

## 7) Observability Plan
- Logs (no PII):
  - Log product/category/batch changes with IDs and correlationId; no sensitive data.
- Metrics (key counters/timers):
  - Number of approved/rejected batches per period.
  - Product listing latency and error rates.
- Alerts (if needed):
  - High rate of batch rejections.
  - Frequent failures in inventory updates.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Optional flag for CATALOG UI sections in Admin Portal.
- Migration order:
  - Deploy CATALOG DB migrations.
  - Deploy backend services with new endpoints.
  - Deploy frontend changes.
- Staging verification steps:
  - Run IT/E2E tests for CATALOG.
  - Perform manual sanity checks on product listing and inventory behavior.
- Production checklist items:
  - Confirm roles/permissions are correctly configured for Admin/QC.
  - Ensure monitoring dashboards for CATALOG APIs are live.
- Rollback procedure:
  - Roll back backend deployment and disable CATALOG UI; keep DB schema if already in use or carefully coordinate removal.

---

## 9) Risks & Mitigations
- R1: Misconfigured uniqueness rules for product/category codes.
  - M1: Validate scenarios with sample data and confirm with BA/customer before go-live; add tests.
- R2: Stock inconsistencies with ORDERS due to race conditions.
  - M2: Define clear contract responsibilities between CATALOG and ORDERS; include joint integration tests.

---

## 10) Estimates & Owners
| Work item | Owner          | Estimate | Notes |
|----------|----------------|----------|------|
| BE       | CATALOG BE dev | TBD      | Controllers, services, tests |
| FE       | CATALOG FE dev | TBD      | Admin & End User catalog UIs |
| DB       | DB engineer    | TBD      | Migrations & tuning |
| QA       | QA engineer    | TBD      | Test design & execution |
| DevOps   | DevOps         | TBD      | CI/CD, monitoring |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
