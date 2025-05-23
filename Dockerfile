# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["src/CampusNavigation/CampusNavigation.csproj", "CampusNavigation/"]
RUN dotnet restore "CampusNavigation/CampusNavigation.csproj"

COPY ["src/CampusNavigation/", "CampusNavigation/"]
WORKDIR "/src/CampusNavigation"
RUN dotnet build "CampusNavigation.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CampusNavigation.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENV PORT=80
ENV ASPNETCORE_URLS=http://+:${PORT}

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:${PORT}/api/diagnostics/dbstatus || exit 1

ENTRYPOINT ["dotnet", "CampusNavigation.dll"]