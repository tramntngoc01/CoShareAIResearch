
# StoryPack — US-XXX: <Title>

> Single entry point for AI + Humans. Do not guess. Follow links.

## 0) Metadata
- Story ID: **US-XXX**
- Module: **<MODULE>** (e.g., ORDERS)
- Priority: P0 / P1 / P2
- Status: Draft / Ready / In Dev / In QA / Done
- Owner (BA):
- Reviewers: Tech Lead / QA
- API mapping owner: Tech Lead
- Target sprint/release:
- Last updated:

---

## 1) What to build (1–2 paragraphs)
- User goal:
- Business value:

**Out of scope**
- OOS-1:
- OOS-2:

---

## 2) Source of truth (links)
- SRS: `docs/02-Requirements/SRS/SRS-<MODULE>.md#...`
- Stories: `docs/02-Requirements/Stories/Stories-<MODULE>.md#US-XXX`
- Business rules: `docs/02-Requirements/Business-Rules.md#BR-...`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml` (paths below)
- DB: `docs/03-Design/DB/DB-<MODULE>.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md#US-XXX`
- Test Cases: `docs/03-Testing/TestCases/TestCases-<MODULE>.md`

---

## 3) Acceptance Criteria (measurable) — summary
(Keep concise. Full text stays in Stories file.)
- AC1:
- AC2:
- AC3:

## 4) Sample data (fake)
- Example request:
- Example response:

## 5) Edge cases (min 3)
- EC1:
- EC2:
- EC3:

---

## 6) API mapping (contract-first)
> **Owner: Tech Lead.** BA must not guess API paths. Tech Lead fills this based on OpenAPI.

List exact OpenAPI paths + operations.
- `POST /api/v1/...` — tag: <MODULE> — schemas: `...`
- `GET /api/v1/...`

**Error codes used**
- `ERR_CODE_1`
- `ERR_CODE_2`

**Idempotency**
- Required? Yes/No
- Key header: `Idempotency-Key`
- Conflict behavior: 409

**Pagination/filtering**
- page/pageSize OR cursor/limit (per conventions)

---

## 7) Data & DB mapping
**Tables impacted**
- table_1 (R/W):
- table_2 (R):

**Migration**
- Needed? Yes/No
- Migration notes:
- Rollback notes:

---

## 8) Authorization & Security notes
**Roles/permissions**
- Role A can:
- Role B cannot:

**Security sanity**
- No PII logs
- Input validation rules
- Rate limit needs (if any)

---

## 9) Test plan (proof) — required IDs
> Create test IDs now and keep Traceability updated.

### Unit tests
- UT-<MODULE>-001:
### Integration tests
- IT-<MODULE>-001:
### E2E/Smoke (staging)
- E2E-<MODULE>-001:
### Security sanity
- SEC-<MODULE>-001 (authz negative)
- SEC-<MODULE>-002 (injection/basic)

---

## 10) “Done” evidence checklist (copy into PR)
- [ ] AC covered (list)
- [ ] OpenAPI implemented exactly
- [ ] AuthZ enforced + negative test exists
- [ ] Tests pass (UT/IT/E2E as applicable)
- [ ] No secrets / no PII logs / correlationId present
- [ ] Staging deploy + smoke evidence link
