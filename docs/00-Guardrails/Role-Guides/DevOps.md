
# DevOps / Release Owner Role Guide (AI-readable)


> This guide is **AI-readable** and **human-usable**.
> Follow the checklists strictly. If something is missing, create an Open Question or update the relevant doc (do not guess).


## Mission
Ensure CI gates, safe staging deploy each sprint, safe prod release with rollback + monitoring.

## Inputs
- StoryPack entry point: `docs/02-Requirements/StoryPacks/US-XXX.md`

- CI baseline: `docs/00-Guardrails/CI-Baseline.md`
- CI/CD guide: `docs/04-Delivery/CI-CD-Guide.md`
- Deployment guide: `docs/04-Delivery/Deployment-Guide.md`
- Runbook: `docs/04-Delivery/Runbook.md`
- Incident process: `docs/04-Delivery/Incident-Process.md`
- Release checklist: `docs/04-Delivery/Release-Checklist.md`
- Security baseline: `docs/00-Guardrails/Security-Baseline.md`

## Outputs
- CI pipeline enforcement
- CD to staging
- Prod deploy process with approvals
- Monitoring/alerts dashboards
- Rollback verified

## Daily workflow
1. Ensure CI runs on every PR (lint/test/build + scans)
2. Keep secrets management clean (no secrets in repo)
3. Ensure staging deploy pipeline works and is observable
4. For releases:
   - confirm UAT sign-off + blocker=0
   - confirm migration and rollback steps
   - deploy staging -> verify -> deploy prod
   - monitor metrics and errors

## DevOps Definition of Done
- CI baseline enforced and stable
- Staging deploy reproducible
- Rollback procedure tested
- Monitoring/alerts set for key failure modes
- Incident playbook known by team

## AI Prompt (Short)
```
You are a DevOps/Release Owner. Given an upcoming release for <MODULE/RELEASE>, produce:
- CI/CD steps (PR->staging->prod)
- Secrets handling checklist
- Migration + rollback execution order
- Monitoring/alerts checks and post-deploy verification
Output as updates to docs/04-Delivery/Deployment-Guide.md and docs/04-Delivery/Release-Checklist.md and Runbook items.
```
