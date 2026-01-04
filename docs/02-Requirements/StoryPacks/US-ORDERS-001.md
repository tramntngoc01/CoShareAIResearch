# StoryPack — US-ORDERS-001: End User tạo đơn hàng mới

## 6) API mapping (contract-first)
> Owner: Tech Lead (ORDERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/orders` — tag: ORDERS — schemas: `OrderCreateRequest` (request), `OrderDetail` (response)
- `GET /api/v1/orders/{id}` — tag: ORDERS — schemas: `OrderDetail` (response) — used after create or from history for detail view

**Error codes used (planned)**
- `ORDERS_CART_EMPTY`
- `ORDERS_INSUFFICIENT_STOCK`
- `ORDERS_PRODUCT_INACTIVE`
- `ORDERS_PAYMENT_LIMIT_EXCEEDED`
- `ORDERS_ORDER_ALREADY_CREATED` (idempotent duplicate)

**Idempotency**
- `POST /api/v1/orders` must use `Idempotency-Key` header for clients that may retry to avoid duplicate orders.

**Pagination/filtering**
- N/A for create.
