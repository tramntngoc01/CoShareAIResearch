# StoryPack — US-ORDERS-003: Ops tạo và Super Admin duyệt yêu cầu hủy đơn

## 6) API mapping (contract-first)
> Owner: Tech Lead (ORDERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/orders/{id}/cancel-request` — tag: ORDERS — schemas: `OrderCancelRequestCreate` (request), `OrderCancelRequest` (response)
- `GET /api/v1/orders/cancel-requests` — tag: ORDERS — schemas: `PagedResult` + `OrderCancelRequest[]` (response `items`)
- `POST /api/v1/orders/{id}/cancel-request/decision` — tag: ORDERS — schemas: `OrderCancelDecisionRequest` (request), `OrderDetail` (response)

**Error codes used (planned)**
- `ORDERS_CANCEL_NOT_ALLOWED_STATUS`
- `ORDERS_CANCEL_ALREADY_PENDING`
- `ORDERS_CANCEL_REQUEST_NOT_FOUND`
- `ORDERS_CANCEL_DECISION_INVALID_STATE`

**Idempotency**
- `POST /api/v1/orders/{id}/cancel-request` should use `Idempotency-Key` to avoid duplicate active requests.
- `POST /api/v1/orders/{id}/cancel-request/decision` should use `Idempotency-Key` to avoid double-approving/rejecting.

**Pagination/filtering**
- `GET /api/v1/orders/cancel-requests` uses standard `page` / `pageSize` with filter: `status` (e.g. PENDING, APPROVED, REJECTED).
