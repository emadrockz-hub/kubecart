# KubeCart Debug Drills (Evidence)

Last updated: 2026-02-01

## Drill 1 — DB_HOST wrongly set to localhost,1433 (inside K8s)
**Goal:** readinessProbe fails when DB_HOST is wrong, then becomes healthy after fixing to host access IP.

### Steps
1) Set DB_HOST to an incorrect value:
   - Example wrong value: localhost,1433
2) Restart deployments:
   - identity-api, catalog-api, orders-api
3) Observe:
   - /health/live = 200
   - /health/ready = Unhealthy (DB connection fails)
4) Fix DB_HOST back to the correct host IP (example: 172.27.16.1,1433 or host.minikube.internal when resolvable)
5) Rollout restart deployments again
6) Observe:
   - /health/ready becomes Healthy (200)

### Evidence to capture (screenshots)
- kubectl get pods -n demo showing readiness failing
- curl http://kubecart.local/api/*/health/ready showing non-200
- ConfigMap value before and after
- curl showing 200 after fix

---

## Drill 2 — Wrong DB_PASSWORD secret
**Goal:** DB connect fails when password secret is wrong, then becomes healthy after secret fix + restart.

### Steps
1) Patch the secret DB_PASSWORD to a wrong value
2) Rollout restart deployments
3) Observe readiness fails + logs show login failure
4) Fix secret back to correct password
5) Rollout restart deployments
6) Observe readiness returns to healthy (200)

### Evidence to capture (screenshots)
- Secret patched (command only; do not screenshot raw secret value)
- Pod logs showing login failure
- curl /health/ready failing then succeeding

