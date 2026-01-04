
# DB — CATALOG: Catalog / Content

## Overview
- Manage products, categories, supplier linkage, batches (QC), and available stock for integration with ORDERS/PAYMENTS/REPORTING.
- No PII; only product and supplier-related data.

All tables follow DB-Design-Rules:
- Primary key: `id bigserial primary key`.
- Common audit columns:
	- `is_deleted boolean not null default false`
	- `created_at timestamptz not null`
	- `created_by bigint null`
	- `updated_at timestamptz null`
	- `updated_by bigint null`

## Entities
- `Product` — master data for products.
- `Category` — product categories for filtering and commissions.
- `Supplier` (lightweight) — reference to source vendor for products and batches.
- `Batch` — inbound lot from supplier, single category per BR-CATALOG-001.
- `BatchItem` — product quantities within a batch.
- `Inventory` — aggregated available stock per product (and optionally by dimension later).

## Tables

### table: catalog_supplier
- `id bigserial primary key`
- `supplier_code varchar(64) not null`
- `name varchar(255) not null`
- `description varchar(500) null`
- `is_active boolean not null default true`
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: catalog_category
- `id bigserial primary key`
- `category_code varchar(64) not null`
- `name varchar(255) not null`
- `description varchar(500) null`
- `is_active boolean not null default true`
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: catalog_product
- `id bigserial primary key`
- `product_code varchar(64) not null`
- `bar_code varchar(64) null`
- `name varchar(255) not null`
- `description text null`
- `category_id bigint not null` — FK → `catalog_category.id`.
- `supplier_id bigint not null` — FK → `catalog_supplier.id`.
- `unit varchar(50) not null`
- `unit_price numeric(18,2) not null`
- `status varchar(32) not null` — e.g. `Active`, `Inactive`.
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: catalog_batch
- `id bigserial primary key`
- `supplier_id bigint not null` — FK → `catalog_supplier.id`.
- `category_id bigint not null` — FK → `catalog_category.id`.
- `status varchar(32) not null` — e.g. `PENDING_REVIEW`, `APPROVED`, `REJECTED`.
- `import_date date not null`
- `documents jsonb null` — URIs/keys to required documents.
- `images jsonb null` — URIs/keys to batch-related images.
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: catalog_batch_item
- `id bigserial primary key`
- `batch_id bigint not null` — FK → `catalog_batch.id`.
- `product_id bigint not null` — FK → `catalog_product.id`.
- `quantity_imported numeric(18,2) not null`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: catalog_inventory
- `id bigserial primary key`
- `product_id bigint not null` — FK → `catalog_product.id`.
- `available_quantity numeric(18,2) not null default 0`
- `last_calculated_at timestamptz not null`
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

## Relationships
- `catalog_product.category_id` → `catalog_category.id`.
- `catalog_product.supplier_id` → `catalog_supplier.id`.
- `catalog_batch.supplier_id` → `catalog_supplier.id`.
- `catalog_batch.category_id` → `catalog_category.id`.
- `catalog_batch_item.batch_id` → `catalog_batch.id`.
- `catalog_batch_item.product_id` → `catalog_product.id`.
- `catalog_inventory.product_id` → `catalog_product.id`.

## Indexes
- `ux_catalog_supplier__supplier_code` on (`supplier_code`) unique.
- `ux_catalog_category__category_code` on (`category_code`) unique.
- `ux_catalog_product__product_code` on (`product_code`) unique.
- `ux_catalog_product__bar_code` on (`bar_code`) unique where `bar_code` is not null (implementation detail in migration).
- `ix_catalog_product__category` on (`category_id`).
- `ix_catalog_product__supplier` on (`supplier_id`).
- `ix_catalog_batch__status` on (`status`).
- `ix_catalog_batch__supplier_status` on (`supplier_id`, `status`).
- `ix_catalog_batch_item__batch` on (`batch_id`).
- `ix_catalog_inventory__product` on (`product_id`) unique.

## Migration notes
- Initial migration:
	- Create `catalog_supplier`, `catalog_category`, `catalog_product`, `catalog_batch`, `catalog_batch_item`, `catalog_inventory` with FKs and indexes.
	- Seed minimal reference data if needed for demo (optional; not required for logic).
- Rollback:
	- Drop tables in reverse order of dependencies: `catalog_inventory`, `catalog_batch_item`, `catalog_batch`, `catalog_product`, `catalog_category`, `catalog_supplier`.

## Notes
- Retention / archival:
	- Batch and batch item records should be retained for financial and operational audits; do not delete, only mark `is_deleted` if needed.
	- Inventory rows may be recomputed; historical snapshots, if required, should be handled via reporting schemas.
- PII fields (if any):
	- None; module stores only product/supplier metadata.
- Open Questions (CATALOG–DB):
	- Q-DB-CATALOG-01: Should inventory be stored only as a derived view/materialized view instead of a table? Recommended default: start with `catalog_inventory` table for simplicity and performance.
	- Q-DB-CATALOG-02: Do we need multi-dimensional inventory (per company/pickup point) in MVP or only global per product as assumed?
