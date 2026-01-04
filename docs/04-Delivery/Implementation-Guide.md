
# Implementation Guide (Where & How to Implement)

This pack intentionally keeps **implementation** in the codebase (`/src`), while `/docs` defines the contract, rules, and gates.
Use this guide so everyone implements consistently.

---

## 1) Where code lives (recommended)
- `/src/api` — ASP.NET Core (.NET 9) Web API
- `/src/web` — React web (Vite or Next.js)
- `/src/mobile` — (optional) mobile app
- `/tests` — integration/E2E test projects (optional)
- `/docs` — requirements/design/delivery/runbooks (this pack)

> If your repo already has a different structure, document it in `docs/00-Guardrails/Stack-Profile.md`.

---

## 2) Contract-first workflow (mandatory)
1. Start from **Story**: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
2. Ensure requirements are clear in **SRS**: `docs/02-Requirements/SRS/SRS-<MODULE>.md`
3. Update **OpenAPI** first (paths + schemas): `docs/03-Design/OpenAPI/openapi.yaml`
4. Update **DB design** (if needed): `docs/03-Design/DB/DB-<MODULE>.md`
5. Update **Traceability**: `docs/02-Requirements/Traceability.md`
6. Implement BE/FE strictly following OpenAPI
7. Add tests
8. PR with checklist + CI pass
9. Deploy staging + smoke evidence

---

## 3) Implementation steps per story (the “beat”)
### A. Refine (Definition of Ready)
A story is “Ready” when:
- AC measurable
- Sample (fake) data included
- 3+ edge cases listed
- API mapping exists (or TBD explicitly)
- AuthZ notes included

### B. Backend implementation (.NET 9)
Checklist:
- Request/response models match OpenAPI
- Validation (server-side)
- AuthZ check in service layer (deny-by-default)
- Business rules + state transitions implemented
- Error codes follow `docs/03-Design/Error-Conventions.md`
- Logging uses correlationId and avoids PII
- DB migrations created (if schema changes)

### C. Frontend / Mobile implementation
Checklist:
- Do not guess fields; follow OpenAPI
- Handle loading/error/empty states
- Map server error codes to UI messages
- Validate forms client-side (in addition to server validation)
- Provide UI evidence in PR (screenshot/video)

### D. PR & Merge
Use:
- `.github/pull_request_template.md`
- `docs/00-Guardrails/PR-Checklist.md`
- `docs/00-Guardrails/Definition-of-Done.md`

---

## 4) Module alignment rule
If you touch a module’s behavior, you likely must update:
- SRS module file
- Stories module file (if AC changes)
- OpenAPI contract
- DB module file (if schema changes)
- Module test cases
- Traceability

---

## 5) “AI usage during implementation” (safe)
Allowed:
- Generate scaffolding code from OpenAPI
- Suggest unit test cases from AC
- Refactor suggestions based on conventions
Not allowed:
- Pasting secrets/real PII into prompts
- Copying security-critical code blindly without review

See `docs/00-Guardrails/AI-Usage-Policy.md`.
