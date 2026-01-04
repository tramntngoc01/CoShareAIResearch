
# DB — REPORTING: Reporting & Exports

Reporting is primarily a read/aggregation module over ORDERS, PAYMENTS, USERS, CATALOG, and ADMIN. The schema below defines logical tables/materialized views used to support performant reporting and dashboard queries.

## 1) Tables

### 1.1 `reporting_sales_summary`
- Purpose: Pre-aggregated sales metrics per period, company, optional pickup point and category.
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `period_start date NOT NULL`
	- `period_end date NOT NULL`
	- `company_id BIGINT NOT NULL` → FK to `admin_company.id`
	- `pickup_point_id BIGINT NULL` → FK to `admin_pickup_point.id`
	- `category_id BIGINT NULL` → FK to `catalog_category.id`
	- `total_orders bigint NOT NULL`
	- `total_revenue numeric(18,2) NOT NULL`
	- `total_items bigint NOT NULL`
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

### 1.2 `reporting_commission_t2`
- Purpose: Aggregated commission metrics for Tier 2 users per period.
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `period_start date NOT NULL`
	- `period_end date NOT NULL`
	- `user_id BIGINT NOT NULL` → FK to `users_user.id`
	- `company_id BIGINT NULL` → FK to `admin_company.id`
	- `total_revenue numeric(18,2) NOT NULL`
	- `commission_rate numeric(9,4) NULL` -- effective blended rate for the period
	- `commission_amount numeric(18,2) NOT NULL`
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

### 1.3 `reporting_cod_shift`
- Purpose: COD & reconciliation metrics per payment shift, aligned with PAYMENTS shift/receipt data.
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `shift_id BIGINT NOT NULL` → FK to `payments_shift.id`
	- `company_id BIGINT NULL` → FK to `admin_company.id`
	- `shipper_user_id BIGINT NOT NULL` → FK to `users_user.id`
	- `expected_cod numeric(18,2) NOT NULL`
	- `collected_cod numeric(18,2) NOT NULL`
	- `difference numeric(18,2) NOT NULL`
	- `reconciliation_status varchar(50) NOT NULL`
	- `shift_date date NOT NULL` -- derived business date for the shift
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

### 1.4 `reporting_debt_company`
- Purpose: Snapshot of company-level deferred debt at a given as-of date.
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `as_of_date date NOT NULL`
	- `company_id BIGINT NOT NULL` → FK to `admin_company.id`
	- `total_debt numeric(18,2) NOT NULL`
	- `in_term_debt numeric(18,2) NOT NULL`
	- `overdue_debt numeric(18,2) NOT NULL`
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

### 1.5 `reporting_debt_user`
- Purpose: Snapshot of user-level deferred debt per company at a given as-of date.
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `as_of_date date NOT NULL`
	- `user_id BIGINT NOT NULL` → FK to `users_user.id`
	- `company_id BIGINT NOT NULL` → FK to `admin_company.id`
	- `debt_balance numeric(18,2) NOT NULL`
	- `status varchar(50) NOT NULL` -- e.g. IN_TERM, OVERDUE, NEGATIVE
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

### 1.6 `reporting_dashboard_snapshot`
- Purpose: Optional pre-aggregated snapshots for Dashboard KPIs (orders today, COD open, overdue debt, top category).
- Columns:
	- `id BIGSERIAL PRIMARY KEY`
	- `snapshot_date date NOT NULL`
	- `company_id BIGINT NULL` → FK to `admin_company.id`
	- `total_orders_today bigint NOT NULL`
	- `received_rate_today numeric(9,4) NULL`
	- `total_cod_open numeric(18,2) NOT NULL`
	- `total_debt_overdue numeric(18,2) NOT NULL`
	- `top_category_id BIGINT NULL` → FK to `catalog_category.id`
	- `is_deleted boolean NOT NULL default false`
	- `created_at timestamptz`
	- `created_by varchar(500)`
	- `updated_at timestamptz`
	- `updated_by varchar(500)`

> Note: Depending on performance tests, some of these can be implemented as materialized views over source tables instead of physical tables; the logical schema remains the same.

## 2) Relationships

- `reporting_sales_summary.company_id` → `admin_company.id`
- `reporting_sales_summary.pickup_point_id` → `admin_pickup_point.id`
- `reporting_sales_summary.category_id` → `catalog_category.id`

- `reporting_commission_t2.user_id` → `users_user.id`
- `reporting_commission_t2.company_id` → `admin_company.id`

- `reporting_cod_shift.shift_id` → `payments_shift.id`
- `reporting_cod_shift.company_id` → `admin_company.id`
- `reporting_cod_shift.shipper_user_id` → `users_user.id`

- `reporting_debt_company.company_id` → `admin_company.id`

- `reporting_debt_user.user_id` → `users_user.id`
- `reporting_debt_user.company_id` → `admin_company.id`

- `reporting_dashboard_snapshot.company_id` → `admin_company.id`
- `reporting_dashboard_snapshot.top_category_id` → `catalog_category.id`

## 3) Indexes

Recommended indexes based on expected queries:

- `ix_reporting_sales_summary__company_period` on (`company_id`, `period_start`, `period_end`)
- `ix_reporting_sales_summary__pickup_point` on (`pickup_point_id`)
- `ix_reporting_sales_summary__category` on (`category_id`)

- `ix_reporting_commission_t2__period_user` on (`period_start`, `period_end`, `user_id`)
- `ix_reporting_commission_t2__company` on (`company_id`)

- `ix_reporting_cod_shift__shift_date_company` on (`shift_date`, `company_id`)
- `ix_reporting_cod_shift__shipper` on (`shipper_user_id`)

- `ix_reporting_debt_company__as_of_company` on (`as_of_date`, `company_id`)

- `ix_reporting_debt_user__as_of_company_user` on (`as_of_date`, `company_id`, `user_id`)

- `ix_reporting_dashboard_snapshot__snapshot_company` on (`snapshot_date`, `company_id`)

## 4) Notes

- **Retention / archival:**
	- Reporting tables may grow quickly; apply retention/archival strategy consistent with global NFRs (e.g. keep detailed rows for N months, then aggregate or archive).
	- Dashboard snapshots can be trimmed to a rolling window (e.g. last 12–24 months) if needed.

- **PII fields (if any):**
	- Schema stores only IDs and aggregate metrics; no direct PII such as phone numbers or CCCD.
	- Any joins to USERS/ORDERS for PII should be done at query time with masking at the application layer.

## 5) Migration & rollback

### 5.1 Initial migration

1. Create `reporting_sales_summary`, `reporting_commission_t2`, `reporting_cod_shift`, `reporting_debt_company`, `reporting_debt_user`, and `reporting_dashboard_snapshot` tables with audit columns.
2. Add indexes as listed above.
3. (Optional) Add initial ETL or materialized views to populate historical data if required for go-live.

### 5.2 Rollback strategy

- In early environments (dev/staging), if REPORTING feature needs to be rolled back, drop tables in reverse dependency order.
- In production, prefer forward-only migrations:
	- Mark REPORTING features as disabled at the application layer.
	- Keep tables for historical reference; apply follow-up migrations to adjust schema if needed.

### 5.3 Open questions

- Exact retention window for detailed vs aggregated reporting data.
- Whether REPORTING should keep its own fact tables or rely entirely on materialized views over source modules for MVP.

