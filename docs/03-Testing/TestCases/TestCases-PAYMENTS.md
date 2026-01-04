# Test Cases — PAYMENTS: Payments

## Section 1: Scope & assumptions
- Scope: Entire PAYMENTS module covering SRS-PAYMENTS, Stories US-PAYMENTS-001 → 005, and OpenAPI paths under `/api/v1/payments/*` (receipts, shifts, credit, credit-limits, debts). No StoryPacks for PAYMENTS. No ScreenSpec `SC-PAYMENTS-*`; UI steps are API-driven.
- Error shape: `Error-Conventions.md` (`error.code`, `message`, `correlationId`), no PII in messages/logs. CorrelationId captured for every call.
- Idempotency: Endpoints marked with `Idempotency-Key` must reject duplicates and avoid double-charging (receipts, close shift, reconcile, credit check).
- Roles: Shipper/Tầng 2 can create receipts & close their own shift; Finance can list and reconcile shifts, manage credit limits/debts; ORDERS supplies order/payment state; USERS supplies shipper/user identity.
- Assumptions:
  - Payment status update on receipt creation is observable via PAYMENTS response and/or ORDERS detail (integration hook assumed available).
  - Credit/debt updates are triggered by ORDERS after `credit/check`; debt read via `/payments/debts/{userId}`.
  - Chênh lệch COD policy thresholds (tolerances) and adjustment workflow are TBD; tests note blockers where relevant.

## Section 2: Test data set (fake)
- Users/Roles: `shipper_200` (Shipper with open shift), `shipper_201` (Shipper without access to target order), `finance_01` (Finance/Admin), `ops_admin_01` (Ops observer), `user_t3` (End User, userId=10030, companyId=501).
- Orders:
  - `order_cod_1` (orderId=70001, amount=43_500, paymentMethod=COD, status ready for receipt, assigned to `shipper_200`, shiftId=30001).
  - `order_cod_diff` (orderId=70002, amount=50_000, paymentMethod=COD, assigned to `shipper_200`, shiftId=30002).
  - `order_credit_1` (orderId=71000, amount=400_000, paymentMethod=CREDIT, companyId=501, userId=10030).
- Shifts: `shift_open_30001` (OPEN for `shipper_200`), `shift_closed_30002` (CLOSED after difference scenario).
- Credit/debt baseline: creditLimit=2_000_000, currentDebt=1_500_000 for `user_t3` (companyId=501).
- Idempotency keys: `idem-receipt-001`, `idem-receipt-dup`, `idem-close-30001`, `idem-reconcile-30002`, `idem-creditcheck-001`.
- Timestamps: ISO 8601 UTC strings e.g., `2026-01-05T03:05:00Z`.

## Section 3: E2E testcases (P0 first, then P1)

### E2E-PAYMENTS-001 — COD receipt → shift close → reconciliation (happy path)
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002, US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A (no SC-PAYMENTS defined)
- Preconditions: `shipper_200` authenticated with OPEN shift `shift_open_30001`; `order_cod_1` delivered/eligible for COD receipt; `finance_01` authenticated.
- Test data (fake): orderId=70001, amount=43_500, shiftId=30001, Idempotency-Key `idem-receipt-001`.
- Steps:
  1. Shipper calls `POST /api/v1/payments/receipts` with orderId 70001, amount 43_500, Idempotency-Key `idem-receipt-001`.
  2. Shipper calls `GET /api/v1/payments/shifts/my-open` to confirm receipt count/collectedCod updated.
  3. Shipper calls `POST /api/v1/payments/shifts/my-open/close` with Idempotency-Key `idem-close-30001`.
  4. Finance calls `GET /api/v1/payments/shifts` (status=CLOSED, shipperId filter) to locate shiftId.
  5. Finance calls `POST /api/v1/payments/shifts/{id}/reconcile` with status `RECONCILED`, note "COD matched", Idempotency-Key `idem-reconcile-30002`.
- Expected results:
  - Step 1: 201 with `PaymentReceipt` showing orderId, amount=43_500, collectedBy `shipper_200`, shiftId populated; correlationId returned; paymentStatus for order reflected as PAID (via PAYMENTS/ORDERS link).
  - Step 3: 200 `PaymentShiftDetail` status=CLOSED, collectedCod equals sum of receipts, difference=0; receipts locked for that shift.
  - Step 4: Paged list includes the closed shift with correct totals.
  - Step 5: 200 `PaymentShiftDetail` status=RECONCILED, reconciliation note saved; subsequent reconcile attempts with same idempotency key do not duplicate.
  - Error shape follows `Error-Conventions.md`; no PII.
- Evidence to capture: API responses (receipt, shift close, reconcile), correlationIds, final shift detail.
- Notes: Serves as main happy path for PAYMENTS COD; ensures end-to-end financial integrity.

### E2E-PAYMENTS-002 — COD difference captured and reconciled as DIFFERENCE
- Priority: P0
- Story IDs: US-PAYMENTS-002, US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: `order_cod_diff` amount=50_000 in OPEN shift `shift_open_30001` (or dedicated shift), `shipper_200` authenticated; Finance authenticated.
- Test data (fake): receipt amount sent=45_000 (< expected), Idempotency-Key `idem-receipt-dup` for first receipt; reconcile note "Customer paid short 5k".
- Steps:
  1. Shipper creates receipt for orderId 70002 with amount=45_000 (under-collection).
  2. Shipper closes the shift via `POST /api/v1/payments/shifts/my-open/close`.
  3. Finance fetches shift detail via `GET /api/v1/payments/shifts/{id}`.
  4. Finance reconciles via `POST /api/v1/payments/shifts/{id}/reconcile` with status `DIFFERENCE`, note "Customer paid short 5k".
- Expected results:
  - Step 1: 201 receipt recorded; amount stored as sent; paymentStatus may remain pending/partial per business rule; correlationId returned.
  - Step 2: Shift closes with collectedCod=45_000, expectedCod=50_000, difference=-5_000; receipts locked.
  - Step 3: Shift detail exposes receiptsCount and difference.
  - Step 4: Status transitions to DIFFERENCE with note persisted; no further receipt edits allowed without adjustment workflow.
- Evidence to capture: Receipt response, shift close response, reconcile response, correlationIds.
- Notes: Blocked by requirement Q-PAYMENTS-004 if tolerance/adjustment policy changes difference handling.

### E2E-PAYMENTS-003 — Deferred payment credit check and debt tracking
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: `finance_01` authenticated; baseline creditLimit=2_000_000, currentDebt=1_500_000 for `user_t3`.
- Test data (fake): PUT limit request (limitAmount=2_000_000, isActive=true, effectiveFrom now); credit check request userId=10030, companyId=501, orderAmount=400_000, Idempotency-Key `idem-creditcheck-001`.
- Steps:
  1. Finance calls `PUT /api/v1/payments/credit-limits/{userId}` to ensure active limit 2_000_000.
  2. ORDERS (simulated) calls `POST /api/v1/payments/credit/check` with orderAmount=400_000.
  3. Verify response fields (`allowed`, `debtAfterOrder`).
  4. After order creation event posts debt (system flow), call `GET /api/v1/payments/debts/{userId}` to confirm `currentDebt` increased accordingly.
- Expected results:
  - Step 1: 200 with `PaymentCreditLimit` reflecting limitAmount and effective dates.
  - Step 2: 200 `PaymentCreditCheckResponse` with allowed=true, creditLimit=2_000_000, currentDebt=1_500_000, debtAfterOrder=1_900_000, failureReasonCode=null.
  - Step 4: Debt balance shows ~1_900_000 and updated timestamp; correlationId returned for each call.
- Evidence to capture: Credit limit update response, credit check response, debt balance response, correlationIds.
- Notes: Blocked by requirement Q-PAYMENTS-001/Q-PAYMENTS-002 until debt-posting trigger after order creation is finalized; Step 4 may be skipped if no write path is available.

## Section 4: Integration/API testcases

### IT-PAYMENTS-001 — Create COD receipt success (Shipper)
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: orderId=70001 assigned to `shipper_200` with paymentMethod=COD; shift open.
- Test data (fake): request `{orderId:70001, amount:43_500, note:"Collected at doorstep"}`, Idempotency-Key `idem-receipt-001`.
- Steps:
  1. Call `POST /api/v1/payments/receipts` with payload and Idempotency-Key.
  2. Query `GET /api/v1/payments/shifts/my-open` to verify collectedCod updated.
- Expected results:
  - 201 with `PaymentReceipt` fields populated; correlationId present.
  - collectedCod increments by 43_500; receipt attached to shift.
  - Payment status for order marked PAID (observable via downstream ORDERS hook).
- Evidence to capture: receipt response, shift summary, correlationIds.
- Notes: —

### IT-PAYMENTS-002 — Reject receipt when amount invalid
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: orderId=70001 expected amount=43_500.
- Test data (fake): request `{orderId:70001, amount:-1}` or `{orderId:70001, amount:40_000}`.
- Steps:
  1. Submit invalid receipt via `POST /api/v1/payments/receipts`.
- Expected results:
  - 400 with `error.code` indicating validation/amount mismatch; no receipt created; correlationId returned.
- Evidence to capture: error response and correlationId.
- Notes: Alignment with V-PAYMENTS-001 (non-negative) and FR-PAYMENTS-002 amount matching.

### IT-PAYMENTS-003 — Prevent duplicate receipt for same order
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: orderId=70001 has one receipt already.
- Test data (fake): second call using new Idempotency-Key `idem-receipt-dup` with same orderId.
- Steps:
  1. Call `POST /api/v1/payments/receipts` again for orderId=70001.
- Expected results:
  - 409 conflict or equivalent error; message indicates receipt already exists; no double-charge; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Covers idempotency/replay.

### IT-PAYMENTS-004 — Close shift when no open shift returns 400
- Priority: P0
- Story IDs: US-PAYMENTS-003
- Screen IDs: N/A
- Preconditions: `shipper_200` has no OPEN shift (all closed).
- Test data (fake): Idempotency-Key `idem-close-30001`.
- Steps:
  1. Call `POST /api/v1/payments/shifts/my-open/close`.
- Expected results:
  - 400 with `error.code` indicating no open shift/invalid state; correlationId returned; no status change.
- Evidence to capture: error response, correlationId.
- Notes: Negative/edge for FR-PAYMENTS-003.

### IT-PAYMENTS-005 — Receipts locked after shift closed
- Priority: P0
- Story IDs: US-PAYMENTS-003
- Screen IDs: N/A
- Preconditions: shiftId=30001 already CLOSED.
- Test data (fake): new receipt attempt on order in closed shift.
- Steps:
  1. Call `POST /api/v1/payments/receipts` for order belonging to closed shift.
- Expected results:
  - 400/403 indicating shift closed; no new receipt stored; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Enforces BR-PAYMENTS-003.

### IT-PAYMENTS-006 — Finance lists shifts with paging and filters
- Priority: P1
- Story IDs: US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: Finance authenticated; multiple shifts exist with mixed statuses.
- Test data (fake): query `status=CLOSED`, `page=1`, `pageSize=20`, `shipperId=shipper_200`.
- Steps:
  1. Call `GET /api/v1/payments/shifts` with filters.
- Expected results:
  - 200 with paged result containing items of type `PaymentShiftSummary`; pagination fields `page`, `totalItems`, `totalPages` populated; filtered by shipper/status.
- Evidence to capture: list response, correlationId.
- Notes: —

### IT-PAYMENTS-007 — Reconcile closed shift to RECONCILED
- Priority: P0
- Story IDs: US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: shiftId=30001 status=CLOSED; Finance authenticated.
- Test data (fake): request `{status:"RECONCILED", note:"Balanced"}`, Idempotency-Key `idem-reconcile-30002`.
- Steps:
  1. Call `POST /api/v1/payments/shifts/{id}/reconcile`.
- Expected results:
  - 200 with status=RECONCILED, reconciliationNote saved, reconciledBy populated; correlationId present.
- Evidence to capture: reconcile response, correlationId.
- Notes: —

### IT-PAYMENTS-008 — Reject reconcile when shift not in reconcilable state
- Priority: P0
- Story IDs: US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: shiftId open or already RECONCILED.
- Steps:
-  1. Call `POST /api/v1/payments/shifts/{id}/reconcile` on non-closable shift.
- Expected results:
  - 400 with `error.code` indicating invalid reconciliation state; no data change; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Negative case for FR-PAYMENTS-004.

### IT-PAYMENTS-009 — Credit check passes within limit
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: creditLimit=2_000_000, currentDebt=1_500_000.
- Test data (fake): request `{userId:10030, companyId:501, orderAmount:400_000}`, Idempotency-Key `idem-creditcheck-001`.
- Steps:
  1. Call `POST /api/v1/payments/credit/check`.
- Expected results:
  - 200 with allowed=true, debtAfterOrder=1_900_000, failureReasonCode=null; correlationId present.
- Evidence to capture: response, correlationId.
- Notes: —

### IT-PAYMENTS-010 — Credit check blocks when exceeding limit
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: creditLimit=2_000_000, currentDebt=1_900_000.
- Test data (fake): request `{userId:10030, companyId:501, orderAmount:300_000}`.
- Steps:
  1. Call `POST /api/v1/payments/credit/check`.
- Expected results:
  - 200 with allowed=false, debtAfterOrder projected > limit, failureReasonCode populated (e.g., CREDIT_LIMIT_EXCEEDED); correlationId present; order creation must be blocked downstream.
- Evidence to capture: response, correlationId.
- Notes: Edge/negative for BR-PAYMENTS-001.

### IT-PAYMENTS-011 — Validate credit limit update input
- Priority: P1
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Finance authenticated.
- Test data (fake): request `{limitAmount:-1}` or missing effectiveFrom when required.
- Steps:
  1. Call `PUT /api/v1/payments/credit-limits/{userId}` with invalid payload.
- Expected results:
  - 400 validation error; no change to existing limit; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Ensures non-negative, effective dates validation.

### IT-PAYMENTS-012 — Read debt balance for user
- Priority: P1
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Debt record exists for userId=10030.
- Steps:
  1. Call `GET /api/v1/payments/debts/{userId}`.
- Expected results:
  - 200 with `PaymentDebtBalance` fields: userId, companyId, currentDebt, lastUpdatedAt; correlationId present.
- Evidence to capture: response, correlationId.
- Notes: —

## Section 5: Security sanity testcases

### SEC-PAYMENTS-001 — Shipper cannot access Finance shift list
- Priority: P0
- Story IDs: US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: `shipper_200` authenticated; shifts exist.
- Steps:
  1. Call `GET /api/v1/payments/shifts` as shipper.
- Expected results:
  - 403 `error.code` for unauthorized role; no shift data leaked; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Authz separation between shipper and finance.

### SEC-PAYMENTS-002 — Shipper cannot create receipt for order outside their shift
- Priority: P0
- Story IDs: US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: `order_cod_1` belongs to another shipper/shift; `shipper_201` authenticated.
- Steps:
  1. `shipper_201` calls `POST /api/v1/payments/receipts` for order_cod_1.
- Expected results:
  - 403 with `error.code` indicating forbidden; no receipt created; correlationId present.
- Evidence to capture: error response, correlationId.
- Notes: Prevents unauthorized COD capture.

### SEC-PAYMENTS-003 — Receipt replay/idempotency protection
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002
- Screen IDs: N/A
- Preconditions: First receipt created with Idempotency-Key `idem-receipt-001`.
- Steps:
  1. Repeat `POST /api/v1/payments/receipts` with same Idempotency-Key but modified amount.
- Expected results:
  - 409 or idempotent response without creating a new receipt; amount not altered; correlationId present.
- Evidence to capture: error/response, correlationId.
- Notes: Guards against replay/double charge.

### SEC-PAYMENTS-004 — Injection attempt in note fields is rejected/escaped
- Priority: P1
- Story IDs: US-PAYMENTS-002, US-PAYMENTS-004, US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Authenticated role matching endpoint.
- Test data (fake): note string `<script>alert(1)</script>` or `' OR 1=1 --`.
- Steps:
  1. Submit note via receipt creation or reconcile request containing injection payload.
- Expected results:
  - 400 validation or stored note safely escaped; no script execution; correlationId present; logs sanitized (no raw payload echoed).
- Evidence to capture: response, correlationId.
- Notes: Input sanitization.

### SEC-PAYMENTS-005 — Access to another user’s debt is denied
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: Authenticated user with no Finance role tries to read debt of userId=10030.
- Steps:
  1. Call `GET /api/v1/payments/debts/{userId}` as unauthorized user.
- Expected results:
  - 403/404 with no debt details; correlationId present; message contains no PII.
- Evidence to capture: error response, correlationId.
- Notes: Protects sensitive financial data.

### SEC-PAYMENTS-006 — Expired/invalid token results in 401
- Priority: P1
- Story IDs: US-PAYMENTS-001 → 005
- Screen IDs: N/A
- Preconditions: Use expired/invalid access token.
- Steps:
  1. Call any PAYMENTS endpoint (e.g., `POST /api/v1/payments/receipts`) with expired token.
- Expected results:
  - 401 per default error response; no side effects; correlationId present (if provided by gateway).
- Evidence to capture: error response, correlationId.
- Notes: Session handling sanity.

## Section 6: Regression Suite (REG-*)

### REG-PAYMENTS-001 — COD happy path (receipt → close → reconcile)
- Priority: P0
- Story IDs: US-PAYMENTS-001, US-PAYMENTS-002, US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: Same as E2E-PAYMENTS-001.
- Test data (fake): orderId=70001, amount=43_500, Idempotency-Keys `idem-receipt-001`, `idem-close-30001`, `idem-reconcile-30002`.
- Steps: Execute steps from E2E-PAYMENTS-001.
- Expected results: Shift reconciled to RECONCILED with difference=0 and receipt persisted; correlationIds captured.
- Evidence to capture: Responses from receipt, close, reconcile.
- Notes: Core regression blocker for release.

### REG-PAYMENTS-002 — Credit limit enforcement rejection path
- Priority: P0
- Story IDs: US-PAYMENTS-005
- Screen IDs: N/A
- Preconditions: creditLimit=2_000_000, currentDebt=1_900_000.
- Test data (fake): credit check request `{userId:10030, companyId:501, orderAmount:300_000}`.
- Steps: Execute IT-PAYMENTS-010.
- Expected results: allowed=false with failureReasonCode; no debt increment; correlationId present.
- Evidence to capture: credit check response.
- Notes: Prevents limit overrun regressions.

### REG-PAYMENTS-003 — COD difference captured and locked post-close
- Priority: P1
- Story IDs: US-PAYMENTS-002, US-PAYMENTS-003, US-PAYMENTS-004
- Screen IDs: N/A
- Preconditions: shift with under-collected order (e.g., order_cod_diff).
- Test data (fake): receipt amount=45_000 for expected 50_000.
- Steps: Execute E2E-PAYMENTS-002.
- Expected results: Shift shows difference=-5_000, status=DIFFERENCE after reconcile; no further receipt edits accepted.
- Evidence to capture: receipt and reconcile responses.
- Notes: Monitors critical discrepancy workflow.

## Section 7: Open Questions / blockers
- Q-PAYMENTS-001 (credit limit calculation policy): Blocks verification of long-term credit limit adjustments; affects E2E-PAYMENTS-003 Step 4 and IT-PAYMENTS-011 expectations on date/limit rules.
- Q-PAYMENTS-002 (debt collection timing): Affects scheduling/aging scenarios for debt; E2E-PAYMENTS-003 Step 4 may be skipped until finalized.
- Q-PAYMENTS-003 (online payment integration): Not in current scope; no test cases added.
- Q-PAYMENTS-004 (handling COD difference/thresholds and adjustment workflow): Impacts E2E-PAYMENTS-002 and REG-PAYMENTS-003; mark as “Blocked by requirement” if tolerance/policy diverges from assumed behavior (record difference, lock receipts).
