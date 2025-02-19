$deploymentFiles = @(
    "namespace.yaml",
    "mongodb-deployment.yaml",
    "rabbitmq-deployment.yaml",
    "seq-deployment.yaml",
    "customer-data-deployment.yaml",
    "products-data-deployment.yaml",
    "saleorder-data-deployment.yaml",
    "saleorderinvoice-deployment.yaml",
    "invoice-data-deployment.yaml",
    "saleorder-processing-deployment.yaml",
    "hpa.yaml "
)

Write-Host "Starting Kubernetes Deployments..."

foreach ($file in $deploymentFiles) {
    if (Test-Path $file) {
        Write-Host "Applying $file..."
        kubectl apply -f $file
        Start-Sleep -Seconds 2  # Optional delay for stability
    } else {
        Write-Host "Warning: $file not found, skipping."
    }
}

Write-Host "All deployments applied successfully!"
