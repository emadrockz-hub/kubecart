# KubeCart Minikube deploy script
# Usage: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-minikube.ps1

$ErrorActionPreference = "Stop"
$NS = "demo"   # change namespace here if needed

Write-Host "== KubeCart: Minikube Deploy =="

function Assert-Cmd($name) {
  if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
    throw "Missing required command: $name"
  }
}

Assert-Cmd kubectl
Assert-Cmd minikube

Write-Host "`n[1/8] Checking minikube status..."
minikube status | Out-Host

Write-Host "`n[2/8] Checking kubectl context..."
kubectl config current-context | Out-Host

Write-Host "`n[3/8] Ensuring namespace exists ($NS)..."
kubectl get ns $NS *> $null
if ($LASTEXITCODE -ne 0) {
  kubectl create ns $NS | Out-Host
}

Write-Host "`n[4/8] Applying Config (ConfigMaps)..."
$cfgFiles = @(
  ".\k8s\config\catalog-configmap.yaml",
  ".\k8s\config\identity-configmap.yaml",
  ".\k8s\config\orders-configmap.yaml",
  ".\k8s\config\ui-configmap.yaml"
)

foreach ($f in $cfgFiles) {
  if (Test-Path $f) { kubectl apply -n $NS -f $f | Out-Host }
  else { Write-Host "WARN: missing $f" }
}

Write-Host "`n[5/8] Applying Secrets (LOCAL ONLY, must exist in ignored files)..."
if (Test-Path ".\k8s\secrets") {
  kubectl apply -n $NS -f .\k8s\secrets\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\secrets not found"
}

Write-Host "`n[6/8] Applying Deployments..."
if (Test-Path ".\k8s\deployments") {
  kubectl apply -n $NS -f .\k8s\deployments\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\deployments not found"
}

Write-Host "`n[7/8] Applying Services..."
if (Test-Path ".\k8s\services") {
  kubectl apply -n $NS -f .\k8s\services\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\services not found"
}

Write-Host "`n[8/8] Applying Ingress (non-overlapping only)..."
if (Test-Path ".\k8s\ingress") {
  # Apply only the consolidated + health ingresses to avoid duplicate host/path webhook errors
  $ingFiles = @(
    ".\k8s\ingress\kubecart-ingress.yaml",
    ".\k8s\ingress\kubecart-ingress-health.yaml"
  )

  foreach ($f in $ingFiles) {
    if (Test-Path $f) { kubectl apply -n $NS -f $f | Out-Host }
    else { Write-Host "WARN: missing $f" }
  }
} else {
  Write-Host "WARN: .\k8s\ingress not found"
}

Write-Host "`n== Current pods =="
kubectl get pods -n $NS | Out-Host

Write-Host "`nTip: If pods are not Running, use:"
Write-Host "  kubectl describe pod -n $NS <pod-name>"
Write-Host "  kubectl logs -n $NS <pod-name> --all-containers"

Write-Host "`n== Done =="
