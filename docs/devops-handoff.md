# DevOps Handoff — KubeCart Shell

Namespace: demo  
Ingress host: kubecart.local

## Services
- identity-api -> /api/auth
- catalog-api  -> /api/catalog
- orders-api   -> /api/orders
- kubecart-ui  -> /

## Health endpoints (required)
- /health/live  (always 200 if process alive)
- /health/ready (200 only if DB reachable)

## External SQL Server (host machine)
Databases:
- KubeCart_Identity
- KubeCart_Catalog
- KubeCart_Orders

## ConfigMap/Secret env contract (must match)
Identity ConfigMap: DB_HOST, DB_NAME=KubeCart_Identity, DB_USER  
Identity Secrets: DB_PASSWORD, JWT_SIGNING_KEY, APP_ENCRYPTION_KEY  

Catalog ConfigMap: DB_HOST, DB_NAME=KubeCart_Catalog, DB_USER  
Catalog Secrets: DB_PASSWORD  

Orders ConfigMap: DB_HOST, DB_NAME=KubeCart_Orders, DB_USER, CATALOG_SERVICE_URL=http://catalog-service.demo.svc.cluster.local  
Orders Secrets: DB_PASSWORD  

UI ConfigMap: optional APP_BASE_URL=http://kubecart.local (UI uses relative /api/*)

## Smoke test
- curl http://kubecart.local/api/auth/health/live
- curl http://kubecart.local/api/auth/health/ready
(and same for catalog + orders)

