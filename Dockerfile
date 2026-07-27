# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for dependency restoration
COPY ["TheUnraveller.sln", "./"]
COPY ["TheUnraveller.API/TheUnraveller.API.csproj", "TheUnraveller.API/"]
COPY ["TheUnraveller.Core/TheUnraveller.Core.csproj", "TheUnraveller.Core/"]
COPY ["TheUnraveller.Infrastructure/TheUnraveller.Infrastructure.csproj", "TheUnraveller.Infrastructure/"]
COPY ["TheUnraveller.Service/TheUnraveller.Service.csproj", "TheUnraveller.Service/"]
COPY ["TheUnraveller.Tests/TheUnraveller.Tests.csproj", "TheUnraveller.Tests/"]

# Restore dependencies
RUN dotnet restore "TheUnraveller.sln"

# Copy the rest of the source code
COPY . .

# Build and publish the API project in Release mode
WORKDIR "/src/TheUnraveller.API"
RUN dotnet publish "TheUnraveller.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Configure ASP.NET Core to bind to port 8080 (standard for .NET 8/9 containers)
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "TheUnraveller.API.dll"]
