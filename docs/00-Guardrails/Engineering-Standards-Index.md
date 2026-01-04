
# Engineering Standards Index (Start here for rules)

This page is the **single entry point** for all engineering rules so humans and AI agents can quickly find the correct standards.

## 1) Stack & environment
- Stack profile (what technologies we use): `docs/00-Guardrails/Stack-Profile.md`
- Security baseline (minimum security rules): `docs/00-Guardrails/Security-Baseline.md`
- AI usage policy (data safety): `docs/00-Guardrails/AI-Usage-Policy.md`

## 2) API & contract rules
- OpenAPI contract (source of truth): `docs/03-Design/OpenAPI/openapi.yaml`
- API conventions (pagination/versioning/idempotency/correlationId): `docs/03-Design/API-Conventions.md`
- Error conventions (error codes + response shape): `docs/03-Design/Error-Conventions.md`

## 3) Database rules
- DB design rules (naming/datatypes/indexing/migrations): `docs/03-Design/DB/DB-Design-Rules.md`
- DB per module: `docs/03-Design/DB/DB-<MODULE>.md`

## 4) Backend rules
- Backend coding standards (.NET): `docs/00-Guardrails/Backend-Coding-Standards.md`

## 5) Frontend rules
- Frontend coding standards (React): `docs/00-Guardrails/Frontend-Coding-Standards.md`

## 6) Testing rules
- Testing strategy (what to test, where): `docs/03-Testing/Testing-Strategy.md`
- Test plan (how we execute): `docs/03-Testing/Test-Plan.md`
- Test case standards: `docs/03-Testing/Testcase-Standards.md`
- Test cases per module: `docs/03-Testing/TestCases/TestCases-<MODULE>.md`

## 7) Quality gates
- Definition of Done: `docs/00-Guardrails/Definition-of-Done.md`
- PR checklist: `docs/00-Guardrails/PR-Checklist.md`
- CI baseline: `docs/00-Guardrails/CI-Baseline.md`
- PR template: `.github/pull_request_template.md`

## 8) Pre-coding approval
- Implementation Plan overview: `docs/03-ImplementationPlan/Implementation-Plan-00-Overview.md`
- Implementation Plan per module: `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`
