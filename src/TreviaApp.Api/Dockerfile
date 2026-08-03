FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["src/TreviaApp.Shared/TreviaApp.Shared.csproj", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Domain/TreviaApp.Domain.csproj", "TreviaApp.Domain/"]
COPY ["src/TreviaApp.Contracts/TreviaApp.Contracts.csproj", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Application/TreviaApp.Application.csproj", "TreviaApp.Application/"]
COPY ["src/TreviaApp.Infrastructure/TreviaApp.Infrastructure.csproj", "TreviaApp.Infrastructure/"]
COPY ["src/TreviaApp.Api/TreviaApp.Api.csproj", "TreviaApp.Api/"]

RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
RUN dotnet restore "TreviaApp.Api/TreviaApp.Api.csproj" -r linux-musl-x64

COPY ["src/TreviaApp.Shared/", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Domain/", "TreviaApp.Domain/"]
COPY ["src/TreviaApp.Contracts/", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Application/", "TreviaApp.Application/"]
COPY ["src/TreviaApp.Infrastructure/", "TreviaApp.Infrastructure/"]
COPY ["src/TreviaApp.Api/", "TreviaApp.Api/"]

WORKDIR /src/TreviaApp.Api
RUN dotnet publish "TreviaApp.Api.csproj" -c Release -o /app/publish \
    -r linux-musl-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:Optimize=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata curl
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY --from=build /app/publish .

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "TreviaApp.Api.dll"]
