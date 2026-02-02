# KubeCart Capstone — STATUS

Last updated: 2026-02-01

## Repo + tooling
- [x] Monorepo root created: `kubecart/`
- [x] `global.json` pins SDK to .NET 8 (8.0.417)
- [x] Solution: `KubeCart.sln`
- [x] Preferred workflow: VS (.NET), VS Code (UI), SSMS (DB)
- [x] `.dockerignore` added to avoid `.vs` file locks during image builds

## Services — Shell (API)
### Identity
- [x] Project path: `services/identity/src/Identity.Api`
- [x] Health endpoints:
  - [x] `GET /health/live` (always 200)
  - [x] `GET /health/ready` (DB check)
- [x] Custom DB readiness check class: `Health/SqlConnectionHealthCheck.cs`
- [x] Program bootstraps `ConnectionStrings:Default` from env vars if missing:
  - `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- [x] Dockerfile present: `services/identity/src/Identity.Api/Dockerfile`
- [ ] .env.example (local runs)
- [ ] DapperContext + Repository skeleton
- [ ] Auth endpoints + JWT + admin seed
- [ ] AuditLogs table + logging

### Catalog
- [x] Project path: `services/catalog/src/Catalog.Api`
- [x] Health endpoints:
  - [x] `GET /health/live`
  - [x] `GET /health/ready`
- [x] Custom DB readiness check class: `Health/SqlConnectionHealthCheck.cs`
- [x] Program bootstraps `ConnectionStrings:Default` from env vars if missing:
  - `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- [x] Dockerfile present: `services/catalog/src/Catalog.Api/Dockerfile`
- [ ] .env.example (local runs)
- [ ] DapperContext + Repository skeleton
- [ ] Categories + Products endpoints
- [ ] AuditLogs table + logging

### Orders
- [x] Project path: `services/orders/src/Orders.Api`
- [x] Health endpoints:
  - [x] `GET /health/live`
  - [x] `GET /health/ready`
- [x] Custom DB readiness check class: `Health/SqlConnectionHealthCheck.cs`
- [x] Program bootstraps `ConnectionStrings:Default` from env vars if missing:
  - `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- [x] Dockerfile present: `services/orders/src/Orders.Api/Dockerfile`
- [ ] .env.example (local runs)
- [ ] DapperContext + Repository skeleton
- [ ] CatalogClient (HTTP) to validate products before checkout
- [ ] Checkout (transactional) + snapshot storage
- [ ] AuditLogs table + logging

## Kubernetes (k8s/)
- [x] Folder structure created:
  - [x] `k8s/config/`
  - [x] `k8s/secrets/`
  - [x] `k8s/deployments/`
  - [x] `k8s/services/`
  - [x] `k8s/ingress/`
- [x] Namespace file: `k8s/00-namespace.yaml` (namespace: `demo`)

### ConfigMaps (created)
- [x] `k8s/config/identity-configmap.yaml`
- [x] `k8s/config/catalog-configmap.yaml`
- [x] `k8s/config/orders-configmap.yaml`
- [x] `k8s/config/ui-configmap.yaml`

### Secrets (templates + local real secrets)
- [x] `k8s/secrets/identity-secrets.template.yaml`
- [x] `k8s/secrets/catalog-secrets.template.yaml`
- [x] `k8s/secrets/orders-secrets.template.yaml`
- [x] `.gitignore` ignores real secrets: `k8s/secrets/*-secrets.yaml`
- [x] Created local real secrets (NOT committed):
  - [x] `k8s/secrets/identity-secrets.yaml` (DB_PASSWORD set to `Cap_project!1`)
  - [x] `k8s/secrets/catalog-secrets.yaml` (DB_PASSWORD set to `Cap_project!1`)
  - [x] `k8s/secrets/orders-secrets.yaml` (DB_PASSWORD set to `Cap_project!1`)

## DevOps handoff skeleton (YAML)
### k8s Services YAML (ClusterIP)
- [x] `identity-service`
- [x] `catalog-service`
- [x] `orders-service`
- [x] `ui-service`
- [x] UI service fixed to targetPort 80 (nginx)

### k8s Deployments YAML
- [x] `identity-api`
- [x] `catalog-api`
- [x] `orders-api`
- [x] `kubecart-ui`
- [x] Probes configured:
  - liveness: `/health/live`
  - readiness: `/health/ready`
- [x] Env vars wired from configmaps + secrets per contract

### k8s Ingress YAML
- [x] host: `kubecart.local`
- [x] Ingress split into 2 resources:
  - [x] UI ingress: `/` -> `ui-service:80` (`k8s/ingress/ui-ingress.yaml`)
  - [x] API ingress: `/api/*` -> services with regex+rewrite (`k8s/ingress/api-ingress.yaml`)

## Cluster setup + apply status
- [x] Minikube running
- [x] Ingress addon enabled
- [x] Windows hosts file entry added: `172.27.19.59 kubecart.local`
- [x] Applied to cluster:
  - [x] namespace
  - [x] configmaps
  - [x] secrets
  - [x] services
  - [x] deployments
  - [x] ingress
- [x] UI verified working via port-forward (`localhost:8085`)
- [ ] UI verified working via ingress (`http://kubecart.local/`) after final checks

## Images / builds
- [x] Built images into Minikube using `minikube image build`:
  - [x] `identity-api:local`
  - [x] `catalog-api:local`
  - [x] `orders-api:local`
  - [x] `kubecart-ui:local` (placeholder nginx page)

## Current blockers / known issues
- [ ] API ingress returns 503 because API pods are not Ready (readiness depends on DB reachability)
- [x] Confirmed `host.minikube.internal` does NOT resolve inside cluster (`nc: bad address`)
- [x] Temporary nettest pod created for network testing (`nettest` in namespace `demo`)
- [ ] Need to patch DB_HOST in configmaps to a reachable host IP (gateway), then rollout restart deployments
- [ ] Fix PowerShell quoting for `kubectl patch` (previous attempts failed)

## Debug drills (must document with screenshots/notes)
- [ ] Drill 1: DB_HOST wrong -> readiness fails -> fix -> rollout restart
- [ ] Drill 2: DB_PASSWORD wrong -> DB connect fails -> fix -> rollout restart

## Notes
- External SQL Server DBs (host machine, outside K8s):
  - `KubeCart_Identity`
  - `KubeCart_Catalog`
  - `KubeCart_Orders`
