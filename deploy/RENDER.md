# Deploy no Render.com

## Recursos

1. PostgreSQL gerenciado;
2. Web Service para a API;
3. Static Site ou Web Service para o Client;
4. armazenamento externo para mídias futuramente.

## Variáveis do Server

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=
Jwt__Issuer=
Jwt__Audience=
Jwt__Key=
Jwt__AccessTokenMinutes=15
Jwt__RefreshTokenDays=30
Cors__AllowedOrigins__0=
AdminSeed__Email=
AdminSeed__Password=
FileStorage__Provider=
```

A API deve escutar a porta fornecida pelo Render. Migrations devem ser executadas por comando ou job controlado.
