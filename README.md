# 🚀 DevOps API - .NET 8 con Azure y CI/CD Automatizado

[![Build Status](https://github.com/your-org/api-devops/workflows/Build%20and%20Test/badge.svg)](https://github.com/your-org/api-devops/actions)
[![Azure Deploy](https://github.com/your-org/api-devops/workflows/Deploy%20to%20Azure/badge.svg)](https://github.com/your-org/api-devops/actions)
[![Coverage](https://img.shields.io/badge/coverage-80%25-green.svg)](https://github.com/your-org/api-devops)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

API REST moderna construida con .NET 8, desplegada en Azure Container Apps con infraestructura como código (Terraform) y pipeline CI/CD completamente automatizado.

---

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Stack Tecnológico](#-stack-tecnológico)
- [Prerequisitos](#-prerequisitos)
- [Quick Start](#-quick-start)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Desarrollo Local](#-desarrollo-local)
- [Docker y Containerización](#-docker-y-containerización)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Arquitectura](#-arquitectura)
- [Variables de Entorno](#-variables-de-entorno)
- [Troubleshooting](#-troubleshooting)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## ✨ Características

- ✅ **API REST** con .NET 8 y ASP.NET Core
- ✅ **Swagger/OpenAPI** para documentación interactiva
- ✅ **Rate Limiting** basado en IP (10 req/min en producción, 100 req/min en desarrollo)
- ✅ **Entity Framework Core** con SQL Server
- ✅ **Docker** y Docker Compose para desarrollo local
- ✅ **Terraform** para infraestructura como código (IaC)
- ✅ **Azure Container Apps** para deployment serverless
- ✅ **GitHub Actions** para CI/CD automatizado
- ✅ **Health Checks** para monitoreo
- ✅ **Application Insights** para telemetría
- ✅ **Tests Unitarios** con xUnit y Moq
- ✅ **Code Coverage** con Coverlet

---

## 🛠️ Stack Tecnológico

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **Serilog** - Logging estructurado
- **AspNetCoreRateLimit** - Rate limiting basado en IP
- **FluentValidation** - Validación de modelos

### Base de Datos
- **Azure SQL Database** - Base de datos en la nube
- **SQL Server** - Para desarrollo local (Docker)

### Infraestructura
- **Azure Container Apps** - Hosting de contenedores
- **Azure Container Registry (ACR)** - Registro de imágenes
- **Terraform** - Infraestructura como código
- **Docker** - Containerización

### CI/CD
- **GitHub Actions** - Pipeline de deployment
- **GitHub Secrets** - Gestión de credenciales

### Monitoreo
- **Application Insights** - Telemetría y monitoreo
- **Health Checks** - Verificación de salud de la app

---

## 📦 Prerequisitos

Antes de comenzar, asegúrate de tener instalado:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (8.0 o superior)
  ```bash
  dotnet --version
  # 8.0.x
  ```

- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (última versión)
  ```bash
  docker --version
  # Docker version 24.x
  ```

- **[Terraform](https://www.terraform.io/downloads)** (>= 1.5.0)
  ```bash
  terraform --version
  # Terraform v1.5.x
  ```

- **[Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)** (para deployment)
  ```bash
  az --version
  # azure-cli 2.x
  ```

- **[Git](https://git-scm.com/downloads)** (para control de versiones)
  ```bash
  git --version
  # git version 2.x
  ```

### Opcional (IDEs)
- Visual Studio 2022 (17.8 o superior)
- Visual Studio Code con extensión C#
- JetBrains Rider

---

## 🚀 Quick Start

### 1. Clonar el repositorio

```bash
git clone https://github.com/your-org/api-devops.git
cd api-devops
```

### 2. Levantar con Docker Compose (Recomendado)

La forma más rápida de ejecutar el proyecto localmente con **un solo comando**:

```bash
# Levantar API + SQL Server + aplicar migraciones automáticamente
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs solo de la API
docker-compose logs -f api

# Verificar que los servicios están corriendo
docker-compose ps
```

La API estará disponible en:
- **API**: http://localhost:5065
- **Swagger UI**: http://localhost:5065/swagger
- **Health Check**: http://localhost:5065/health
- **SQL Server**: localhost:1433 (sa/YourStrong@Passw0rd)

### 3. Ejecutar sin Docker (Alternativa)

```bash
# Restaurar dependencias
dotnet restore src/DevOpsApi.csproj

# Aplicar migraciones
dotnet ef database update --project src

# Ejecutar API
dotnet run --project src/DevOpsApi.csproj

# Abrir Swagger
start http://localhost:5000/swagger
```

### 4. Detener servicios

```bash
docker-compose down
```

---

## 📂 Estructura del Proyecto

```
api-devops/
├── .github/
│   └── workflows/              # GitHub Actions workflows
│       ├── build-test.yml      # CI: Build y tests
│       └── deploy.yml          # CD: Deploy a Azure
│
├── src/                        # Código fuente del API
│   ├── Controllers/            # Endpoints del API
│   ├── Models/                 # Entidades y DbContext
│   ├── Services/               # Lógica de negocio
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Migrations/             # Migraciones de EF Core
│   ├── Program.cs              # Entry point
│   └── DevOpsApi.csproj        # Proyecto .NET
│
├── tests/                      # Tests
│   └── DevOpsApi.UnitTests/    # Tests unitarios
│       ├── Controllers/
│       ├── Services/
│       └── DevOpsApi.UnitTests.csproj
│
├── terraform/                  # Infraestructura como código
│   ├── main.tf                 # Recursos de Azure
│   ├── variables.tf            # Variables
│   ├── outputs.tf              # Outputs
│   ├── providers.tf            # Configuración de providers
│   └── environments/           # Configs por ambiente
│       ├── dev.tfvars         # Desarrollo
│       └── prod.tfvars        # Producción
│
├── docs/                       # Documentación adicional
│   ├── architecture.md         # Arquitectura de la solución
│   ├── api-guide.md           # Guía de uso del API
│   └── deployment.md          # Guía de deployment
│
├── plan-de-trabajo/            # Historias de usuario
│   ├── plan.md                # Plan maestro
│   └── user-stories-*.md      # Historias de usuario por sprint
│
├── Dockerfile                  # Dockerfile multi-stage
├── docker-compose.yml          # Orquestación local
├── .gitignore                  # Archivos ignorados
├── .editorconfig              # Reglas de código
└── README.md                   # Este archivo
```

---

## 💻 Desarrollo Local

### Configurar Base de Datos Local

```bash
# Con Docker Compose (recomendado)
docker-compose up -d sqlserver

# Esperar a que SQL Server esté listo
docker-compose logs -f sqlserver

# Aplicar migraciones
dotnet ef database update --project src
```

### Crear Nueva Migración

```bash
dotnet ef migrations add NombreMigracion --project src
dotnet ef database update --project src
```

### Ejecutar en Modo Desarrollo

```bash
# Watch mode (hot reload)
dotnet watch run --project src

# Modo debug en Visual Studio
# Presionar F5
```

### Variables de Entorno de Desarrollo

Crear archivo `src/appsettings.Development.json` (ya excluido en .gitignore):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DevOpsDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

---

## 🐳 Docker y Containerización

### Dockerfile Multi-Stage

El proyecto incluye un Dockerfile optimizado con Alpine Linux:

**Características:**
- ✅ **Multi-stage build** (build + runtime)
- ✅ **Imagen ligera**: 201MB (Alpine Linux)
- ✅ **Usuario no-root** para seguridad
- ✅ **Health check** integrado
- ✅ **Layer caching** optimizado

```bash
# Build imagen manualmente
docker build -t devops-api:latest .

# Ver tamaño de imagen
docker images devops-api:latest

# Ejecutar contenedor
docker run -d -p 5065:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;..." \
  devops-api:latest
```

### Docker Compose - Ambiente Completo

#### Servicios Incluidos:

**1. SQL Server 2022 Developer Edition**
- Puerto: 1433
- Usuario: sa
- Password: Configurable en `.env`
- Volumen persistente para datos
- Health check automático

**2. API (.NET 8)**
- Puerto: 5065
- Build desde Dockerfile local
- Migraciones automáticas al iniciar
- Seed data de ejemplo (5 productos)
- Depende de SQL Server (espera health check)

#### Comandos Docker Compose

```bash
# Iniciar todos los servicios
docker-compose up

# Iniciar en background (detached)
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f api
docker-compose logs -f sqlserver

# Verificar estado de servicios
docker-compose ps

# Detener servicios (mantiene datos)
docker-compose stop

# Detener y eliminar contenedores
docker-compose down

# Detener y eliminar contenedores + volúmenes (limpieza completa)
docker-compose down -v

# Rebuild imagen y reiniciar
docker-compose up --build

# Ver recursos utilizados
docker-compose stats
```

#### Variables de Entorno (.env)

Crear archivo `.env` basado en `.env.example`:

```bash
# Copiar template
cp .env.example .env

# Editar valores
nano .env
```

Configuraciones disponibles:

```bash
# SQL Server
SQL_SA_PASSWORD=YourStrong@Passw0rd
SQL_PORT=1433
DB_NAME=DevOpsDb

# API
API_PORT=5065
ASPNETCORE_ENVIRONMENT=Development

# CORS
CORS_ORIGIN=*

# Rate Limiting
ENABLE_RATE_LIMITING=true
RATE_LIMIT=100
RATE_PERIOD=1m
```

### Migraciones Automáticas

Las migraciones se aplican **automáticamente** al iniciar la API:

**Proceso:**
1. API verifica conexión a SQL Server (retry logic: 10 intentos × 3s)
2. Lista migraciones pendientes
3. Aplica migraciones si hay pendientes
4. Inserta seed data si la DB está vacía (5 productos de ejemplo)
5. Inicia el servidor

**Logs de ejemplo:**
```
🔄 Checking database connection and applying migrations...
✅ Database connection established
📝 Found 1 pending migration(s). Applying...
  - 20241021_InitialCreate
✅ Database migrations applied successfully
🌱 Seeding initial data...
✅ Seed data applied successfully. Added 5 products
```

**Productos de ejemplo incluidos:**
- Laptop Dell XPS 15 - $1,299.99
- Wireless Mouse Logitech MX Master 3 - $99.99
- Mechanical Keyboard Keychron K2 - $79.99
- USB-C Hub Anker 7-in-1 - $49.99
- Monitor LG 27 UltraFine 4K - $599.99

### Troubleshooting Docker

#### SQL Server no inicia

```bash
# Ver logs detallados
docker-compose logs sqlserver

# Verificar recursos disponibles
docker system df

# Reiniciar contenedor
docker-compose restart sqlserver

# Verificar health check
docker inspect devops-sqlserver --format='{{.State.Health.Status}}'
```

#### API no puede conectar a SQL Server

```bash
# Verificar que SQL Server está healthy
docker-compose ps

# Debe mostrar: healthy en la columna Status
# Si muestra unhealthy, esperar más tiempo o revisar logs

# Ver variables de entorno de la API
docker-compose exec api env | grep Connection

# Verificar network
docker network inspect devops-network
```

#### Migraciones no se aplican

```bash
# Ver logs de la API durante startup
docker-compose logs api | grep migration

# Aplicar migraciones manualmente (dentro del contenedor)
docker-compose exec api dotnet ef database update

# O aplicar desde host (si tienes .NET SDK)
dotnet ef database update --project src
```

#### Puerto en uso

```bash
# Cambiar puertos en docker-compose.yml
ports:
  - "5066:8080"  # Cambiar 5065 por 5066

# O detener el proceso que usa el puerto
# Windows:
netstat -ano | findstr :5065
taskkill /PID <PID> /F

# Linux/Mac:
lsof -i :5065
kill -9 <PID>
```

#### Limpiar todo y empezar de cero

```bash
# Detener y eliminar todo
docker-compose down -v

# Eliminar imágenes
docker rmi devops-api:latest

# Limpiar sistema Docker
docker system prune -a --volumes

# Rebuild desde cero
docker-compose up --build
```

---

## 🧪 Testing

### Ejecutar Tests Unitarios

```bash
# Ejecutar todos los tests
dotnet test

# Con output detallado
dotnet test --verbosity normal

# Filtrar tests específicos
dotnet test --filter "FullyQualifiedName~ProductService"
```

### Code Coverage

```bash
# Generar reporte de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Generar reporte HTML
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

# Abrir reporte
start coverage-report/index.html
```

### Agregar Nuevos Tests

```bash
# Crear proyecto de tests (si no existe)
dotnet new xunit -n DevOpsApi.UnitTests -o tests/DevOpsApi.UnitTests

# Agregar referencia al proyecto principal
dotnet add tests/DevOpsApi.UnitTests reference src/DevOpsApi.csproj

# Instalar paquetes de testing
dotnet add tests/DevOpsApi.UnitTests package Moq
dotnet add tests/DevOpsApi.UnitTests package FluentAssertions
```

---

## 🚀 Deployment

### Deployment Automático (CI/CD)

El proyecto tiene configurado GitHub Actions para deployment automático:

1. **Pull Request** → Se ejecuta pipeline de build y tests
2. **Merge a master** → Se ejecuta pipeline de deployment completo:
   - Build de imagen Docker
   - Push a Azure Container Registry
   - Terraform apply (infraestructura)
   - Deploy a Azure Container Apps
   - Health check post-deployment

### Deployment Manual a Azure

#### 1. Configurar Azure

```bash
# Login a Azure
az login

# Seleccionar suscripción
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Crear Service Principal para Terraform
az ad sp create-for-rbac --name "sp-api-devops" --role Contributor \
  --scopes /subscriptions/{subscription-id}
```

#### 2. Inicializar Terraform

```bash
cd terraform

# Inicializar
terraform init

# Ver plan de cambios
terraform plan -var-file="environments/prod.tfvars"

# Aplicar infraestructura
terraform apply -var-file="environments/prod.tfvars"

# Ver outputs (URL del API, connection strings, etc.)
terraform output
```

#### 3. Build y Push de Imagen Docker

```bash
# Login a ACR
az acr login --name YOUR_ACR_NAME

# Build imagen
docker build -t YOUR_ACR_NAME.azurecr.io/api-devops:latest .

# Push a ACR
docker push YOUR_ACR_NAME.azurecr.io/api-devops:latest
```

#### 4. Deploy a Container Apps

```bash
# Actualizar Container App con nueva imagen
az containerapp update \
  --name ca-api-devops-prod \
  --resource-group rg-api-devops-prod \
  --image YOUR_ACR_NAME.azurecr.io/api-devops:latest
```

### Verificar Deployment

```bash
# Health check
curl https://your-app.azurecontainerapps.io/health

# Swagger UI
start https://your-app.azurecontainerapps.io/swagger
```

---

## 🏗️ Arquitectura

### Diagrama de Alto Nivel

```
┌─────────────────────────────────────────────────────────────┐
│                     GitHub Repository                        │
│                    (Source Code + IaC)                       │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ git push
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Actions                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Build & Test│  │ Docker Build │  │   Terraform  │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                      Azure Cloud                             │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │    Azure Container Registry (ACR)                  │    │
│  │        Docker Images Repository                    │    │
│  └────────────────────┬───────────────────────────────┘    │
│                       │                                     │
│                       ▼                                     │
│  ┌────────────────────────────────────────────────────┐    │
│  │    Azure Container Apps Environment                │    │
│  │  ┌──────────────────────────────────────────┐     │    │
│  │  │    Container App (API .NET 8)            │     │    │
│  │  │    - Auto-scaling                        │     │    │
│  │  │    - HTTPS Ingress                       │     │    │
│  │  │    - Health Checks                       │     │    │
│  │  └──────────────────┬───────────────────────┘     │    │
│  └────────────────────────────────────────────────────┘    │
│                       │                                     │
│                       ▼                                     │
│  ┌────────────────────────────────────────────────────┐    │
│  │         Azure SQL Database                         │    │
│  │         - Managed SQL Server                       │    │
│  │         - Automatic backups                        │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │       Application Insights                         │    │
│  │       - Logs & Telemetry                           │    │
│  │       - Performance Monitoring                     │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Recursos de Azure Provisionados

| Recurso | Propósito | SKU |
|---------|-----------|-----|
| **Resource Group** | Contenedor de recursos | N/A |
| **Container Registry** | Almacenar imágenes Docker | Basic/Standard |
| **Container Apps Environment** | Entorno de ejecución | N/A |
| **Container App** | Hosting del API | Consumption |
| **SQL Database** | Base de datos | Basic/S2 |
| **Application Insights** | Monitoreo y telemetría | Standard |
| **Log Analytics Workspace** | Centralización de logs | PerGB2018 |

---

## 🔐 Variables de Entorno

### Desarrollo Local (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DevOpsDb;..."
  },
  "ApplicationInsights": {
    "ConnectionString": ""
  }
}
```

### Producción (Azure - Variables de Entorno)

Configuradas en Azure Container Apps:

- `ConnectionStrings__DefaultConnection`: Connection string a SQL Database
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: App Insights connection string
- `ASPNETCORE_ENVIRONMENT`: Production

### GitHub Secrets (para CI/CD)

Configurar en: **Settings → Secrets and variables → Actions**

| Secret | Descripción |
|--------|-------------|
| `AZURE_CREDENTIALS` | Credenciales del Service Principal (JSON) |
| `AZURE_SUBSCRIPTION_ID` | ID de suscripción de Azure |
| `ACR_LOGIN_SERVER` | URL del Container Registry |
| `ACR_USERNAME` | Usuario admin del ACR |
| `ACR_PASSWORD` | Password del ACR |
| `SQL_ADMIN_PASSWORD` | Password del SQL Server |

---

## 🔧 Troubleshooting

### Problema: Error de conexión a SQL Server en Docker

**Síntoma**: `Cannot open database "DevOpsDb"`

**Solución**:
```bash
# Verificar que SQL Server está ejecutándose
docker-compose ps

# Ver logs de SQL Server
docker-compose logs sqlserver

# Reiniciar contenedor
docker-compose restart sqlserver

# Esperar 30 segundos y aplicar migraciones
dotnet ef database update --project src
```

### Problema: Migraciones no se aplican

**Síntoma**: Tablas no existen en base de datos

**Solución**:
```bash
# Verificar migraciones pendientes
dotnet ef migrations list --project src

# Aplicar migraciones manualmente
dotnet ef database update --project src

# Si hay problemas, recrear base de datos
dotnet ef database drop --project src
dotnet ef database update --project src
```

### Problema: Puerto 5000 ya está en uso

**Síntoma**: `Unable to bind to http://localhost:5000`

**Solución**:
```bash
# Cambiar puerto en docker-compose.yml
ports:
  - "5001:8080"  # Cambiar 5000 por 5001

# O detener el proceso que usa el puerto
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Problema: Tests fallan en CI/CD

**Síntoma**: Pipeline de GitHub Actions falla en step de tests

**Solución**:
1. Ejecutar tests localmente: `dotnet test`
2. Verificar que todas las dependencias están en `.csproj`
3. Revisar logs del workflow en GitHub Actions
4. Asegurarse de que InMemory database está configurada en tests

### Problema: Deployment a Azure falla

**Síntoma**: `terraform apply` falla

**Solución**:
```bash
# Verificar Service Principal
az account show

# Validar sintaxis de Terraform
terraform validate

# Ver plan detallado
terraform plan -var-file="environments/prod.tfvars"

# Revisar state de Terraform
terraform state list
```

---

## 👥 Contribución

### Proceso de Desarrollo

1. **Fork** el proyecto
2. Crear **feature branch**: `git checkout -b feature/nueva-funcionalidad`
3. **Commit** cambios: `git commit -m 'feat: Add nueva funcionalidad'`
4. **Push** a branch: `git push origin feature/nueva-funcionalidad`
5. Crear **Pull Request** a `develop`

### Convenciones de Commits

Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `test:` Agregar o modificar tests
- `chore:` Tareas de mantenimiento
- `refactor:` Refactorización de código
- `ci:` Cambios en CI/CD

Ejemplos:
```
feat: Add Product CRUD endpoints
fix: Resolve null reference in ProductService
docs: Update README with deployment steps
test: Add unit tests for ProductsController
```

### Code Style

- Seguir las reglas definidas en `.editorconfig`
- Ejecutar `dotnet format` antes de commit
- Mantener coverage > 80%
- Documentar APIs públicas con XML comments

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver archivo [LICENSE](LICENSE) para más detalles.

---

## 📞 Contacto y Soporte

- **Equipo**: DevOps Team
- **Email**: devops@example.com
- **Documentación**: [Wiki del Proyecto](https://github.com/your-org/api-devops/wiki)
- **Issues**: [GitHub Issues](https://github.com/your-org/api-devops/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/api-devops/discussions)

---

## 📚 Recursos Adicionales

- [Plan de Trabajo](plan-de-trabajo/plan.md)
- [Historias de Usuario](plan-de-trabajo/user-stories-00-index.md)
- [Documentación de Terraform](terraform/README.md)
- [Guía de Testing](tests/README.md)
- [Workflows de CI/CD](.github/workflows/README.md)

---

**Última actualización**: Octubre 2025  
**Versión**: 1.0.0  
**Mantenido por**: DevOps Team

---

⭐ Si este proyecto te resulta útil, no olvides darle una estrella en GitHub!
