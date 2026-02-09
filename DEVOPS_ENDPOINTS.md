# KubeCart – DevOps Endpoints, Ports, Health Checks

## 1) Local dev ports (Visual Studio / localhost)
Identity API: http://localhost:5276
Catalog API:  http://localhost:5254
Orders API:   http://localhost:5102
UI (Vite):    http://localhost:5173

### Swagger (local)
Identity: http://localhost:5276/swagger
Catalog:  http://localhost:5254/swagger
Orders:   http://localhost:5102/swagger


## 2) Kubernetes / Minikube (Ingress)
### Ingress Host
http://kubecart.local

### Ingress routing (paths)
UI:
- GET  http://kubecart.local/

Identity:
- POST http://kubecart.local/api/auth/register
- POST http://kubecart.local/api/auth/login
- GET  http://kubecart.local/api/auth/me

Catalog (public browsing):
- GET  http://kubecart.local/api/catalog/categories
- GET  http://kubecart.local/api/catalog/products
- GET  http://kubecart.local/api/catalog/products/{id}

Orders (JWT-only):
- GET    http://kubecart.local/api/orders/carts/active/items
- POST   http://kubecart.local/api/orders/carts/items
- PUT    http://kubecart.local/api/orders/carts/items/{id}
- DELETE http://kubecart.local/api/orders/carts/items/{id}
- POST   http://kubecart.local/api/orders/checkout
- GET    http://kubecart.local/api/orders/orders
- GET    http://kubecart.local/api/orders/orders/{id}

### Swagger (k8s, via ingress)
Identity: http://kubecart.local/api/auth/swagger
Catalog:  http://kubecart.local/api/catalog/swagger
Orders:   http://kubecart.local/api/orders/swagger

> If swagger paths differ, use:
> - http://kubecart.local/swagger (if your APIs expose swagger at root behind rewrite)
> - Or check each service directly via `kubectl port-forward`.


## 3) Health / Liveness / Readiness
### If health endpoints exist (recommended)
Try these first (common patterns):
- LIVE:  /health/live  OR /healthz/live  OR /live
- READY: /health/ready OR /healthz/ready OR /ready
- BASIC: /health

Examples:
- http://localhost:5276/health
- http://kubecart.local/health

> If these return 404, health endpoints are not implemented yet.
> In that case, DevOps should treat “Swagger loads + DB connection succeeds” as the readiness check.

### Quick runtime verification (k8s)
```powershell
kubectl get pods -n <NAMESPACE>
kubectl get svc -n <NAMESPACE>
kubectl get ingress -n <NAMESPACE>
kubectl describe pod -n <NAMESPACE> <pod-name>
kubectl logs -n <NAMESPACE> <pod-name> --all-containers
