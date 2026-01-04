
# API Conventions (Contract Rules)

These conventions must be applied consistently across all endpoints and reflected in OpenAPI.

## Versioning
- Prefer URL versioning: `/api/v1/...`
- Any breaking change => bump major version (`v2`)
- Non-breaking additions are allowed within same version

## IDs
- Prefer `long/bigint` IDs for internal entities, and optionally `uuid` for public references.
- Never expose sequential IDs publicly if it increases enumeration risk; use UUID or opaque IDs for external-facing resources.

## Pagination
Use one standard approach and keep it consistent.

### Option A: Offset pagination (simple)
Request:
- `page` (1-based), `pageSize`

Response:
```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 123,
  "totalPages": 7
}
```

### Option B: Cursor pagination (preferred for large datasets)
Request:
- `cursor`, `limit`

Response:
```json
{
  "items": [],
  "nextCursor": "opaque",
  "hasMore": true
}
```

## Filtering & sorting
- `sortBy=createdAt&sortDir=desc`
- Filters should be explicit: `status=ACTIVE&from=2026-01-01&to=2026-01-31`
- Avoid overloaded query params.

## Idempotency
Required for endpoints that create external side effects (payments, webhooks handling, order submission).

- Client sends header: `Idempotency-Key: <uuid>`
- Server stores key + request hash + resulting resource id for a TTL window.
- Same key + same request => return same response.
- Same key + different request => return 409 conflict.

## Error handling
Standard error response (see `Error-Conventions.md`):
- Stable `error.code`
- Human message without sensitive details
- `correlationId` always included

HTTP mapping guidelines:
- 400 validation errors
- 401 unauthenticated
- 403 unauthorized (authz)
- 404 not found
- 409 conflict (idempotency, state conflicts)
- 429 rate limit
- 500 generic

## Correlation ID
- Accept incoming `X-Correlation-Id` header; if missing, generate one.
- Include it in responses and logs.

## Security defaults
- TLS only in prod
- Rate limit auth endpoints
- Strict CORS allowlist
- Audit important state changes
