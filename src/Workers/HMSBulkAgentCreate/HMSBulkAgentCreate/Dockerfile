# --------------------------------------------------------------------------------
# STAGE 1: Build and Publish the Worker Service
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
# Set the working directory to /src, which represents C:\Navin\HMS\Code\
WORKDIR /src

# Copy the entire contents of C:\Navin\HMS\Code\ into the container at /src
COPY . .

# 1. Restore: Execute at the solution root (/src). Explicitly restore the Worker's solution file.
# This correctly resolves all dependencies, including those in the 'src' subfolders.
RUN dotnet restore "HMSBulkAgentCreate.sln"

# 2. Publish: Execute from the root (/src) but specify the FULL relative path to the Worker project.
# This bypasses all implicit path issues.
RUN dotnet publish "src/Workers/HMSBulkAgentCreate/HMSBulkAgentCreate/HMSBulkAgentCreate.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false


# --------------------------------------------------------------------------------
# STAGE 2: Final, Production Image (Minimal Runtime Image)
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy only the compiled binaries and necessary files from the publish stage
COPY --from=publish /app/publish .

# Set the startup command for the background service
ENTRYPOINT ["dotnet", "HMSBulkAgentCreate.dll"]