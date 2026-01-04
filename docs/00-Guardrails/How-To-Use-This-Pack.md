# How to Use This Docs Pack (AI-first Safe) — Role-based Guide

Generated: 2026-01-03

This file explains **what to do, where to do it, and who owns it** so anyone joining the project can ramp up quickly.

---

\1
## Engineering rules (single entry)
- Read all project rules here: `docs/00-Guardrails/Engineering-Standards-Index.md`


## 2) Where Things Live (Quick Map)

### Discovery
- Vision: `docs/01-Discovery/Vision.md`
- MVP scope/out-of-scope: `docs/01-Discovery/MVP-Scope.md`
- User journeys: `docs/01-Discovery/User-Journey.md`
- Glossary: `docs/01-Discovery/Glossary.md`
- Open questions: `docs/01-Discovery/Open-Questions.md`

### Requirements
- SRS overview: `docs/02-Requirements/SRS/SRS-00-Overview.md`
- SRS by module: `docs/02-Requirements/SRS/SRS-<MODULE>.md`
- Stories index: `docs/02-Requirements/Stories/Stories-00-Index.md`
- Stories by module: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- Business rules (central): `docs/02-Requirements/Business-Rules.md`
- Sample (fake) data: `docs/02-Requirements/Sample-Data.md`
- Traceability: `docs/02-Requirements/Traceability.md`

### Design
- Architecture: `docs/03-Design/Architecture.md`
- API conventions: `docs/03-Design/API-Conventions.md`
- Error conventions: `docs/03-Design/Error-Conventions.md`
- OpenAPI contract: `docs/03-Design/OpenAPI/openapi.yaml`
- DB overview: `docs/03-Design/DB/DB-00-Overview.md`
- DB by module: `docs/03-Design/DB/DB-<MODULE>.md`
- Threat model: `docs/03-Design/Threat-Model.md`

### Delivery & Operations
- Sprint playbook: `docs/04-Delivery/Sprint-Playbook.md`
- Test plan: `docs/03-Testing/Test-Plan.md`
- Test cases by module: `docs/03-Testing/TestCases/TestCases-<MODULE>.md`
- CI/CD guide: `docs/04-Delivery/CI-CD-Guide.md`
- Deployment guide: `docs/04-Delivery/Deployment-Guide.md`
- Release checklist: `docs/04-Delivery/Release-Checklist.md`
- Runbook: `docs/04-Delivery/Runbook.md`
- Incident process: `docs/04-Delivery/Incident-Process.md`

### Engineering workflow
- DoD: `docs/00-Guardrails/Definition-of-Done.md`
- PR checklist: `docs/00-Guardrails/PR-Checklist.md`
- PR template: `.github/pull_request_template.md`
- ADRs (decisions): `decisions/ADR-0000-Index.md` and `decisions/ADR-0001-template.md`

---

## 3) Onboarding Flow (Read in This Order)

1. **Guardrails**
   - `docs/00-Guardrails/AI-Usage-Policy.md`
   - `docs/00-Guardrails/Security-Baseline.md`
   - `docs/00-Guardrails/Definition-of-Done.md`

2. **Product understanding**
   - `docs/01-Discovery/Vision.md`
   - `docs/01-Discovery/MVP-Scope.md`
   - `docs/01-Discovery/User-Journey.md`
   - `docs/01-Discovery/Glossary.md`

3. **How to build**
   - `docs/02-Requirements/SRS/SRS-00-Overview.md`
   - Your module’s SRS + Stories
   - `docs/03-Design/OpenAPI/openapi.yaml`
   - `docs/03-Design/DB/DB-00-Overview.md`

4. **How to verify & ship**
   - `docs/03-Testing/Test-Plan.md`
   - `docs/04-Delivery/Deployment-Guide.md`
   - `docs/04-Delivery/Release-Checklist.md`
   - `docs/04-Delivery/Runbook.md`

---

## 4) Role-based “What Do I Do?” (Actionable)

### BA (Business Analyst)
**Main outputs**
- Discovery artifacts: Vision/MVP/Journey/Glossary/Open Questions
- SRS (overview + per module)
- Stories with measurable AC + sample fake data + edge cases
- Traceability kept updated

**Where to work**
- `docs/01-Discovery/*`
- `docs/02-Requirements/SRS/*`
- `docs/02-Requirements/Stories/*`
- `docs/02-Requirements/Traceability.md`
- `docs/02-Requirements/Business-Rules.md`

**Daily checklist**
- Update Open Questions (close or set assumption/owner/date)
- Ensure each story has:
  - Measurable AC
  - Sample data (fake)
  - 3+ edge cases
  - API + Screen mapping (or mark TBD explicitly)
- Update traceability for any new/changed story

**Hand-off “Ready for Dev” definition**
- Story status = Ready
- AC measurable + data + edge cases
- Glossary terms used consistently
- No “silent assumptions” (must be recorded)

---

### Tech Lead / Architect
**Main outputs**
- Architecture decisions, module boundaries
- API conventions + error conventions
- Threat model baseline
- OpenAPI structure discipline
- DB strategy + migration approach

**Where to work**
- `docs/03-Design/Architecture.md`
- `docs/03-Design/API-Conventions.md`
- `docs/03-Design/Error-Conventions.md`
- `docs/03-Design/Threat-Model.md`
- `docs/03-Design/OpenAPI/openapi.yaml`
- `docs/03-Design/DB/*`
- `decisions/*` (ADRs)

**Daily checklist**
- Review new endpoints for consistency (authz/idempotency/errors)
- Enforce “contract-first” changes: OpenAPI updated before FE/BE implementation
- Record major decisions using ADR template

---

### Dev Backend (.NET 9)
**Main outputs**
- API implementation aligned with OpenAPI
- Business rules + state transitions implemented
- Unit/integration tests for critical rules
- Migrations and rollback notes

**Where to work**
- Contract: `docs/03-Design/OpenAPI/openapi.yaml`
- Rules: `docs/02-Requirements/Business-Rules.md`
- Module SRS/Stories: `docs/02-Requirements/SRS/SRS-<MODULE>.md`, `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- DB module doc: `docs/03-Design/DB/DB-<MODULE>.md`
- PR template + DoD: `.github/pull_request_template.md`, `docs/00-Guardrails/Definition-of-Done.md`

**Before coding**
- Confirm story is Ready
- Confirm endpoint exists in OpenAPI (or add it first)
- Identify authorization rules and logging policy (no PII)

**In PR**
- List covered AC
- Add tests for critical rules
- Add/verify migration + rollback notes
- Ensure no secrets/PII in logs

---

### Dev Frontend / Mobile
**Main outputs**
- UI implementation strictly following OpenAPI
- Proper state handling: loading/error/empty
- Form validation + server error mapping
- E2E smoke tests for critical flows

**Where to work**
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- Stories (screen behaviors): `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- Traceability: `docs/02-Requirements/Traceability.md`
- Error conventions: `docs/03-Design/Error-Conventions.md`

**Rules**
- Do not invent fields; request contract changes if missing
- Always attach UI evidence (screenshots/video) in PR

---

### QA / QC
**Main outputs**
- Test cases derived from AC, including edge cases
- Regression suite for release
- UAT execution + sign-off artifacts
- Bug reports with evidence and correlationId (when available)

**Where to work**
- Test plan: `docs/03-Testing/Test-Plan.md`
- Module test cases: `docs/03-Testing/TestCases/TestCases-<MODULE>.md`
- Traceability: `docs/02-Requirements/Traceability.md`
- Stories: `docs/02-Requirements/Stories/*`
- Release gate: `docs/04-Delivery/Release-Checklist.md`

**Daily checklist**
- Ensure every Ready story has test coverage planned
- Add negative authz tests for sensitive endpoints
- Keep regression suite current when bugs are found/fixed

---

### DevOps / Release Owner
**Main outputs**
- CI baseline enforced
- Staging automated deploy
- Production deploy process + rollback verification
- Monitoring/alerts + runbook readiness

**Where to work**
- CI baseline doc: `docs/00-Guardrails/CI-Baseline.md`
- CI/CD guide: `docs/04-Delivery/CI-CD-Guide.md`
- Deployment guide: `docs/04-Delivery/Deployment-Guide.md`
- Release checklist: `docs/04-Delivery/Release-Checklist.md`
- Runbook + incident: `docs/04-Delivery/Runbook.md`, `docs/04-Delivery/Incident-Process.md`

**Release checklist**
- Ensure blocker=0, UAT sign-off
- Verify rollback steps and monitoring after deploy

---

## 5) How to Work by Module (Practical)

For any module (e.g., ORDERS), keep these files aligned:

1. Requirements: `docs/02-Requirements/SRS/SRS-ORDERS.md`
2. Stories: `docs/02-Requirements/Stories/Stories-ORDERS.md`
3. DB design: `docs/03-Design/DB/DB-ORDERS.md`
4. Test cases: `docs/04-Delivery/TestCases/TestCases-ORDERS.md`
5. Contract: `docs/03-Design/OpenAPI/openapi.yaml` (paths tagged ORDERS)
6. Traceability: `docs/02-Requirements/Traceability.md`

**Rule:** If you change one, you likely must update at least two others.

---

## 6) Minimal Working Agreement (Team Habits)

- Every sprint: staging deploy + smoke evidence
- Every PR: list AC, tests, security checks, migration/rollback
- Every breaking decision: write an ADR
- Every open question: has owner + due date or a documented assumption

---

## 7) “First Week” Plan for New Joiners

Day 1:
- Read guardrails + vision + glossary
- Pick one module and read its SRS + Stories

Day 2:
- Review OpenAPI and error conventions
- Walk through traceability examples

Day 3–5:
- Take a small story and complete:
  - contract update (if needed)
  - implementation
  - tests
  - PR with checklist
  - staging deploy + smoke


## Implementation Plan (Approve before coding)

Before major coding begins, create and approve an Implementation Plan:
- Overview: `docs/03-ImplementationPlan/Implementation-Plan-00-Overview.md`
- Per module: `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`

Approval requires (minimum): BA + Tech Lead + QA (and DevOps if deploy changes).
