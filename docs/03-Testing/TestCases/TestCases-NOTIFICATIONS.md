
# Test Cases — NOTIFICATIONS: Notifications

## Section 1: Scope & assumptions
- Scope: Module NOTIFICATIONS across all endpoints tagged `NOTIFICATIONS` in `openapi.yaml`, covering SRS-NOTIFICATIONS FR-001 → FR-005 and Stories US-NOTIFICATIONS-001 → 005 (no StoryPack provided).
- Screens: No `SC-NOTIFICATIONS-*` ScreenSpecs; UI coverage is API-driven.
- Error shape: `{ "error": { "code", "message", "correlationId" } }` per `Error-Conventions.md`; no PII in errors/logs.
- Assumptions / open points that impact tests:
  - Q-NOTI-004: Rate-limit thresholds for OTP/event sends not defined → related tests marked “Blocked by requirement”.
  - Masking format for phone numbers in logs (e.g., `0900***456`) and fields allowed to be shown in Support UI need confirmation (Q-NOTI-006).
  - Opt-in/opt-out preferences and additional channels are out of MVP scope (A-NOTI-001/003).
  - Idempotency/replay behavior follows `Idempotency-Key` header and/or `eventId` when provided; duplicates should not create extra sends.

## Section 2: Test data set (fake)
- Actors/tokens (fake): `admin_super` (Super Admin), `ops_admin` (Ops), `support_agent` (Support), `auth_service` (AUTH service account), `orders_service` (ORDERS/PAYMENTS service account).
- Phone numbers: `0900123456`, `0900456789` (expected masked as `0900***456`, `0900***789` in logs).
- Templates:
  - OTP: code `ZNS_OTP_LOGIN`, type `OTP`, body `Ma OTP cua ban la {{otpCode}}, hieu luc {{ttlMinutes}} phut. KHONG chia se ma nay voi bat ky ai.`, status `ACTIVE`.
  - Pickup reminder: code `ZNS_REMIND_PICKUP`, type `PICKUP_REMINDER`, body `Don {{order_code}} san sang tai {{pickup_point}}, nhan truoc {{pickup_deadline}}.`, status `ACTIVE`.
  - Debt reminder: code `ZNS_DEBT_REMINDER`, type `DEBT_REMINDER`, body `Cong no {{debt_amount}} VND, han {{due_date}}.`, status `ACTIVE`.
- Company config samples:
  - Company 100: all groups enabled.
  - Company 200: pickup reminder disabled, others enabled.
- CorrelationIds/IdempotencyKeys: `corr-otp-0001`, `corr-event-0001`, `idem-otp-001`, `idem-event-001` (unique per run).
- Event payload sample: `eventType=ORDER_READY_FOR_PICKUP`, `companyId=100`, `recipients=["0900123456"]`, `payload={"order_code":"ORD-2026-0001","pickup_point":"Diem nhan A","pickup_deadline":"2026-01-10"}`, optional `eventId=evt-pickup-0001`.

## Section 3: E2E testcases (P0 then P1)

### E2E-NOTIFICATIONS-001 — Admin creates OTP template and AUTH triggers OTP send (happy path)
- Priority: P0
- Story IDs: US-NOTIFICATIONS-001, US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: `admin_super` authenticated; template code `ZNS_OTP_LOGIN` not yet existing or can be updated to ACTIVE.
- Test data (fake): create template body above; OTP request `{phoneNumber:"0900123456", otpCode:"123456", ttlMinutes:5, templateCode:"ZNS_OTP_LOGIN"}`, Idempotency-Key `idem-otp-001`, correlationId `corr-otp-0001`.
- Steps:
  1. `admin_super` calls `POST /api/v1/notifications/templates` to create OTP template (or PATCH existing to ACTIVE).
  2. `auth_service` calls `POST /api/v1/notifications/otp/send` with request body and Idempotency-Key.
  3. `support_agent` calls `GET /api/v1/notifications/logs?correlationId=corr-otp-0001&page=1&pageSize=10`.
- Expected results:
  - Step 1: 201 with created template detail; status `ACTIVE`.
  - Step 2: 200 with `status=ACCEPTED` (or provider success), correlationId header present, no PII in response.
  - Step 3: 200 paged list contains entry with `recipientMasked=0900***456`, `templateCode=ZNS_OTP_LOGIN`, `status=SUCCESS/ACCEPTED`, providerMessageId present/nullable; body does not expose full OTP.
- Evidence to capture: API responses (template create, OTP send, log search) and correlationId headers.
- Notes: Validates main happy path for OTP delivery.

### E2E-NOTIFICATIONS-002 — Event notification sent when company config enabled
- Priority: P0
- Story IDs: US-NOTIFICATIONS-003
- Screen IDs: N/A
- Preconditions: `admin_super` authenticated; template `ZNS_REMIND_PICKUP` ACTIVE; company 100 config enables pickup reminders.
- Test data (fake): event request per Section 2 with Idempotency-Key `idem-event-001`, correlationId `corr-event-0001`.
- Steps:
  1. `admin_super` calls `PUT /api/v1/notifications/config/companies/100` setting `enablePickupReminder=true`.
  2. `orders_service` calls `POST /api/v1/notifications/events/send` with event request and Idempotency-Key.
  3. `support_agent` searches logs by correlationId `corr-event-0001`.
- Expected results:
  - Step 1: 200 returns config for company 100 with pickup reminder enabled.
  - Step 2: 200 with `status=ACCEPTED`, correlationId header present.
  - Step 3: Log entry shows `eventType=ORDER_READY_FOR_PICKUP`, `templateCode=ZNS_REMIND_PICKUP`, masked recipient, status success; no payload PII beyond masked phone.
- Evidence to capture: Config update response, event send response, log entry with correlationId.
- Notes: Covers main business-event flow and config usage.

### E2E-NOTIFICATIONS-003 — Support searches notification logs by correlationId (masking check)
- Priority: P1
- Story IDs: US-NOTIFICATIONS-004
- Screen IDs: N/A
- Preconditions: At least one log exists (from E2E-NOTIFICATIONS-001/002); `support_agent` authenticated.
- Test data (fake): correlationId from prior run; filters `page=1&pageSize=5`.
- Steps:
  1. Call `GET /api/v1/notifications/logs?correlationId=<existing>&page=1&pageSize=5`.
  2. Verify pagination and masking rules.
- Expected results:
  - 200 with `items` including the target log; `recipientMasked` present, no full phone or OTP content.
  - Pagination fields `page`, `totalItems`, `totalPages` populated.
  - CorrelationId header returned.
- Evidence to capture: Log search response highlighting masking and pagination.
- Notes: Ensures troubleshooting flow and PII masking.

### E2E-NOTIFICATIONS-004 — Toggle company config off and suppress event notification
- Priority: P1
- Story IDs: US-NOTIFICATIONS-003, US-NOTIFICATIONS-005
- Screen IDs: N/A
- Preconditions: Template `ZNS_REMIND_PICKUP` ACTIVE; company 200 exists with pickup reminder disabled; `admin_super` authenticated.
- Test data (fake): eventType `ORDER_READY_FOR_PICKUP`, companyId `200`, recipient `0900456789`, Idempotency-Key `idem-event-002`, correlationId `corr-event-0002`.
- Steps:
  1. `admin_super` sets company 200 config with `enablePickupReminder=false`.
  2. `orders_service` calls `POST /api/v1/notifications/events/send` for company 200.
  3. Query logs by `corr-event-0002`.
- Expected results:
  - Step 1: 200 returns config showing pickup reminder disabled.
  - Step 2: 409 with `error.code=NOTIFICATIONS_GROUP_DISABLED`, correlationId header present; no send to ZNS.
  - Step 3: Log entry (status `SUPPRESSED`/failure) records reason group disabled; no outbound messageId.
- Evidence to capture: Config response, error response, log entry with suppression reason.
- Notes: Negative journey validating company-level toggle.

## Section 4: Integration/API testcases

### IT-NOTIFICATIONS-001 — Reject duplicate template code in ACTIVE status
- Priority: P0
- Story IDs: US-NOTIFICATIONS-001
- Screen IDs: N/A
- Preconditions: Template `ZNS_OTP_LOGIN` already ACTIVE.
- Test data (fake): Create request with same `code=ZNS_OTP_LOGIN`, status `ACTIVE`.
- Steps:
  1. Call `POST /api/v1/notifications/templates` with duplicate code.
- Expected results:
  - 409 with `error.code=NOTIFICATIONS_TEMPLATE_CODE_DUPLICATE`, correlationId present; no new record created.
- Evidence to capture: Error response and correlationId.
- Notes: Ensures uniqueness constraint on template code.

### IT-NOTIFICATIONS-002 — Validate placeholder whitelist on template body
- Priority: P0
- Story IDs: US-NOTIFICATIONS-001
- Screen IDs: N/A
- Preconditions: Admin authenticated.
- Test data (fake): Create/update request with body containing `{{unknown_placeholder}}`.
- Steps:
  1. Call `POST /api/v1/notifications/templates` (or PATCH) with invalid placeholder.
- Expected results:
  - 400 with validation error message indicating invalid placeholder; correlationId present; template not saved.
- Evidence to capture: Error response and correlationId.
- Notes: Blocks saving malformed templates.

### IT-NOTIFICATIONS-003 — Reject OTP send with invalid phone format
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: Template `ZNS_OTP_LOGIN` ACTIVE.
- Test data (fake): OTP request with `phoneNumber="0900abc456"`, Idempotency-Key `idem-otp-bad`.
- Steps:
-  1. Call `POST /api/v1/notifications/otp/send` with invalid phone.
- Expected results:
  - 400 validation error; no ZNS call; correlationId present.
- Evidence to capture: Error response; ensure no log entry created.
- Notes: Covers V-NOTI-001.

### IT-NOTIFICATIONS-004 — Reject OTP send when template inactive/missing
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: Template `ZNS_OTP_LOGIN` set to `INACTIVE` or deleted.
- Test data (fake): OTP request with valid phone and otpCode.
- Steps:
  1. Call `POST /api/v1/notifications/otp/send`.
- Expected results:
  - 404 with `error.code=NOTIFICATIONS_TEMPLATE_INACTIVE`, correlationId present; no send attempted.
- Evidence to capture: Error response and correlationId.
- Notes: Negative/edge for template lifecycle.

### IT-NOTIFICATIONS-005 — Event send fails when template mapping missing
- Priority: P0
- Story IDs: US-NOTIFICATIONS-003
- Screen IDs: N/A
- Preconditions: No template configured for `eventType="ORDER_READY_FOR_PICKUP"` (remove mapping or use `UNKNOWN_EVENT`).
- Test data (fake): Event request with `eventType="UNKNOWN_EVENT"`.
- Steps:
  1. Call `POST /api/v1/notifications/events/send`.
- Expected results:
  - 404 with `error.code=NOTIFICATIONS_EVENT_TEMPLATE_NOT_CONFIGURED`, correlationId present.
- Evidence to capture: Error response and correlationId.
- Notes: Ensures events without template are blocked.

### IT-NOTIFICATIONS-006 — Event send suppressed by company config
- Priority: P0
- Story IDs: US-NOTIFICATIONS-003, US-NOTIFICATIONS-005
- Screen IDs: N/A
- Preconditions: Company 200 config `enablePickupReminder=false`; template exists.
- Test data (fake): Pickup reminder event for company 200.
- Steps:
  1. Call `POST /api/v1/notifications/events/send` with companyId 200.
- Expected results:
  - 409 with `error.code=NOTIFICATIONS_GROUP_DISABLED`, correlationId present; log records suppression.
- Evidence to capture: Error response and log entry (status SUPPRESSED/failure reason).
- Notes: Validates company toggle application.

### IT-NOTIFICATIONS-007 — Reject event send when payload missing placeholder data
- Priority: P0
- Story IDs: US-NOTIFICATIONS-003
- Screen IDs: N/A
- Preconditions: Template `ZNS_REMIND_PICKUP` ACTIVE.
- Test data (fake): Event request missing `order_code` placeholder.
- Steps:
  1. Call `POST /api/v1/notifications/events/send` with incomplete `payload`.
- Expected results:
  - 400 validation error (missing placeholder data), correlationId present; no send.
- Evidence to capture: Error response and correlationId.
- Notes: Covers V-NOTI-003.

### IT-NOTIFICATIONS-008 — OTP send rate-limit returns 429 (blocked by requirement)
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: Rate-limit threshold defined for OTP (Q-NOTI-004) and configurable in test env.
- Test data (fake): Burst of OTP requests (e.g., 5 rapid calls) for `0900123456` with same template.
- Steps:
  1. Send OTP requests repeatedly until threshold exceeded.
- Expected results:
  - Once threshold hit, response 429 with `error.code=NOTIFICATIONS_OTP_RATE_LIMITED`; earlier requests succeed.
- Evidence to capture: Sequence of responses with correlationIds; log entries showing throttling.
- Notes: **Blocked by requirement Q-NOTI-004 (rate-limit thresholds TBD).**

## Section 5: Security sanity testcases

### SEC-NOTIFICATIONS-001 — Authz: protect admin/search/config endpoints
- Priority: P0
- Story IDs: US-NOTIFICATIONS-001, US-NOTIFICATIONS-004, US-NOTIFICATIONS-005
- Screen IDs: N/A
- Preconditions: Unauthenticated user or non-admin token.
- Test data (fake): Calls to templates list/create, logs search, company config update.
- Steps:
  1. Call `GET /api/v1/notifications/templates` without token.
  2. Call `POST /api/v1/notifications/templates` and `GET /api/v1/notifications/logs` with non-admin token.
- Expected results:
  - 401 for missing token; 403 for insufficient role; no data leaked; correlationId present when authenticated.
- Evidence to capture: Error responses; verify no templates/configs returned.
- Notes: Basic authz sanity.

### SEC-NOTIFICATIONS-002 — Idempotency/replay guard on OTP send
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: Template ACTIVE; idempotency supported.
- Test data (fake): Same OTP payload with Idempotency-Key `idem-otp-replay`.
- Steps:
  1. Send OTP request with Idempotency-Key.
  2. Repeat with identical payload and same Idempotency-Key.
  3. Send again with different Idempotency-Key.
- Expected results:
  - First two calls return same outcome without duplicate ZNS sends/log entries (idempotent).
  - Third call treated as new request (new log entry / providerMessageId).
  - CorrelationIds unique per call; no PII leakage.
- Evidence to capture: Responses, log counts for correlationIds/Idempotency-Key.
- Notes: Protects against replay/duplicate sends.

### SEC-NOTIFICATIONS-003 — Input injection in template body/payload
- Priority: P0
- Story IDs: US-NOTIFICATIONS-001, US-NOTIFICATIONS-003
- Screen IDs: N/A
- Preconditions: Admin authenticated.
- Test data (fake): Template body containing `<script>alert(1)</script>` and event payload containing script-like strings.
- Steps:
  1. Attempt to create/update template with script content.
  2. Send event with payload fields containing script tags, then fetch logs/template detail.
- Expected results:
  - Validation rejects dangerous placeholders if policy disallows; otherwise stored and returned as plain text (escaped) without execution.
  - No script execution when rendering/logging; correlationId present.
- Evidence to capture: Responses, stored body/payload as returned by detail/log endpoints.
- Notes: Ensures sanitization/escaping of dynamic content.

### SEC-NOTIFICATIONS-004 — OTP brute-force/rate-limit sanity (blocked by requirement)
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002
- Screen IDs: N/A
- Preconditions: Rate-limit threshold available (Q-NOTI-004).
- Test data (fake): Rapid OTP requests for same phone/template.
- Steps:
  1. Send OTP requests in quick succession until threshold reached.
- Expected results:
  - 429 with `NOTIFICATIONS_OTP_RATE_LIMITED` after threshold; earlier calls succeed.
  - No PII in error message; correlationId present.
- Evidence to capture: Response sequence and correlationIds.
- Notes: **Blocked by requirement Q-NOTI-004.**

### SEC-NOTIFICATIONS-005 — Expired/invalid token handling on send endpoints
- Priority: P1
- Story IDs: US-NOTIFICATIONS-002, US-NOTIFICATIONS-003
- Screen IDs: N/A
- Preconditions: Expired token or invalid signature.
- Test data (fake): OTP/event send requests with expired token.
- Steps:
  1. Call `/api/v1/notifications/otp/send` with expired token.
  2. Call `/api/v1/notifications/events/send` with invalid token.
- Expected results:
  - 401 default error; no state change/log entry created.
- Evidence to capture: Error responses; confirm absence of new logs.
- Notes: Session/token handling sanity.

### SEC-NOTIFICATIONS-006 — PII masking in logs and errors
- Priority: P0
- Story IDs: US-NOTIFICATIONS-002, US-NOTIFICATIONS-003, US-NOTIFICATIONS-004
- Screen IDs: N/A
- Preconditions: Logs exist for OTP/event sends.
- Test data (fake): Use correlationIds from prior sends.
- Steps:
  1. Fetch logs via `/api/v1/notifications/logs`.
  2. Trigger controlled error (e.g., invalid phone) and inspect response/log.
- Expected results:
  - Logs show masked phone (`0900***456`), no OTP content; errors do not expose PII.
  - CorrelationId present for traceability.
- Evidence to capture: Log response snippets and error response.
- Notes: Aligns with BR-NOTIFICATIONS-003.

## Section 6: Regression Suite (IDs list)
- REG-NOTIFICATIONS-001 — OTP happy path (reuse E2E-NOTIFICATIONS-001).
- REG-NOTIFICATIONS-002 — Template validation guards (IT-NOTIFICATIONS-001, IT-NOTIFICATIONS-002).
- REG-NOTIFICATIONS-003 — Event send with config on/off (E2E-NOTIFICATIONS-002, E2E-NOTIFICATIONS-004, IT-NOTIFICATIONS-006).
- REG-NOTIFICATIONS-004 — Log search & masking (E2E-NOTIFICATIONS-003, SEC-NOTIFICATIONS-006).
- REG-NOTIFICATIONS-005 — Idempotency and rate-limit safety (SEC-NOTIFICATIONS-002; SEC-NOTIFICATIONS-004 once unblocked).

## Section 7: Open Questions / blockers
- Q-NOTI-004: Exact OTP/event rate-limit thresholds and expected reset window. Tests IT-NOTIFICATIONS-008 and SEC-NOTIFICATIONS-004 **blocked** until defined.
- Q-NOTI-006: Masking format and which roles can view partially masked data in logs. Affects verification steps in E2E-NOTIFICATIONS-003 and SEC-NOTIFICATIONS-006.
- Q-NOTI-001/002/003: Additional notification types, multi-channel support, and user opt-in/opt-out remain undefined; future test coverage may expand when clarified.
- Idempotency behavior for `eventId` vs `Idempotency-Key` on `/events/send` should be confirmed to assert duplicate suppression confidently.
