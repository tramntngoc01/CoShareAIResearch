
# Test Cases — USERS: User & Profile Management

## 1) Scope & assumptions
- Scope covers USERS module only; inputs from SRS-USERS, Stories-USERS, StoryPacks US-USERS-001/002/003/005/006, OpenAPI `docs/03-Design/OpenAPI/openapi.yaml`, Error-Conventions, Testing Strategy/Plan.
- No ScreenSpec `SC-USERS-*` files exist; UI coverage is based on described screens in stories (Import, Network/Ref Tier, User detail, Status change, My Profile, User list). Marked as assumptions where UI behavior is unspecified.
- AUTH/login flows assumed working (out of scope). Fake data only; no real PII.
- CorrelationId must be captured for all API calls; audit logging expectations are observed via API responses/logs where available.

## 2) Test data set (fake)
- Company: `CTY_A`, Pickup Point: `PUP_A1`
- Tiers: Tier 1 `T1_001` (Tầng 1), Tier 2 `T2_001` (parent `T1_001`), Tier 2 `T2_005` (alternate parent), Tier 3 `T3_001` (child of `T2_001`)
- Shipper: `SHIP_001` (separate role, not part of tier tree)
- Users (ids are sample placeholders): `10010` (T3), `10020` (Admin-editable KYC), `10030` (End User), `10040` (status toggle), `10050` (ref tier change candidate)
- Files: Valid CSV with headers `employee_code,full_name,phone,company_code,pickup_point_code,tier,ref_tier_code`; oversized file placeholder of 200k rows to trigger limit.

## 3) E2E testcases (P0 first)

### E2E-USERS-001 — Admin imports HR file and views results
- Priority: P0
- Story IDs: US-USERS-001, US-USERS-006
- Screen IDs: Import nhân sự; Kết quả import (UI spec TBD)
- Preconditions: Admin logged in; import template approved; network stable.
- Test data: Valid CSV (3 rows for T1_001, T2_001, T3_001).
- Steps:
  1. Navigate to Import nhân sự screen; upload valid CSV; submit.
  2. Poll job status via GET `/api/v1/users/import/{importId}` until completed.
  3. Navigate to user list with filters company=`CTY_A`, tier=3; search.
- Expected results: Import accepted (202) with `importId`; job result shows total=3, created=3, errors=[]; user list shows T3_001 present with correct company/tier. CorrelationId returned for calls.
- Evidence: Screenshots of import submission + result; API logs with correlationId.
- Notes: Blocked by requirement if final column list differs from provided sample (DoR pending).

### E2E-USERS-002 — Super Admin updates Ref Tier with history
- Priority: P0
- Story IDs: US-USERS-002
- Screen IDs: Quản lý mạng lưới/Tier; Chỉnh sửa Ref Tier (UI spec TBD)
- Preconditions: Super Admin logged in; user `10050` currently child of `T2_001`; target parent `T2_005` exists.
- Test data: Ref tier change payload `{ "parentRefCode": "T2_005" }`.
- Steps:
  1. Open user `10050` ref tier view.
  2. Submit PUT `/api/v1/users/10050/ref-tier` with new parent.
  3. Refresh ref tier view.
- Expected results: Response 200 with new parent `T2_005`; history entry recorded (old/new, changedBy, timestamp); no orphan tiers.
- Evidence: UI screenshot post-change; API response with correlationId.
- Notes: None.

### E2E-USERS-003 — Admin edits allowed KYC fields
- Priority: P0
- Story IDs: US-USERS-003
- Screen IDs: Danh sách User; Chi tiết hồ sơ User (UI spec TBD)
- Preconditions: Admin logged in; user `10020` exists; allowed fields include `fullName`, `email`; company fields locked.
- Test data: Update `{ "fullName": "Lê Thị Mới", "email": "new.email@example.com" }`.
- Steps:
  1. Open user detail for `10020`.
  2. Edit allowed fields and save (PATCH `/api/v1/users/10020/kyc`).
  3. Re-open detail to verify persisted values.
- Expected results: 200 response; updated values reflected; locked fields unchanged; audit entry captured.
- Evidence: Before/after screenshots; API response with correlationId.
- Notes: Blocked by requirement if allowed-field list not finalized.

### E2E-USERS-004 — Admin locks and unlocks user
- Priority: P0
- Story IDs: US-USERS-005
- Screen IDs: Danh sách User; Thay đổi trạng thái User (UI spec TBD)
- Preconditions: Admin with status-change permission; user `10040` in Active status.
- Test data: Status payload `{ "status": "Locked", "reason": "Policy violation" }`.
- Steps:
  1. View user status (GET `/api/v1/users/10040`).
  2. Lock user via PATCH `/api/v1/users/10040/status`.
  3. Attempt to lock again (idempotency/no-op expected).
  4. Unlock back to Active.
- Expected results: Status transitions succeed with audit log; second lock returns no duplicate change; status visible as Locked/Active accordingly.
- Evidence: UI screenshots before/after; API responses with correlationId.
- Notes: Coordination with AUTH/ORDERS for downstream enforcement assumed.

### E2E-USERS-005 — Admin searches users with multi-filter
- Priority: P0
- Story IDs: US-USERS-006
- Screen IDs: Danh sách User (filters) (UI spec TBD)
- Preconditions: Admin logged in; dataset contains mixed tiers/status.
- Test data: Query `companyId=CTY_A, tier=3, status=Active, page=1, pageSize=20`.
- Steps:
  1. Open user list; set filters; submit search.
  2. Paginate to next page.
- Expected results: 200 with paged structure `{ items, page, totalItems, totalPages }`; items match filters; page boundaries honored.
- Evidence: UI screenshot of list; API response with correlationId.
- Notes: Performance budget TBD; monitor response time.

### E2E-USERS-006 — End User edits permitted profile fields (P1)
- Priority: P1
- Story IDs: US-USERS-004
- Screen IDs: "Hồ sơ của tôi" (End User) (UI spec TBD)
- Preconditions: End User logged in; allowed fields include `email`, `addressDetail`; token valid.
- Test data: `{ "email": "pham.d.new@example.com", "addressDetail": "Nhà trọ KCN X - đường 2, phòng 305" }`.
- Steps:
  1. Open My Profile.
  2. Edit allowed fields and save.
  3. Refresh profile.
- Expected results: Allowed fields updated; locked fields (company, tier) remain read-only; if token expired during save, user prompted to re-login.
- Evidence: UI screenshot after save; API response with correlationId.
- Notes: Blocked by requirement until allowed-field list confirmed.

## 4) Integration/API testcases

### IT-USERS-001 — Import rejected for missing required column
- Priority: P0
- Story IDs: US-USERS-001
- Preconditions: Admin token; upload file missing `company_code`.
- Test data: Malformed CSV lacking required column.
- Steps: POST `/api/v1/users/import` with malformed file.
- Expected results: 400 with code `USERS_IMPORT_STRUCTURE_INVALID`; no job created; no users changed.
- Evidence: API response with correlationId; error payload.
- Notes: Blocked if column list changes.

### IT-USERS-002 — Import rejected for oversized file
- Priority: P0
- Story IDs: US-USERS-001
- Preconditions: Admin token; file size over configured limit.
- Test data: 200k-row CSV placeholder.
- Steps: POST `/api/v1/users/import`.
- Expected results: 413 with code `USERS_IMPORT_TOO_LARGE`; no job created.
- Evidence: API response with correlationId.
- Notes: Ensure limit value from config.

### IT-USERS-003 — Ref Tier change denied for invalid parent tier
- Priority: P0
- Story IDs: US-USERS-002
- Preconditions: Super Admin token; user is T3; provided parent is T3.
- Test data: `{ "parentRefCode": "T3_invalid" }`.
- Steps: PUT `/api/v1/users/{id}/ref-tier`.
- Expected results: 400 with code `USERS_REF_TIER_INVALID_PARENT`; no history entry added.
- Evidence: API response with correlationId.
- Notes: Validates tier rule (T3 must point to T2).

### IT-USERS-004 — Ref Tier change unauthorized
- Priority: P0
- Story IDs: US-USERS-002
- Preconditions: Admin (non-Super) token.
- Test data: Valid parent payload.
- Steps: PUT `/api/v1/users/{id}/ref-tier`.
- Expected results: 403 DefaultError; ref tier unchanged.
- Evidence: API response with correlationId.
- Notes: AuthZ negative.

### IT-USERS-005 — KYC update rejects forbidden field
- Priority: P0
- Story IDs: US-USERS-003
- Preconditions: Admin token; company field locked.
- Test data: `{ "companyId": 99 }`.
- Steps: PATCH `/api/v1/users/{id}/kyc`.
- Expected results: 403 or 400 with code `USERS_KYC_FIELD_NOT_ALLOWED`; no changes.
- Evidence: API response with correlationId.
- Notes: Blocked until allowed-field list finalized.

### IT-USERS-006 — KYC validation error for bad format
- Priority: P0
- Story IDs: US-USERS-003
- Preconditions: Admin token.
- Test data: `{ "birthDate": "1995-99-99" }`.
- Steps: PATCH `/api/v1/users/{id}/kyc`.
- Expected results: 400 with code `USERS_KYC_VALIDATION_FAILED` and field-level message; audit not created.
- Evidence: API response with correlationId.
- Notes: Validation schema per OpenAPI.

### IT-USERS-007 — Status change invalid transition
- Priority: P0
- Story IDs: US-USERS-005
- Preconditions: User already Locked.
- Test data: `{ "status": "Locked", "reason": "duplicate" }`.
- Steps: PATCH `/api/v1/users/{id}/status`.
- Expected results: 400 with code `USERS_STATUS_INVALID_TRANSITION` or no-op with clear message; history not duplicated.
- Evidence: API response with correlationId.
- Notes: Clarify final behavior; currently treated as blocked by requirement if rule not finalized.

### IT-USERS-008 — Status change unauthorized user
- Priority: P0
- Story IDs: US-USERS-005
- Preconditions: Role without status-change permission.
- Test data: `{ "status": "Locked", "reason": "unauthorized test" }`.
- Steps: PATCH `/api/v1/users/{id}/status`.
- Expected results: 403 DefaultError; no status change.
- Evidence: API response with correlationId.
- Notes: AuthZ negative.

### IT-USERS-009 — Search rejects invalid filter
- Priority: P0
- Story IDs: US-USERS-006
- Preconditions: Admin token.
- Test data: `page=0` and `pageSize=0` in the same request to trigger validation on both parameters.
- Steps: GET `/api/v1/users?page=0&pageSize=0`.
- Expected results: 400 with code `USERS_SEARCH_FILTER_INVALID`; no data returned.
- Notes: API pagination convention for `/api/v1` requires page/pageSize ≥ 1; 0-based paging is not supported.
- Evidence: API response with correlationId.
- Notes: Validates pagination guardrails.

### IT-USERS-010 — Search rejects pageSize over limit
- Priority: P0
- Story IDs: US-USERS-006
- Preconditions: Admin token.
- Test data: `pageSize=1000` (beyond max).
- Steps: GET `/api/v1/users?page=1&pageSize=1000`.
- Expected results: 400 with code `USERS_SEARCH_FILTER_INVALID`; guidance on max pageSize.
- Evidence: API response with correlationId.
- Notes: Performance safeguard.

### IT-USERS-011 — End User edit blocked when token expired (P1)
- Priority: P1
- Story IDs: US-USERS-004
- Preconditions: User token expired.
- Test data: Valid profile payload.
- Steps: PATCH `/api/v1/users/{id}/kyc` as End User with expired token.
- Expected results: 401 DefaultError; no changes saved; UI prompts re-login.
- Evidence: API response with correlationId; UI message.
- Notes: Blocked until token handling UX defined.

## 5) Security sanity testcases

### SEC-USERS-001 — AuthZ bypass on Ref Tier change
- Priority: P0
- Story IDs: US-USERS-002, US-USERS-005
- Preconditions: Non-privileged token.
- Steps: Attempt PUT `/api/v1/users/{id}/ref-tier`.
- Expected results: 403; no history entry; correlationId present.
- Evidence: API response with correlationId; audit log unchanged.

### SEC-USERS-002 — Injection attempt in search filters
- Priority: P0
- Story IDs: US-USERS-006
- Preconditions: Admin token.
- Test data (example, non-destructive patterns — use only in isolated test environments):
  ```
  employeeCode="T3_001' OR '1'='1'--"
  phone="0900'; SELECT 1; --"
  ```
  Implementations must ensure payloads remain safe and non-destructive; never run against production. Optional additional patterns for breadth: NoSQL-like `{ "$gt": "" }` or XSS `<script>alert(1)</script>` in text fields.
- Steps: GET `/api/v1/users` with injected query params.
- Expected results: 400 validation or empty result; no error leakage; correlationId present.
- Evidence: API response; server logs absence of SQL error.

### SEC-USERS-003 — Rate limit / brute-force on search
- Priority: P0
- Story IDs: US-USERS-006
- Preconditions: Rate limit configured.
- Steps: Burst 100 search requests within limit window.
- Expected results: Excess requests receive 429 DefaultError; no degraded performance; correlationIds logged.
- Evidence: Responses showing 429 after threshold.
- Notes: Blocked if rate-limit threshold not defined.

### SEC-USERS-004 — Import upload size/structure abuse
- Priority: P0
- Story IDs: US-USERS-001
- Preconditions: Admin token.
- Steps: Upload oversized or malformed file repeatedly.
- Expected results: 413/400 with stable error code; system does not start job; audit logs include correlationId without PII.
- Evidence: API responses; audit log entry.

### SEC-USERS-005 — PII masking in errors and logs for KYC
- Priority: P0
- Story IDs: US-USERS-003, US-USERS-004
- Preconditions: Admin or End User session.
- Steps: Submit invalid KYC with fake/full-format CCCD (Vietnam citizen ID) only; trigger validation failure.
- Expected results: Error message masks sensitive fields; no full CCCD/birthDate in logs; correlationId present.
- Evidence: Error payload; sanitized server log snippet (if accessible).

### SEC-USERS-006 — Session expiry during End User profile update (P1)
- Priority: P1
- Story IDs: US-USERS-004
- Preconditions: Token about to expire.
- Steps: Begin profile edit, let token expire, submit.
- Expected results: Request rejected (401); no partial update; user prompted to re-auth.
- Evidence: API response with correlationId; UI prompt screenshot.

## 6) Regression Suite
- E2E-USERS-001, E2E-USERS-002, E2E-USERS-003, E2E-USERS-004, E2E-USERS-005
- IT-USERS-001, IT-USERS-002, IT-USERS-003, IT-USERS-004, IT-USERS-005, IT-USERS-007, IT-USERS-009
- SEC-USERS-001, SEC-USERS-002, SEC-USERS-004, SEC-USERS-005
- Note: Excludes IT-USERS-006/008/010/011 and SEC-USERS-003/006 from smoke regression because they are validation-volume, rate-limit, or session-expiry scenarios better suited for extended suites.

## 7) Open Questions / blockers
- OQ-USERS-001: Final import file schema, required columns, and dedupe/overwrite policy are TBD → blocks E2E-USERS-001, IT-USERS-001, IT-USERS-002, SEC-USERS-004.
- OQ-USERS-002: Allowed vs locked KYC fields for Admin and End User not finalized → blocks E2E-USERS-003, E2E-USERS-006, IT-USERS-005, IT-USERS-006, IT-USERS-011, SEC-USERS-005.
- OQ-USERS-003: Status transition rules for repeat locking/no-op vs error not finalized → blocks IT-USERS-007 behavior expectation.
- OQ-USERS-004: Rate-limit thresholds for search/import not defined → blocks SEC-USERS-003 expectation thresholds.
- OQ-USERS-005: No ScreenSpec `SC-USERS-*` published; UI behavior inferred from stories → UI evidence may vary when specs arrive.
