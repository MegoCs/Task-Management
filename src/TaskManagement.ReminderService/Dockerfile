# Task Management Reminder Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagement.ReminderService/TaskManagement.ReminderService.csproj", "TaskManagement.ReminderService/"]
COPY ["TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "TaskManagement.Infrastructure/"]
COPY ["TaskManagement.Domain/TaskManagement.Domain.csproj", "TaskManagement.Domain/"]
RUN dotnet restore "TaskManagement.ReminderService/TaskManagement.ReminderService.csproj"
COPY TaskManagement.ReminderService/ TaskManagement.ReminderService/
COPY TaskManagement.Infrastructure/ TaskManagement.Infrastructure/
COPY TaskManagement.Domain/ TaskManagement.Domain/
WORKDIR "/src/TaskManagement.ReminderService"
RUN dotnet build "TaskManagement.ReminderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.ReminderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.ReminderService.dll"]
