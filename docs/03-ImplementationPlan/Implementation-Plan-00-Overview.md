
# Implementation Plan — Overview (Approve before coding)

Date: 2026-01-03

This is the **pre-coding approval artifact**. The goal is to reduce risk and align the team on:
- what will be built in this iteration/sprint/release
- how it will be implemented (BE/FE/DB/Infra)
- what will be tested and how we prove it works
- rollout strategy and risk controls

> Rule: No major coding starts until this plan is reviewed and approved by Tech Lead + BA + QA (and DevOps if deployment changes).

---

## 1) Inputs (must be ready)
- SRS module: `docs/02-Requirements/SRS/SRS-<MODULE>.md`
- Stories module: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- OpenAPI draft: `docs/03-Design/OpenAPI/openapi.yaml`
- DB module: `docs/03-Design/DB/DB-<MODULE>.md`
- Traceability: `docs/02-Requirements/Traceability.md`
- Threat model notes: `docs/03-Design/Threat-Model.md`

---

## 2) Outputs (what this plan contains)
For each module or feature slice, the plan must include:
- Scope and goals (what is included/excluded)
- Work breakdown (BE/FE/DB/Jobs/Infra)
- API endpoints to implement/change (OpenAPI references)
- Data/migration plan (tables/indexes, migration steps, rollback notes)
- Authorization plan (roles/permissions/policies)
- Test plan for this slice (unit/integration/e2e) with test IDs
- Observability plan (logs/metrics, correlationId)
- Risks + mitigations
- Estimation and owners
- Rollout plan (feature flag, staged rollout, migration order)

---

## 3) Approval checklist
- [ ] Scope matches MVP and out-of-scope list
- [ ] All stories have measurable AC + sample data + edge cases
- [ ] OpenAPI updated (no “guess fields”)
- [ ] AuthZ rules are explicit
- [ ] DB migrations and rollback steps are defined
- [ ] Tests defined with IDs and mapped in Traceability
- [ ] Security baseline satisfied (no PII logs, no secrets, OWASP basics)
- [ ] Staging deploy + smoke plan included

---

## 4) Where to create plans
- Module plans live here: `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`
- Cross-module plans (feature slice) can use: `Implementation-Plan-XX-FeatureSlice.md`
