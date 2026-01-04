
# Error Conventions

## Standard error shape (example)
```json
{
  "error": {
    "code": "ORDERS_INVALID_STATE",
    "message": "Order must be CONFIRMED before payment.",
    "correlationId": "..."
  }
}
```

## Rules
- Use stable `code` for programmatic handling
- Do not leak sensitive info in `message`
- Include correlation id for debugging
- Map validation errors to field-level details where applicable
