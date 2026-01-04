
# Testcase Standards (Project-wide)

## 1) Test ID convention
Use consistent IDs so traceability is easy:
- Unit: `UT-<MODULE>-###`
- Integration: `IT-<MODULE>-###`
- E2E/Smoke: `E2E-<MODULE>-###`
- Security sanity: `SEC-<MODULE>-###`

## 2) Test case format
Each testcase must include:
- ID
- Title
- Preconditions
- Test data (fake)
- Steps
- Expected result
- Notes (logs/correlationId to capture)
- Severity (Blocker/Critical/Major/Minor)

## 3) Coverage rules
- Every P0 story must have:
  - at least 1 integration test OR E2E smoke (as appropriate)
  - authz negative test if protected
- Money/state transitions require unit + integration minimum

## 4) Regression suite
- Mark tests as Regression-worthy when they cover critical flows or bug fixes.
- Keep regression suite lean and high value.

## 5) Traceability rule
- Every story must map to test IDs in `docs/02-Requirements/Traceability.md`.
