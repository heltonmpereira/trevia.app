FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

ARG API_BASE_URL=http://localhost:5000
ENV ApiBaseUrl=${API_BASE_URL}

COPY ["src/TreviaApp.Shared/TreviaApp.Shared.csproj", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Contracts/TreviaApp.Contracts.csproj", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Client/TreviaApp.Client.csproj", "TreviaApp.Client/"]

RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org || true
RUN dotnet restore "TreviaApp.Client/TreviaApp.Client.csproj"

COPY ["src/TreviaApp.Shared/", "TreviaApp.Shared/"]
COPY ["src/TreviaApp.Contracts/", "TreviaApp.Contracts/"]
COPY ["src/TreviaApp.Client/", "TreviaApp.Client/"]

WORKDIR /src/TreviaApp.Client
RUN dotnet publish "TreviaApp.Client.csproj" -c Release -o /app/publish \
    /p:Optimize=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

FROM nginx:1.27-alpine AS final

RUN apk add --no-cache tzdata curl openssl envsubst bash

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/TreviaApp.Client/nginx.conf /etc/nginx/templates/default.conf.template

ENV NGINX_PORT=8080
ENV API_BASE_URL=http://localhost:8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

EXPOSE 8080

CMD ["/bin/sh", "-c", "envsubst '${API_BASE_URL} ${NGINX_PORT}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
