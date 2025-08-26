# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MiniLMS.Api/MiniLMS.Api.csproj", "src/MiniLMS.Api/"]
COPY ["src/MiniLMS.Core/MiniLMS.Core.csproj", "src/MiniLMS.Core/"]
COPY ["src/MiniLMS.Infrastructure/MiniLMS.Infrastructure.csproj", "src/MiniLMS.Infrastructure/"]
RUN dotnet restore "src/MiniLMS.Api/MiniLMS.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/MiniLMS.Api"
RUN dotnet build "MiniLMS.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MiniLMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create imports directory
RUN mkdir -p /app/imports

EXPOSE 80

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniLMS.Api.dll"]
