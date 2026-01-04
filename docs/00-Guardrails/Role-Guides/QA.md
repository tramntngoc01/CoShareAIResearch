
# QA / QC Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Prove the system works according to AC, prevent regressions, and run UAT with evidence. Maintain traceability to tests.

## Inputs
- StoryPack entry point: `docs/02-Requirements/StoryPacks/US-XXX.md`

- Stories: `docs/02-Requirements/Stories/*`
- Business rules: `docs/02-Requirements/Business-Rules.md`
- OpenAPI: `docs/03-Design/OpenAPI/openapi.yaml`
- Testing plan: `docs/03-Testing/Test-Plan.md`
- Module test cases: `docs/03-Testing/TestCases/TestCases-<MODULE>.md`
- Release checklist: `docs/04-Delivery/Release-Checklist.md`

## Outputs
- Test cases per module with IDs
- Regression suite for release
- Bug reports with repro steps + expected/actual + evidence (logs/correlationId)
- UAT sign-off evidence

## Daily workflow
1. For each Ready story: ensure test cases exist (happy + edge)
2. Add security sanity tests:
   - authz negative checks
   - basic injection/input validation
3. Keep Traceability updated with Test Case IDs
4. During testing:
   - capture evidence (screenshots, correlationId, logs)
5. Before release:
   - ensure blocker=0 and regression passed

## QA Definition of Done
- Every P0/P1 story has test coverage planned
- Module test cases contain IDs referenced in Traceability
- Regression suite updated when bugs fixed
- UAT evidence stored and sign-off recorded

## AI Prompt (Short)
```
You are a QA lead. For story <US-XXX>, create test cases from AC:
- Include preconditions, steps, expected, test data, severity
- Include 3+ edge cases and 2 security sanity tests
- Assign test IDs and update Traceability mapping
Output updates for docs/03-Testing/TestCases/TestCases-<MODULE>.md and docs/02-Requirements/Traceability.md.
```
