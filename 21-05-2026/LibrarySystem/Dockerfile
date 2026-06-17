# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY LibrarySystem.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published files from build stage
COPY --from=build /app/publish .

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:8080

# Run the application - NOTE: LibrarySystem.dll (not LibraryApi.dll)
ENTRYPOINT ["dotnet", "LibrarySystem.dll"]
