
# Test Plan

## Test levels
- Unit: business rules & pure logic
- Integration: API + DB
- E2E smoke: critical user journeys
- Regression: prioritized suite for releases
- Performance smoke: basic p95 checks for critical endpoints

## Environments
- Staging mirrors prod config as close as possible (without real data)

## Test data strategy
- Use factories/seed scripts; never prod dumps
- Keep deterministic IDs and reusable datasets

## Minimum E2E smoke (examples)
- Auth login/logout
- Browse catalog
- Create order
- Initiate payment (mock/sandbox)
- Admin basic operations
