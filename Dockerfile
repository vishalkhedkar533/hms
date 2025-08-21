# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY HMS.sln ./

# Copy project files
COPY Models/Models/Models.csproj Models/Models/
COPY APIs/HMS/HMS/HMS.csproj APIs/HMS/HMS/
COPY CommonLibrary/Communication/Communication.csproj CommonLibrary/Communication/

# Restore dependencies
RUN dotnet restore APIs/HMS/HMS/HMS.csproj

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish APIs/HMS/HMS/HMS.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Entry point
ENTRYPOINT ["dotnet", "HMS.dll"]
