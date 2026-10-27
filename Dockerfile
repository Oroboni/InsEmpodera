# Imagem da aplicação EcoRedes (InsEmpodera). Uso: docker compose up -d (ver docs/docker.md)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY InsEmpodera.csproj ./
RUN dotnet restore InsEmpodera.csproj
COPY . .
RUN dotnet publish InsEmpodera.csproj -c Release -o /publicado --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publicado .
# Pastas com dados que precisam sobreviver às atualizações (montadas como volumes no compose.yaml):
# fotos e cores dos usuários e chaves de proteção de dados (mantêm os logins).
RUN mkdir -p /app/App_Data /var/lib/empodera/keys \
    && chown -R app:app /app/App_Data /var/lib/empodera
USER app
ENV ASPNETCORE_URLS=http://+:8080 \
    DataProtection__KeysPath=/var/lib/empodera/keys
EXPOSE 8080
ENTRYPOINT ["dotnet", "InsEmpodera.dll"]
