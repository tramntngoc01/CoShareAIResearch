
# Implementation Plan — ORDERS: Orders

## 0) Status
- Status: In Review (ready for approval)
- Owner: Tech Lead (ORDERS)
- Reviewers: BA / Tech Lead / QA / DevOps
- Target sprint/release: MVP ORDERS

---

## 1) Scope
### In scope
- ORDERS-SCOPE-01: P0 stories for ORDERS — US-ORDERS-001 (Create order), US-ORDERS-002 (Update status), US-ORDERS-003 (Cancel flow), US-ORDERS-004 (POD & returns).
- ORDERS-SCOPE-02: Supporting P1 story US-ORDERS-005 (End User order history) at API level.

### Out of scope
- ORDERS-OOS-01: Detailed commission, debt, and credit limit calculations (handled primarily in PAYMENTS/REPORTING).
- ORDERS-OOS-02: Complex logistics orchestration (multi-leg delivery, routing optimization) beyond basic status updates.

### Success criteria
- SC1: End Users can reliably place orders only with sellable products and sufficient stock, and see consistent statuses across sessions.
- SC2: Ops/Shipper can manage order lifecycle (status changes, cancel, POD, returns) with full auditability and no oversell due to ORDERS logic.

---

## 2) Inputs (links)
- SRS: `docs/02-Requirements/SRS/SRS-ORDERS.md`
- Stories: `docs/02-Requirements/Stories/Stories-ORDERS.md`
- OpenAPI paths:
  - POST /api/v1/orders
  - GET /api/v1/orders
  - GET /api/v1/orders/my
  - GET /api/v1/orders/{id}
  - PATCH /api/v1/orders/{id}/status
  - POST /api/v1/orders/{id}/cancel-request
  - GET /api/v1/orders/cancel-requests
  - POST /api/v1/orders/{id}/cancel-request/decision
  - POST /api/v1/orders/{id}/delivery-proof
  - POST /api/v1/orders/{id}/returns
- DB doc: `docs/03-Design/DB/DB-ORDERS.md`
- Business rules:
  - BR-ORDERS-001 — Đơn chỉ chứa sản phẩm khả dụng.
  - BR-ORDERS-002 — Trạng thái đơn nhất quán.
  - BR-ORDERS-003 — Hủy đơn theo phê duyệt.
  - BR-ORDERS-004 — Đổi trả có ghi nhận chứng từ.

---

## 3) Work Breakdown (what we will implement)
### Backend (.NET 9)
- Controllers/Endpoints:
  - Implement ORDERS API controllers for the paths listed above, following API-Conventions (paging, error envelope, correlationId).
- Service layer:
  - OrderService for create, list, detail, and status transitions.
  - CancelRequestService for create/list/decision of cancel requests.
  - DeliveryService for POD recording and return creation.
  - Integrations to CATALOG (stock check) and PAYMENTS (payment method/limit validation, COD handoff hooks).
- Validation:
  - Enforce cart non-empty, positive quantities, valid pickup point, and sellable products.
  - Enforce allowed status transitions and cancellable statuses according to BR-ORDERS-002/003.
  - Validate POD payload (at least one image URL, timestamps) and return quantities vs ordered quantities.
- Error codes:
  - Define and document module-specific codes, e.g. `ORDERS_CART_EMPTY`, `ORDERS_INSUFFICIENT_STOCK`, `ORDERS_PRODUCT_INACTIVE`, `ORDERS_PAYMENT_LIMIT_EXCEEDED`, `ORDERS_INVALID_STATUS_TRANSITION`, `ORDERS_CANCEL_NOT_ALLOWED_STATUS`, `ORDERS_CANCEL_ALREADY_PENDING`, `ORDERS_POD_REQUIRED`, `ORDERS_RETURN_NOT_ALLOWED_STATUS`.
  - Map these into the shared Error-Conventions document.
- AuthZ policies:
  - Enforce End User access only to own orders on `/api/v1/orders/my` and `/api/v1/orders/{id}`.
  - Restrict `/api/v1/orders` and cancel/decision endpoints to Ops/Super Admin roles with company / pickupPoint scoping.
  - Restrict POD/return endpoints to Shipper or appropriate Tầng 2 roles.
- Logging/CorrelationId:
  - Use centralized logging with correlationId for all ORDER operations, masking any PII in messages (e.g. receiver names).

### Database (PostgreSQL)
- Tables/columns/indexes changes:
  - Create `orders_order`, `orders_order_item`, `orders_order_status_history`, `orders_order_cancel_request`, `orders_order_delivery_proof`, `orders_order_return_request`, `orders_order_return_item` as per DB-ORDERS.
  - Add recommended indexes and unique constraints (e.g. one active cancel request per order).
- Migration steps:
  - Add new tables and constraints in a single migration following DB-Design-Rules.
  - Ensure FKs to `users_user`, `admin_company`, `admin_pickup_point`, and `catalog_product` are created without breaking existing data.
- Rollback steps:
  - For early environments, roll back by dropping child tables then parent tables in reverse dependency order.
  - For production, use feature flags to disable ORDERS features instead of dropping tables; apply additive migrations only.
- Data seeding (if any):
  - None required for ORDERS core tables; any reference data stays in ADMIN/PAYMENTS.

### Frontend (React Web) / Mobile (if any)
- Screens/components:
  - End User Portal: Cart confirmation + order success screen; "Đơn hàng của tôi" list and detail views.
  - Admin/Ops Portal: Order list, order detail with status timeline, cancel request list, cancel decision UI.
  - Shipper view: Order list for a pickup point/shift, order detail with actions for status change, POD, and returns.
- State management:
  - Centralized order store (per module) with paged results for lists and cached detail.
  - Handle optimistic UI for status updates and cancellations with backend confirmation.
- Form validation + error mapping:
  - Map server-side `ORDERS_*` error codes to user-friendly messages (e.g. out-of-stock, payment limit exceeded).
  - Surface field-level validation for quantities, POD images, and reason fields.
- UI evidence expectations:
  - Capture screenshots for key flows (create order, view history, cancel, POD, return) and attach to test cases.

### Integrations / Jobs (if any)
- External APIs:
  - CATALOG: use `/api/v1/catalog/products/{id}/stock` or internal repository to validate stock before creating orders.
  - PAYMENTS: call payment APIs to validate method/limit and to notify on final statuses (Completed, Cancelled, Returned).
- Webhooks:
  - Optional: consume payment webhooks for asynchronous updates (e.g. payment success/failure) to adjust order status.
- Background jobs (idempotency, retries):
  - Scheduled job to auto-cancel stale orders (e.g. long-lived PENDING_CONFIRMATION) — subject to business decision.
  - Reconciliation jobs to detect and repair inconsistent order vs payment vs stock states.

---

## 4) API & Contract Plan (contract-first)
- OpenAPI changes required (schemas, endpoints, error codes):
  - ORDERS paths and schemas added to `openapi.yaml` as listed above.
  - Define `OrderCreateRequest`, `OrderDetail`, `OrderSummary`, `MyOrderSummary`, status history, cancel, POD, and return request/response types.
  - Document ORDERS-specific error codes within Error-Conventions, reusing the shared `ErrorResponse` envelope.
- Backward compatibility considerations:
  - ORDERS is new for MVP; no external clients depend on previous contracts.
  - Future changes must remain backward compatible (additive fields, new optional filters) or go through versioning.
- Idempotency requirements:
  - Require `Idempotency-Key` for `POST /api/v1/orders` in clients that may retry.
  - Recommend (but not strictly require) `Idempotency-Key` for cancel, decision, POD, and return endpoints to avoid duplicate effects.
- Pagination/filtering rules:
  - Use standard `page`/`pageSize` for `/api/v1/orders`, `/api/v1/orders/my`, and `/api/v1/orders/cancel-requests`.
  - Apply global maximum `pageSize` from API-Conventions.
  - Support filtering by `status`, `pickupPointId` where relevant.

---

## 5) Authorization Plan (explicit)
- Roles involved:
  - End User (Tầng 3), Shipper/Tầng 2, Tầng 1, Ops, Super Admin, system jobs.
- Permissions matrix (action -> role):
  - Create order: End User only (authenticated); uses own userId and company context.
  - List my orders: End User only; scoped to own orders.
  - List orders (Ops/Shipper): Ops/Shipper/Tầng 1 with scoping by company/pickup point.
  - Update status: Ops/Shipper roles per allowed transitions.
  - Create cancel request: Ops for eligible orders.
  - Decide cancel request: Super Admin only.
  - Record POD and returns: Shipper (or equivalent Tầng 2) for orders within their assignment.
- Sensitive operations requiring audit trail:
  - All status changes, cancel/decision actions, and POD/return actions must be written to ADMIN audit log with correlationId and actor.

---

## 6) Testing Plan (proof)
> Define test IDs **now** and map them in Traceability.

### Unit tests
- UT-ORDERS-001: Order creation business rules (stock validation, cart non-empty, payment method and limit checks).
- UT-ORDERS-002: Order status state machine (allowed vs disallowed transitions).
- UT-ORDERS-003: Cancel request lifecycle (create, single active request, decision outcomes).
- UT-ORDERS-004: POD and return validations (POD required, quantities, eligible statuses).
- UT-ORDERS-005: End User order history filtering and authorization.

### Integration tests
- IT-ORDERS-001: End-to-end order creation with CATALOG stock integration (happy path + insufficient stock).
- IT-ORDERS-002: Status changes and history persistence, including conflict handling on concurrent updates.
- IT-ORDERS-003: Cancel flow with PAYMENTS/CATALOG side effects stubbed/mocked.
- IT-ORDERS-004: POD + returns end-to-end, ensuring consistent status and DB records.

### E2E / Smoke (staging)
- E2E-ORDERS-001: End User places order, tracks status, and sees completion in "Đơn hàng của tôi".
- E2E-ORDERS-002: Ops and Super Admin manage cancel flow from creation to approval/rejection.
- E2E-ORDERS-003: Shipper delivers order with POD and records a return where applicable.
- E2E-ORDERS-004: Regression of status transitions across roles (Ops, Shipper) including negative paths.

### Security sanity
- SEC-ORDERS-001 (authz negative): Verify that users cannot access orders they do not own and that Ops/Shipper scoping is enforced.
- SEC-ORDERS-002 (input validation/injection basic): Validate inputs for injection, overflows, and tampering (quantities, IDs, POD URLs, reasons).

---

## 7) Observability Plan
- Logs (no PII):
  - Log order lifecycle events (create, status changes, cancel, POD, returns) with correlationId and minimal non-PII context.
  - Avoid logging raw POD URLs and receiver names where not necessary.
- Metrics (key counters/timers):
  - Count of orders created per company/day; rate of cancellations and returns.
  - Latency of order creation, cancel decisions, and POD recording.
- Alerts (if needed):
  - Alert on spikes in cancellations/returns or unusually high error rates for ORDERS endpoints.

---

## 8) Rollout & Deployment Plan
- Feature flags (if any):
  - Feature flag for ORDERS module to enable/disable endpoints without schema rollback.
- Migration order:
  - Deploy DB migration for ORDERS tables.
  - Deploy backend with ORDERS services behind feature flag.
  - Gradually enable ORDERS features per environment.
- Staging verification steps:
  - Run integration and E2E test suites for ORDERS.
  - Manual exploratory tests for critical flows (create, cancel, POD, returns).
- Production checklist items:
  - Confirm monitoring dashboards and alerts for ORDERS are live.
  - Confirm access controls and roles are configured correctly in production.
- Rollback procedure:
  - Disable ORDERS feature flag and route traffic away from ORDERS endpoints.
  - Leave DB schema intact; plan additive fixing migrations instead of destructive rollback.

---

## 9) Risks & Mitigations
- R1: Misconfigured status transitions leading to invalid order states.
  - Mitigation: Implement unit tests around the state machine and require business sign-off on allowed transitions.
- R2: Oversell due to race conditions between ORDERS and CATALOG/PAYMENTS.
  - Mitigation: Design transactional reservation logic and reconciliation jobs; monitor for anomalies.
- R3: Misconfigured authorization allowing cross-company order access.
  - Mitigation: Add explicit authz tests (SEC-ORDERS-001) and review claims/scoping logic.

---

## 10) Estimates & Owners
| Work item | Owner | Estimate | Notes |
|----------|-------|----------|------|
| BE | TBD | TBD | ORDERS APIs and services |
| FE | TBD | TBD | End User + Admin/Shipper UI for orders |
| DB | TBD | TBD | ORDERS tables, indexes, migrations |
| QA | TBD | TBD | Test design and automation for ORDERS |
| DevOps | TBD | TBD | Monitoring, alerts, deployment pipelines for ORDERS |

---

## 11) Approval
- [ ] BA approved
- [ ] Tech Lead approved
- [ ] QA approved
- [ ] DevOps approved (if applicable)
