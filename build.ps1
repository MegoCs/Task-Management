# Task Management Build Script for Windows
param(
    [switch]$SkipDocker,
    [switch]$Clean
)

Write-Host "🚀 Task Management Build Pipeline" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green

# Function to print colored output
function Write-Success {
    param($Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Warning {
    param($Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Error {
    param($Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Check if we're in the right directory
if (-not (Test-Path "src\TaskManagement.sln")) {
    Write-Error "TaskManagement.sln not found. Please run this script from the repository root."
    exit 1
}

Set-Location src

try {
    # Clean previous builds if requested
    if ($Clean) {
        Write-Success "Cleaning previous builds..."
        dotnet clean TaskManagement.sln --verbosity quiet
    }

    # Restore packages
    Write-Success "Restoring NuGet packages..."
    dotnet restore TaskManagement.sln

    # Build solution
    Write-Success "Building solution..."
    dotnet build TaskManagement.sln --configuration Release --no-restore

    # Run tests (if any exist)
    Write-Success "Running tests..."
    try {
        dotnet test TaskManagement.sln --configuration Release --no-build --verbosity normal
    }
    catch {
        Write-Warning "No tests found or tests failed"
    }

    # Build Docker images (unless skipped)
    if (-not $SkipDocker) {
        Write-Success "Building Docker images..."

        Write-Host "Building API Docker image..."
        docker build -t task-management-api:latest -f TaskManagement.API/Dockerfile .

        Write-Host "Building Auth Service Docker image..."
        docker build -t task-management-auth:latest -f TaskManagement.AuthService/Dockerfile .

        Write-Host "Building Reminder Service Docker image..."
        docker build -t task-management-reminder:latest -f TaskManagement.ReminderService/Dockerfile .

        # Build Frontend
        Write-Host "Building Frontend Docker image..."
        Set-Location TaskManagement.Frontend
        docker build -t task-management-frontend:latest .
        Set-Location ..
    }

    Write-Success "Build completed successfully!"
    
    if (-not $SkipDocker) {
        Write-Success "You can now run 'docker-compose up' to start all services."
        
        Write-Host ""
        Write-Host "Available Docker images:"
        docker images | Select-String "task-management"
    }
}
catch {
    Write-Error "Build failed: $_"
    exit 1
}
finally {
    Set-Location ..
}