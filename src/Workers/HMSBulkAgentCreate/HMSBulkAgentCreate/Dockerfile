# --------------------------------------------------------------------------------
# STAGE 1: Build and Publish the Worker Service
# This stage uses the dotnet SDK image for building the application.
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
# Set the working directory to /src, which represents C:\Navin\HMS\Code\
WORKDIR /src

# Copy the entire contents of the C:\Navin\HMS\Code\ directory into the container at /src.
# This makes all projects and solution files available for the build process.
COPY . .

# 1. Universal Restore: Executes at the solution root (/src).
# This is the most reliable way to restore dependencies for a complex multi-project solution.
RUN dotnet restore

# 2. Publish: Execute from the root (/src) but specify the FULL relative path to the Worker project.
# This path is now correct and unambiguous relative to the /src working directory.
RUN dotnet publish "src/Workers/HMSBulkAgentCreate/HMSBulkAgentCreate/HMSBulkAgentCreate.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false


# --------------------------------------------------------------------------------
# STAGE 2: Final, Production Image (Minimal Runtime Image)
# This stage creates the small, secure image for deployment.
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy only the published, ready-to-run binaries from the 'publish' stage.
COPY --from=publish /app/publish .

# Set the startup command for the background service
ENTRYPOINT ["dotnet", "HMSBulkAgentCreate.dll"]