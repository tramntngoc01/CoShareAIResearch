
# Frontend Coding Standards (React)

## 1) Contract-first
- FE must not guess fields. Use OpenAPI as source of truth.
- Prefer generating typed API client from OpenAPI.

## 2) UI states
Every screen must handle:
- loading
- error
- empty
- success

## 3) Forms
- Client validation for fast feedback
- Always map server validation errors (do not hide them)
- Show correlationId for support when server returns it

## 4) Error mapping
- Map `error.code` to user-friendly messages.
- Do not rely on raw server messages for UX.

## 5) State management
- Use consistent state strategy per project (document here if Redux/Zustand/React Query).
- Avoid duplicated sources of truth.

## 6) E2E smoke
- Add/update E2E smoke tests for critical journeys.
- Keep them stable (avoid flakiness).

## 7) PR evidence
- UI changes must include screenshot/video evidence in PR.

## 8) E2E + `data-field` selectors (mandatory)

### `data-field` is required for test stability
- Use `data-field` (NOT `data-testid`) for **all critical / interactive elements** (inputs, primary actions, key containers).
- Naming: lowercase kebab-case.
- Minimum required tags:
  - Screen root: `data-field="screen.<module>.<screen-id>"`
  - Primary CTA: `data-field="action.<name>"`
  - Inputs: `data-field="input.<field-name>"`
  - Error summary/field errors: `data-field="error.summary"`, `data-field="error.<field-name>"`
  - States: `data-field="state.loading" | "state.empty" | "state.success"`

### Playwright E2E required for each user story that has UI
- For every UI story (US-xxx), FE must:
  - Add/extend Playwright tests: **1 happy path + ≥2 edge/negative cases**
  - Run tests before PR and attach evidence (command + summary output)
- Selector rule:
  - Prefer Playwright locators by `data-field`, e.g. `[data-field="input.phone"]`
  - Avoid `waitForTimeout`; use `expect(...)` with proper waiting.
