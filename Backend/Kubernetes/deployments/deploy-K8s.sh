#!/bin/bash

# Define the list of deployment files
deploymentFiles=(
    "namespace.yaml"
    "mongodb-deployment.yaml"
    "rabbitmq-deployment.yaml"
    "seq-deployment.yaml"
    "customer-data-deployment.yaml"
    "products-data-deployment.yaml"
    "saleorder-data-deployment.yaml"
    "saleorderinvoice-deployment.yaml"
    "invoice-data-deployment.yaml"
    "saleorder-processing-deployment.yaml"
    "hpa.yaml"
)

echo "Starting Kubernetes Deployments..."

# Loop over each deployment file
for file in "${deploymentFiles[@]}"; do
    if [[ -f "$file" ]]; then
        echo "Applying $file..."
        kubectl apply -f "$file"
        sleep 2  # Optional delay for stability
    else
        echo "Warning: $file not found, skipping."
    fi
done

echo "All deployments applied successfully!"
