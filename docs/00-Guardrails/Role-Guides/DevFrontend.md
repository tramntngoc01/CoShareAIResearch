
# Dev Frontend (React) Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Implement UI đúng theo OpenAPI contract, handle states đầy đủ, validate form, mapping error codes, có E2E smoke tối thiểu.

## Inputs
- StoryPack entry point: `docs/02-Requirements/StoryPacks/US-XXX.md`

- Stories module: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- Error conventions: `docs/03-Design/Error-Conventions.md`
- Traceability: `docs/02-Requirements/Traceability.md`
- Implementation Plan (Approved): `docs/03-ImplementationPlan/Implementation-Plan-<MODULE>.md`

## Outputs
- UI screens/components for the story
- Client-side validation + server error mapping
- E2E smoke (minimum) for critical flow
- PR with screenshots/video evidence

## Daily workflow (per story)
1. Confirm story is Ready + plan Approved
2. Identify OpenAPI endpoints used; generate typed client if applicable
3. Implement screen:
   - loading/error/empty states
   - optimistic UI only if consistent with backend idempotency/state rules
4. Map server errors:
   - use `error.code` to display correct messages
5. Add/extend E2E smoke if this is a critical flow
6. Attach UI evidence in PR (screenshots/video)

## Frontend Definition of Done
- No guessed fields; everything follows OpenAPI
- Error states fully handled
- Form validation (client) + mapping server validation errors
- UI evidence provided
- E2E smoke updated for critical flows

## What NOT to do
- Don’t invent request fields because UI “needs it”
- Don’t swallow errors without surfacing correlationId for support

## AI Prompt (Short)
```
You are a senior React engineer. Implement story <US-XXX> end-to-end on frontend:
- Use OpenAPI contract only (no guessing)
- Implement UI with loading/error/empty states
- Add client validation + map server error codes
- Add/extend E2E smoke for main journey
Return: component list, API calls, state model, and test steps.
```
