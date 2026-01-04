
# DB — USERS: User & Profile Management

## Overview
- Manage core user master data (shippers, tiers 1/2/3) imported from HR and updated by Admin.
- Expose read models to AUTH, ORDERS, PAYMENTS, REPORTING.
- Store KYC/profile information with clear ownership between Company-imported vs. Admin-editable fields.

Conventions follow DB-Design-Rules:
- `snake_case` naming, `id` as BIGSERIAL PK, `<entity>_id` as BIGINT FK.
- All tables include logical deletion and audit columns:
	- `is_deleted boolean not null default false`
	- `created_at timestamptz not null`
	- `created_by bigint null`
	- `updated_at timestamptz null`
	- `updated_by bigint null`

## Entities
- `User` — core user (tier, company, pickup point, status).
- `UserRefTierHistory` — history of ref tier (parent) changes.
- `UserStatusHistory` — history of status changes.
- `UserImportJob` — one HR import execution.
- `UserImportRow` — per-row validation outcome for an import.

## Tables

### table: users_user
- `id bigserial primary key`
- `external_employee_code varchar(64) not null` — employee code from HR; together with `company_id` forms a logical key.
- `full_name varchar(255) not null`
- `phone varchar(32) not null`
- `company_id bigint not null` — FK → `admin_company.id` (ADMIN module).
- `pickup_point_id bigint not null` — FK → `admin_pickup_point.id` (ADMIN module).
- `tier varchar(16) not null` — e.g. `T1`, `T2`, `T3`, `SHIPPER`.
- `status varchar(32) not null` — logical status, e.g. `Draft`, `Active`, `Inactive`, `Locked`, `Deleted` (logical).
- `email varchar(255) null`
- `address_detail varchar(500) null`
- `birth_date date null`
- `cccd_hash varchar(256) null` — hashed/tokenized CCCD; raw value never stored.
- `kyc_metadata jsonb null` — extensible key/value bag for additional KYC fields.
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: users_ref_tier_history
- `id bigserial primary key`
- `user_id bigint not null` — FK → `users_user.id`.
- `old_parent_user_id bigint null` — FK → `users_user.id`.
- `new_parent_user_id bigint null` — FK → `users_user.id`.
- `old_tier varchar(16) not null`
- `new_tier varchar(16) not null`
- `changed_at timestamptz not null`
- `changed_by bigint not null` — FK → `admin_admin_user.id` (actor in Admin portal).
- `note varchar(500) null`

### table: users_status_history
- `id bigserial primary key`
- `user_id bigint not null` — FK → `users_user.id`.
- `old_status varchar(32) not null`
- `new_status varchar(32) not null`
- `reason varchar(500) null`
- `changed_at timestamptz not null`
- `changed_by bigint not null` — FK → `admin_admin_user.id`.

### table: users_import_job
- `id bigserial primary key`
- `import_uuid uuid not null` — external ID exposed via API.
- `source varchar(100) null` — optional HR source identifier.
- `file_name varchar(255) not null`
- `status varchar(32) not null` — e.g. `Pending`, `Processing`, `Completed`, `Failed`.
- `total_rows integer null`
- `created_rows integer null`
- `updated_rows integer null`
- `failed_rows integer null`
- `started_at timestamptz null`
- `completed_at timestamptz null`
- `requested_by bigint not null` — FK → `admin_admin_user.id`.
- `is_deleted boolean not null default false`
- `created_at timestamptz not null`
- `created_by bigint null`
- `updated_at timestamptz null`
- `updated_by bigint null`

### table: users_import_row
- `id bigserial primary key`
- `import_job_id bigint not null` — FK → `users_import_job.id`.
- `row_number integer not null`
- `logical_key varchar(255) null` — e.g. concatenation of employee code + company.
- `result varchar(32) not null` — `Created`, `Updated`, `Failed`, `Skipped`.
- `error_code varchar(64) null`
- `error_message varchar(500) null`
- `raw_payload jsonb null` — raw parsed data for troubleshooting (subject to retention & masking).
- `created_at timestamptz not null`
- `created_by bigint null`

## Relationships
- `users_user.company_id` → `admin_company.id`.
- `users_user.pickup_point_id` → `admin_pickup_point.id`.
- `users_ref_tier_history.user_id` → `users_user.id`.
- `users_ref_tier_history.old_parent_user_id` → `users_user.id` (nullable).
- `users_ref_tier_history.new_parent_user_id` → `users_user.id` (nullable).
- `users_ref_tier_history.changed_by` → `admin_admin_user.id`.
- `users_status_history.user_id` → `users_user.id`.
- `users_status_history.changed_by` → `admin_admin_user.id`.
- `users_import_job.requested_by` → `admin_admin_user.id`.
- `users_import_row.import_job_id` → `users_import_job.id`.

## Indexes
- `ux_users_user__company_employee` on (`company_id`, `external_employee_code`) unique — logical HR key.
- `ux_users_user__phone_company` on (`phone`, `company_id`) unique (subject to confirmed business rule).
- `ix_users_user__company` on (`company_id`).
- `ix_users_user__pickup_point` on (`pickup_point_id`).
- `ix_users_user__tier_status` on (`tier`, `status`).
- `ix_users_ref_tier_history__user` on (`user_id`, `changed_at desc`).
- `ix_users_status_history__user` on (`user_id`, `changed_at desc`).
- `ix_users_import_job__import_uuid` on (`import_uuid`) unique.
- `ix_users_import_row__import_job` on (`import_job_id`, `row_number`).

## Notes
- Retention / archival:
	- `users_import_row.raw_payload` may contain sensitive HR-originated data → apply strict retention (e.g. 90 days) and never expose directly via API; only via sampled error summaries.
	- Historical tables (`users_ref_tier_history`, `users_status_history`) can be archived to cold storage beyond X years as per compliance.
- PII fields:
	- `full_name`, `phone`, `email`, `address_detail`, and KYC-related metadata are PII and must follow Data-Classification rules.
	- `cccd_hash` stores only a hashed/tokenized representation; raw CCCD number must not be stored in plaintext.
- Security:
	- All access to USERS tables must go through application services; no direct ad-hoc querying in lower environments with prod-like data.
	- Logging must never write full PII or raw payloads; use `import_uuid` and `user_id` with correlation id instead.
