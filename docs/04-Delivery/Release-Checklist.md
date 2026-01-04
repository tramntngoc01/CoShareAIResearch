
# Release Checklist

## Pre-release
- [ ] Regression suite passed
- [ ] Security scans (deps/secrets/SAST baseline) passed
- [ ] Release notes drafted
- [ ] Migration plan reviewed
- [ ] Rollback plan reviewed
- [ ] Monitoring dashboards ready

## Deploy
- [ ] Deploy staging -> smoke
- [ ] Deploy production
- [ ] Verify key KPIs/alerts

## Post-release
- [ ] Tag version
- [ ] Publish release notes
- [ ] Retrospective issues logged
