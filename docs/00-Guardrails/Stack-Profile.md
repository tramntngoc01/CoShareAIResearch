
# Stack Profile (Baseline)

Date: 2026-01-03

This docs pack is prefilled for:
- **Backend:** ASP.NET Core (.NET 9) Web API
- **Frontend Web:** React (Vite) OR Next.js (choose one)
- **Mobile:** (Optional) React Native / Flutter (add if applicable)
- **Database:** PostgreSQL
- **Auth:** JWT (access/refresh) + RBAC (roles/permissions)
- **API Contract:** OpenAPI 3.0 single source of truth
- **Migrations:** EF Core migrations (recommended)

## Repository layout (suggested)
- `/src/api` — .NET 9 API
- `/src/web` — React web
- `/docs` — this documentation
- `/decisions` — ADRs
- `/.github` — PR templates, workflows

## Environments
- Dev: local + dev DB
- Staging: CI/CD deployed for QA/UAT
- Prod: controlled deploy + monitoring + rollback

## Tooling baseline (recommended)
- .NET: `dotnet format`, `dotnet test`, `dotnet build`
- FE: `eslint`, `prettier`, `vitest/jest`, `playwright` (smoke)
- Security: `gitleaks` (secrets), `trivy` or `dependabot` (deps)


## Standards
- All engineering rules index: `docs/00-Guardrails/Engineering-Standards-Index.md`
