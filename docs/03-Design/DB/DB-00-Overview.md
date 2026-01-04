
# DB Design — Overview

## Naming conventions
- Table names: snake_case (or your standard)
- Primary key: id (bigint/uuid)
- Audit fields: created_at, created_by, updated_at, updated_by
- Soft delete: is_deleted (if used)

## Migration strategy
- Use migrations as single source of truth
- Forward-only migrations preferred
- Provide rollback notes for each migration

## Indexing guidelines
- Index foreign keys
- Add composite indexes for common queries
- Avoid over-indexing early; measure and iterate
