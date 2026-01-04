
# Testing Strategy (What tests to write, where)

This pack already includes:
- `docs/04-Delivery/Test-Plan.md` (overall strategy)
- `docs/04-Delivery/TestCases/TestCases-<MODULE>.md` (module test cases)
This guide adds **implementation-level guidance** so devs know what to code.

---

## 1) Test pyramid (recommended)
### Unit tests (fast)
Goal: cover business rules, state transitions, pure logic
- Location: `/src/api/tests/<Project>.UnitTests` (suggestion)
- Examples:
  - validation rules
  - status transition rules
  - calculation functions

### Integration tests (medium)
Goal: verify API + DB behavior
- Location: `/src/api/tests/<Project>.IntegrationTests`
- Examples:
  - repository queries
  - transactional operations
  - authz negative checks

### E2E smoke (slow, minimum set)
Goal: prove critical journeys work on staging
- Location: `/src/web/e2e` (Playwright) or `/tests/e2e`
- Minimum:
  - login
  - core journey (browse -> create order -> payment sandbox)
  - admin critical action (if applicable)

---

## 2) What must be tested (non-negotiable)
- Business rules that affect money/state/permissions
- Authorization checks (403) for protected resources
- Idempotency for side-effect endpoints (payments, submit order)
- Validation & error codes shape
- Migration safety (apply migrations on clean DB)

---

## 3) Mapping tests to docs (traceability)
For each story:
1. Add test IDs in `docs/04-Delivery/TestCases/TestCases-<MODULE>.md`
2. Add the same IDs in `docs/02-Requirements/Traceability.md`
3. Reference the test IDs in PR description as evidence

---

## 4) CI expectations
On PR:
- lint/format
- unit tests
- build
- secrets scan + dependency scan
Optional:
- integration tests (or nightly if heavy)
- e2e smoke (on staging after merge)

See `docs/00-Guardrails/CI-Baseline.md` and `docs/04-Delivery/CI-CD-Guide.md`.
