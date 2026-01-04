
# DB Design Rules (Project-wide)

These rules apply to all modules. If a rule must change, record an ADR.

## 1) Naming
- Tables: `snake_case` (or your existing convention) — be consistent.
- Columns: `snake_case`.
- Primary key: `id` (BIGSERIAL/BIGINT).
- Foreign keys: `<ref_table>_id` (BIGINT).
- Indexes: `ix_<table>__<col1>_<col2>`
- Unique indexes: `ux_<table>__<col1>_<col2>`

## 2) Required audit columns
If your system has mandatory columns (e.g., `is_deleted`, `created_at`, `updated_at`), define them here and apply consistently.
- `is_deleted boolean not null default false`
- `created_at timestamptz`
- `created_by varchar(500)`
- `updated_at timestamptz`
- `updated_by varchar(500)`

(Adjust to match your organization standards.)

## 3) Data types
- IDs: BIGINT (FKs also BIGINT)
- Money: `numeric(18,2)` (avoid float)
- Timestamps: `timestamptz` (store UTC)
- Text: `text` or `varchar(n)` when length bounded
- JSON: `jsonb` only when schema-less is necessary

## 4) Constraints
- Use `NOT NULL` where possible
- Use FK constraints unless there is a strong reason not to
- Use CHECK constraints for enums/ranges where applicable

## 5) Indexing
- Index foreign keys used in joins
- Index columns used in filtering/sorting for hot queries
- Avoid indexing high-cardinality fields without evidence

## 6) Soft delete
- If soft delete is used, all queries must filter `is_deleted = false` (enforce via repository patterns).

## 7) Migrations
- All schema changes must be via migrations (no manual prod changes).
- Migrations must be:
  - repeatable on clean DB
  - safe for staging/prod order
  - have a rollback plan (documented)

## 8) Seed data
- Seed only deterministic reference data (roles, permissions, catalog options).
- Do not seed real PII.

## 9) Module DB docs
Each module DB file must include:
- Tables list
- Relationships
- Key constraints
- Suggested indexes
- Migration notes
