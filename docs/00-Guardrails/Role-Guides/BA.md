
# BA Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Chốt đúng thứ cần làm, viết yêu cầu **test được**, và giữ **Traceability** để Dev/QA/AI không “đoán”.

## Inputs (read these first)
- Vision: `docs/01-Discovery/Vision.md`
- MVP scope: `docs/01-Discovery/MVP-Scope.md`
- Glossary: `docs/01-Discovery/Glossary.md`
- Open questions: `docs/01-Discovery/Open-Questions.md`

## Outputs (you must produce)
- StoryPacks per story: `docs/02-Requirements/StoryPacks/US-XXX.md`
- SRS overview: `docs/02-Requirements/SRS/SRS-00-Overview.md`
- SRS module: `docs/02-Requirements/SRS/SRS-<MODULE>.md`
- Stories module: `docs/02-Requirements/Stories/Stories-<MODULE>.md`
- Business rules: `docs/02-Requirements/Business-Rules.md`
- Sample data (fake): `docs/02-Requirements/Sample-Data.md`
- Traceability: `docs/02-Requirements/Traceability.md`
- StoryPacks (single-entry per story): `docs/02-Requirements/StoryPacks/US-XXX.md`


## Daily workflow
1. Update/close **Open Questions**
2. For each feature/module:
   - Update SRS module
   - Write/adjust Stories with measurable AC
   - Add/confirm sample data (fake)
   - Add edge cases (≥3/story)
3. Update Traceability for any changes (Story ↔ Screen ↔ API ↔ DB ↔ Test IDs)
4. Review Implementation Plan drafts (scope/AC consistency)

## Story template (minimum)
Each story must include:
- Title + Persona + Goal
- Priority (P0/P1/P2)
- Acceptance Criteria (measurable)
- Sample data (fake)
- Edge cases (≥3)
- API mapping (OpenAPI paths) or mark **TBD** explicitly
- Screens (if any)
- Business rules referenced (BR-xxx)

## BA Definition of Done
- Glossary terms are consistent (no synonyms)
- Story has measurable AC + fake data + edge cases
- No hidden assumptions: either closed question or recorded assumption
- Traceability row exists and is updated

## What NOT to do
- Do not invent API fields/endpoints; request update in OpenAPI
- Do not accept “AC mơ hồ” (nhanh/đẹp/dễ dùng) without metrics

## AI Prompt (Short)
Use when you want AI to act as BA:
```
You are a BA. For module <MODULE>, update SRS and write/refresh user stories with measurable AC, fake sample data, and 3+ edge cases each. 
Update Traceability mapping Story↔Screen↔API↔DB↔Test IDs. Do not guess unknown APIs; mark TBD and add Open Questions.
```

## AI Prompt (Per-story)
```
You are a BA. Create/refresh story <US-XXX>:
- Provide measurable AC
- Provide fake sample data
- Provide 3+ edge cases
- List required API paths (or TBD)
- List screens
- List impacted business rules (BR-xxx)
```


## StoryPacks responsibility
- BA creates StoryPack and fills everything **except** the **API mapping** section.
- API mapping is owned by Tech Lead.
