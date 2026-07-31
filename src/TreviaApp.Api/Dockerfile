FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /src

COPY ["src/TreviaApp.Shared/TreviaApp.Shared.csproj", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Domain/TreviaApp.Domain.csproj", "TreviaApp.Domain/"]
COPY ["src/TreviaApp.Contracts/TreviaApp.Contracts.csproj", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Application/TreviaApp.Application.csproj", "TreviaApp.Application/"]
COPY ["src/TreviaApp.Infrastructure/TreviaApp.Infrastructure.csproj", "TreviaApp.Infrastructure/"]
COPY ["src/TreviaApp.Api/TreviaApp.Api.csproj", "TreviaApp.Api/"]
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
RUN dotnet restore "TreviaApp.Api/TreviaApp.Api.csproj"

COPY ["src/TreviaApp.Shared/", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Domain/", "TreviaApp.Domain/"]
COPY ["src/TreviaApp.Contracts/", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Application/", "TreviaApp.Application/"]
COPY ["src/TreviaApp.Infrastructure/", "TreviaApp.Infrastructure/"]
COPY ["src/TreviaApp.Api/", "TreviaApp.Api/"]
WORKDIR /src/TreviaApp.Api
RUN dotnet publish "TreviaApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build-env /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "TreviaApp.Api.dll"]
