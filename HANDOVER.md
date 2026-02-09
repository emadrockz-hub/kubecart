# KubeCart – DevOps Handover / Runbook

## 1) What this repo is
Monorepo: KubeCart (production-style Minikube app)
- UI (frontend)
- 3 backend APIs (Identity, Catalog, Orders)
- External SQL Server DB (outside k8s)

## 2) Repo structure (important paths)
- kubecart/
  - services/
    - identity/src/Identity.Api
    - catalog/src/Catalog.Api
    - orders/src/Orders.Api
  - ui/  (frontend)
  - k8s/
    - configmaps/
    - secrets/   (REAL secrets must NOT be committed; this folder is gitignored for *-secrets.yaml)
    - deployments/
    - services/
    - ingress/

## 3) Running locally (developer machine)
### Prereqs
- .NET 8 SDK
- Node (for UI)
- SQL Server running externally (local machine SQL Server or separate host)
- Optional: Minikube + kubectl for k8s

### Local run order
1) SQL Server external DB must be reachable
2) Run Identity API
3) Run Catalog API
4) Run Orders API
5) Run UI

## 4) Environment variables / secrets policy
### Policy
- NEVER commit real secrets into git.
- Use:
  - dotnet user-secrets (local dev), OR
  - k8s/secrets/*-secrets.yaml (ignored by git), OR
  - CI/CD secret store (GitHub Actions secrets / Vault / etc.)

### Local env workflow (recommended)
- Repo includes `.env.example` (placeholders only).
- For local dev, copy `.env.example` → `.env.local` and fill real values locally.
- `.env.local` is gitignored and must never be committed.


### Required secret values (examples only)
- DB:
  - DB_HOST=REPLACE_ME
  - DB_NAME=REPLACE_ME
  - DB_USER=REPLACE_ME
  - DB_PASSWORD=REPLACE_ME
- JWT / Signing:
  - JWT_SIGNING_KEY=REPLACE_ME
  - JWT_ISSUER=REPLACE_ME
  - JWT_AUDIENCE=REPLACE_ME

## 5) Kubernetes / Minikube deployment flow (high level)
1) Start minikube
2) Apply ConfigMaps
3) Apply Secrets (from local ignored files)
4) Apply Deployments + Services
5) Apply Ingress
6) Verify endpoints (health checks / swagger)

## 6) Verification checklist
### APIs
- Identity swagger loads
- Catalog swagger loads
- Orders swagger loads
- Each can connect to DB (no connection exceptions)

### UI
- UI loads
- UI can call Catalog/Identity/Orders endpoints (no CORS/network errors)

## 7) Troubleshooting quick hits
- 404 via ingress: check ingress rules + service ports + path mappings
- DB connection errors: validate DB_HOST reachable from runtime (minikube vs local)
- CrashLoopBackOff: check env vars injected (ConfigMap/Secret names match)
- Unauthorized JWT: confirm JWT_ISSUER/AUDIENCE/SIGNING_KEY are aligned across services

## 8) How to provide secrets to new devops member (safe)
- Send secrets out-of-band (NOT in git): password manager / secure chat / vault
- Provide only placeholders in repo
- Confirm `k8s/secrets/*-secrets.yaml` is excluded by .gitignore
