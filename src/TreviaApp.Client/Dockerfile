FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /src

COPY ["src/TreviaApp.Shared/TreviaApp.Shared.csproj", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Contracts/TreviaApp.Contracts.csproj", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Client/TreviaApp.Client.csproj", "TreviaApp.Client/"]
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
RUN dotnet restore "TreviaApp.Client/TreviaApp.Client.csproj"

COPY ["src/TreviaApp.Shared/", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Contracts/", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Client/", "TreviaApp.Client/"]
WORKDIR /src/TreviaApp.Client
ARG API_BASE_URL=http://localhost:5000
ENV ApiBaseUrl=${API_BASE_URL}
RUN dotnet publish "TreviaApp.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build-env /app/publish/wwwroot .
COPY src/TreviaApp.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
