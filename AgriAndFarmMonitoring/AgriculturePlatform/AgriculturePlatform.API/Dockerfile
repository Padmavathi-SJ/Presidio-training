# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["AgriculturePlatform.API/AgriculturePlatform.API.csproj", "AgriculturePlatform.API/"]
COPY ["AgriculturePlatform.Application/AgriculturePlatform.Application.csproj", "AgriculturePlatform.Application/"]
COPY ["AgriculturePlatform.Domain/AgriculturePlatform.Domain.csproj", "AgriculturePlatform.Domain/"]
COPY ["AgriculturePlatform.Infrastructure/AgriculturePlatform.Infrastructure.csproj", "AgriculturePlatform.Infrastructure/"]

RUN dotnet restore "AgriculturePlatform.API/AgriculturePlatform.API.csproj"

# Copy all source code
COPY . .

# Publish the application
RUN dotnet publish "AgriculturePlatform.API/AgriculturePlatform.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

# Expose the port (8080 for .NET 8)
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "AgriculturePlatform.API.dll"]