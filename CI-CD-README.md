# Task Management CI/CD Pipeline

This repository includes a comprehensive CI/CD pipeline that automatically builds, tests, and validates the Task Management application on every pull request and push to the main branch.

## 🚀 Pipeline Overview

The CI/CD pipeline consists of several jobs that run in parallel and sequentially:

### 1. Build and Test (.NET Solution)
- **Triggers**: All pushes and pull requests
- **Actions**:
  - Restores NuGet packages with caching
  - Builds the entire .NET solution in Release configuration
  - Runs unit tests (if available)
  - Uploads test results as artifacts

### 2. Docker Build Validation
- **Triggers**: After .NET build completes
- **Actions**:
  - Builds Docker images for all services:
    - `task-management-api` (Main API)
    - `task-management-auth` (Authentication Service)
    - `task-management-reminder` (Reminder Service)
    - `task-management-frontend` (React Frontend)
  - Uses Docker BuildKit with layer caching for faster builds
  - Validates that all Dockerfiles build successfully

### 3. Docker Compose Integration Test
- **Triggers**: Only on pull requests, after Docker builds complete
- **Actions**:
  - Starts all dependencies (MongoDB, RabbitMQ, Redis)
  - Waits for services to be healthy
  - Starts application services (Auth and API)
  - Performs health checks on running services
  - Validates service integration

### 4. Security Scan
- **Triggers**: Only on pull requests
- **Actions**:
  - Runs Trivy vulnerability scanner on the codebase
  - Scans for critical and high-severity vulnerabilities
  - Uploads results to GitHub Security tab

### 5. Code Quality Check
- **Triggers**: All pushes and pull requests
- **Actions**:
  - Verifies code formatting with `dotnet format`
  - Runs static code analysis
  - Ensures coding standards compliance

## 🛠️ Local Development

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose
- Git

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd Task-Management
   ```

2. **Build locally** (Linux/macOS):
   ```bash
   ./build.sh
   ```

   **Build locally** (Windows):
   ```powershell
   .\build.ps1
   ```

3. **Start all services**:
   ```bash
   cd src
   docker-compose up -d
   ```

4. **Access the applications**:
   - Frontend: http://localhost:3000
   - API: http://localhost:5000
   - Auth Service: http://localhost:5001
   - RabbitMQ Management: http://localhost:15672

### Build Script Options

The build scripts support several options:

**Linux/macOS (`build.sh`)**:
- No parameters: Full build including Docker images
- Set environment variables for customization

**Windows (`build.ps1`)**:
- `-SkipDocker`: Skip Docker image builds
- `-Clean`: Clean previous builds before building

## 🔍 Health Checks

The pipeline includes health check endpoints for monitoring:

- **API Service**: `GET /api/health`
- **Auth Service**: `GET /api/auth/health`

Both endpoints return:
```json
{
  "Status": "Healthy",
  "Service": "ServiceName",
  "Timestamp": "2024-01-01T00:00:00.000Z"
}
```

## 🏗️ Architecture

The solution is structured as a microservices architecture:

```
src/
├── TaskManagement.sln              # Main solution file
├── TaskManagement.API/             # Main API service
├── TaskManagement.AuthService/     # Authentication service
├── TaskManagement.ReminderService/ # Background reminder service
├── TaskManagement.Application/     # Application layer
├── TaskManagement.Domain/          # Domain entities
├── TaskManagement.Infrastructure/  # Data access layer
├── TaskManagement.Frontend/        # React frontend
└── docker-compose.yml             # Container orchestration
```

## 🚨 Pipeline Status

The pipeline will fail if any of the following conditions are met:

- .NET solution fails to build
- Docker images fail to build
- Integration tests fail
- Critical or high-severity vulnerabilities are found
- Code formatting standards are not met

## 📋 Troubleshooting

### Common Issues

1. **Build fails locally but passes in CI**:
   - Ensure you have the correct .NET SDK version (8.0)
   - Run `dotnet clean` and try again

2. **Docker build fails**:
   - Check Docker daemon is running
   - Ensure sufficient disk space
   - Try `docker system prune` to clean up

3. **Integration tests timeout**:
   - Services may take longer to start in resource-constrained environments
   - Check Docker logs: `docker-compose logs <service-name>`

### Getting Help

If you encounter issues with the CI/CD pipeline:

1. Check the GitHub Actions logs for detailed error messages
2. Ensure all prerequisites are installed for local development
3. Verify that your changes don't introduce breaking changes
4. Run the build scripts locally before pushing changes

## 🔐 Security Considerations

The pipeline includes several security measures:

- Vulnerability scanning with Trivy
- Secrets are managed through GitHub environment variables
- Docker images are scanned for known vulnerabilities
- HTTPS is enforced in production builds
- Security headers are validated

For production deployments, ensure:
- Use strong JWT secrets
- Configure CORS appropriately
- Enable HTTPS/TLS
- Set up proper monitoring and logging
- Regular security updates