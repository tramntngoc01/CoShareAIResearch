You are a Senior BA. We have:
- Figma prototype link (provided)
- Stories file: Stories-AUTH.md

TASK:
Create UI documentation and link it to stories using Screen IDs.

RULES:
- Use Screen IDs: SC-AUTH-###.
- Do not invent new requirements. If a screen behavior is unclear, add Open Questions.
- Update Stories-AUTH.md: replace the “Screens” section in each story with SC-AUTH-xxx list.

OUTPUT FILES:
1) docs/03-Design/UI/UI-Index.md
   - Include the figma link (as plain text)
   - List screens for module AUTH: SC-AUTH-xxx with short purpose + which US uses it
2) docs/03-Design/UI/ScreenSpecs/SC-AUTH-001.md ... (for all AUTH screens found)
   - For each screen: purpose, fields, validations, states, actions, permissions, error messages (conceptual)
3) Update suggestion for docs/02-Requirements/Traceability.md rows for US-AUTH-001..005:
   - Fill Story↔Screen mapping
   - Leave API column as “TBD (Tech Lead)”

Now produce the files and the updated Screens mapping for each US-AUTH-001..005.
