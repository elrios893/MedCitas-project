# =============================================================
# STAGE 1: Build
# SDK completo de .NET 9 para compilar y publicar
# =============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solo los archivos .csproj primero para aprovechar el cache
# de capas de Docker (si el código cambia pero las deps no, no se
# vuelve a ejecutar dotnet restore)
COPY MedCitas.Core/MedCitas.Core.csproj             MedCitas.Core/
COPY MedCitas.Infrastructure/MedCitas.Infrastructure.csproj  MedCitas.Infrastructure/
COPY MedCitas.Web/MedCitas.Web.csproj               MedCitas.Web/

# Restaurar dependencias de NuGet
RUN dotnet restore MedCitas.Web/MedCitas.Web.csproj

# Copiar el resto del código fuente
COPY MedCitas.Core/        MedCitas.Core/
COPY MedCitas.Infrastructure/ MedCitas.Infrastructure/
COPY MedCitas.Web/         MedCitas.Web/

# Publicar en modo Release (sin restore porque ya se hizo)
RUN dotnet publish MedCitas.Web/MedCitas.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# =============================================================
# STAGE 2: Runtime
# Solo el runtime de ASP.NET 9 — imagen más liviana (~250 MB)
# =============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Crear usuario no-root por seguridad
RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup --no-create-home appuser

# Copiar los artefactos publicados desde el stage de build
COPY --from=build /app/publish .

# Puerto en el que escucha Kestrel dentro del contenedor
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Cambiar al usuario no-root
USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "MedCitas.Web.dll"]
