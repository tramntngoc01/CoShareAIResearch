
# Tech Lead / Architect Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Đảm bảo team implement đúng cách, an toàn, và giảm rủi ro trước khi code lớn: **contract-first**, kiến trúc rõ, authz rõ, migration/rollback rõ.

## Inputs
- SRS overview + module: `docs/02-Requirements/SRS/*`
- Stories: `docs/02-Requirements/Stories/*`
- Business rules: `docs/02-Requirements/Business-Rules.md`
- Security baseline: `docs/00-Guardrails/Security-Baseline.md`

## Outputs
- Review StoryPacks for OpenAPI/DB/AuthZ correctness: `docs/02-Requirements/StoryPacks/US-XXX.md`

## Outputs
- Architecture: `docs/03-Design/Architecture.md`
- API conventions: `docs/03-Design/API-Conventions.md`
- Error conventions: `docs/03-Design/Error-Conventions.md`
- Threat model: `docs/03-Design/Threat-Model.md`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- DB docs: `docs/03-Design/DB/*`
- Implementation Plans (approve before coding):
  - `docs/03-ImplementationPlan/Implementation-Plan-00-Overview.md`
  - `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`
- ADRs: `decisions/ADR-0000-Index.md`

## Daily workflow
1. Ensure **contract-first**: OpenAPI updated before implementation
2. Review or author **Implementation Plan** for module/feature slice
3. Define AuthZ policies and audit requirements
4. Validate DB migrations & rollback approach
5. Security sanity: OWASP basics + secrets + logging PII constraints
6. Record major decisions with ADR

## Implementation Plan (Ownership & Approval)
- **Owner (writes plan):** Tech Lead/Architect
- **Approvals (minimum):** BA + Tech Lead + QA (+ DevOps if deploy changes)
- Rule: no major coding until plan is **Approved**

## Tech Lead Definition of Done
- OpenAPI updated and consistent with conventions
- Error codes standardized
- AuthZ model explicit (roles/permissions/policies)
- DB and migration/rollback strategy defined
- Implementation Plan approved
- Traceability can be completed without guessing

## What NOT to do
- Don’t allow FE/BE to “đoán contract”
- Don’t merge security-critical changes without review/tests

## AI Prompt (Short)
```
You are a Tech Lead. Create or update Implementation Plan for <MODULE> with:
scope, work breakdown (BE/FE/DB), OpenAPI paths, authz plan, migration/rollback, test plan (UT/IT/E2E + IDs), observability, risks, rollout.
Output in docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md format. Do not guess missing requirements; list Open Questions.
```

## AI Prompt (Design review)
```
You are a Tech Lead reviewer. Review changes to OpenAPI + DB + Implementation Plan for <MODULE>.
Check: conventions, authz, error codes, idempotency, migration safety, and test coverage plan.
Return issues + concrete fixes.
```


## AI Prompt (StoryPack review)
```text
You are a Tech Lead. Review StoryPack for <US-XXX>: check OpenAPI paths, schemas, authz rules, migration/rollback notes, error codes, idempotency, and test IDs. List issues and propose concrete fixes.
```


## StoryPacks responsibility
- Tech Lead owns the **API mapping** section in StoryPacks: OpenAPI paths, operations, schema names, error codes, idempotency.
