# 📖 Historias de Usuario - Parte 4: Docker y Contenedorización
**Identificador**: US-04-DOCKER  
**Orden de lectura**: 4 de 8  
**Sprint**: Sprint 3 - Containerización  

---

## 🎯 Objetivo del Sprint 3
Contenedorizar la aplicación con Docker, crear docker-compose para desarrollo local con SQL Server.

---

## US-018: Crear Dockerfile Multi-Stage ✅

**Estado**: ✅ **COMPLETADA**

**Como** DevOps engineer  
**Quiero** un Dockerfile optimizado multi-stage  
**Para** crear imágenes Docker ligeras y eficientes

### Criterios de Aceptación
- ✅ Dockerfile con 2 stages (build y runtime)
- ✅ Stage build usa SDK de .NET 8
- ✅ Stage runtime usa ASP.NET Runtime (imagen ligera)
- ✅ Cache de layers optimizado
- ✅ Usuario no-root para seguridad
- ✅ Health check incluido
- ✅ Imagen final < 200MB

### Dockerfile

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (layer caching)
COPY ["DevOpsApi.csproj", "./"]
RUN dotnet restore "DevOpsApi.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "DevOpsApi.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "DevOpsApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 appuser \
    && adduser --system --uid 1001 --ingroup appuser appuser

# Copy published app from build stage
COPY --from=build /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "DevOpsApi.dll"]
```

### Tareas Técnicas
1. Crear `Dockerfile` en la raíz del proyecto
2. Ajustar rutas según estructura (si src/ está en subdirectorio)
3. Build imagen localmente:
   ```bash
   docker build -t api-devops:latest -f Dockerfile ./src
   ```
4. Verificar tamaño de imagen:
   ```bash
   docker images api-devops:latest
   ```
5. Ejecutar contenedor:
   ```bash
   docker run -d -p 5000:8080 --name api-test api-devops:latest
   ```
6. Probar health check:
   ```bash
   docker inspect --format='{{.State.Health.Status}}' api-test
   ```
7. Commit: "feat: Add optimized multi-stage Dockerfile"

### Dependencias
- ✅ US-005 (Proyecto Web API)
- ✅ US-007 (Health checks)

### Estimación
**Esfuerzo**: 3 puntos (1.5 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Dockerfile creado
- Imagen builds correctamente
- Health check funcionando
- Imagen optimizada < 200MB
- Documentado en README

---

## US-019: Crear .dockerignore

**Como** DevOps engineer  
**Quiero** un archivo .dockerignore configurado  
**Para** excluir archivos innecesarios del contexto de build

### Criterios de Aceptación
- ✅ `.dockerignore` creado
- ✅ Excluye bin/, obj/, node_modules/
- ✅ Excluye archivos de Git, IDE
- ✅ Excluye documentación y tests
- ✅ Build context reducido significativamente

### Contenido .dockerignore

```
# Build outputs
**/bin/
**/obj/
**/out/

# NuGet packages
*.nupkg
*.snupkg

# User-specific files
*.suo
*.user
*.userosscache
*.sln.docstates

# IDE
.vscode/
.vs/
.idea/
*.swp
*.swo
*~

# Git
.git/
.gitignore
.gitattributes

# Docker
Dockerfile*
docker-compose*
.dockerignore

# Documentation
README.md
docs/
*.md

# Tests
**/tests/
**/*Tests/
**/*.Tests/

# Terraform
terraform/
*.tf
*.tfvars
*.tfstate

# CI/CD
.github/
azure-pipelines.yml

# Misc
LICENSE
.editorconfig
```

### Tareas Técnicas
1. Crear `.dockerignore` en la raíz
2. Verificar reducción de tamaño del contexto:
   ```bash
   # Antes
   docker build -t test .
   # Ver línea "Sending build context to Docker daemon"
   ```
3. Commit: "chore: Add .dockerignore to optimize build context"

### Dependencias
- ✅ US-017 (Dockerfile creado)

### Estimación
**Esfuerzo**: 1 punto (15 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- `.dockerignore` creado
- Contexto de build reducido
- Tiempo de build mejorado

---

## US-020: Crear docker-compose.yml para Desarrollo Local ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador  
**Quiero** levantar todo el ambiente con un solo comando  
**Para** desarrollar y probar localmente sin configuraciones complejas

### Criterios de Aceptación
- ✅ `docker-compose.yml` con servicios API y SQL Server
- ✅ Red interna configurada
- ✅ Volúmenes persistentes para SQL Server
- ✅ Variables de entorno parametrizadas
- ✅ Health checks configurados
- ✅ Dependencias entre servicios
- ✅ Un comando para levantar todo: `docker-compose up`

### docker-compose.yml

⚠️ **IMPORTANTE**: Este archivo NO debe contener passwords. Ver [SECURITY.md](./SECURITY.md)

```yaml
version: '3.8'

services:
  # SQL Server Service
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: devops-sqlserver
    environment:
      - ACCEPT_EULA=Y
      # ✅ SEGURIDAD: Leer password desde .env (NO commitear .env)
      - SA_PASSWORD=${SQL_SA_PASSWORD}
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - devops-network
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $${SA_PASSWORD} -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 3s
      retries: 5
      start_period: 30s

  # API Service
  api:
    build:
      context: ./src
      dockerfile: ../Dockerfile
    container_name: devops-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      # ✅ SEGURIDAD: Connection string usando variable de entorno
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=DevOpsDb;User Id=sa;Password=$${SQL_SA_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true
    ports:
      - "5000:8080"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - devops-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 10s
    restart: unless-stopped

volumes:
  sqldata:
    name: devops-sqldata

networks:
  devops-network:
    name: devops-network
    driver: bridge
```

### Tareas Técnicas

⚠️ **SEGURIDAD**: Leer [SECURITY.md](./SECURITY.md) antes de continuar

1. Crear archivo `.env` con variables sensibles (NO commitear):
   ```bash
   # Crear .env en la raíz del proyecto
   cat > .env << EOF
   SQL_SA_PASSWORD=TuPasswordSeguro123!
   EOF
   
   # Agregar .env a .gitignore
   echo ".env" >> .gitignore
   ```

2. Crear `.env.example` (SÍ commitear como plantilla):
   ```bash
   cat > .env.example << EOF
   # SQL Server SA Password
   SQL_SA_PASSWORD=YOUR_STRONG_PASSWORD_HERE
   EOF
   ```

3. Crear `docker-compose.yml` en la raíz
4. Ajustar paths del build context
5. Levantar servicios:
   ```bash
   docker-compose up -d
   ```
4. Verificar servicios:
   ```bash
   docker-compose ps
   docker-compose logs -f api
   ```
5. Probar conectividad:
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - Health: http://localhost:5000/health
6. Aplicar migraciones:
   ```bash
   docker-compose exec api dotnet ef database update
   ```
   O ejecutar desde host si EF Tools está instalado
7. Detener servicios:
   ```bash
   docker-compose down
   # Con volúmenes:
   docker-compose down -v
   ```
8. Commit: "feat: Add docker-compose for local development"

### Dependencias
- ✅ US-017 (Dockerfile)
- ✅ US-013 (Migraciones EF Core)

### Estimación
**Esfuerzo**: 5 puntos (2 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- docker-compose.yml funcionando
- API y SQL Server levantando correctamente
- Migraciones aplicables
- Health checks pasando
- Documentado en README con comandos

---

## US-021: Crear Script de Inicialización de Base de Datos ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador  
**Quiero** que las migraciones se apliquen automáticamente al levantar  
**Para** no tener que ejecutar comandos manualmente

### Criterios de Aceptación
- ✅ Script que aplica migraciones al iniciar contenedor
- ✅ Verificación de conexión a SQL Server antes de migrar
- ✅ Logging de proceso de migración
- ✅ Retry logic si SQL Server no está listo
- ✅ Seed data aplicado automáticamente

### Opciones de Implementación

**Opción 1: Modificar Program.cs (Recomendada)**

```csharp
// En Program.cs, antes de app.Run()
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        var context = services.GetRequiredService<ApiDbContext>();
        
        // Retry logic
        var retries = 10;
        while (retries > 0)
        {
            try
            {
                context.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully");
                break;
            }
            catch (Exception ex)
            {
                retries--;
                logger.LogWarning(ex, "Could not apply migrations. Retries left: {Retries}", retries);
                if (retries == 0) throw;
                Thread.Sleep(2000);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}
```

**Opción 2: Script Shell en Dockerfile**

Crear `scripts/entrypoint.sh`:
```bash
#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S $DB_HOST -U $DB_USER -P $DB_PASSWORD -Q "SELECT 1" > /dev/null 2>&1; do
  echo "SQL Server is unavailable - sleeping"
  sleep 2
done

echo "SQL Server is up - applying migrations"
dotnet ef database update --no-build

echo "Starting API"
exec dotnet DevOpsApi.dll
```

### Tareas Técnicas
1. **Opción 1 (Recomendada)**: Modificar `Program.cs` con código de migración automática
2. Agregar logging detallado
3. Probar con:
   ```bash
   docker-compose down -v
   docker-compose up
   ```
4. Verificar en logs que migraciones se aplicaron
5. Commit: "feat: Add automatic database migration on startup"

### Dependencias
- ✅ US-019 (docker-compose creado)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Migraciones aplican automáticamente
- Retry logic funcionando
- Logs claros del proceso
- Probado destruyendo y recreando contenedores

---

## US-022: Documentar Comandos Docker ✅

**Estado**: ✅ **COMPLETADA**

**Como** miembro del equipo  
**Quiero** documentación clara de todos los comandos Docker  
**Para** poder trabajar eficientemente con contenedores

### Criterios de Aceptación
- ✅ README actualizado con sección Docker
- ✅ Comandos comunes documentados
- ✅ Troubleshooting de problemas frecuentes
- ✅ Ejemplos de desarrollo local

### Documentación a Agregar

**README.md - Sección Docker**

```markdown
## 🐳 Desarrollo Local con Docker

### Prerequisitos
- Docker Desktop instalado
- Docker Compose v2+

### Comandos Principales

#### Levantar ambiente completo
```bash
docker-compose up -d
```

#### Ver logs en tiempo real
```bash
# Todos los servicios
docker-compose logs -f

# Solo API
docker-compose logs -f api

# Solo SQL Server
docker-compose logs -f sqlserver
```

#### Detener servicios
```bash
docker-compose down

# También eliminar volúmenes (data de SQL Server)
docker-compose down -v
```

#### Rebuild de imagen API
```bash
docker-compose build api
docker-compose up -d api
```

#### Ejecutar comandos dentro del contenedor
```bash
# Migración manual
docker-compose exec api dotnet ef database update

# Bash shell
docker-compose exec api /bin/bash
```

#### Ver estado de servicios
```bash
docker-compose ps
```

### Troubleshooting

**SQL Server no levanta**
```bash
# Ver logs detallados
docker-compose logs sqlserver

# Verificar que tienes suficiente memoria (min 2GB)
docker info | grep Memory
```

**API no conecta a SQL Server**
```bash
# Verificar red
docker network inspect devops-network

# Verificar que SQL Server está healthy
docker-compose ps
```

**Recrear todo desde cero**
```bash
docker-compose down -v
docker system prune -a
docker-compose up --build
```
```

### Tareas Técnicas
1. Actualizar README.md con sección Docker completa
2. Agregar sección de troubleshooting
3. Incluir diagramas de arquitectura de contenedores (ASCII)
4. Commit: "docs: Add comprehensive Docker documentation"

### Dependencias
- ✅ US-019 (docker-compose funcionando)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- README actualizado
- Comandos probados
- Troubleshooting útil
- Revisado por equipo

---

## 📋 Resumen Sprint 3

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-018 | Crear Dockerfile Multi-Stage | 🔴 Crítica | 3 pts | ✅ Completado |
| US-019 | Crear .dockerignore | 🟢 Media | 1 pt | ✅ Completado |
| US-020 | Docker Compose Local | 🔴 Crítica | 5 pts | ✅ Completado |
| US-021 | Auto-migration en Startup | 🟡 Alta | 3 pts | ✅ Completado |
| US-022 | Documentar Docker | 🟢 Media | 2 pts | ✅ Completado |

**Total Sprint 3**: 14 puntos (~7 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-03-database.md](./user-stories-03-database.md)
- **➡️ Siguiente**: [user-stories-05-testing.md](./user-stories-05-testing.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
