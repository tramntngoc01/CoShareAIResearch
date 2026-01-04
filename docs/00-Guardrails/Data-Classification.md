
# Data Classification

| Class | Examples | Allowed in AI prompts? | Required handling |
|------|----------|-------------------------|------------------|
| Public | marketing copy, public docs | Yes | none |
| Internal | clean requirements, fake data | Yes | keep minimal |
| Confidential | internal architecture, schema | Amber | mask/minimize |
| Restricted | PII, secrets, prod dumps | No | never share |

## Masking rules (minimum)
- Replace names with UserA/UserB, IDs with UUID-like placeholders
- Remove tokens, headers, cookies, connection strings
- Replace addresses/phones/emails with dummy values
