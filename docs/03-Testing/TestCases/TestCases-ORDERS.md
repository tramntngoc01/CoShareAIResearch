# Test Cases — ORDERS: Orders

## Section 1: Scope & assumptions
- Scope:
  - Module ORDERS across all APIs in OpenAPI `orders` tag, covering SRS-ORDERS and Stories US-ORDERS-001 → 005.
  - StoryPacks available: US-ORDERS-001 → 004. StoryPack for US-ORDERS-005 is not provided; coverage derives from the Story + SRS.
  - ScreenSpec: no `SC-ORDERS-*` provided; UI checks use API-driven assertions.
- Assumptions / open questions:
  - Single-delivery per order (A-ORDERS-001).
  - POD required to mark `Đã nhận` unless policy says otherwise (A-ORDERS-003).
  - Cancellable statuses and auto-cancel window TBD (Q-ORDERS-001/002); tests blocked until defined.
  - Return/exchange policy and quantity limits TBD (Q-ORDERS-003); tests blocked until defined.
  - Tests blocked on TBD items are marked explicitly as "Blocked by requirement ...".
- Status naming uses Vietnamese display status with English status code in parentheses where needed (e.g., `Đã nhận` / `COMPLETED`) for clarity.
- Error shape per `Error-Conventions.md` with `error.code` and `correlationId`. No PII in messages/logs.

## Section 2: Test data set (fake)
- Users: `user_t3` (End User, userId=10030, companyId=501, pickupPointId=9001), `ops_admin_01` (Ops), `shipper_200` (Shipper/Tầng 2), `super_admin_01` (Super Admin).
- Products: `prod_active_1` (productId=1001, price=6500, stock=10), `prod_active_2` (productId=1002, price=12000, stock=5), `prod_inactive` (productId=1010, inactive), `prod_low_stock` (productId=1020, stock=0).
- Orders (created during tests): use idempotency key `idem-ord-###` to avoid duplicates. Sample order `order_cod_1` created via `POST /api/v1/orders` with items `(1001 x2, 1002 x1)`, paymentMethod `COD`.
- POD assets: `https://example.com/pod/order_cod_1.jpg` (non-PII).

## Section 3: E2E testcases (P0 first, then P1)

### E2E-ORDERS-001 — End User creates order (COD happy path)
- Priority: P0
- Story IDs: US-ORDERS-001
- Screen IDs: N/A (API-driven; no SC-ORDERS provided)
- Preconditions: `user_t3` authenticated; products `prod_active_1`, `prod_active_2` active with sufficient stock.
- Test data (fake): request body per `OrderCreateRequest` with pickupPointId `9001`, paymentMethod `COD`, items `[ {productId:1001, quantity:2}, {productId:1002, quantity:1} ]`; Idempotency-Key `idem-ord-001`.
- Steps:
  1. Call `POST /api/v1/orders` with the request above and Idempotency-Key header.
  2. Capture response `orderId`.
  3. Call `GET /api/v1/orders/{id}` to verify stored data.
- Expected results:
  - 201 with `OrderDetail` containing correct `userId`, `companyId`, `pickupPointId`, `paymentMethod=COD`, status initialized per payment policy (Chờ xác nhận/Chờ thanh toán).
  - `items` match request; `totalAmount` equals sum of line amounts.
  - `statusHistory` includes initial status entry.
  - CorrelationId returned in headers; no PII in body.
- Evidence to capture: API responses, correlationId, computed total.
- Notes: Serves as base order for subsequent E2E cases.

### E2E-ORDERS-002 — Full lifecycle: confirm → deliver with POD
- Priority: P0
- Story IDs: US-ORDERS-002, US-ORDERS-004
- Screen IDs: N/A (API-driven)
- Preconditions: Order exists from E2E-ORDERS-001 and is in a confirmable status; `ops_admin_01` and `shipper_200` authenticated with scope over the order’s pickup point.
- Test data (fake): orderId from E2E-ORDERS-001; POD urls `[https://example.com/pod/order_cod_1.jpg]`.
- Steps:
  1. Ops calls `PATCH /api/v1/orders/{id}/status` to move to `Đã xác nhận`/`CONFIRMED`.
  2. Shipper calls status updates sequentially: `IN_DELIVERY`, then `READY_FOR_PICKUP`.
  3. Shipper calls `POST /api/v1/orders/{id}/delivery-proof` with deliveredAt timestamp, deliveredBy `shipper_200`, podImageUrls, receiverName fake.
  4. Call `GET /api/v1/orders/{id}` to verify final state.
- Expected results:
  - Each transition returns 200 and appends to `statusHistory` in order.
  - Final status `Đã nhận`/`COMPLETED` with POD recorded.
  - No forbidden jumps; audit/statusHistory includes actor and timestamps.
  - CorrelationId present for each call.
- Evidence to capture: status transition responses, POD payload, final order detail.
- Notes: Validates main happy path journey for module.

### E2E-ORDERS-003 — Cancel request and approval
- Priority: P0
- Story IDs: US-ORDERS-003
- Screen IDs: N/A (API-driven)
- Preconditions: Order in cancelable status (e.g., `Đã xác nhận`) belonging to `user_t3`; `ops_admin_01` and `super_admin_01` authenticated.
- Test data (fake): cancel reason "NCC hết hàng", Idempotency-Key `idem-ord-cancel-001`.
- Steps:
  1. Ops calls `POST /api/v1/orders/{id}/cancel-request` with reason.
  2. Verify order transitions to cancellation pending via `GET /api/v1/orders/{id}`.
  3. Super Admin calls `POST /api/v1/orders/{id}/cancel-request/decision` with decision `APPROVE`.
  4. Fetch order detail again.
- Expected results:
  - Cancel request creation returns 201 with `status=PENDING`; order status frozen for other transitions.
  - Decision call returns 200; order status becomes `Đã hủy`/`CANCELLED`; cancel request status `APPROVED`.
  - Audit fields `requestedBy`, `approvedBy`, timestamps populated.
  - CorrelationId present; no PII in messages.
- Evidence to capture: cancel request response, decision response, final order detail.
- Notes: Blocked by requirement Q-ORDERS-001 until cancelable statuses are defined; execute once finalized.

### E2E-ORDERS-004 — Record return/exchange at pickup point
- Priority: P0
- Story IDs: US-ORDERS-004
- Screen IDs: N/A (API-driven)
- Preconditions: Order delivered (status `Đã nhận`), created from E2E-ORDERS-002; return policy scope confirmed.
- Test data (fake): return items `[ {productId:1002, quantity:1} ]`, reason "Torn packaging (Bao bì rách)", Idempotency-Key `idem-ord-return-001`.
- Steps:
  1. Shipper/support calls `POST /api/v1/orders/{id}/returns` with items and reason.
  2. Query return object or order detail (depending on implementation) to confirm recorded status.
- Expected results:
  - 201 with `OrderReturnRequest` showing items, status (e.g., `PENDING`/`RECORDED`), createdBy `shipper_200`.
  - Order/return status reflects return flow per OpenAPI schema.
  - CorrelationId returned; no PII leakage.
- Evidence to capture: return creation response, subsequent order/return detail.
- Notes: Blocked by requirement Q-ORDERS-003 until return eligibility rules are finalized.

### E2E-ORDERS-005 — End User views order history
- Priority: P1
- Story IDs: US-ORDERS-005
- Screen IDs: N/A (API-driven)
- Preconditions: `user_t3` authenticated with at least two orders (one active, one cancelled).
- Test data (fake): filters `status=Đang giao`, `page=1`, `pageSize=10`.
- Steps:
  1. Call `GET /api/v1/orders/my` without filter.
  2. Call again with `status` filter.
  3. Open a returned `orderId` via `GET /api/v1/orders/{id}`.
- Expected results:
  - 200 paged list scoped to `user_t3`; orders sorted newest first; pagination fields populated.
  - Filtered call only returns matching statuses.
  - Detail API hides other users’ data and includes `statusHistory`.
  - CorrelationId present.
- Evidence to capture: list responses, filtered response, detail response.
- Notes: Ensures read-only UX for End User history.

## Section 4: Integration/API testcases

### IT-ORDERS-001 — Reject create order when cart is empty
- Priority: P0
- Story IDs: US-ORDERS-001
- Screen IDs: N/A
- Preconditions: `user_t3` authenticated.
- Test data (fake): `OrderCreateRequest` with empty `items` array; Idempotency-Key `idem-ord-empty`.
- Steps:
  1. Call `POST /api/v1/orders` with empty items.
- Expected results:
  - 400 with `error.code=ORDERS_CART_EMPTY`; message generic, correlationId present.
  - No order created (subsequent list shows none).
- Evidence to capture: error response, correlationId.
- Notes: Validates mandatory items minItems=1.

### IT-ORDERS-002 — Reject create order for inactive or out-of-stock product
- Priority: P0
- Story IDs: US-ORDERS-001
- Screen IDs: N/A
- Preconditions: `prod_inactive` or `prod_low_stock` exists.
- Test data (fake): request with items `[ {productId:1010, quantity:1} ]` or `[ {productId:1020, quantity:2} ]`.
- Steps:
  1. Call `POST /api/v1/orders` with inactive product.
  2. Call again with out-of-stock product.
- Expected results:
  - 400 with `error.code=ORDERS_PRODUCT_INACTIVE` or `ORDERS_INSUFFICIENT_STOCK` respectively; correlationId present.
  - No reservation/creation.
- Evidence to capture: both error responses, stock verification log if available.
- Notes: Covers BR-ORDERS-001.

### IT-ORDERS-003 — Reject create order when payment limit exceeded (trả chậm)
- Priority: P0
- Story IDs: US-ORDERS-001
- Screen IDs: N/A
- Preconditions: User configured with deferred payment method that exceeds limit.
- Test data (fake): paymentMethod `DEFERRED`, items total > limit, Idempotency-Key `idem-ord-paylim`.
- Steps:
  1. Call `POST /api/v1/orders` with deferred payment exceeding limit.
- Expected results:
  - 400 with `error.code=ORDERS_PAYMENT_LIMIT_EXCEEDED` (per StoryPack); correlationId present.
  - No order created.
- Evidence to capture: error response, correlationId.
- Notes: Blocked by environment until PAYMENTS deferred-limit configuration/mocks are available.

### IT-ORDERS-004 — Prevent invalid status transition
- Priority: P0
- Story IDs: US-ORDERS-002
- Screen IDs: N/A
- Preconditions: Order in `Chờ xác nhận`; `ops_admin_01` authenticated.
- Test data (fake): `OrderStatusUpdateRequest` with `newStatus=Đã nhận` directly.
- Steps:
  1. Call `PATCH /api/v1/orders/{id}/status` skipping allowed sequence.
- Expected results:
  - 400 with `error.code=ORDERS_INVALID_STATUS_TRANSITION`; order status unchanged.
  - statusHistory not appended.
  - CorrelationId present.
- Evidence to capture: error response; follow-up `GET /api/v1/orders/{id}`.
- Notes: Ensures BR-ORDERS-002.

### IT-ORDERS-005 — Status update denied for actor outside scope
- Priority: P0
- Story IDs: US-ORDERS-002
- Screen IDs: N/A
- Preconditions: Order assigned to pickupPoint different from `shipper_200`; `shipper_200` authenticated.
- Test data (fake): `newStatus=IN_DELIVERY`.
- Steps:
  1. Call `PATCH /api/v1/orders/{id}/status` by unauthorized shipper.
- Expected results:
  - 403 or 400 with `error.code=ORDERS_STATUS_UPDATE_NOT_ALLOWED_FOR_ACTOR`; no status change.
  - CorrelationId present.
- Evidence to capture: error response, order detail unchanged.
- Notes: Authz negative case.

### IT-ORDERS-006 — Status updates frozen during pending cancel
- Priority: P0
- Story IDs: US-ORDERS-002, US-ORDERS-003
- Screen IDs: N/A
- Preconditions: Order has active cancel request (status `CANCELLATION_PENDING`).
- Test data (fake): `newStatus=IN_DELIVERY`.
- Steps:
  1. Attempt `PATCH /api/v1/orders/{id}/status` while cancel pending.
- Expected results:
  - 400 with `error.code=ORDERS_STATUS_FROZEN_DURING_CANCEL`; order status/history unchanged.
  - CorrelationId present.
- Evidence to capture: error response, follow-up order detail.
- Notes: Enforces StoryPack-002 error mapping.

### IT-ORDERS-007 — Reject cancel request when status not cancelable
- Priority: P0
- Story IDs: US-ORDERS-003
- Screen IDs: N/A
- Preconditions: Order already `Đã nhận`/`CANCELLED` where cancel not allowed.
- Test data (fake): reason "Already delivered (Đã giao xong)", Idempotency-Key `idem-ord-cancel-bad`.
- Steps:
  1. Call `POST /api/v1/orders/{id}/cancel-request`.
- Expected results:
  - 400 with `error.code=ORDERS_CANCEL_NOT_ALLOWED_STATUS`; no cancel record created.
  - CorrelationId present.
- Evidence to capture: error response, cancel-requests list.
- Notes: Blocked by requirement Q-ORDERS-001 until cancelable statuses are finalized.

### IT-ORDERS-008 — Prevent duplicate cancel request
- Priority: P0
- Story IDs: US-ORDERS-003
- Screen IDs: N/A
- Preconditions: Order has existing pending cancel request.
- Test data (fake): same reason, Idempotency-Key `idem-ord-cancel-dup`.
- Steps:
  1. Call `POST /api/v1/orders/{id}/cancel-request` again.
- Expected results:
  - 409 with `error.code=ORDERS_CANCEL_ALREADY_PENDING`; no new record.
  - CorrelationId present.
- Evidence to capture: error response, ensure only one request exists.
- Notes: Uses StoryPack error codes.

### IT-ORDERS-009 — Require POD when completing order
- Priority: P0
- Story IDs: US-ORDERS-004
- Screen IDs: N/A
- Preconditions: Order in `READY_FOR_PICKUP`; `shipper_200` authenticated.
- Test data (fake): `OrderDeliveryProofRequest` missing `podImageUrls`.
- Steps:
  1. Call `POST /api/v1/orders/{id}/delivery-proof` without podImageUrls.
- Expected results:
  - 400 with `error.code=ORDERS_POD_REQUIRED`; order status remains `READY_FOR_PICKUP`.
  - CorrelationId present.
- Evidence to capture: error response, order detail unchanged.
- Notes: Aligns with SRS requirement for POD.

### IT-ORDERS-010 — Reject return with invalid quantities
- Priority: P0
- Story IDs: US-ORDERS-004
- Screen IDs: N/A
- Preconditions: Order delivered with known item quantities.
- Test data (fake): `OrderReturnRequestCreate` items `[ {productId:1001, quantity:0}, {productId:1002, quantity:10} ]`.
- Steps:
  1. Call `POST /api/v1/orders/{id}/returns` with invalid quantities.
- Expected results:
  - 400 with `error.code=ORDERS_RETURN_QUANTITY_INVALID`; no return created.
- Evidence to capture: error response, order detail.
- Notes: Blocked by requirement Q-ORDERS-003 until return quantity rules are confirmed.

### IT-ORDERS-011 — List my orders scoped to current user
- Priority: P1
- Story IDs: US-ORDERS-005
- Screen IDs: N/A
- Preconditions: `user_t3` and another user `user_other` each have orders.
- Test data (fake): `GET /api/v1/orders/my?page=1&pageSize=5`.
- Steps:
  1. Call endpoint as `user_t3`.
  2. Attempt to access `user_other` order via `GET /api/v1/orders/{otherId}`.
- Expected results:
  - List only contains orders for `user_t3`; pagination fields set.
  - Accessing other user’s order returns 403/404 with correlationId; no data leak.
- Evidence to capture: list response, error response for unauthorized detail.
- Notes: Complements SEC-ORDERS-001.

## Section 5: Security sanity testcases

### SEC-ORDERS-001 — Authz: prevent viewing others’ orders
- Priority: P0
- Story IDs: US-ORDERS-001, US-ORDERS-005
- Screen IDs: N/A
- Preconditions: `user_other` has order; `user_t3` authenticated.
- Test data (fake): orderId belonging to `user_other`.
- Steps:
  1. `user_t3` calls `GET /api/v1/orders/{otherId}`.
  2. `user_t3` calls `GET /api/v1/orders` (admin list) without proper role.
- Expected results:
  - 403/404 with generic message, `error.code` per implementation (e.g., `ORDERS_ORDER_NOT_FOUND`); no order data leaked.
  - CorrelationId present.
- Evidence to capture: error responses, headers.
- Notes: ORDERS authz negative test.

### SEC-ORDERS-002 — Authz: status update bypass attempt
- Priority: P0
- Story IDs: US-ORDERS-002
- Screen IDs: N/A
- Preconditions: Order outside `shipper_200` scope; `shipper_200` authenticated.
- Test data (fake): `newStatus=IN_DELIVERY`.
- Steps:
  1. Attempt status update as out-of-scope shipper.
  2. Repeat with expired/invalid token to ensure 401 handling.
- Expected results:
  - Out-of-scope attempt rejected with `error.code=ORDERS_STATUS_UPDATE_NOT_ALLOWED_FOR_ACTOR`.
  - Expired token returns 401 default error; no state change.
  - CorrelationId present where applicable.
- Evidence to capture: both responses, order detail unchanged.
- Notes: Covers role and session handling.

### SEC-ORDERS-003 — Idempotency replay on order creation
- Priority: P1
- Story IDs: US-ORDERS-001
- Screen IDs: N/A
- Preconditions: `user_t3` authenticated; valid create payload.
- Test data (fake): same payload, Idempotency-Key `idem-ord-replay`.
- Steps:
  1. Send `POST /api/v1/orders` with key `idem-ord-replay`.
  2. Retry same request with identical key.
  3. Send again with different key to ensure new order is created.
- Expected results:
  - First two calls return same orderId without duplicate creation; 201 then 200/201 idempotent behavior per design.
  - Third call with new key creates a new orderId.
  - CorrelationIds unique per call; no conflicting state.
- Evidence to capture: responses, order counts.
- Notes: Protects against replay/duplicate orders.

### SEC-ORDERS-004 — Input injection in cancel reason / notes
- Priority: P1
- Story IDs: US-ORDERS-003, US-ORDERS-004
- Screen IDs: N/A
- Preconditions: Order in cancelable or deliverable status.
- Test data (fake): reason `<script>alert(1)</script>`; note with SQL-like input.
- Steps:
  1. Submit cancel request with injected reason.
  2. Submit delivery proof with note containing special chars.
- Expected results:
  - Requests accepted/rejected based on business rules but outputs do not echo raw script; stored/surfaced safely (escaped), no script execution, logs avoid PII.
  - CorrelationId present.
- Evidence to capture: API responses, stored reason/note as returned by detail APIs.
- Notes: Security sanity for input sanitization.

### SEC-ORDERS-005 — Error responses exclude PII and include correlationId
- Priority: P1
- Story IDs: All ORDERS stories
- Screen IDs: N/A
- Preconditions: Trigger controlled errors (e.g., invalid orderId).
- Test data (fake): `GET /api/v1/orders/999999` where not found.
- Steps:
  1. Call invalid order detail as authenticated user.
- Expected results:
  - 404 default error shape with `error.code` generic (no user names/phone), includes `correlationId`.
  - Response time acceptable; no stack trace.
- Evidence to capture: error response body and headers.
- Notes: Aligns with Error-Conventions.

### SEC-ORDERS-006 — Rate-limit/abuse check on status updates (sanity)
- Priority: P2
- Story IDs: US-ORDERS-002
- Screen IDs: N/A
- Preconditions: Rate-limit policy available; `shipper_200` authenticated.
- Test data (fake): burst of 10 rapid `PATCH /api/v1/orders/{id}/status` retries with same payload.
- Steps:
  1. Send rapid consecutive status updates (valid + duplicate).
- Expected results:
  - Only first valid transition succeeds; duplicates rejected (400/409) without multiple state changes.
  - If rate-limit configured, 429 returned after threshold; correlationId present.
- Evidence to capture: sequence of responses, final order detail.
- Notes: Blocked by policy (Q-ORDERS-004) until rate-limit configuration is defined.

## Section 6: Regression Suite (IDs list with brief references)
- REG-ORDERS-001: Regression marker for full lifecycle happy path (reuse E2E-ORDERS-002 steps).
- REG-ORDERS-002: Regression marker for order creation validation (reuse IT-ORDERS-001 & IT-ORDERS-002).
- REG-ORDERS-003: Regression marker for cancel request approval flow (reuse E2E-ORDERS-003).
- REG-ORDERS-004: Regression marker for POD requirement (reuse IT-ORDERS-009).
- REG-ORDERS-005: Regression marker for authz on order access (reuse SEC-ORDERS-001).

## Section 7: Open Questions / blockers
- Q-ORDERS-001: Exact list of cancelable statuses. Tests IT-ORDERS-007 and E2E-ORDERS-003 blocked until clarified.
- Q-ORDERS-002: Auto-cancel/stock release timeout for pending confirmation. No testcase until rule defined.
- Q-ORDERS-003: Detailed return/ exchange policy (eligible statuses, quantity caps, timelines). Tests E2E-ORDERS-004 and IT-ORDERS-010 blocked until clarified.
- Q-ORDERS-004: Rate-limit policies for ORDERS APIs. SEC-ORDERS-006 blocked until limits defined.
