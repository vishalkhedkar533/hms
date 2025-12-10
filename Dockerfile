# --------------------------------------------------------------------------------
# STAGE 1: Build and Publish (uses the full SDK image)
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Copy the entire solution context to the /src folder.
COPY . .

# Restore dependencies at the solution root (/src) to resolve all ProjectReferences.
# Assuming your .sln file or project hierarchy is correctly set up here.
RUN dotnet restore

# --- FIX 1: Change to the Project directory ONLY for publishing ---
# Move to the directory containing HMSBulkAgentCreate.csproj
WORKDIR /src/HMSBulkAgentCreate/HMSBulkAgentCreate

# --- FIX 2: Simplify the publish command ---
# Since we are now in the correct directory, we just use the filename.
# This should resolve the MSB1009 error.
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# --------------------------------------------------------------------------------
# STAGE 2: Final, Production Image (uses the minimal Runtime image)
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HMSBulkAgentCreate.dll"]