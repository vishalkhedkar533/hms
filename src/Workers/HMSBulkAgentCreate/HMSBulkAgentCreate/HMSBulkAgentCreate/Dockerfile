# Multi-stage build for the HMSBulkAgentCreate .NET 8 Worker Service
# Place this Dockerfile next to HMSBulkAgentCreate.csproj in the project folder.
# NOTE: Because this project uses ProjectReference paths outside the project folder,
# build from the repository root and pass the root as the build context:
#   docker build -f ./HMSBulkAgentCreate/Dockerfile -t myregistry/hmsbulkagentcreate:latest .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set this to the folder that contains HMSBulkAgentCreate.csproj (relative to repo root)
ARG PROJECT_DIR=HMSBulkAgentCreate
ENV PROJECT_PATH=/src/${PROJECT_DIR}

# Copy entire repo into the build image so ProjectReference paths outside the project folder resolve
COPY . .

# Restore and publish the project
RUN dotnet restore "${PROJECT_PATH}/HMSBulkAgentCreate.csproj"
RUN dotnet publish "${PROJECT_PATH}/HMSBulkAgentCreate.csproj" \
    -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.source="https://github.com/projectshashi/hms"

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy published output from build stage
COPY --from=build /app/publish .

# Create a non-root user and drop privileges
RUN adduser --system --group app && chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "HMSBulkAgentCreate.dll"]