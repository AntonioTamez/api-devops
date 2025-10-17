# Plan de Implementación - API .NET 8 con DevOps Automatizado

## 📋 Resumen Ejecutivo

Proyecto completo de API REST en .NET 8 con infraestructura automatizada usando Azure, Terraform, Docker y CI/CD con GitHub Actions.

---

## 🎯 Objetivos

- Crear API REST en .NET 8 con Swagger integrado
- Contenedorizar la aplicación con Docker
- Provisionar infraestructura en Azure usando Terraform
- Implementar CI/CD automatizado con GitHub Actions
- Deploy automático al hacer PR a rama `master`

---

## 🏗️ Stack Tecnológico

### Backend
- **.NET 8** - ASP.NET Core Web API
- **Entity Framework Core 8** - ORM
- **Swagger/OpenAPI** - Documentación de API
- **xUnit** - Testing framework

### Base de Datos
- **Azure SQL Database** - SQL Server en la nube
- **SQL Server 2022** - Para desarrollo local (Docker)

### Contenedorización
- **Docker** - Contenedores
- **Docker Compose** - Orquestación local
- **Azure Container Registry (ACR)** - Registro de imágenes

### Infraestructura
- **Terraform** - Infrastructure as Code
- **Azure** - Cloud provider

### CI/CD
- **GitHub Actions** - Pipeline de deployment
- **Azure DevOps** - (Alternativa opcional)

---

## 🏛️ Arquitectura Azure

### Recursos de Azure a Provisionar

1. **Azure Resource Group**
   - Contenedor lógico de todos los recursos
   - Ambiente: Development, Production

2. **Azure Container Registry (ACR)**
   - Registro privado de imágenes Docker
   - SKU: Basic (dev), Standard (prod)

3. **Azure Container Apps**
   - Hosting del API (serverless)
   - Auto-scaling basado en demanda
   - HTTPS automático

4. **Azure SQL Database**
   - SQL Server como servicio
   - SKU: Basic (dev), Standard S2 (prod)
   - Backup automático

5. **Azure Application Insights**
   - Monitoreo de aplicación
   - Telemetría y logs
   - Performance metrics

6. **Azure Key Vault**
   - Gestión segura de secrets
   - Connection strings
   - API keys

7. **Azure Virtual Network** (Opcional)
   - Red privada
   - Seguridad adicional

---

## 📁 Estructura del Proyecto

```
api-devops/
├── .github/
│   └── workflows/
│       └── deploy.yml              # Pipeline CI/CD
│
├── src/
│   ├── Program.cs                  # Entry point
│   ├── DevOpsApi.csproj           # Proyecto .NET
│   ├── appsettings.json           # Configuración local
│   ├── appsettings.Production.json # Configuración producción
│   │
│   ├── Controllers/
│   │   ├── ProductsController.cs   # CRUD de productos
│   │   └── HealthController.cs     # Health checks
│   │
│   ├── Models/
│   │   ├── Product.cs              # Entidad de producto
│   │   └── ApiDbContext.cs         # EF Core DbContext
│   │
│   ├── Services/
│   │   ├── IProductService.cs      # Interface
│   │   └── ProductService.cs       # Implementación
│   │
│   └── Migrations/                 # EF Core migrations
│       └── InitialCreate.cs
│
├── terraform/
│   ├── main.tf                     # Recursos principales
│   ├── variables.tf                # Variables parametrizables
│   ├── outputs.tf                  # Outputs del deployment
│   ├── providers.tf                # Azure provider config
│   └── environments/
│       ├── dev.tfvars             # Variables desarrollo
│       └── prod.tfvars            # Variables producción
│
├── Dockerfile                      # Imagen Docker multi-stage
├── docker-compose.yml              # Ambiente local completo
├── .dockerignore                   # Exclusiones Docker
├── .gitignore                      # Exclusiones Git
├── README.md                       # Documentación del proyecto
└── plan.md                         # Este archivo
```

---

## 🐳 Docker Setup

### Dockerfile Multi-Stage

**Stage 1: Build**
- Imagen base: `mcr.microsoft.com/dotnet/sdk:8.0`
- Restore de dependencias
- Build del proyecto
- Publish optimizado

**Stage 2: Runtime**
- Imagen base: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Solo runtime (imagen ligera)
- Copy de archivos publicados
- Exposición de puerto 8080

### docker-compose.yml

**Servicios:**

1. **api**
   - Puerto: 5000 (HTTP), 5001 (HTTPS)
   - Dependencia: SQL Server
   - Variables de entorno
   - Connection string

2. **sqlserver**
   - Imagen: `mcr.microsoft.com/mssql/server:2022-latest`
   - Puerto: 1433
   - SA password
   - Volumen persistente: `sqldata`

---

## 🔧 Terraform - Infraestructura como Código

### Archivos de Terraform

#### providers.tf
```hcl
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  backend "azurerm" {
    # State file en Azure Storage
  }
}

provider "azurerm" {
  features {}
}
```

#### variables.tf
Variables parametrizables:
- `environment` (dev, prod)
- `location` (East US, West Europe)
- `sql_admin_username`
- `sql_admin_password` (sensitive)
- `container_registry_sku`
- `sql_database_sku`

#### main.tf
Recursos a crear:
- Resource Group
- Container Registry
- Container App Environment
- Container App
- SQL Server
- SQL Database
- Application Insights
- Key Vault
- Key Vault Secrets (connection strings)

#### outputs.tf
Outputs importantes:
- Container App URL
- SQL Server FQDN
- Application Insights connection string
- Container Registry login server

---

## 🚀 CI/CD Pipeline - GitHub Actions

### Archivo: `.github/workflows/deploy.yml`

### Triggers
```yaml
on:
  push:
    branches:
      - master      # Deploy automático a producción
  pull_request:
    branches:
      - master      # Solo build y test
```

### Jobs

#### 1. Build and Test
```yaml
- Checkout código
- Setup .NET 8
- Restore dependencies
- Build proyecto
- Run unit tests
- Run EF Core migration check
- Code coverage report
```

#### 2. Build Docker Image
```yaml
- Login to Azure Container Registry
- Build imagen Docker
- Tag con SHA + timestamp
- Push imagen a ACR
```

#### 3. Terraform Apply
```yaml
- Login to Azure (Service Principal)
- Terraform init
- Terraform plan
- Terraform apply (auto-approve en master)
- Store terraform outputs
```

#### 4. Deploy Application
```yaml
- Update Azure Container App
- Set nueva imagen desde ACR
- Run EF Core migrations
- Health check validation
- Rollback si falla health check
- Notification (Slack/Teams)
```

---

## 🔐 Secrets de GitHub

Configurar en: **Settings → Secrets and variables → Actions**

### Secrets Requeridos

1. **`AZURE_CREDENTIALS`**
   ```json
   {
     "clientId": "xxx",
     "clientSecret": "xxx",
     "subscriptionId": "xxx",
     "tenantId": "xxx"
   }
   ```

2. **`AZURE_SUBSCRIPTION_ID`** - ID de suscripción Azure

3. **`ACR_LOGIN_SERVER`** - URL del Container Registry

4. **`ACR_USERNAME`** - Admin username del ACR

5. **`ACR_PASSWORD`** - Admin password del ACR

6. **`SQL_ADMIN_PASSWORD`** - Password del SQL Server

---

## 📦 Dependencias del Proyecto .NET

### Paquetes NuGet

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
<PackageReference Include="Azure.Identity" Version="1.10.0" />
<PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
```

---

## 🎬 Comandos de Inicio Rápido

### 1. Crear Proyecto .NET 8

```bash
cd c:/ATS/GIT/api-devops
dotnet new webapi -n DevOpsApi -o src
cd src
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package Azure.Identity
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 2. Crear Estructura de Base de Datos

```bash
# Crear DbContext y modelos
# ... (código C#)

# Crear migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update
```

### 3. Levantar Ambiente Local

```bash
# Levantar contenedores
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener contenedores
docker-compose down
```

### 4. Acceder a la API Local

- **Swagger UI**: http://localhost:5000
- **API**: http://localhost:5000/api
- **Health Check**: http://localhost:5000/health

### 5. Terraform - Provisionar Infraestructura

```bash
cd terraform

# Inicializar
terraform init

# Planificar cambios
terraform plan -var-file="environments/dev.tfvars"

# Aplicar cambios
terraform apply -var-file="environments/prod.tfvars"

# Ver outputs
terraform output
```

---

## 🔄 Flujo de Trabajo (Workflow)

### Desarrollo Local

1. Desarrollador crea feature branch
2. Desarrolla y prueba localmente con `docker-compose`
3. Commit y push a GitHub
4. Crea Pull Request a `master`

### Pull Request

1. GitHub Actions ejecuta pipeline
2. **Build** del proyecto
3. **Tests** unitarios e integración
4. **Build** de imagen Docker
5. **Validación** de Terraform (plan)
6. Revisión de código por equipo

### Merge a Master

1. Merge aprobado
2. GitHub Actions ejecuta pipeline completo
3. **Build** y push de imagen a ACR
4. **Terraform apply** - actualiza infraestructura
5. **Deploy** a Azure Container Apps
6. **Migrations** de EF Core
7. **Health check** post-deployment
8. **Notificación** de éxito/fallo

---

## 📊 Monitoreo y Observabilidad

### Application Insights

- **Logs**: Todos los logs de la aplicación
- **Metrics**: CPU, memoria, requests
- **Traces**: Distributed tracing
- **Exceptions**: Errores capturados
- **Dependencies**: SQL queries, HTTP calls

### Health Checks

Endpoints de health check:
- `/health` - Health general
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

Checks incluidos:
- SQL Server connectivity
- Disk space
- Memory usage

---

## 🔒 Seguridad

### Buenas Prácticas Implementadas

1. **Secrets en Key Vault** - No hardcodear credenciales
2. **HTTPS obligatorio** - Redirect HTTP → HTTPS
3. **CORS configurado** - Política restrictiva en producción
4. **Connection strings encriptados**
5. **Service Principal** para GitHub Actions
6. **Least privilege** en permisos Azure
7. **SQL Server con firewall** - Solo IPs permitidas

---

## 📝 Configuración de Ambientes

### appsettings.json (Local)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DevOpsDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### appsettings.Production.json (Azure)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/sql-connection-string)"
  },
  "ApplicationInsights": {
    "ConnectionString": "..."
  }
}
```

---

## 🧪 Testing

### Estructura de Tests

```
tests/
├── DevOpsApi.UnitTests/
│   ├── Controllers/
│   │   └── ProductsControllerTests.cs
│   └── Services/
│       └── ProductServiceTests.cs
│
└── DevOpsApi.IntegrationTests/
    └── ApiTests.cs
```

### Comandos de Testing

```bash
# Run all tests
dotnet test

# Run con coverage
dotnet test --collect:"XPlat Code Coverage"

# Run específico
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
```

---

## 📈 Roadmap

### Fase 1 - MVP (Esta implementación)
- ✅ API .NET 8 con Swagger
- ✅ Docker + Docker Compose
- ✅ Terraform para Azure
- ✅ CI/CD con GitHub Actions
- ✅ SQL Server

### Fase 2 - Mejoras
- 🔲 Autenticación JWT
- 🔲 Rate limiting
- 🔲 Redis para caching
- 🔲 API Gateway (Azure API Management)

### Fase 3 - Avanzado
- 🔲 Microservicios adicionales
- 🔲 Event-driven con Azure Service Bus
- 🔲 Azure Functions para workers
- 🔲 Kubernetes (AKS) en lugar de Container Apps

---

## 🆘 Troubleshooting

### Problemas Comunes

**1. Docker no inicia SQL Server**
```bash
# Verificar logs
docker-compose logs sqlserver

# Recrear contenedor
docker-compose down -v
docker-compose up -d
```

**2. EF Migrations fallan**
```bash
# Verificar connection string
dotnet ef database update --verbose

# Recrear migración
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
```

**3. GitHub Actions falla en Deploy**
- Verificar secrets configurados
- Verificar Service Principal tiene permisos
- Revisar logs de Terraform

**4. Container App no levanta**
- Verificar imagen en ACR
- Revisar Application Insights logs
- Verificar variables de entorno

---

## 📚 Recursos y Referencias

### Documentación Oficial

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Azure Container Apps](https://docs.microsoft.com/azure/container-apps)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [GitHub Actions](https://docs.github.com/actions)

### Comandos Útiles

```bash
# .NET
dotnet --version
dotnet build
dotnet run
dotnet ef migrations add <name>
dotnet ef database update

# Docker
docker build -t api-devops:latest .
docker run -p 5000:8080 api-devops:latest
docker-compose up -d
docker-compose logs -f

# Terraform
terraform init
terraform plan
terraform apply
terraform destroy
terraform output

# Azure CLI
az login
az account show
az group list
az container app list
```

---

## 👥 Equipo y Contacto

- **Arquitecto**: DevOps Team
- **Email**: devops@example.com
- **Repository**: https://github.com/your-org/api-devops

---

## 📄 Licencia

Este proyecto es de uso interno.

---

**Última actualización**: Octubre 2025
**Versión del plan**: 1.0
**Estado**: Listo para implementación
