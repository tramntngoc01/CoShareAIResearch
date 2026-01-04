
# Definition of Done (DoD)

A story is **Done** only when:
- Acceptance Criteria are fully covered and listed in PR
- Required tests exist (unit/integration/e2e smoke) for critical rules
- No TODO/DEBUG logs; no hardcoded secrets
- No PII in logs; inputs validated/sanitized
- OpenAPI/DB/docs updated if contract or rules change
- Deployed to staging and passed smoke tests
