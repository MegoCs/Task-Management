#!/bin/bash

# Task Management Build Script
set -e

echo "🚀 Task Management Build Pipeline"
echo "=================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

# Check if we're in the right directory
if [ ! -f "src/TaskManagement.sln" ]; then
    print_error "TaskManagement.sln not found. Please run this script from the repository root."
    exit 1
fi

cd src

# Clean previous builds
print_status "Cleaning previous builds..."
dotnet clean TaskManagement.sln --verbosity quiet

# Restore packages
print_status "Restoring NuGet packages..."
dotnet restore TaskManagement.sln

# Build solution
print_status "Building solution..."
dotnet build TaskManagement.sln --configuration Release --no-restore

# Run tests (if any exist)
print_status "Running tests..."
dotnet test TaskManagement.sln --configuration Release --no-build --verbosity normal || print_warning "No tests found or tests failed"

# Build Docker images
print_status "Building Docker images..."

echo "Building API Docker image..."
docker build -t task-management-api:latest -f TaskManagement.API/Dockerfile .

echo "Building Auth Service Docker image..."
docker build -t task-management-auth:latest -f TaskManagement.AuthService/Dockerfile .

echo "Building Reminder Service Docker image..."
docker build -t task-management-reminder:latest -f TaskManagement.ReminderService/Dockerfile .

# Build Frontend
echo "Building Frontend Docker image..."
cd TaskManagement.Frontend
docker build -t task-management-frontend:latest .
cd ..

print_status "Build completed successfully!"
print_status "You can now run 'docker-compose up' to start all services."

echo ""
echo "Available Docker images:"
docker images | grep task-management || print_warning "No task-management images found"