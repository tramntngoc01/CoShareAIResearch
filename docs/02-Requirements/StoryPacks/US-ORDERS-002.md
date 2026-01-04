# StoryPack — US-ORDERS-002: Ops/Ops+Shipper cập nhật trạng thái đơn theo luồng vận hành

## 6) API mapping (contract-first)
> Owner: Tech Lead (ORDERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/orders` — tag: ORDERS — schemas: `PagedResult` + `OrderSummary[]` (response `items`) — list for Ops/Shipper dashboards
- `GET /api/v1/orders/{id}` — tag: ORDERS — schemas: `OrderDetail` (response) — includes `statusHistory`
- `PATCH /api/v1/orders/{id}/status` — tag: ORDERS — schemas: `OrderStatusUpdateRequest` (request), `OrderDetail` (response)

**Error codes used (planned)**
- `ORDERS_INVALID_STATUS_TRANSITION`
- `ORDERS_STATUS_FROZEN_DURING_CANCEL`
- `ORDERS_ORDER_NOT_FOUND`
- `ORDERS_STATUS_UPDATE_NOT_ALLOWED_FOR_ACTOR`

**Idempotency**
- `PATCH /api/v1/orders/{id}/status` is not strictly idempotent, but clients should not retry blindly; backend must handle optimistic concurrency to avoid races.

**Pagination/filtering**
- `GET /api/v1/orders` uses standard `page` / `pageSize` with filters: `status`, `pickupPointId`.
