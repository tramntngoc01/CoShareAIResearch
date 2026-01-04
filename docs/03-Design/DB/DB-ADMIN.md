
# DB — ADMIN: Admin Console

> Logical schema for ADMIN module. EF Core mappings must align with this document.

## 1. Scope

ADMIN DB stores:
- Master data for **companies** and **pickup points** used by USERS, ORDERS, PAYMENTS, REPORTING.
- **Admin users and roles** for Admin Portal RBAC (with credentials in AUTH).
- **System configuration** (logo, legal info, terms) used by frontends.
- Central **audit log** for sensitive actions across modules.

---

## 2. Tables

### 2.1 `admin_company`

Represents a company in the industrial zone; referenced by users, orders, payments, and reports.

Columns (snake_case):
- `id` BIGSERIAL PK
- `company_code` varchar(100) NOT NULL
- `company_name` varchar(255) NOT NULL
- `zone` varchar(255) NULL
- `status` varchar(30) NOT NULL DEFAULT 'ACTIVE'
	- Enum in code: `ACTIVE`, `INACTIVE` (extendable).
- `description` text NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- `ux_admin_company__company_code` UNIQUE (`company_code`) — enforces AC/BR that companyCode is unique.
- Optional CHECK on `status` domain.

Notes:
- Companies must **not be hard-deleted** if referenced; use `status` + `is_deleted` for logical deactivation (BR-ADMIN-002).

---

### 2.2 `admin_pickup_point`

Represents pickup points belonging to a company; used by USERS and ORDERS.

Columns:
- `id` BIGSERIAL PK
- `company_id` BIGINT NOT NULL
- `name` varchar(255) NOT NULL
- `is_default` boolean NOT NULL DEFAULT false
- `status` varchar(30) NOT NULL DEFAULT 'ACTIVE'
	- Enum in code: `ACTIVE`, `INACTIVE`.
- `address` text NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- FK: `company_id` → `admin_company.id`.
- Unique default per company enforced with partial unique index:
	- `ux_admin_pickup_point__company_id_is_default_true` UNIQUE (`company_id`) WHERE `is_default = true`.
- Index: `ix_admin_pickup_point__company_id` (`company_id`).

Notes:
- Pickup points should not be hard-deleted while referenced by users/orders; instead mark `status = 'INACTIVE'` and/or `is_deleted = true`.

---

### 2.3 `admin_admin_user`

Logical admin user profile; credentials live in `auth_admin_account` (DB-AUTH).

Columns:
- `id` BIGSERIAL PK
- `display_name` varchar(255) NULL
- `company_id` BIGINT NULL
	- Optional scoping of admin to a company.
- `status` varchar(30) NOT NULL DEFAULT 'ACTIVE'
	- Enum in code: `ACTIVE`, `INACTIVE`, `LOCKED`.
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- FK (optional): `company_id` → `admin_company.id`.
- Index: `ix_admin_admin_user__company_id` (`company_id`).

Notes:
- `auth_admin_account.admin_user_id` links credentials to this profile.

---

### 2.4 `admin_role`

Stores admin roles (Super Admin, Ops, QC, Finance, Support, ...).

Columns:
- `id` BIGSERIAL PK
- `code` varchar(100) NOT NULL
- `name` varchar(255) NOT NULL
- `description` text NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- `ux_admin_role__code` UNIQUE (`code`) — backs error `ADMIN_ROLE_CODE_DUPLICATE`.

---

### 2.5 `admin_admin_user_role`

Many-to-many link between `admin_admin_user` and `admin_role`.

Columns:
- `id` BIGSERIAL PK
- `admin_user_id` BIGINT NOT NULL
- `role_id` BIGINT NOT NULL
- Audit:
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL

Constraints & indexes:
- FK: `admin_user_id` → `admin_admin_user.id`.
- FK: `role_id` → `admin_role.id`.
- Unique mapping:
	- `ux_admin_admin_user_role__admin_user_id_role_id` UNIQUE (`admin_user_id`, `role_id`).
- Index: `ix_admin_admin_user_role__role_id` (`role_id`).

Notes:
- Application layer must enforce that an admin user always has at least one role (BR-ADMIN-001).

---

### 2.6 `admin_system_config`

Stores global system configuration used by portals.

Columns:
- `id` BIGSERIAL PK
- `system_name` varchar(255) NOT NULL
- `logo_url` text NULL
- `legal_entity_name` varchar(255) NULL
- `legal_address` text NULL
- `terms_url` text NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- For MVP we expect a single active row; application can address `id = 1` or latest non-deleted row.

---

### 2.7 `admin_audit_log`

Central audit log for sensitive operations across modules (FR-ADMIN-005, BR-ADMIN-003).

Columns:
- `id` BIGSERIAL PK
- `timestamp` timestamptz NOT NULL
- `actor_type` varchar(50) NOT NULL
	- Enum in code: `AdminUser`, `EndUser`, `SystemJob`, ...
- `actor_id` BIGINT NULL
- `action_type` varchar(100) NOT NULL
- `module` varchar(50) NOT NULL
- `correlation_id` varchar(100) NULL
- `result` varchar(30) NOT NULL
	- Enum in code: `SUCCESS`, `FAILURE` (extendable).
- `metadata` jsonb NULL
	- Redacted details; must not contain raw PII/OTP/KYC per Security-Baseline.

Constraints & indexes:
- Index: `ix_admin_audit_log__timestamp` (`timestamp`).
- Index: `ix_admin_audit_log__module_action_type` (`module`, `action_type`).
- Index: `ix_admin_audit_log__actor_type_actor_id` (`actor_type`, `actor_id`).
- Index: `ix_admin_audit_log__correlation_id` (`correlation_id`).

Notes:
- High-write table; consider partitioning by time in future if volume is high (Open Question for infra).

---

## 3. Relationships (logical)

- `admin_pickup_point.company_id` → `admin_company.id`.
- `admin_admin_user.company_id` → `admin_company.id` (optional).
- `auth_refresh_token.admin_user_id` → `admin_admin_user.id` (see DB-AUTH).
- `auth_admin_account.admin_user_id` → `admin_admin_user.id` (see DB-AUTH).
- `admin_admin_user_role.admin_user_id` → `admin_admin_user.id`.
- `admin_admin_user_role.role_id` → `admin_role.id`.
- `orders_order.company_id`, `users_user.company_id`, etc. reference `admin_company.id` logically (see DB-ORDERS, DB-USERS).

All relationships must respect soft-delete semantics (`is_deleted = false`) in application queries.

---

## 4. Indexing & performance considerations

- Company & pickup points:
	- Queries typically filter by `status`, `zone`, and `company_id`; index accordingly if hotspots appear.
- Admin users & roles:
	- List/lookups by role and company; indexes on `admin_admin_user_role.role_id` and `admin_admin_user.company_id` support this.
- Audit log:
	- Search by time range + module/action + actorId + correlationId; indexes above support typical filters.

Follow DB-Design-Rules for any additional indexes, based on observed query patterns.

---

## 5. Migration & rollback notes

### Initial ADMIN schema migration

1. Create `admin_company` and `admin_pickup_point` tables.
2. Create `admin_admin_user`, `admin_role`, and `admin_admin_user_role` tables.
3. Create `admin_system_config` table and seed a default row if required by UI.
4. Create `admin_audit_log` table with indexes.
5. Update AUTH migrations (if needed) to ensure FKs to `admin_admin_user` are created after this migration.

Rollback strategy:
- In early environments, drop child tables (`admin_admin_user_role`, `admin_pickup_point`, `admin_audit_log`, ...) before parent tables.
- In shared/stable environments, prefer disabling features at application level and keeping tables; avoid destructive rollbacks for audit data.

---

## 6. Retention / archival

- `admin_company`, `admin_pickup_point`, `admin_admin_user`, `admin_role`, `admin_system_config`:
	- Long-lived configuration/master data; use soft-delete (`is_deleted`, `status`) rather than physical deletion.
- `admin_audit_log`:
	- Retain per regulatory and business requirements (e.g., 1–3 years).
	- Consider archival strategy (cold storage or partition pruning) — Open Question for infra/compliance.

PII fields:
- `admin_company` and `admin_pickup_point` may contain non-PII business info (addresses) but still treat carefully.
- `admin_admin_user.display_name` may contain personal names; avoid exposing via unrestricted APIs and do not log in full in sensitive logs.

