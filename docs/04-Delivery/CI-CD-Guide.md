
# CI/CD Guide (Recommended)

## CI on Pull Request
- Backend: restore -> build -> test -> (optional) coverage
- Frontend: install -> lint -> test -> build
- Security: secrets scan (gitleaks), dependency scan (Dependabot/Trivy)

## CD to Staging
- Trigger: merge to `develop` or `main` (your choice)
- Steps:
  1. Build artifacts
  2. Apply DB migrations (staging)
  3. Deploy API
  4. Deploy FE
  5. Run smoke tests

## Prod deploy (controlled)
- Manual approval required
- Must run Release Checklist
- Rollback plan verified
