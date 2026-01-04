
# Role Guides (AI-readable)

Date: 2026-01-03

Mục tiêu: mỗi file là **hướng dẫn riêng cho 1 vai trò**, viết theo kiểu **AI agent đọc vào là làm được**.

## Cách dùng nhanh
- Nếu bạn đang dùng AI agent: mở file đúng vai trò và copy prompt ở mục **“AI Prompt (Short)”**.
- Nếu bạn là người: làm theo mục **“Daily workflow”** và **“Definition of Done for this role”**.

## Files
- BA: `BA.md`
- Tech Lead / Architect: `TechLead.md`
- Dev Backend (.NET 9): `DevBackend.md`
- Dev Frontend (React): `DevFrontend.md`
- QA/QC: `QA.md`
- DevOps / Release Owner: `DevOps.md`

## Global rules (applies to all roles)
- OpenAPI is source of truth: `docs/03-Design/OpenAPI/openapi.yaml`
- Traceability must be updated: `docs/02-Requirements/Traceability.md`
- No secrets / no real PII in AI prompts: `docs/00-Guardrails/AI-Usage-Policy.md`
- Implementation starts only after Implementation Plan is Approved:
  - `docs/03-ImplementationPlan/Implementation-Plan-00-Overview.md`
  - `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`

## StoryPacks
- Template and guide: `docs/02-Requirements/StoryPacks/`
- Owner: BA (overall), **API mapping owner: Tech Lead**, Contributor: QA
