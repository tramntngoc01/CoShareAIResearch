
# Deployment Guide (.NET 9 + React + PostgreSQL)

## Environments
- Dev: local API + local Postgres (Docker recommended)
- Staging: automated deploy for QA/UAT, isolated DB
- Prod: controlled deploy with monitoring + rollback

## Configuration & secrets
- Use environment variables or secret manager (never commit secrets)
- Typical secrets:
  - DB connection string
  - JWT signing key
  - External integration keys (payment/email/push)
- Rotate policy:
  - On leak: immediate rotation
  - Regular: quarterly (or per compliance)

## Database migrations
- Use EF Core migrations (recommended)
- Staging:
  - Apply migrations automatically in pipeline (with safeguards)
- Prod:
  - Prefer manual approval step before migration, or run migration job with rollback plan

## Deploy steps (staging)
1. Build API (`dotnet publish`)
2. Build web (`npm run build`)
3. Run migrations
4. Deploy API
5. Deploy web
6. Smoke test (critical flows)

## Deploy steps (prod)
- Same as staging + approvals + monitoring checks + rollback readiness
