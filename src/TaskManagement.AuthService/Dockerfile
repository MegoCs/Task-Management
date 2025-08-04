FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["TaskManagement.AuthService/TaskManagement.AuthService.csproj", "TaskManagement.AuthService/"]
COPY ["TaskManagement.Domain/TaskManagement.Domain.csproj", "TaskManagement.Domain/"]
COPY ["TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "TaskManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TaskManagement.AuthService/TaskManagement.AuthService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TaskManagement.AuthService"
RUN dotnet build "TaskManagement.AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.AuthService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.AuthService.dll"]
