
# Test Cases — CATALOG: Catalog / Content

## 1) Scope & assumptions
- Coverage: US-CATALOG-001 → US-CATALOG-005 (all P0). No ScreenSpec IDs exist for CATALOG yet (`UI-Index` lists none), so Screen IDs are marked `TBD`.
- Based on SRS-CATALOG, Stories-CATALOG, StoryPacks US-CATALOG-001..005, OpenAPI paths under `/api/v1/catalog/**`, and Error-Conventions.
- Assumptions (per SRS): single-category per batch (BR-CATALOG-001); soft-delete/inactive instead of hard delete; stock must never be negative.

## 2) Test data set (fake)
- Category: `{categoryId: 10, categoryCode: "FOOD-INSTANT", name: "Mì ăn liền", isActive: true}`
- Secondary category (for negative mix): `{categoryId: 20, categoryCode: "DRINK-WATER", name: "Nước uống đóng chai", isActive: true}`
- Supplier: `{supplierId: 300, supplierCode: "NCC_TP_A"}`
- Products:
  - P1 `{productId: 1001, productCode: "SP-MI-TOM-001", barCode: "8931234567890", unit: "Gói", unitPrice: 6500, isActive: true}`
  - P2 `{productId: 1002, productCode: "SP-MI-TOM-002", unit: "Thùng", unitPrice: 120000, isActive: true}`
- P3 (other category) `{productId: 2001, productCode: "NUOC-500", categoryId: 20, unit: "Chai", unitPrice: 5000, isActive: true}`
- Batch (approved): `{batchId: 5001, categoryId: 10, supplierId: 300, status: "APPROVED", importDate: "<YYYY-MM-DD - use execution date>", items: [{productId:1001, quantityImported:1000},{productId:1002, quantityImported:800}], documents:["https://fakehost/bill_0123.pdf"], images:["https://fakehost/lot_5001_1.jpg"]}`
- Batch (rejected): `{batchId: 5002, categoryId: 10, status: "REJECTED"}` for negative flows.

## 3) E2E testcases (P0 then P1)

### E2E-CATALOG-001 — Admin creates sellable products visible to End User (happy path)
- Priority: P0  
- Story IDs: US-CATALOG-001, US-CATALOG-002, US-CATALOG-003, US-CATALOG-004, US-CATALOG-005  
- Screen IDs: TBD (Admin Product List, Batch Detail, End User Product List)  
- Preconditions: Admin and QC tokens available; supplier exists; category not yet present in system.  
- Test data: Category `FOOD-INSTANT`; Products P1, P2; Batch 5001 (see dataset).  
- Steps:  
  1. Admin `POST /api/v1/catalog/categories` with `FOOD-INSTANT`.  
  2. Admin `POST /api/v1/catalog/admin/products` for P1 then P2, linking categoryId=10, supplierId=300.  
  3. QC `POST /api/v1/catalog/batches` with items P1/P2 under categoryId=10, status defaults PENDING_REVIEW.  
  4. QC `PATCH /api/v1/catalog/batches/{id}/status` to `APPROVED`.  
  5. End User `GET /api/v1/catalog/products?categoryId=10&page=1&pageSize=20`.  
  6. End User `GET /api/v1/catalog/products/{productId}/stock` for P1 and P2.  
- **Expected results:**  
  - Category created with id returned; correlationId present.  
  - Products created return `CatalogProductAdminDetail`; codes unique; `isActive=true`.  
  - Batch created with items only from category 10; status is `PENDING_REVIEW`.  
  - Status change returns `APPROVED`; available quantities increased (>= import quantities).  
  - Public list returns paged payload `{items, page, totalItems, totalPages}` containing P1 & P2 with `inStock=true`.  
  - Stock endpoint returns `availableQuantity` > 0 for each product.  
- **Evidence:** API responses + correlationId logs.  
- **Notes:** Establishes end-to-end readiness for ORDERS to consume stock.

### E2E-CATALOG-002 — Rejected batch never surfaces to End User
- Priority: P0  
- Story IDs: US-CATALOG-003, US-CATALOG-004, US-CATALOG-005  
- Screen IDs: TBD (Batch Detail, End User Product List)  
- Preconditions: Products P1/P2 exist and linked to category 10; batch 5002 created with status `PENDING_REVIEW`.  
- Test data: Batch 5002 with items P1/P2, missing QC approval.  
- Steps:  
  1. QC `PATCH /api/v1/catalog/batches/{id}/status` to `REJECTED`.  
  2. End User `GET /api/v1/catalog/products?categoryId=10`.  
  3. End User `GET /api/v1/catalog/products/{productId}/stock` for P1.  
- **Expected results:**  
  - Status change succeeds with `REJECTED`.  
  - Public list does **not** include products solely backed by rejected batch.  
  - Stock endpoint returns `availableQuantity` unchanged/0 (no increment from rejected batch).  
- **Evidence:** API responses + correlationId logs.  
- **Notes:** Confirms BR-CATALOG-003 and BR-CATALOG-004.

### E2E-CATALOG-003 — Product deactivation hides from public catalog
- Priority: P1  
- Story IDs: US-CATALOG-001, US-CATALOG-004  
- Screen IDs: TBD (Admin Product Detail, End User Product List)  
- Preconditions: Product P1 exists, active, has approved stock from batch 5001.  
- Test data: P1 `isActive` toggle.  
- Steps:  
  1. Admin `PATCH /api/v1/catalog/admin/products/{id}` set `isActive=false`.  
  2. Admin `GET /api/v1/catalog/admin/products/{id}` to confirm status.  
  3. End User `GET /api/v1/catalog/products?searchText=MI%20TOM`.  
- **Expected results:**  
  - Product detail reflects `isActive=false`.  
  - Public list excludes P1 or marks `inStock=false` per policy; no ability to buy.  
- **Evidence:** API responses; correlationId.  
- **Notes:** Covers FR-CATALOG-001/004 interplay.

## 4) Integration/API testcases

### IT-CATALOG-001 — Create product with minimum valid fields (admin)
- Priority: P0  
- Story IDs: US-CATALOG-001  
- Screen IDs: TBD (Admin Product Form)  
- Preconditions: Category 10 and Supplier 300 exist.  
- Test data: `{productCode:"SP-MI-TOM-003", name:"Mì cay cấp độ 1", categoryId:10, supplierId:300, unit:"Gói", unitPrice:7500, isActive:true}`  
- Steps: `POST /api/v1/catalog/admin/products` with payload.  
- Expected: 201 with `CatalogProductAdminDetail`, id generated, status Active, correlationId header.  
- Evidence: Response body + headers.  
- Notes: Baseline for subsequent negative cases.

### IT-CATALOG-002 — Missing mandatory field blocked (product creation)
- Priority: P0  
- Story IDs: US-CATALOG-001  
- Screen IDs: TBD  
- Preconditions: Category 10, Supplier 300 exist.  
- Test data: Same as IT-CATALOG-001 but omit `unit` or `unitPrice`.  
- Steps: `POST /api/v1/catalog/admin/products` without mandatory field.  
- Expected: 400 `DefaultError`, code `CATALOG_PRODUCT_VALIDATION_FAILED`, correlationId present, no product created.  
- Evidence: Error payload + correlationId.  
- Notes: Negative/validation.

### IT-CATALOG-003 — Duplicate product code rejected
- Priority: P0  
- Story IDs: US-CATALOG-001  
- Screen IDs: TBD  
- Preconditions: Product with `productCode "SP-MI-TOM-003"` already exists.  
- Test data: Duplicate payload.  
- Steps: `POST /api/v1/catalog/admin/products` with same code.  
- Expected: 409 `DefaultError`, code `CATALOG_PRODUCT_CODE_DUPLICATE`; existing product unchanged.  
- Evidence: Error payload; DB/API listing unchanged.  
- Notes: Negative/uniqueness.

### IT-CATALOG-004 — Create category success
- Priority: P0  
- Story IDs: US-CATALOG-002  
- Screen IDs: TBD (Category Config)  
- Preconditions: Category code not used.  
- Test data: `{categoryCode:"DRINK-WATER", name:"Nước uống", description:"Nước đóng chai", isActive:true}`  
- Steps: `POST /api/v1/catalog/categories`.  
- Expected: 201 `CatalogCategory` with `isActive=true`.  
- Evidence: Response + correlationId.  
- Notes: Baseline for category negative cases.

### IT-CATALOG-005 — Duplicate category code rejected
- Priority: P0  
- Story IDs: US-CATALOG-002  
- Screen IDs: TBD  
- Preconditions: Category `DRINK-WATER` exists.  
- Test data: Same payload.  
- Steps: `POST /api/v1/catalog/categories` duplicate code.  
- Expected: 409 `DefaultError`, code `CATALOG_CATEGORY_CODE_DUPLICATE`; existing category intact.  
- Evidence: Error payload.  
- Notes: Negative/uniqueness.

### IT-CATALOG-006 — Deactivate category with attached products blocked
- Priority: P0  
- Story IDs: US-CATALOG-002, US-CATALOG-001  
- Screen IDs: TBD  
- Preconditions: Category 10 linked to at least one product.  
- Test data: `PATCH /api/v1/catalog/categories/10` with `{isActive:false}`.  
- Steps: Send patch; then list products to verify links.  
- Expected: Request rejected (409 `DefaultError` such as `CATALOG_CATEGORY_IN_USE`); product-category linkage remains.  
- Evidence: Error payload; product listing still shows category name/id.  
- Notes: Negative; enforces AC3.

### IT-CATALOG-007 — Create batch with single category succeeds
- Priority: P0  
- Story IDs: US-CATALOG-003  
- Screen IDs: TBD (Batch Form)  
- Preconditions: Products P1/P2 active; category 10 exists.  
- Test data: Batch 5001 payload (items only category 10).  
- Steps: `POST /api/v1/catalog/batches`.  
- Expected: 201 `CatalogBatchDetail` with status `PENDING_REVIEW`, items echoed, correlationId present.  
- Evidence: Response body + headers.  
- Notes: Positive baseline.

### IT-CATALOG-008 — Batch with mixed categories rejected
- Priority: P0  
- Story IDs: US-CATALOG-003  
- Screen IDs: TBD  
- Preconditions: Products from different categories available.  
- Test data: Items `[{productId:1001, quantityImported:100}, {productId:2001, quantityImported:50}]` where productId 2001 is P3 (`NUOC-500`, categoryId 20/`DRINK-WATER`).  
- Steps: `POST /api/v1/catalog/batches` mixing categories.  
- Expected: 400/409 `DefaultError`, code `CATALOG_BATCH_INVALID_CATEGORY_MIX`; batch not created.  
- Evidence: Error payload + correlationId.  
- Notes: Negative; BR-CATALOG-001.

### IT-CATALOG-009 — Approve batch without required documents (BLOCKED - requirement TBD)
- Priority: P0  
- Story IDs: US-CATALOG-003  
- Screen IDs: TBD  
- Preconditions: Batch created without documents/images.  
- Test data: `PATCH /api/v1/catalog/batches/{id}/status` with `newStatus:"APPROVED"`.  
- Steps: Attempt status change.  
- Expected: 400 `DefaultError`, code `CATALOG_BATCH_MISSING_DOCUMENTS`; remains `PENDING_REVIEW`.  
- Evidence: Error payload.  
- Notes: **Blocked by requirement** Q-CATALOG-004 (mandatory document list not finalized); execute once rule is confirmed.

### IT-CATALOG-010 — Approve batch updates available stock
- Priority: P0  
- Story IDs: US-CATALOG-003, US-CATALOG-005  
- Screen IDs: TBD  
- Preconditions: Batch 5001 `PENDING_REVIEW`, products active.  
- Test data: `PATCH /api/v1/catalog/batches/{id}/status` to `APPROVED`.  
- Steps:  
  1. Capture current `availableQuantity` for P1 via `GET /api/v1/catalog/products/{id}/stock`.  
  2. Approve batch.  
  3. Re-check stock for P1.  
- Expected: Stock increases by `quantityImported` from batch; status response includes `APPROVED`.  
- Evidence: Stock before/after; correlationId.  
- Notes: Positive/state transition.

### IT-CATALOG-011 — Public list excludes inactive/unapproved products
- Priority: P0  
- Story IDs: US-CATALOG-004  
- Screen IDs: TBD (End User Product List)  
- Preconditions: One product inactive or only in rejected/unapproved batch.  
- Test data: Query `GET /api/v1/catalog/products?categoryId=10&page=1&pageSize=10`.  
- Steps: Call endpoint after toggling product inactive or leaving batch pending.  
- Expected: Response 200 with paged shape; `items` excludes inactive/unapproved products; totalItems reflects visible ones only.  
- Evidence: Response payload.  
- Notes: Negative/visibility.

### IT-CATALOG-012 — Public list returns empty page gracefully
- Priority: P1  
- Story IDs: US-CATALOG-004  
- Screen IDs: TBD  
- Preconditions: Use filter that matches nothing (e.g., `searchText="khongtontai"`).  
- Test data: GET with searchText mismatch.  
- Steps: Call endpoint.  
- Expected: 200 with `{items:[], page, totalItems:0, totalPages:0 or 1}`; no errors.  
- Evidence: Response payload + correlationId.  
- Notes: Edge case/empty dataset.

### IT-CATALOG-013 — Stock endpoint returns 0 when no approved batches
- Priority: P0  
- Story IDs: US-CATALOG-005  
- Screen IDs: N/A  
- Preconditions: Product exists but has no approved batch quantities.  
- Test data: `GET /api/v1/catalog/products/{id}/stock` for such product.  
- Steps: Call endpoint.  
- Expected: 200 with `availableQuantity=0`; never negative.  
- Evidence: Response payload.  
- Notes: Negative/edge ensures BR-CATALOG-004.

### IT-CATALOG-014 — Stock endpoint 404 for unknown product
- Priority: P0  
- Story IDs: US-CATALOG-005  
- Screen IDs: N/A  
- Preconditions: Product ID not in system.  
- Test data: `GET /api/v1/catalog/products/999999/stock`.  
- Steps: Call endpoint.  
- Expected: 404 `DefaultError`, code `CATALOG_PRODUCT_NOT_FOUND`, correlationId present.  
- Evidence: Error payload.  
- Notes: Negative/not found.

## 5) Security sanity testcases

### SEC-CATALOG-001 — Unauthenticated request to admin product list
- Priority: P0  
- Story IDs: US-CATALOG-001  
- Screen IDs: TBD  
- Preconditions: None (no token).  
- Steps: `GET /api/v1/catalog/admin/products` without Authorization header.  
- Expected: 401 `DefaultError`; no data leaked; correlationId present.  
- Evidence: Error payload + headers.  
- Notes: AuthN enforcement.

### SEC-CATALOG-002 — End User token blocked from admin endpoints
- Priority: P0  
- Story IDs: US-CATALOG-001, US-CATALOG-003  
- Screen IDs: TBD  
- Preconditions: Valid End User token (non-admin).  
- Steps: `POST /api/v1/catalog/batches` or `PATCH /api/v1/catalog/admin/products/{id}` using user token.  
- Expected: 403 `DefaultError`; no state change.  
- Evidence: Error payload + correlationId; verify batch/product unchanged.  
- Notes: AuthZ separation.

### SEC-CATALOG-003 — Injection in searchText handled safely
- Priority: P1  
- Story IDs: US-CATALOG-001, US-CATALOG-004  
- Screen IDs: TBD  
- Preconditions: Products exist.  
- Test data (security test-only; send URL-encoded): `searchText=%22%20OR%201%3D1%3B%20DROP%20TABLE%20catalog_product%3B--`  
- Steps: Call `GET /api/v1/catalog/admin/products` and public `GET /api/v1/catalog/products` with the payload URL-encoded as a query parameter.  
- Expected: 200/400 with safe handling; no server error; no unexpected records; correlationId present.  
- Evidence: Responses + logs (no stack trace/PII).  
- Notes: Input sanitization; payload is test-only, must be URL-encoded, and system must treat it strictly as user input (never executed).

### SEC-CATALOG-004 — Expired/invalid token on batch status change
- Priority: P0  
- Story IDs: US-CATALOG-003  
- Screen IDs: TBD  
- Preconditions: Batch exists.  
- Steps: `PATCH /api/v1/catalog/batches/{id}/status` with expired token.  
- Expected: 401 `DefaultError`; status unchanged.  
- Evidence: Error payload; verify batch status via GET.  
- Notes: Session/token handling.

### SEC-CATALOG-005 — Error response hides sensitive data on product not found
- Priority: P1  
- Story IDs: US-CATALOG-004, US-CATALOG-005  
- Screen IDs: TBD  
- Preconditions: Use nonexistent product id.  
- Steps: `GET /api/v1/catalog/products/999999` and `/products/999999/stock`.  
- Expected: 404 `DefaultError` with generic message, no PII/internal paths; correlationId present.  
- Evidence: Error payload + logs.  
- Notes: Sensitive data exposure check.

## 6) Regression Suite (run each release)
- E2E-CATALOG-001, E2E-CATALOG-002  
- IT-CATALOG-001, IT-CATALOG-003, IT-CATALOG-006, IT-CATALOG-008, IT-CATALOG-010, IT-CATALOG-011, IT-CATALOG-014  
- SEC-CATALOG-001, SEC-CATALOG-002, SEC-CATALOG-004

## 7) Open Questions / blockers
- Q-CATALOG-001: What constitutes the unique product identifier (product_code vs barcode vs supplier+code)? Impacts IT-CATALOG-003 scope.  
- Q-CATALOG-002: Public display rule for out-of-stock products (hide vs show with label) affects E2E-CATALOG-003 expected UI state.  
- Q-CATALOG-003: Whether stock is per pickup point/company or global; affects IT-CATALOG-013 data setup.  
- Q-CATALOG-004: Mandatory document/image list for batch approval; IT-CATALOG-009 **blocked** until finalized.  
- Q-CATALOG-005: Whether batch category can change post-approval and how stock adjusts; may add more negative state-change tests later.  
