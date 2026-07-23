# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ============================================
# STEP 1: Restore dependencies (CACHED)
# Copy only csproj files first (changes rarely)
# ============================================
COPY ["AgriculturePlatform.API/AgriculturePlatform.API.csproj", "AgriculturePlatform.API/"]
COPY ["AgriculturePlatform.Application/AgriculturePlatform.Application.csproj", "AgriculturePlatform.Application/"]
COPY ["AgriculturePlatform.Domain/AgriculturePlatform.Domain.csproj", "AgriculturePlatform.Domain/"]
COPY ["AgriculturePlatform.Infrastructure/AgriculturePlatform.Infrastructure.csproj", "AgriculturePlatform.Infrastructure/"]

# Restore packages (this layer will be cached)
RUN dotnet restore "AgriculturePlatform.API/AgriculturePlatform.API.csproj"

# ============================================
# STEP 2: Build the application
# Copy the rest of the source code
# ============================================
COPY . .

# Build and publish (REMOVE --no-restore)
RUN dotnet publish "AgriculturePlatform.API/AgriculturePlatform.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install utilities
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Expose the port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "AgriculturePlatform.API.dll"]