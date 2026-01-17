# Test Cases — PAYMENTS: Payments

## Section 1: Scope & assumptions
- Scope: Full PAYMENTS module across SRS-PAYMENTS, Stories US-PAYMENTS-001 → 005, and OpenAPI `PAYMENTS` tag (`/api/v1/payments/*`). No StoryPacks or ScreenSpecs (`SC-PAYMENTS-*`) are provided; UI coverage is API-driven.
- Error shape: `{ "error": { "code", "message", "correlationId" } }` per Error-Conventions.md; no PII in error/log messages.
- Idempotency: Endpoints supporting side effects declare `Idempotency-Key`; tests assert non-duplication for retries.
- Amount fields follow V-PAYMENTS-001 (non-negative) and must align with order totals for COD.
- Open questions in SRS (Q-PAYMENTS-001 → 004) remain; cases depending on unresolved business policy are marked "Blocked by requirement".

## Section 2: Test data set (fake)
- Users/roles: `user_t3` (userId=70010, companyId=5001), `shipper_200` (shipperId=200, t2 role), `finance_01` (finance admin), `ops_admin_01` (ops).
- Orders: `order_cod_1` (orderId=71001, amount=43500, paymentMethod=COD, status=DELIVERED), `order_credit_1` (orderId=72001, amount=400000, paymentMethod=CREDIT, status=PENDING_PAYMENT).
- Shifts: `shift_open_1` (shiftId=30001, shipperId=200, status=OPEN, expectedCod=43500), `shift_closed_1` (shiftId=30002, status=CLOSED, receipts aggregated).
- Credit data: `creditLimit_user_t3` = 2_000_000, `currentDebt_user_t3` = 1_500_000.
- Idempotency keys: `idem-rec-###`, `idem-shift-close-###`, `idem-reconcile-###`, `idem-credit-###`.
- Timestamps: use UTC ISO strings (e.g., `2026-01-05T03:05:00Z`).

## Section 3: E2E testcases (P0 then P1)

### E2E-PAYMENTS-001 — COD receipt → close shift → finance reconcile (happy path)
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002, US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: `order_cod_1` delivered and assigned to `shipper_200`; `shift_open_1` exists for same shipper; actors authenticated.
- Test data (fake): request for receipt `{orderId:71001, amount:43500, note:"COD full"}`, Idempotency-Key `idem-rec-001`; shift close key `idem-shift-close-001`; reconcile key `idem-reconcile-001` with note "Matched".
- Steps:
  1. Shipper calls `POST /api/v1/payments/receipts` with payload and Idempotency-Key.
  2. Shipper calls `GET /api/v1/payments/shifts/my-open` and verifies the receipt is counted in collectedCod.
  3. Shipper calls `POST /api/v1/payments/shifts/my-open/close` with Idempotency-Key `idem-shift-close-001`.
  4. Finance calls `GET /api/v1/payments/shifts` (filter status=CLOSED) and opens the closed shift detail `GET /api/v1/payments/shifts/{id}`.
  5. Finance calls `POST /api/v1/payments/shifts/{id}/reconcile` with status `RECONCILED`, note "Matched", Idempotency-Key `idem-reconcile-001`.
- Expected results:
  - Step 1: 201 with `PaymentReceipt` (id, orderId, amount, collectedBy, collectedAt, shiftId populated); order paymentStatus set to PAID (per SRS FR-001/002).
  - Step 2: 200 with `PaymentShiftDetail` showing collectedCod=43500, receiptsCount>=1, status=OPEN.
  - Step 3: 200 with `status=CLOSED`, `expectedCod=43500`, `collectedCod=43500`, `difference=0`; further receipt creation for the shift rejected (checked implicitly via status).
  - Step 4: List includes the closed shift; detail shows receiptsCount and no reconciliation yet.
  - Step 5: 200 with `status=RECONCILED`, `reconciledBy=finance_01`, `reconciledAt` set; subsequent reconcile attempts with same Idempotency-Key are idempotent (no duplicate state change).
  - CorrelationId returned for each call; error messages absent; no PII leaked.
- Evidence to capture: API responses from each step, correlationId, computed totals.
- Notes: Serves as main journey; also validates idempotency headers.

### E2E-PAYMENTS-002 — Configure credit limit and validate deferred payment check
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: `finance_01` authenticated; `user_t3` exists with currentDebt=1_500_000.
- Test data (fake): `PUT /api/v1/payments/credit-limits/{userId}` body `{limitAmount:2000000,isActive:true,effectiveFrom:"2026-01-01T00:00:00Z"}`; credit check requests with orderAmount 400000 then 600000.
- Steps:
  1. Finance upserts credit limit for `user_t3`.
  2. Call `GET /api/v1/payments/credit-limits/{userId}` to verify persisted values.
  3. ORDERS service (simulated) calls `POST /api/v1/payments/credit/check` with orderAmount 400000, Idempotency-Key `idem-credit-001`.
  4. Repeat credit check with orderAmount 600000, Idempotency-Key `idem-credit-002`.
- Expected results:
  - Step 1: 200 with `PaymentCreditLimit` reflecting limitAmount 2_000_000, effectiveFrom set, isActive true.
  - Step 2: 200 returns same limit; effective dates intact.
  - Step 3: 200 with `allowed=true`, `creditLimit=2_000_000`, `currentDebt=1_500_000`, `debtAfterOrder=1_900_000`, `failureReasonCode=null`.
  - Step 4: 200 with `allowed=false`, `failureReasonCode` populated (e.g., CREDIT_LIMIT_EXCEEDED), `debtAfterOrder` > creditLimit.
  - CorrelationId present; no sensitive info in message.
- Evidence to capture: limit upsert response, credit check responses, correlationIds.
- Notes: Debt balance update after successful order is expected downstream (ORDERS); not directly covered by this API set.

### E2E-PAYMENTS-003 — Shift difference handling (P1)
- Priority: P1
- Story IDs: US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: `shift_closed_1` has expectedCod=1_500_000, collectedCod=1_480_000, status=CLOSED; finance user authenticated.
- Test data (fake): reconcile request `{status:"DIFFERENCE", note:"Short 20k - customer paid later"}`, Idempotency-Key `idem-reconcile-010`.
- Steps:
  1. Finance calls `GET /api/v1/payments/shifts/{id}` for `shift_closed_1`.
  2. Finance calls `POST /api/v1/payments/shifts/{id}/reconcile` with status DIFFERENCE and note.
  3. Finance re-reads shift detail to confirm persisted note and status.
- Expected results:
  - Step 1: 200 detail shows difference=-20_000, status=CLOSED.
  - Step 2: 200 with status=DIFFERENCE, reconciliationNote stored, processedBy set.
  - Step 3: detail unchanged, preventing further receipt edits (implied by status).
  - CorrelationId present.
- Evidence to capture: reconcile request/response, final shift detail.
- Notes: Follow-up adjustment workflow is outside PAYMENTS scope (Open Question Q-PAYMENTS-004).

## Section 4: Integration/API testcases

### IT-PAYMENTS-001 — Reject COD receipt with invalid amount
- Priority: P0
- Story IDs: US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: `order_cod_1` payable via COD and in delivered state; shipper authenticated.
- Test data (fake): payload `{orderId:71001, amount:-1}` then `{orderId:71001, amount:999999}`.
- Steps:
  1. Call `POST /api/v1/payments/receipts` with negative amount.
  2. Retry with amount not matching order total.
- Expected results:
  - 400 for both with `error.code` indicating validation (e.g., PAYMENTS_INVALID_AMOUNT or PAYMENTS_AMOUNT_MISMATCH); correlationId returned.
  - No receipt persisted; order paymentStatus unchanged (UNPAID/PENDING).
- Evidence to capture: error responses, follow-up `GET /api/v1/payments/shifts/my-open` showing unchanged totals.
- Notes: Covers V-PAYMENTS-001 and FR-PAYMENTS-002 validation.

### IT-PAYMENTS-002 — Duplicate COD receipt blocked via idempotency
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: Order eligible for COD receipt; shipper authenticated.
- Test data (fake): same payload as valid receipt; Idempotency-Key `idem-rec-dup-01`.
- Steps:
  1. Call `POST /api/v1/payments/receipts` with Idempotency-Key `idem-rec-dup-01`.
  2. Repeat the same call with identical headers/body.
- Expected results:
  - First call 201 with `PaymentReceipt`.
  - Second call returns 409 (or 200 idempotent response) with no duplicate record; paymentStatus not double-counted; correlationId present.
- Evidence to capture: both responses, receipt count before/after, order paymentStatus.
- Notes: Ensures payment status is single-source and idempotent (AC3 US-PAYMENTS-001).

### IT-PAYMENTS-003 — Close shift fails when no open shift
- Priority: P0
- Story IDs: US-PAYMENTS-003
- Screen IDs: N/A
- Preconditions: Shipper has no OPEN shift (all CLOSED).
- Test data (fake): Idempotency-Key `idem-shift-close-err-01`.
- Steps:
  1. Call `POST /api/v1/payments/shifts/my-open/close` without an open shift.
- Expected results:
  - 400 with `error.code` such as PAYMENTS_SHIFT_NOT_FOUND_OR_STATE_INVALID; correlationId present.
  - No shift state changes created.
- Evidence to capture: error response, shift list for shipper.
- Notes: Negative for AC1/AC3 of US-PAYMENTS-003.

### IT-PAYMENTS-004 — Prevent closing shift with missing receipts
- Priority: P0
- Story IDs: US-PAYMENTS-003
- Screen IDs: N/A
- Preconditions: `shift_open_1` contains at least one COD order without receipt; shipper authenticated.
- Test data (fake): Idempotency-Key `idem-shift-close-err-02`.
- Steps:
  1. Attempt `POST /api/v1/payments/shifts/my-open/close` while missing receipts.
- Expected results:
  - 400 with descriptive `error.code` (e.g., PAYMENTS_SHIFT_HAS_UNCOLLECTED_ORDERS); correlationId present.
  - Shift remains OPEN; missing orders enumerated if supported.
- Evidence to capture: error response, shift detail before/after.
- Notes: Covers edge case EC1 US-PAYMENTS-003.

### IT-PAYMENTS-005 — Reconcile shift invalid state
- Priority: P0
- Story IDs: US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: Shift still OPEN or already RECONCILED; finance authenticated.
- Test data (fake): `shiftId` not in CLOSED state; payload `{status:"RECONCILED"}`.
- Steps:
  1. Call `POST /api/v1/payments/shifts/{id}/reconcile` on non-closable state.
- Expected results:
  - 400 with `error.code` such as PAYMENTS_SHIFT_NOT_RECONCILABLE; correlationId present.
  - No mutation to reconciliation fields.
- Evidence to capture: error response, shift detail unchanged.
- Notes: Negative coverage for AC4 US-PAYMENTS-004.

### IT-PAYMENTS-006 — Credit check declines when exceeding limit
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: creditLimit=2_000_000, currentDebt=1_500_000.
- Test data (fake): credit check payload `{userId:70010, orderAmount:600000}`, Idempotency-Key `idem-credit-err-01`.
- Steps:
  1. Call `POST /api/v1/payments/credit/check` with above payload.
- Expected results:
  - 200 with `allowed=false`, `failureReasonCode` populated (e.g., CREDIT_LIMIT_EXCEEDED), `debtAfterOrder=2_100_000`, correlationId present.
  - No debt balance mutation.
- Evidence to capture: response body, correlationId.
- Notes: Negative for AC1 US-PAYMENTS-005.

### IT-PAYMENTS-007 — Reject credit check when company not eligible / limit disabled
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: company has credit disabled or limitAmount=0.
- Test data (fake): payload `{userId:70010, companyId:5001, orderAmount:100000}`, Idempotency-Key `idem-credit-err-02`.
- Steps:
  1. Call `POST /api/v1/payments/credit/check` when limitAmount=0 or isActive=false.
- Expected results:
  - 400 with `error.code` like PAYMENTS_CREDIT_NOT_ALLOWED or PAYMENTS_LIMIT_INACTIVE; correlationId present.
  - `allowed` not returned (error path).
- Evidence to capture: error response.
- Notes: Covers EC1 US-PAYMENTS-005.

### IT-PAYMENTS-008 — Credit limit update validation (dates and amount)
- Priority: P1
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: finance authenticated.
- Test data (fake): payload `{limitAmount:-1, effectiveFrom:"2025-12-31T00:00:00Z"}` then payload with `effectiveTo` earlier than `effectiveFrom`.
- Steps:
  1. Call `PUT /api/v1/payments/credit-limits/{userId}` with negative limit.
  2. Retry with `effectiveTo` < `effectiveFrom`.
- Expected results:
  - Both return 400 with validation error codes; correlationId present.
  - No limit stored/updated.
- Evidence to capture: error responses.
- Notes: Validation per OpenAPI schema for PaymentCreditLimitUpdateRequest.

### IT-PAYMENTS-009 — Payment status idempotency on repeated paid events
- Priority: P1
- Story IDs: US-PAYMENTS-001
- Screen IDs: N/A
- Preconditions: Order transitions to PAID via a valid receipt; system supports retry using same Idempotency-Key.
- Test data (fake): reuse payload from IT-PAYMENTS-002, Idempotency-Key `idem-rec-status-01`.
- Steps:
  1. Create receipt to move paymentStatus to PAID.
  2. Retry with same Idempotency-Key.
  3. Query order/payment status via ORDER integration or audit log (as available).
- Expected results:
  - Payment status remains PAID and amount not doubled.
  - Second call returns idempotent response (409 or 200 with same body); correlationId present.
- Evidence to capture: responses, order payment status.
- Notes: Aligns with AC2/AC3 US-PAYMENTS-001.

## Section 5: Security sanity testcases

### SEC-PAYMENTS-001 — Authz: Shipper cannot create receipt for another shipper’s order
- Priority: P0
- Story IDs: US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: `order_cod_1` assigned to different shipper; `shipper_200` authenticated.
- Test data (fake): valid receipt payload but mismatched assignment.
- Steps:
  1. Call `POST /api/v1/payments/receipts` for the foreign order.
- Expected results:
  - 403 with `error.code` (e.g., PAYMENTS_RECEIPT_FORBIDDEN); correlationId present; no receipt created.
- Evidence to capture: error response, audit/log entry if exposed.
- Notes: Ensures deny-by-default.

### SEC-PAYMENTS-002 — Authn: Expired/invalid token on shift close
- Priority: P0
- Story IDs: US-PAYMENTS-003
- Screen IDs: N/A
- Preconditions: expired access token.
- Test data (fake): shift close request with Idempotency-Key `idem-shift-close-auth-01`.
- Steps:
  1. Call `POST /api/v1/payments/shifts/my-open/close` with expired token.
- Expected results:
  - 401 DefaultError, correlationId present; no shift state change.
- Evidence to capture: error response headers/body.
- Notes: Validates session enforcement.

### SEC-PAYMENTS-003 — Idempotency abuse attempt on reconciliation
- Priority: P1
- Story IDs: US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: Shift already RECONCILED.
- Test data (fake): repeat `POST /api/v1/payments/shifts/{id}/reconcile` with new Idempotency-Key `idem-reconcile-abuse-01` and altered status.
- Steps:
  1. Re-run reconcile with different payload after success.
- Expected results:
  - 409/400 preventing state flip; audit immutable; correlationId present.
- Evidence to capture: error response, shift detail unchanged.
- Notes: Guards against tampering after reconciliation lock.

### SEC-PAYMENTS-004 — Input injection on receipt note
- Priority: P1
- Story IDs: US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: order eligible; shipper authenticated.
- Test data (fake): note field `"<script>alert('x')</script>"`.
- Steps:
  1. Call `POST /api/v1/payments/receipts` with injection note.
- Expected results:
  - 201 or 400 with sanitized/escaped note storage; response body and logs do not execute or echo raw script; correlationId present.
- Evidence to capture: stored note retrieval (if API returns), logs if available.
- Notes: Validates input sanitization; ensure no script reflection.

### SEC-PAYMENTS-005 — Rate limiting / brute-force on credit check
- Priority: P1
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Rate limit policy enabled.
- Test data (fake): burst of 20 requests with same payload `{userId:70010, orderAmount:100000}`, Idempotency-Key rotated.
- Steps:
  1. Send rapid repeated `POST /api/v1/payments/credit/check` calls.
- Expected results:
  - After threshold, 429 or throttling response with correlationId; no leakage of internal details; earlier requests still validated correctly.
- Evidence to capture: sequence of responses, headers (Retry-After if provided).
- Notes: Covers security sanity for misuse/abuse.

### SEC-PAYMENTS-006 — PII absence in error messages
- Priority: P1
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Trigger validation errors (reuse IT-PAYMENTS-001 and IT-PAYMENTS-007).
- Test data (fake): invalid payloads from referenced tests.
- Steps:
  1. Inspect error responses for receipt and credit check validation failures.
- Expected results:
  - Error messages generic, no user name/phone/account numbers; correlationId present.
- Evidence to capture: error bodies.
- Notes: Ensures compliance with security rule (no PII in errors).

## Section 6: Regression Suite (IDs list)
- REG-PAYMENTS-001 — Execute E2E-PAYMENTS-001 (main COD lifecycle).
- REG-PAYMENTS-002 — Execute E2E-PAYMENTS-002 (credit limit + credit check).
- REG-PAYMENTS-003 — Execute IT-PAYMENTS-002 (receipt idempotency).
- REG-PAYMENTS-004 — Execute IT-PAYMENTS-006 (credit limit exceed decline).
- REG-PAYMENTS-005 — Execute SEC-PAYMENTS-001 (authz guard on receipt creation).

## Section 7: Open Questions / blockers
- Q-PAYMENTS-001/002 (limit calculation & collection schedule): Needed to assert debt update timing; tests touching debt mutations are blocked.
- Q-PAYMENTS-003 (payment gateway integration): No online transfer flow defined; transfer-specific cases omitted.
- Q-PAYMENTS-004 (difference handling workflow): Post-reconciliation adjustment flow unspecified; tests only assert state and note persistence.
- ScreenSpecs for Shipper/Finance UI are missing; UI evidence relies on API responses/logs.
