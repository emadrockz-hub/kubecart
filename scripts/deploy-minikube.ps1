# KubeCart Minikube deploy script
# Usage: powershell -ExecutionPolicy Bypass -File .\scripts\deploy-minikube.ps1

$ErrorActionPreference = "Stop"

$NS = "demo"   # <-- change namespace here if needed

Write-Host "== KubeCart: Minikube Deploy =="

function Assert-Cmd($name) {
  if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
    throw "Missing required command: $name"
  }
}

Assert-Cmd kubectl
Assert-Cmd minikube

Write-Host "`n[1/7] Checking minikube status..."
minikube status | Out-Host

Write-Host "`n[2/7] Ensuring namespace exists ($NS)..."
kubectl get ns $NS 1>$null 2>$null
if ($LASTEXITCODE -ne 0) {
  kubectl create ns $NS | Out-Host
}

Write-Host "`n[3/7] Applying ConfigMaps..."
if (Test-Path ".\k8s\configmaps") {
  kubectl apply -n $NS -f .\k8s\configmaps\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\configmaps not found"
}

Write-Host "`n[4/7] Applying Secrets (LOCAL ONLY, must exist in ignored files)..."
if (Test-Path ".\k8s\secrets") {
  kubectl apply -n $NS -f .\k8s\secrets\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\secrets not found"
}

Write-Host "`n[5/7] Applying Deployments..."
if (Test-Path ".\k8s\deployments") {
  kubectl apply -n $NS -f .\k8s\deployments\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\deployments not found"
}

Write-Host "`n[6/7] Applying Services..."
if (Test-Path ".\k8s\services") {
  kubectl apply -n $NS -f .\k8s\services\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\services not found"
}

Write-Host "`n[7/7] Applying Ingress..."
if (Test-Path ".\k8s\ingress") {
  kubectl apply -n $NS -f .\k8s\ingress\ | Out-Host
} else {
  Write-Host "WARN: .\k8s\ingress not found"
}

Write-Host "`n== Current pods =="
kubectl get pods -n $NS | Out-Host

Write-Host "`nTip: If pods are not Running, use:"
Write-Host "  kubectl describe pod -n $NS <pod-name>"
Write-Host "  kubectl logs -n $NS <pod-name> --all-containers"
Write-Host "`n== Done =="
