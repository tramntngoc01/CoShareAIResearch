# StoryPack — US-ORDERS-004: Shipper ghi nhận giao hàng & POD, xử lý đổi/trả tại Điểm nhận

## 6) API mapping (contract-first)
> Owner: Tech Lead (ORDERS). Derived from OpenAPI.

List exact OpenAPI paths + operations.
- `GET /api/v1/orders` — tag: ORDERS — schemas: `PagedResult` + `OrderSummary[]` (response `items`) — list for Shipper by pickup point/shift (filtered server-side)
- `GET /api/v1/orders/{id}` — tag: ORDERS — schemas: `OrderDetail` (response)
- `POST /api/v1/orders/{id}/delivery-proof` — tag: ORDERS — schemas: `OrderDeliveryProofRequest` (request), `OrderDetail` (response)
- `POST /api/v1/orders/{id}/returns` — tag: ORDERS — schemas: `OrderReturnRequestCreate` (request), `OrderReturnRequest` (response)

**Error codes used (planned)**
- `ORDERS_POD_REQUIRED`
- `ORDERS_POD_NOT_ALLOWED_FOR_STATUS`
- `ORDERS_RETURN_NOT_ALLOWED_STATUS`
- `ORDERS_RETURN_QUANTITY_INVALID`

**Idempotency**
- `POST /api/v1/orders/{id}/delivery-proof` should use `Idempotency-Key` to avoid duplicate POD submissions.
- `POST /api/v1/orders/{id}/returns` should use `Idempotency-Key` if the client may retry.

**Pagination/filtering**
- `GET /api/v1/orders` uses standard `page` / `pageSize` with filters: `status`, `pickupPointId` (and optionally assignment/shift via claims or additional filters).
