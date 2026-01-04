You are a Senior Business Analyst. We have completed Discovery. Now produce Phase 2 Requirements artifacts following BA Role Guide.

INPUTS (Discovery files exist):
- docs/01-Discovery/Vision.md
- docs/01-Discovery/MVP-Scope.md
- docs/01-Discovery/User-Journey.md
- docs/01-Discovery/Glossary.md
- docs/01-Discovery/Open-Questions.md
- docs/01-Discovery/Modules.md

STRICT RULES:
- Do not invent requirements. If missing, record as Open Questions or explicit Assumptions.
- Use consistent terms from Glossary.
- Every story must have measurable AC, fake sample data, and 3+ edge cases.
- Create traceability mapping Story↔Screen↔(API TBD)↔DB↔Test IDs.
- Do NOT list API paths; mark API as TBD (Tech Lead will fill later).

OUTPUT FILES (write as separate markdown sections with filename headers):
1) docs/02-Requirements/SRS/SRS-00-Overview.md
2) docs/02-Requirements/SRS/SRS-<MODULE>.md for each module in Modules.md (at least MVP modules)
3) docs/02-Requirements/Stories/Stories-00-Index.md
4) docs/02-Requirements/Stories/Stories-<MODULE>.md (create US-xxx for MVP, prioritized)
5) docs/02-Requirements/Business-Rules.md (BR-xxx list extracted from Discovery/Journeys)
6) docs/02-Requirements/Sample-Data.md (fake data sets usable for dev/test)
7) docs/02-Requirements/Traceability.md (mapping table with API column = TBD)

PROCESS:
- First, propose MVP module order and story list P0/P1.
- Then write SRS overview and module SRS.
- Then write Stories per module.
- Then produce Business Rules, Sample Data, and Traceability.

Now ask me only if you cannot proceed due to missing Discovery content; otherwise generate the documents.
