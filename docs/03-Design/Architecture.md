
# Architecture (.NET 9 + React + PostgreSQL)

## High-level
- **Clients:** Web (React) / Mobile (optional)
- **API:** ASP.NET Core (.NET 9) Web API
- **DB:** PostgreSQL
- **Contract:** OpenAPI 3.0 (single source of truth)
- **Auth:** JWT + RBAC

## Module boundaries
Modules (requirements + stories are split accordingly):
- AUTH, USERS, CATALOG, ORDERS, PAYMENTS, NOTIFICATIONS, ADMIN, REPORTING

## Layering (recommended)
### API Layer
- Controllers / Endpoints
- Model binding + validation
- Auth filters / policies

### Application/Service Layer
- Use cases, orchestration, business rules
- Transaction boundaries
- AuthZ checks (centralized)

### Domain (optional but recommended for complex rules)
- Domain models + domain services
- Invariants and state transitions

### Infrastructure
- EF Core repositories
- External integrations (payment, email, push)
- Caching providers
- Background jobs

## Cross-cutting concerns
### AuthN/AuthZ
- Access token (short TTL) + Refresh token
- RBAC: roles, permissions
- Policy-based authorization in .NET

### Logging & Observability
- Structured logs (JSON)
- Include `correlationId` in every request log
- Never log PII or secrets

### Error handling
- Global exception middleware
- Standard `ErrorResponse` contract

### Caching
- Cache read-heavy endpoints (catalog/options)
- Invalidate on writes
- Avoid caching sensitive user data unless necessary and justified

### Background jobs
- Use a job runner (Hangfire/Quartz/worker service) if needed
- Jobs must be idempotent and observable

## Deployment topology (generic)
- Dev: local services
- Staging: automated deploy on merge (or nightly)
- Prod: controlled deploy with rollback

See:
- `docs/03-Design/API-Conventions.md`
- `docs/03-Design/OpenAPI/openapi.yaml`
