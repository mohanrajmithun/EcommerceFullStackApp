# Navigate to the backend folder
cd backend

# Step 1: Start Docker containers for MongoDB, RabbitMQ, and Seq_Logging
$dockerComposeFolders = @("MongoDB", "RabbitMQ", "Seq_Logging")

Write-Host "Starting Docker containers..."
foreach ($folder in $dockerComposeFolders) {
    Write-Host "Starting containers in $folder..."
    Push-Location $folder
    docker-compose up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to start containers in $folder. Stopping the script." -ForegroundColor Red
        exit 1
    }
    Pop-Location
}
Write-Host "All Docker containers started successfully!" -ForegroundColor Green

# Step 2: Build the SalesAPILibrary
Write-Host "Building SalesAPILibrary..."
dotnet build "SalesAPILibrary/SalesAPILibrary.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build SalesAPILibrary. Stopping the script." -ForegroundColor Red
    exit 1
}
Write-Host "SalesAPILibrary built successfully!" -ForegroundColor Green

# Step 3: Define an array of project paths and their titles
$projects = @(
    @{ Path = "CustomerDataAPI/CustomerDataAPI/CustomerDataAPI.csproj"; Title = "Microservice: CustomerDataAPI" },
    @{ Path = "ProductsDataApiService/ProductsDataApiService/ProductsDataApiService.csproj"; Title = "Microservice: ProductsDataApiService" },
    @{ Path = "SaleOrderDataService/SaleOrderDataService/SaleOrderDataService.csproj"; Title = "Microservice: SaleOrderDataService" },
    @{ Path = "SaleOrderProcessingAPI/SaleOrderProcessingAPI/SaleOrderProcessingAPI.csproj"; Title = "Microservice: SaleOrderProcessingAPI" },
    @{ Path = "SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI/SalesInvoiceGeneratorServiceAPI.csproj"; Title = "Microservice: SalesInvoiceGeneratorServiceAPI" },
    @{ Path = "InvoiceDataService/InvoiceDataService/InvoiceDataService.csproj"; Title = "Microservice: InvoiceDataService" }
)

# Step 4: Launch each project in a new PowerShell window with a specific title
foreach ($project in $projects) {
    $path = $project.Path
    $title = $project.Title
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle = '$title'; cd $(Split-Path $path); dotnet run"
}

Write-Host "All microservices are starting with specific window titles..."
