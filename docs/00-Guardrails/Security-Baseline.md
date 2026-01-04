
# Security Baseline (Minimum)

## Secrets
- Store secrets only in secret managers / CI secrets (never in repo)
- Scan repo for secrets (gitleaks) on every PR
- Rotate keys on leakage

## Auth
- Passwords: hash with a strong algorithm (e.g., BCrypt/Argon2 via established libraries)
- JWT: short-lived access tokens; refresh tokens stored securely
- Lockout / throttling on auth endpoints

## Authorization (AuthZ)
- Centralize policy checks in service layer
- Deny-by-default approach
- Protect admin endpoints with stricter permissions

## Input validation
- Validate request models (server-side)
- Reject unexpected fields where feasible
- Sanitize user-generated content if rendered as HTML

## OWASP basics
- Broken access control: add negative tests
- Injection: parameterized queries (EF Core), validate filters/sorts
- Sensitive data exposure: encrypt at rest where required, TLS in transit

## Logging
- No PII, no secrets
- Use correlationId
- Audit critical operations (role changes, payment actions)

## Dependencies
- Enable Dependabot (or equivalent)
- Block high/critical CVEs unless exception approved
