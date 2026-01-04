
# Backend Coding Standards (.NET 9)

## 1) Architecture & layering
- Controllers: thin (request parsing + auth + basic validation)
- Services: business logic + transactions + authz checks
- Repositories: data access only, no business rules

## 2) Contract-first rule
- Implement strictly according to OpenAPI.
- Any change in request/response => update OpenAPI first.

## 3) Error handling
- Use centralized exception handling middleware.
- Return standardized error response per `docs/03-Design/Error-Conventions.md`.
- Do not expose internal exception details in production responses.

## 4) Validation & security
- Validate all inputs server-side.
- Deny-by-default authorization; enforce policies in service layer.
- Do not log secrets or PII; use correlationId for tracing.

## 5) Logging & observability
- Structured logging.
- Include correlationId on every request.
- Log at appropriate levels (Info/Warn/Error), avoid noisy logs.

## 6) Database access
- Use parameterized queries (EF Core already does).
- Migrations must be included when schema changes.

## 7) Testing
- Unit tests for core business rules.
- Integration tests for critical API+DB behavior.
- Authz negative tests for protected endpoints.

## 8) PR requirements
- Use PR template, list covered AC, tests, migration/rollback notes.
