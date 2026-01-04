
# AI Usage Policy (Safe AI-first)

## Red / Amber / Green data
### RED (Never share with AI)
- Real PII/PCI/PHI (IDs, phone, email, addresses, bank details)
- Secrets: tokens/keys/passwords/connection strings
- Production database dumps, customer contracts, private URLs

### AMBER (Only if masked + minimal)
- Stack traces with payload removed
- DB schema without customer identifiers
- Sanitized screenshots (blur sensitive data)

### GREEN (OK)
- Clean requirements, user stories, OpenAPI samples, fake test data
- Non-sensitive code snippets without secrets
- Threat models, checklists, templates

## Prompt hygiene
- Always include: Goal, Context, Constraints, Output format, Acceptance
- Never ask the AI to “guess fields/APIs”; link to OpenAPI/schema instead
- Require: Assumptions + Edge cases + Risks in draft outputs

## Output policy
- AI outputs are drafts only. Human review + CI gates are mandatory before merge.
- Security-critical code (auth, crypto, payments) must be reviewed by Tech Lead.
