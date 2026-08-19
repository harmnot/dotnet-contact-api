# =========================================================
# Stage 1: build — compiles the app. Discarded from the final image.
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project file first so the restore layer caches independently of source edits
COPY src/ContactApi/ContactApi.csproj src/ContactApi/
RUN dotnet restore src/ContactApi/ContactApi.csproj

# Now the source
COPY . .
RUN dotnet publish src/ContactApi/ContactApi.csproj \
    -c Release -o /app/publish --no-restore


# =========================================================
# Stage 2: migrator-build — produces a standalone migration executable
# =========================================================
FROM build AS migrator-build
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project src/ContactApi \
    --self-contained -r linux-x64 \
    --configuration Release \
    --force \
    -o /app/efbundle


# =========================================================
# Stage 3: migrator — runs once, applies migrations, exits
# =========================================================
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrator
WORKDIR /app
COPY --from=migrator-build /app/efbundle .
RUN chmod +x ./efbundle && mkdir -p /data && chown app:app /data
USER app
ENTRYPOINT ["./efbundle"]


# =========================================================
# Stage 4: runtime — the image you actually ship
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN mkdir -p /data && chown app:app /data
USER app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default="Data Source=/data/contacts.db"

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -fsS http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "ContactApi.dll"]
