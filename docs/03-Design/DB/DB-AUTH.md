
# DB — AUTH: Authentication & Authorization

> This document defines the **logical** AUTH schema. Exact EF Core mappings live in code, but must not contradict this.

## 1. Scope

AUTH DB stores:
- OTP challenges for End User registration/login.
- Refresh tokens (and optionally server-side session state) for End Users and Admins.
- Admin authentication credentials (hashes), linked to ADMIN roles.

User master data (tiers, companies, etc.) remains in USERS / ADMIN modules.

---

## 2. Tables

### 2.1 `auth_otp_request`

Stores one-time passwords (OTP) sent to phone numbers for **registration** and **first-time login**, per BR-AUTH-001/002.

Columns (snake_case):
- `id` BIGSERIAL PK
- `phone` varchar(20) NOT NULL
- `purpose` varchar(50) NOT NULL
	- Enum domain in code: `END_USER_REGISTER`, `END_USER_LOGIN_FIRST_TIME` (extendable).
- `otp_code_hash` varchar(255) NOT NULL
	- Hash of OTP, **not** the raw code (Security-Baseline).
- `expires_at` timestamptz NOT NULL
- `status` varchar(30) NOT NULL
	- Enum domain in code: `PENDING`, `VERIFIED`, `EXPIRED`, `CANCELLED`.
- `attempt_count` int NOT NULL DEFAULT 0
- `last_attempt_at` timestamptz NULL
- `notification_template_code` varchar(100) NULL
- `notification_message_id` varchar(100) NULL
- `correlation_id` varchar(100) NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- CHECK on `purpose` and `status` per enum values.
- Indexes:
	- `ix_auth_otp_request__phone_purpose_status` (`phone`, `purpose`, `status`)
	- `ix_auth_otp_request__expires_at` (`expires_at`)

Notes:
- Retention: short-lived (e.g., 30–90 days) — to be confirmed in an ADR.
- PII: `phone` is PII; access must be restricted and masking applied in logs.

---

### 2.2 `auth_refresh_token`

Stores refresh tokens for both End Users and Admins (FR-AUTH-002/003/005).

Columns:
- `id` BIGSERIAL PK
- `user_id` BIGINT NULL
- `admin_user_id` BIGINT NULL
	- Exactly one of `user_id` / `admin_user_id` must be non-null.
- `token_hash` varchar(255) NOT NULL
- `device_id` varchar(128) NULL
- `user_agent` varchar(512) NULL
- `platform` varchar(128) NULL
- `expires_at` timestamptz NOT NULL
- `revoked_at` timestamptz NULL
- `revoked_reason` varchar(255) NULL
- `correlation_id` varchar(100) NULL
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- CHECK to enforce `user_id IS NOT NULL XOR admin_user_id IS NOT NULL` (implemented in DB or via application logic if XOR is not supported natively).
- FKs (logical):
	- `user_id` → `users_user.id` (DB-USERS)
	- `admin_user_id` → `admin_admin_user.id` (DB-ADMIN)
- Indexes:
	- `ux_auth_refresh_token__token_hash` UNIQUE (`token_hash`)
	- `ix_auth_refresh_token__user_id` (`user_id`)
	- `ix_auth_refresh_token__admin_user_id` (`admin_user_id`)
	- `ix_auth_refresh_token__expires_at` (`expires_at`)

Notes:
- Only hashes of refresh tokens are stored; raw values are sent once to clients.
- Session/access tokens remain stateless JWTs; revocation is driven primarily by refresh tokens.

---

### 2.3 `auth_admin_account`

Stores authentication credentials for Admin Portal users; role/permissions live in ADMIN tables.

Columns:
- `id` BIGSERIAL PK
- `admin_user_id` BIGINT NOT NULL
	- FK → `admin_admin_user.id` (DB-ADMIN).
- `login_id` varchar(255) NOT NULL
	- Email or phone; must be unique.
- `password_hash` varchar(255) NOT NULL
	- BCrypt/Argon2 hash as per Security-Baseline.
- `password_salt` varchar(255) NULL
	- Optional, depending on hashing library.
- `is_locked` boolean NOT NULL DEFAULT false
- `failed_attempts` int NOT NULL DEFAULT 0
- `last_failed_at` timestamptz NULL
- `last_login_at` timestamptz NULL
- `status` varchar(30) NOT NULL DEFAULT 'ACTIVE'
- Audit:
	- `is_deleted` boolean NOT NULL DEFAULT false
	- `created_at` timestamptz NOT NULL
	- `created_by` varchar(500) NULL
	- `updated_at` timestamptz NULL
	- `updated_by` varchar(500) NULL

Constraints & indexes:
- `ux_auth_admin_account__login_id` UNIQUE (`login_id`)
- `ix_auth_admin_account__admin_user_id` (`admin_user_id`)

Notes:
- Lockout thresholds (failed attempts, cool-down duration) are **Open Questions** and must be parameterized, not hard-coded.

---

## 3. Relationships (logical)

- `auth_otp_request` has no direct FK to USERS; matching from phone → user is performed in the AUTH service layer.
- `auth_refresh_token.user_id` → `users_user.id` (End User tokens).
- `auth_refresh_token.admin_user_id` → `admin_admin_user.id` (Admin tokens).
- `auth_admin_account.admin_user_id` → `admin_admin_user.id`.

All FKs must respect `is_deleted = false` semantics in the referenced tables.

---

## 4. Indexing & performance considerations

- OTP lookups are primarily by `phone`, `purpose`, `status` and recent `created_at`.
- Refresh token validation is primarily by `token_hash` (unique) and occasionally by `user_id` / `admin_user_id` for mass revocation.
- Admin login is by `login_id`.

Follow DB-Design-Rules for additional indexes only when there is evidence of hotspots.

---

## 5. Migration & rollback notes

### Initial AUTH schema migration

1. Create `auth_otp_request` with all columns and indexes listed above.
2. Create `auth_refresh_token` with columns, FKs to USERS/ADMIN, and indexes.
3. Create `auth_admin_account` with FK to ADMIN and unique index on `login_id`.

Rollback strategy:
- In early environments, drop the three tables if the feature is rolled back entirely.
- In shared environments, mark features as disabled at the application level and keep tables (no destructive rollback) unless a dedicated data migration is planned.

### Future changes

- Any change to OTP retention, token TTL fields, or account lockout policy must:
	- Update this document.
	- Be captured in a migration with a clear rollback plan.

---

## 6. Retention / archival

- `auth_otp_request`: retain only for troubleshooting/audit within a short window (e.g., 30–90 days); then purge or archive.
- `auth_refresh_token`: retain active and recently revoked tokens; periodically purge expired tokens.
- `auth_admin_account`: long-lived; rows should be soft-deleted via `is_deleted` / `status` changes, not hard-deleted.

PII fields:
- `phone` in `auth_otp_request` is PII and must be masked in logs and restricted in query tools.
- No passwords or OTPs are stored in clear text; only hashes.
