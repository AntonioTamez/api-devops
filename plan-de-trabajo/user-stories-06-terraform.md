# 📖 Historias de Usuario - Parte 6: Terraform e Infraestructura Azure
**Identificador**: US-06-TERRAFORM  
**Orden de lectura**: 6 de 8  
**Sprint**: Sprint 5 - Infraestructura como Código  

---

## 🎯 Objetivo del Sprint 5
Crear toda la infraestructura de Azure con Terraform para deployment automatizado.

---

## US-027: Configurar Terraform Providers y Backend

**Como** DevOps engineer  
**Quiero** configurar Terraform con el provider de Azure  
**Para** provisionar recursos en Azure de forma declarativa

### Criterios de Aceptación
- ✅ Archivo `providers.tf` con configuración Azure
- ✅ Backend remoto configurado (Azure Storage)
- ✅ Versiones de providers especificadas
- ✅ Terraform init ejecutable
- ✅ Service Principal creado para autenticación

### Archivo providers.tf

```hcl
terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5.0"
    }
  }

  # Backend remoto en Azure Storage
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "tfstatedevops"
    container_name       = "tfstate"
    key                  = "api-devops.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
}
```

### Tareas Técnicas
1. Crear carpeta `terraform/` en raíz
2. Crear `terraform/providers.tf`
3. Crear Service Principal en Azure:
   ```bash
   az login
   az account set --subscription "YOUR_SUBSCRIPTION_ID"
   az ad sp create-for-rbac --name "sp-api-devops" --role Contributor --scopes /subscriptions/{subscription-id}
   ```
   Guardar output (clientId, clientSecret, tenantId)

4. Crear Resource Group y Storage Account para Terraform state:
   ```bash
   az group create --name terraform-state-rg --location eastus
   az storage account create --name tfstatedevops --resource-group terraform-state-rg --location eastus --sku Standard_LRS
   az storage container create --name tfstate --account-name tfstatedevops
   ```

5. Inicializar Terraform:
   ```bash
   cd terraform
   terraform init
   ```

6. Commit: "feat: Configure Terraform with Azure provider"

### Dependencias
- Ninguna (primer paso de infraestructura)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- providers.tf creado
- Service Principal configurado
- Terraform init exitoso
- Backend remoto funcionando

---

## US-028: Crear Variables y Outputs de Terraform

**Como** DevOps engineer  
**Quiero** variables parametrizadas  
**Para** reutilizar configuración entre ambientes

### Criterios de Aceptación
- ✅ Archivo `variables.tf` con todas las variables
- ✅ Archivos `.tfvars` por ambiente (dev, prod)
- ✅ Variables sensitivas marcadas
- ✅ Valores default apropiados
- ✅ Documentación de cada variable

### Archivo variables.tf

```hcl
# General
variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  validation {
    condition     = can(regex("^(dev|staging|prod)$", var.environment))
    error_message = "Environment must be dev, staging, or prod."
  }
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "East US"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "api-devops"
}

# Networking
variable "vnet_address_space" {
  description = "Virtual Network address space"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

# Container Registry
variable "acr_sku" {
  description = "SKU for Azure Container Registry"
  type        = string
  default     = "Basic"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.acr_sku)
    error_message = "ACR SKU must be Basic, Standard, or Premium."
  }
}

# Container Apps
variable "container_app_min_replicas" {
  description = "Minimum number of container replicas"
  type        = number
  default     = 1
}

variable "container_app_max_replicas" {
  description = "Maximum number of container replicas"
  type        = number
  default     = 10
}

variable "container_cpu" {
  description = "CPU cores per container"
  type        = number
  default     = 0.5
}

variable "container_memory" {
  description = "Memory in GB per container"
  type        = string
  default     = "1Gi"
}

# SQL Database
variable "sql_admin_username" {
  description = "SQL Server administrator username"
  type        = string
  default     = "sqladmin"
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "sql_database_sku" {
  description = "SQL Database SKU"
  type        = string
  default     = "Basic"
}

variable "sql_max_size_gb" {
  description = "Maximum size of SQL Database in GB"
  type        = number
  default     = 2
}

# Tags
variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
```

### Archivos de Variables por Ambiente

**environments/dev.tfvars**
```hcl
environment                  = "dev"
location                     = "East US"
acr_sku                      = "Basic"
container_app_min_replicas   = 1
container_app_max_replicas   = 3
container_cpu                = 0.5
container_memory             = "1Gi"
sql_database_sku             = "Basic"
sql_max_size_gb              = 2

tags = {
  Environment = "Development"
  Project     = "API DevOps"
  ManagedBy   = "Terraform"
}
```

**environments/prod.tfvars**
```hcl
environment                  = "prod"
location                     = "East US"
acr_sku                      = "Standard"
container_app_min_replicas   = 2
container_app_max_replicas   = 10
container_cpu                = 1.0
container_memory             = "2Gi"
sql_database_sku             = "S2"
sql_max_size_gb              = 10

tags = {
  Environment = "Production"
  Project     = "API DevOps"
  ManagedBy   = "Terraform"
}
```

### Tareas Técnicas
1. Crear `terraform/variables.tf`
2. Crear carpeta `terraform/environments/`
3. Crear `terraform/environments/dev.tfvars`
4. Crear `terraform/environments/prod.tfvars`
5. Agregar `*.tfvars` a `.gitignore` (excepto ejemplos)
6. Validar:
   ```bash
   terraform validate
   ```
7. Commit: "feat: Add Terraform variables for multi-environment support"

### Dependencias
- ✅ US-027 (Providers configurados)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Variables definidas
- Archivos .tfvars por ambiente
- Validación exitosa
- Documentadas en README

---

## US-029: Crear Recursos Base de Azure

**Como** DevOps engineer  
**Quiero** provisionar recursos base (Resource Group, Container Registry)  
**Para** tener la infraestructura fundamental

### Criterios de Aceptación
- ✅ Resource Group creado
- ✅ Azure Container Registry (ACR) creado
- ✅ Naming conventions consistentes
- ✅ Tags aplicados a todos los recursos
- ✅ Admin user habilitado en ACR para CI/CD

### Archivo main.tf (Parte 1)

```hcl
locals {
  resource_prefix = "${var.project_name}-${var.environment}"
  common_tags = merge(
    var.tags,
    {
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  )
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.resource_prefix}"
  location = var.location
  tags     = local.common_tags
}

# Container Registry
resource "azurerm_container_registry" "acr" {
  name                = replace("acr${local.resource_prefix}", "-", "")
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = var.acr_sku
  admin_enabled       = true

  tags = local.common_tags
}

# Log Analytics Workspace (para Application Insights y Container Apps)
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "appi-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = local.common_tags
}
```

### Tareas Técnicas
1. Crear `terraform/main.tf`
2. Implementar recursos base
3. Plan de ejecución:
   ```bash
   terraform plan -var-file="environments/dev.tfvars"
   ```
4. Aplicar (solo si el plan es correcto):
   ```bash
   terraform apply -var-file="environments/dev.tfvars"
   ```
5. Commit: "feat: Add base Azure resources (RG, ACR, App Insights)"

### Dependencias
- ✅ US-028 (Variables definidas)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Recursos creados en Azure
- Plan ejecutado exitosamente
- Recursos visibles en Azure Portal
- Outputs verificados

---

## US-030: Crear SQL Server y Base de Datos

**Como** DevOps engineer  
**Quiero** provisionar Azure SQL Database  
**Para** tener persistencia de datos gestionada

### Criterios de Aceptación
- ✅ SQL Server creado
- ✅ SQL Database creada
- ✅ Firewall rules configuradas
- ✅ Connection string almacenado en Key Vault
- ✅ Backup automático habilitado

### main.tf (Parte 2 - SQL)

```hcl
# SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "sql-${local.resource_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  minimum_tls_version = "1.2"

  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = data.azurerm_client_config.current.object_id
  }

  tags = local.common_tags
}

# SQL Database
resource "azurerm_mssql_database" "main" {
  name           = "sqldb-${local.resource_prefix}"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = var.sql_max_size_gb
  sku_name       = var.sql_database_sku
  zone_redundant = false

  tags = local.common_tags
}

# Firewall Rule - Allow Azure Services
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Firewall Rule - Allow specific IP (opcional para desarrollo)
resource "azurerm_mssql_firewall_rule" "allow_dev_ip" {
  count            = var.environment == "dev" ? 1 : 0
  name             = "AllowDevIP"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

# Data source para obtener client config
data "azurerm_client_config" "current" {}
```

### Tareas Técnicas
1. Agregar recursos SQL a `main.tf`
2. Agregar data source `azurerm_client_config`
3. Plan y apply:
   ```bash
   terraform plan -var-file="environments/dev.tfvars" -var="sql_admin_password=YourStrongP@ssw0rd!"
   terraform apply -var-file="environments/dev.tfvars" -var="sql_admin_password=YourStrongP@ssw0rd!"
   ```
4. Verificar en Azure Portal
5. Commit: "feat: Add Azure SQL Server and Database"

### Dependencias
- ✅ US-029 (Recursos base creados)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- SQL Server creado
- Database creada
- Firewall configurado
- Conectividad verificada

---

## US-031: Crear Azure Container Apps Environment

**Como** DevOps engineer  
**Quiero** Container App Environment y Container App  
**Para** hostear la aplicación de forma serverless

### Criterios de Aceptación
- ✅ Container App Environment creado
- ✅ Container App creado
- ✅ Integrado con Application Insights
- ✅ Variables de entorno configuradas
- ✅ Auto-scaling configurado
- ✅ Health probes configurados

### main.tf (Parte 3 - Container Apps)

```hcl
# Container App Environment
resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${local.resource_prefix}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  tags = local.common_tags
}

# Container App
resource "azurerm_container_app" "api" {
  name                         = "ca-${local.resource_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"

  template {
    min_replicas = var.container_app_min_replicas
    max_replicas = var.container_app_max_replicas

    container {
      name   = "api"
      image  = "${azurerm_container_registry.acr.login_server}/api-devops:latest"
      cpu    = var.container_cpu
      memory = var.container_memory

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment == "prod" ? "Production" : "Development"
      }

      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "sql-connection-string"
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = azurerm_application_insights.main.connection_string
      }

      liveness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health/live"
      }

      readiness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health/ready"
      }

      startup_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health"
      }
    }
  }

  secret {
    name  = "sql-connection-string"
    value = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};User ID=${var.sql_admin_username};Password=${var.sql_admin_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "acr-password"
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.acr.admin_password
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = local.common_tags
}
```

### Tareas Técnicas
1. Agregar Container Apps a `main.tf`
2. Plan y apply
3. Verificar URL del Container App en outputs
4. Commit: "feat: Add Azure Container Apps for API hosting"

### Dependencias
- ✅ US-028 (ACR creado)
- ✅ US-029 (SQL Database creada)

### Estimación
**Esfuerzo**: 5 puntos (2 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Container App creado
- Health probes configurados
- Auto-scaling funcionando
- URL accesible (después de deploy de imagen)

---

## US-032: Crear Outputs de Terraform

**Como** DevOps engineer  
**Quiero** outputs de los recursos creados  
**Para** usar valores en CI/CD y configuración

### Criterios de Aceptación
- ✅ Archivo `outputs.tf` creado
- ✅ Outputs de URLs, connection strings, IDs
- ✅ Sensitive values marcados
- ✅ Outputs documentados

### outputs.tf

```hcl
# Resource Group
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

# Container Registry
output "acr_login_server" {
  description = "Login server for Azure Container Registry"
  value       = azurerm_container_registry.acr.login_server
}

output "acr_admin_username" {
  description = "Admin username for ACR"
  value       = azurerm_container_registry.acr.admin_username
  sensitive   = true
}

output "acr_admin_password" {
  description = "Admin password for ACR"
  value       = azurerm_container_registry.acr.admin_password
  sensitive   = true
}

# Container App
output "container_app_url" {
  description = "URL of the Container App"
  value       = "https://${azurerm_container_app.api.latest_revision_fqdn}"
}

output "container_app_name" {
  description = "Name of the Container App"
  value       = azurerm_container_app.api.name
}

# SQL Server
output "sql_server_fqdn" {
  description = "Fully qualified domain name of SQL Server"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "Name of the SQL Database"
  value       = azurerm_mssql_database.main.name
}

output "sql_connection_string" {
  description = "SQL Server connection string"
  value       = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};User ID=${var.sql_admin_username};Encrypt=True;TrustServerCertificate=False;"
  sensitive   = true
}

# Application Insights
output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}
```

### Tareas Técnicas
1. Crear `terraform/outputs.tf`
2. Ver outputs:
   ```bash
   terraform output
   terraform output -json
   ```
3. Commit: "feat: Add Terraform outputs for CI/CD integration"

### Dependencias
- ✅ US-030 (Todos los recursos creados)

### Estimación
**Esfuerzo**: 1 punto (30 minutos)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- outputs.tf creado
- Outputs accesibles
- Valores correctos
- Sensitives marcados

---

## 📋 Resumen Sprint 5

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-026 | Configurar Terraform Providers | 🔴 Crítica | 3 pts | ⏳ Pendiente |
| US-027 | Crear Variables Terraform | 🔴 Crítica | 2 pts | ⏳ Pendiente |
| US-028 | Recursos Base Azure | 🔴 Crítica | 3 pts | ⏳ Pendiente |
| US-029 | SQL Server y Database | 🔴 Crítica | 3 pts | ⏳ Pendiente |
| US-030 | Container Apps | 🔴 Crítica | 5 pts | ⏳ Pendiente |
| US-031 | Terraform Outputs | 🟡 Alta | 1 pt | ⏳ Pendiente |

**Total Sprint 5**: 17 puntos (~8.5 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-05-testing.md](./user-stories-05-testing.md)
- **➡️ Siguiente**: [user-stories-07-cicd.md](./user-stories-07-cicd.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
