
# DB — ORDERS: Orders

This document defines the relational schema for the ORDERS module, aligned to SRS-ORDERS and project-wide DB-Design-Rules.

## 1) Tables

### 1.1 `orders_order`
- `id BIGSERIAL PRIMARY KEY`
- `user_id BIGINT NOT NULL` — FK → `users_user.id` (End User placing the order)
- `company_id BIGINT NOT NULL` — FK → `admin_company.id`
- `pickup_point_id BIGINT NOT NULL` — FK → `admin_pickup_point.id`
- `total_amount numeric(18,2) NOT NULL` — total order value at creation time
- `payment_method varchar(50) NOT NULL` — key defined by PAYMENTS (e.g. COD, DEFERRED)
- `status varchar(50) NOT NULL` — current business status (e.g. PENDING_CONFIRMATION, PENDING_PAYMENT, CONFIRMED, IN_DELIVERY, READY_FOR_PICKUP, COMPLETED, CANCELLATION_PENDING, CANCELLED, RETURNED)
- `status_reason text NULL` — optional human-readable note for current status
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

### 1.2 `orders_order_item`
- `id BIGSERIAL PRIMARY KEY`
- `order_id BIGINT NOT NULL` — FK → `orders_order.id`
- `product_id BIGINT NOT NULL` — FK → `catalog_product.id`
- `product_name text NOT NULL` — snapshot of product name at order time
- `quantity integer NOT NULL` — must be > 0 (CHECK)
- `unit_price numeric(18,2) NOT NULL`
- `line_amount numeric(18,2) NOT NULL` — `quantity * unit_price` at time of order
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

### 1.3 `orders_order_status_history`
- `id BIGSERIAL PRIMARY KEY`
- `order_id BIGINT NOT NULL` — FK → `orders_order.id`
- `status varchar(50) NOT NULL`
- `changed_by varchar(500) NOT NULL` — user id / system identifier string
- `changed_at timestamptz NOT NULL`
- `note text NULL`
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

### 1.4 `orders_order_cancel_request`
- `id BIGSERIAL PRIMARY KEY`
- `order_id BIGINT NOT NULL` — FK → `orders_order.id`
- `requested_by varchar(500) NOT NULL`
- `requested_at timestamptz NOT NULL`
- `reason text NOT NULL`
- `approved_by varchar(500) NULL`
- `approved_at timestamptz NULL`
- `status varchar(20) NOT NULL` — e.g. PENDING, APPROVED, REJECTED
- `is_active boolean NOT NULL default true` — only one active cancel request per order
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

### 1.5 `orders_order_delivery_proof`
- `id BIGSERIAL PRIMARY KEY`
- `order_id BIGINT NOT NULL` — FK → `orders_order.id`
- `delivered_by varchar(500) NOT NULL`
- `delivered_at timestamptz NOT NULL`
- `pod_image_urls jsonb NOT NULL` — array of POD image URLs
- `receiver_name text NULL` — subject to Data-Classification rules
- `note text NULL`
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

### 1.6 `orders_order_return_request`
- `id BIGSERIAL PRIMARY KEY`
- `order_id BIGINT NOT NULL` — FK → `orders_order.id`
- `status varchar(20) NOT NULL` — e.g. PENDING, CONFIRMED, REJECTED
- `reason text NOT NULL`
- `created_by varchar(500) NOT NULL`
- `created_at timestamptz NOT NULL`
- `updated_by varchar(500)`
- `updated_at timestamptz`
- `is_deleted boolean NOT NULL default false`

### 1.7 `orders_order_return_item`
- `id BIGSERIAL PRIMARY KEY`
- `return_request_id BIGINT NOT NULL` — FK → `orders_order_return_request.id`
- `product_id BIGINT NOT NULL` — FK → `catalog_product.id`
- `quantity integer NOT NULL` — must be > 0 (CHECK)
- `is_deleted boolean NOT NULL default false`
- `created_at timestamptz NOT NULL`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

## 2) Relationships

- `orders_order.user_id` → `users_user.id`
- `orders_order.company_id` → `admin_company.id`
- `orders_order.pickup_point_id` → `admin_pickup_point.id`
- `orders_order_item.order_id` → `orders_order.id`
- `orders_order_item.product_id` → `catalog_product.id`
- `orders_order_status_history.order_id` → `orders_order.id`
- `orders_order_cancel_request.order_id` → `orders_order.id`
- `orders_order_delivery_proof.order_id` → `orders_order.id`
- `orders_order_return_request.order_id` → `orders_order.id`
- `orders_order_return_item.return_request_id` → `orders_order_return_request.id`
- `orders_order_return_item.product_id` → `catalog_product.id`

## 3) Indexes

Recommended indexes for main query patterns:

- `ix_orders_order__user_id_created_at` on (`user_id`, `created_at DESC`) — End User order history.
- `ix_orders_order__company_id_pickup_point_id_status` on (`company_id`, `pickup_point_id`, `status`) — Ops/Shipper views.
- `ix_orders_order_item__order_id` on (`order_id`) — join items.
- `ix_orders_order_status_history__order_id_changed_at` on (`order_id`, `changed_at DESC`).
- `ix_orders_order_cancel_request__status_order_id` on (`status`, `order_id`).
- `ix_orders_order_return_request__order_id_status` on (`order_id`, `status`).

Unique / business constraints:

- At most one active cancel request per order:
	- partial unique index `ux_orders_order_cancel_request__order_id_active` on (`order_id`) WHERE `is_active = true AND is_deleted = false`.

## 4) Notes, retention & PII

- PII:
	- `orders_order_delivery_proof.receiver_name` may contain personal data; usage must follow Data-Classification and may be masked at API level.
- POD URLs:
	- `pod_image_urls` should point to storage with appropriate access controls; no raw image data stored in DB.
- Retention:
	- Detailed status history, cancel/return requests, and delivery proofs should follow retention policies aligned with audit/reporting needs (e.g. 3–5 years — **Open Question**).

## 5) Migration & rollback notes

Initial introduction of ORDERS module (no legacy data assumed):

1. Create `orders_order` table.
2. Create `orders_order_item` table.
3. Create `orders_order_status_history` table.
4. Create `orders_order_cancel_request` table.
5. Create `orders_order_delivery_proof` table.
6. Create `orders_order_return_request` and `orders_order_return_item` tables.
7. Add indexes and unique constraints listed above.

Rollback strategy (for early-stage environments):

- Drop child tables in reverse dependency order:
	- `orders_order_return_item`, `orders_order_return_request`, `orders_order_delivery_proof`, `orders_order_cancel_request`, `orders_order_status_history`, `orders_order_item`, then `orders_order`.
- In production, prefer additive migrations; avoid dropping tables with live data. Use feature flags in the application to disable ORDERS functionality if rollback is needed.

## 6) Open Questions (DB-ORDERS)

- Q-DB-ORDERS-001: Do we need to store additional payment linkage on `orders_order` (e.g. `payment_id` or `payment_status`) or keep this solely in PAYMENTS referencing ORDERS?
- Q-DB-ORDERS-002: Exact retention period for delivery proofs and cancel/return details (legal/compliance input needed). Recommended default: keep for at least 3 years.
- Q-DB-ORDERS-003: Whether multiple sequential return requests for the same order are allowed, or only one active return per order at a time (would require a similar partial unique index).
