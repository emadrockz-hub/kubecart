# KubeCart secrets

- `kubecart-secrets.template.yaml` is safe to commit (REPLACE_ME only).
- `kubecart-secrets.local.yaml` is local-only and must never be committed.
- Real values are set via Visual Studio Debug env vars locally, and via Kubernetes secrets when deploying.
