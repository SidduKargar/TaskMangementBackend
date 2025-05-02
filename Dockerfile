# Use the .NET SDK image for build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and API project files
COPY *.sln . 
COPY TaskManagementAPI/*.csproj ./TaskManagementAPI/

# Restore dependencies (only the API project)
RUN dotnet restore TaskManagementAPI/TaskManagementAPI.csproj

# Copy the rest of the source code (API project only)
COPY TaskManagementAPI/. ./TaskManagementAPI/

# Build and publish the API project
WORKDIR /app/TaskManagementAPI
RUN dotnet publish -c Release -o /app/publish

# Use the .NET runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose port 5053 for Docker container
EXPOSE 5053

# Define the entry point for the application
ENTRYPOINT ["dotnet", "TaskManagementAPI.dll"]
