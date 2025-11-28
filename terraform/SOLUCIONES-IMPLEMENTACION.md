# 🛠️ Guía de Implementación de Soluciones de Seguridad

**Fecha**: Noviembre 27, 2025  
**Versión**: 1.0  
**Relacionado con**: SECURITY-AUDIT-REPORT.md

---

## 📋 Índice

1. [Correcciones Críticas Aplicadas](#correcciones-críticas-aplicadas)
2. [Archivos a Crear - Variables](#1-crear-variablestf)
3. [Archivos a Crear - Main](#2-crear-maintf)
4. [Archivos a Crear - Outputs](#3-crear-outputstf)
5. [Archivos a Crear - Locals](#4-crear-localstf)
6. [Mejoras en Scripts](#mejoras-en-scripts-powershell)
7. [Archivos de Ambiente](#archivos-de-ambiente)
8. [Checklist de Implementación](#checklist-de-implementación)

---

## ✅ Correcciones Críticas Aplicadas

### Archivo: `providers.tf` - YA CORREGIDO

**Cambios realizados**:

1. ✅ **Terraform version constraint** mejorado:
   ```hcl
   required_version = ">= 1.5.0, < 2.0.0"
   ```

2. ✅ **Provider versions** optimizadas:
   ```hcl
   version = "~> 3.80"  # Permite 3.80, 3.81, 3.82... pero no 4.0
   ```

3. ✅ **Key Vault purge protection** CORREGIDA:
   ```hcl
   purge_soft_delete_on_destroy = false  # CAMBIADO de true a false
   ```

4. ✅ **Backend configuration** mejorada con Azure AD auth:
   ```hcl
   use_azuread_auth = true  # Agregado
   ```

5. ✅ **Comentarios de seguridad** agregados

---

## 📝 ARCHIVOS A CREAR

### 1. Crear `variables.tf`

**Ubicación**: `terraform/variables.tf`

**Contenido**:

```hcl
# ========================================
# VARIABLES GENERALES
# ========================================

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

# ========================================
# TAGGING
# ========================================

variable "cost_center" {
  description = "Cost center for billing"
  type        = string
  default     = "Engineering"
}

variable "owner_email" {
  description = "Email of resource owner"
  type        = string
}

variable "tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# ========================================
# NETWORKING
# ========================================

variable "vnet_address_space" {
  description = "Virtual Network address space"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

# ========================================
# CONTAINER REGISTRY
# ========================================

variable "acr_sku" {
  description = "SKU for Azure Container Registry"
  type        = string
  default     = "Basic"
  
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.acr_sku)
    error_message = "ACR SKU must be Basic, Standard, or Premium."
  }
}

variable "acr_admin_enabled" {
  description = "Enable admin user for ACR (NOT RECOMMENDED - use Managed Identity)"
  type        = bool
  default     = false
}

# ========================================
# CONTAINER APPS
# ========================================

variable "container_app_min_replicas" {
  description = "Minimum number of container replicas"
  type        = number
  default     = 1
  
  validation {
    condition     = var.container_app_min_replicas >= 0 && var.container_app_min_replicas <= 30
    error_message = "Min replicas must be between 0 and 30."
  }
}

variable "container_app_max_replicas" {
  description = "Maximum number of container replicas"
  type        = number
  default     = 10
  
  validation {
    condition     = var.container_app_max_replicas >= 1 && var.container_app_max_replicas <= 30
    error_message = "Max replicas must be between 1 and 30."
  }
}

variable "container_cpu" {
  description = "CPU cores per container"
  type        = number
  default     = 0.5
  
  validation {
    condition     = contains([0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0], var.container_cpu)
    error_message = "CPU must be one of: 0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0."
  }
}

variable "container_memory" {
  description = "Memory in GB per container"
  type        = string
  default     = "1Gi"
  
  validation {
    condition     = can(regex("^(0.5|1|1.5|2|2.5|3|3.5|4)Gi$", var.container_memory))
    error_message = "Memory must be between 0.5Gi and 4Gi in 0.5Gi increments."
  }
}

# ========================================
# SQL DATABASE
# ========================================

variable "sql_admin_username" {
  description = "SQL Server administrator username"
  type        = string
  default     = "sqladmin"
  
  validation {
    condition     = length(var.sql_admin_username) >= 1 && length(var.sql_admin_username) <= 128
    error_message = "SQL admin username must be between 1 and 128 characters."
  }
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
  
  validation {
    condition     = length(var.sql_admin_password) >= 8
    error_message = "SQL admin password must be at least 8 characters."
  }
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

variable "allowed_sql_ips" {
  description = "List of allowed IPs for SQL Server access"
  type = list(object({
    name = string
    ip   = string
  }))
  default = []
}

# ========================================
# KEY VAULT
# ========================================

variable "key_vault_allowed_ips" {
  description = "List of allowed IPs for Key Vault access"
  type        = list(string)
  default     = []
}

variable "key_vault_purge_protection" {
  description = "Enable purge protection for Key Vault (recommended for prod)"
  type        = bool
  default     = true
}

# ========================================
# FEATURE FLAGS
# ========================================

variable "enable_deletion_protection" {
  description = "Enable deletion protection for critical resources"
  type        = bool
  default     = true
}

variable "enable_monitoring" {
  description = "Enable Application Insights and monitoring"
  type        = bool
  default     = true
}
```

**Comando para crear**:
```powershell
# Crear el archivo con el contenido anterior
# O copiar desde esta documentación
```

---

### 2. Crear `main.tf`

**Ubicación**: `terraform/main.tf`

**Contenido** (Esqueleto - completar según US-029 a US-031):

```hcl
# ========================================
# DATA SOURCES
# ========================================

data "azurerm_client_config" "current" {}

# ========================================
# RESOURCE GROUP
# ========================================

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.resource_prefix}"
  location = var.location
  tags     = local.common_tags
  
  lifecycle {
    prevent_destroy = var.enable_deletion_protection && var.environment == "prod"
  }
}

# ========================================
# LOG ANALYTICS & APPLICATION INSIGHTS
# ========================================

resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

resource "azurerm_application_insights" "main" {
  count               = var.enable_monitoring ? 1 : 0
  name                = "appi-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = local.common_tags
}

# ========================================
# CONTAINER REGISTRY
# ========================================

resource "azurerm_container_registry" "acr" {
  name                = replace("acr${local.resource_prefix}", "-", "")
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = var.acr_sku
  
  # ✅ SEGURIDAD: NO usar admin credentials, usar Managed Identity
  admin_enabled = var.acr_admin_enabled  # Default: false

  tags = local.common_tags
}

# ========================================
# MANAGED IDENTITY
# ========================================

resource "azurerm_user_assigned_identity" "container_app" {
  name                = "id-${local.resource_prefix}-ca"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tags                = local.common_tags
}

# ✅ SEGURIDAD: Dar permisos AcrPull a la Managed Identity
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.container_app.principal_id
}

# ========================================
# KEY VAULT
# ========================================

resource "azurerm_key_vault" "main" {
  name                       = substr("kv-${replace(local.resource_prefix, "-", "")}", 0, 24)
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = var.key_vault_purge_protection && var.environment == "prod"

  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  enable_rbac_authorization       = false

  # ✅ SEGURIDAD: Restringir acceso en producción
  network_acls {
    bypass                     = "AzureServices"
    default_action             = var.environment == "prod" ? "Deny" : "Allow"
    ip_rules                   = var.key_vault_allowed_ips
    virtual_network_subnet_ids = []
  }

  tags = local.common_tags
  
  lifecycle {
    prevent_destroy = var.enable_deletion_protection
  }
}

# Access Policy para Terraform Service Principal
resource "azurerm_key_vault_access_policy" "terraform" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore",
    "Purge"
  ]

  certificate_permissions = [
    "Get",
    "List",
    "Create",
    "Delete"
  ]
}

# Access Policy para Container App Managed Identity
resource "azurerm_key_vault_access_policy" "container_app" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.container_app.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# ========================================
# SQL SERVER & DATABASE
# ========================================

# Generar password aleatorio si no se proporciona
resource "random_password" "sql_admin" {
  count   = var.sql_admin_password == "" ? 1 : 0
  length  = 32
  special = true
}

resource "azurerm_mssql_server" "main" {
  name                         = "sql-${local.resource_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password != "" ? var.sql_admin_password : random_password.sql_admin[0].result

  minimum_tls_version = "1.2"

  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = data.azurerm_client_config.current.object_id
  }

  tags = local.common_tags
}

resource "azurerm_mssql_database" "main" {
  name           = "sqldb-${local.resource_prefix}"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = var.sql_max_size_gb
  sku_name       = var.sql_database_sku
  zone_redundant = var.environment == "prod"

  tags = local.common_tags
  
  lifecycle {
    prevent_destroy = var.enable_deletion_protection && var.environment == "prod"
  }
}

# ✅ SEGURIDAD: Solo Azure Services (0.0.0.0 es especial en Azure)
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# ✅ SEGURIDAD: Solo IPs específicas permitidas
resource "azurerm_mssql_firewall_rule" "allowed_ips" {
  for_each = { for ip in var.allowed_sql_ips : ip.name => ip }
  
  name             = each.value.name
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = each.value.ip
  end_ip_address   = each.value.ip
}

# Guardar credenciales en Key Vault
resource "azurerm_key_vault_secret" "sql_admin_username" {
  name         = "sql-admin-username"
  value        = var.sql_admin_username
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.terraform]
}

resource "azurerm_key_vault_secret" "sql_admin_password" {
  name         = "sql-admin-password"
  value        = var.sql_admin_password != "" ? var.sql_admin_password : random_password.sql_admin[0].result
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.terraform]
}

resource "azurerm_key_vault_secret" "sql_connection_string" {
  name  = "sql-connection-string"
  value = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};User ID=${var.sql_admin_username};Password=${var.sql_admin_password != "" ? var.sql_admin_password : random_password.sql_admin[0].result};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.terraform]
}

# ========================================
# CONTAINER APPS
# ========================================

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${local.resource_prefix}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  tags = local.common_tags
}

resource "azurerm_container_app" "api" {
  name                         = "ca-${local.resource_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"

  # ✅ SEGURIDAD: Usar Managed Identity
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_app.id]
  }
  
  # ✅ SEGURIDAD: Usar Managed Identity para ACR (no admin credentials)
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.container_app.id
  }

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

      # ✅ SEGURIDAD: Referenciar Key Vault en lugar de secret directo
      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/sql-connection-string)"
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.enable_monitoring ? azurerm_application_insights.main[0].connection_string : ""
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

---

### 3. Crear `outputs.tf`

**Ubicación**: `terraform/outputs.tf`

**Contenido**: (Ver contenido completo en SECURITY-AUDIT-REPORT.md sección 11)

---

### 4. Crear `locals.tf`

**Ubicación**: `terraform/locals.tf`

**Contenido**:

```hcl
locals {
  # Prefix para nombres de recursos
  resource_prefix = "${var.project_name}-${var.environment}"
  
  # Tags comunes para todos los recursos
  common_tags = merge(
    var.tags,
    {
      Environment        = var.environment
      Project            = var.project_name
      ManagedBy          = "Terraform"
      CostCenter         = var.cost_center
      Owner              = var.owner_email
      CreatedDate        = formatdate("YYYY-MM-DD", timestamp())
      TerraformWorkspace = terraform.workspace
      Repository         = "api-devops"
    }
  )
  
  # Tags de seguridad
  security_tags = {
    DataClassification = var.environment == "prod" ? "Confidential" : "Internal"
    Compliance         = "GDPR"
  }
  
  # Configuraciones condicionales
  is_production = var.environment == "prod"
  
  # Configuraciones de red
  vnet_name    = "vnet-${local.resource_prefix}"
  subnet_name  = "snet-${local.resource_prefix}-apps"
}
```

---

## 🔧 MEJORAS EN SCRIPTS POWERSHELL

### Mejorar `setup-service-principal.ps1`

**Agregar al inicio del script**:

```powershell
# Validación de prerequisites
function Test-Prerequisites {
    Write-Host "Validando prerequisites..." -ForegroundColor Yellow
    $errors = @()
    
    # Azure CLI
    try {
        $azVersion = az version --output json | ConvertFrom-Json
        $minVersion = [version]"2.40.0"
        $currentVersion = [version]$azVersion.'azure-cli'
        
        if ($currentVersion -lt $minVersion) {
            $errors += "Azure CLI version >= 2.40.0 required (current: $currentVersion)"
        } else {
            Write-Host "✓ Azure CLI version $currentVersion" -ForegroundColor Green
        }
    } catch {
        $errors += "Azure CLI not installed or not in PATH"
    }
    
    # Verificar login
    try {
        $currentUser = az account show --output json 2>$null | ConvertFrom-Json
        if ($currentUser) {
            Write-Host "✓ Logged in as: $($currentUser.user.name)" -ForegroundColor Green
        } else {
            $errors += "Not logged in to Azure"
        }
    } catch {
        $errors += "Not logged in to Azure"
    }
    
    if ($errors.Count -gt 0) {
        Write-Host ""
        Write-Host "✗ Prerequisites check failed:" -ForegroundColor Red
        $errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        Write-Host ""
        exit 1
    }
    
    Write-Host "✓ All prerequisites met" -ForegroundColor Green
    Write-Host ""
}

Test-Prerequisites
```

**Reemplazar la sección de guardar credenciales**:

```powershell
# OPCIÓN SEGURA: Guardar en Key Vault en lugar de archivo
Write-Host "¿Deseas guardar las credenciales en Azure Key Vault? (s/n)" -ForegroundColor Yellow
$saveToKv = Read-Host

if ($saveToKv -eq 's' -or $saveToKv -eq 'S') {
    $kvName = Read-Host "Nombre del Key Vault (debe existir)"
    
    Write-Host "Guardando credenciales en Key Vault: $kvName" -ForegroundColor Yellow
    
    az keyvault secret set --vault-name $kvName --name "terraform-client-id" --value $sp.appId --output none
    az keyvault secret set --vault-name $kvName --name "terraform-client-secret" --value $sp.password --output none
    az keyvault secret set --vault-name $kvName --name "terraform-tenant-id" --value $sp.tenant --output none
    az keyvault secret set --vault-name $kvName --name "terraform-subscription-id" --value $currentSub.id --output none
    
    Write-Host "✓ Credenciales guardadas en Key Vault" -ForegroundColor Green
} else {
    # Guardar en archivo CON advertencia
    $outputFile = "terraform-sp-credentials-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    # ... código existente ...
    
    Write-Host "⚠️  IMPORTANTE: Este archivo se debe eliminar después de configurar las variables de entorno" -ForegroundColor Red
    Write-Host "⚠️  Archivo creado: $outputFile" -ForegroundColor Yellow
}
```

---

### Mejorar `setup-azure-backend.ps1`

**Generar nombre único para Storage Account**:

```powershell
# Generar nombre único basado en subscription ID
$subscriptionHash = ($currentSub.id -replace '-', '').Substring(0, 8).ToLower()
$storageAccountName = "tfstate$subscriptionHash"

# Validar disponibilidad del nombre
Write-Host "Validando disponibilidad del nombre: $storageAccountName" -ForegroundColor Yellow
$checkResult = az storage account check-name --name $storageAccountName --output json | ConvertFrom-Json

if ($checkResult.nameAvailable -eq $false) {
    Write-Host "⚠️  Nombre no disponible: $($checkResult.reason)" -ForegroundColor Yellow
    
    # Agregar sufijo aleatorio
    $randomSuffix = -join ((97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
    $storageAccountName = "tfstate$subscriptionHash$randomSuffix"
    
    Write-Host "Usando nombre alternativo: $storageAccountName" -ForegroundColor Cyan
}

Write-Host "✓ Nombre validado: $storageAccountName" -ForegroundColor Green
```

**Agregar configuración de RBAC para Azure AD auth**:

```powershell
# Después de crear el storage account y container

Write-Host ""
Write-Host "Configurando Azure AD authentication para Terraform..." -ForegroundColor Yellow

# Obtener el Service Principal object ID
if ($env:ARM_CLIENT_ID) {
    $spObjectId = az ad sp show --id $env:ARM_CLIENT_ID --query id -o tsv 2>$null
    
    if ($spObjectId) {
        # Dar permisos de Storage Blob Data Contributor
        az role assignment create `
            --role "Storage Blob Data Contributor" `
            --assignee $spObjectId `
            --scope "/subscriptions/$SubscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Storage/storageAccounts/$storageAccountName" `
            --output none
        
        Write-Host "✓ Configurado RBAC para Service Principal" -ForegroundColor Green
    } else {
        Write-Host "⚠️  No se pudo obtener el Service Principal. Configura RBAC manualmente." -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  Variable ARM_CLIENT_ID no encontrada. Ejecuta configure-terraform-env.ps1 primero." -ForegroundColor Yellow
}

# Actualizar instrucciones finales
Write-Host ""
Write-Host "IMPORTANTE: Actualiza providers.tf con:" -ForegroundColor Cyan
Write-Host "  storage_account_name = `"$storageAccountName`"" -ForegroundColor White
Write-Host "  use_azuread_auth = true" -ForegroundColor White
```

---

## 📁 ARCHIVOS DE AMBIENTE

### Crear `environments/dev.tfvars.example`

```hcl
# ========================================
# EJEMPLO: Configuración de Desarrollo
# ========================================
# INSTRUCCIONES:
# 1. Copiar este archivo: cp dev.tfvars.example dev.tfvars
# 2. Editar dev.tfvars con valores reales
# 3. NUNCA commitear dev.tfvars a Git (está en .gitignore)

# General
environment  = "dev"
location     = "East US"
project_name = "api-devops"

# Owner
owner_email = "YOUR_EMAIL@example.com"  # CAMBIAR

# Container Registry
acr_sku = "Basic"

# Container Apps
container_app_min_replicas = 1
container_app_max_replicas = 3
container_cpu              = 0.5
container_memory           = "1Gi"

# SQL Database
sql_admin_username = "sqladmin"
sql_database_sku   = "Basic"
sql_max_size_gb    = 2

# ⚠️ SEGURIDAD: sql_admin_password NO debe estar aquí
# Configurar como variable de entorno:
# $env:TF_VAR_sql_admin_password = "YourStrongP@ssw0rd!"

# IPs permitidas para SQL (EJEMPLO - usar tus IPs reales)
allowed_sql_ips = [
  {
    name = "Office"
    ip   = "YOUR_IP_HERE"  # CAMBIAR
  }
]

# Key Vault
key_vault_allowed_ips = ["YOUR_IP_HERE"]  # CAMBIAR
key_vault_purge_protection = false  # En dev, permitir purge para testing

# Features
enable_deletion_protection = false  # En dev, permitir destroy fácil
enable_monitoring          = true

# Tags
tags = {
  Environment = "Development"
  Project     = "API DevOps"
  ManagedBy   = "Terraform"
}
```

### Crear `environments/prod.tfvars.example`

```hcl
# ========================================
# EJEMPLO: Configuración de Producción
# ========================================

environment  = "prod"
location     = "East US"
project_name = "api-devops"

owner_email = "YOUR_EMAIL@example.com"

# Container Registry
acr_sku = "Standard"  # Mejor performance que Basic

# Container Apps
container_app_min_replicas = 2   # Alta disponibilidad
container_app_max_replicas = 10
container_cpu              = 1.0  # Más recursos
container_memory           = "2Gi"

# SQL Database
sql_admin_username = "sqladmin"
sql_database_sku   = "S2"  # Mejor que Basic
sql_max_size_gb    = 10

# IPs permitidas - Solo las necesarias
allowed_sql_ips = []  # Container App usa Azure Services

# Key Vault
key_vault_allowed_ips      = []  # Deny all, solo Azure Services
key_vault_purge_protection = true  # ⚠️ CRÍTICO en producción

# Features
enable_deletion_protection = true   # ⚠️ CRÍTICO en producción
enable_monitoring          = true

# Tags
tags = {
  Environment = "Production"
  Project     = "API DevOps"
  ManagedBy   = "Terraform"
  CostCenter  = "Engineering"
}
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### Paso 1: Archivos Core
- [ ] Crear `variables.tf`
- [ ] Crear `main.tf`
- [ ] Crear `outputs.tf`
- [ ] Crear `locals.tf`
- [ ] Verificar sintaxis: `terraform fmt`
- [ ] Validar: `terraform validate`

### Paso 2: Archivos de Ambiente
- [ ] Crear `environments/dev.tfvars.example`
- [ ] Crear `environments/prod.tfvars.example`
- [ ] Copiar a `.tfvars` real y editar valores
- [ ] Configurar `sql_admin_password` como variable de entorno

### Paso 3: Mejoras en Scripts
- [ ] Actualizar `setup-service-principal.ps1`
- [ ] Actualizar `setup-azure-backend.ps1`
- [ ] Probar scripts en ambiente limpio

### Paso 4: Backend Remoto
- [ ] Ejecutar `setup-azure-backend.ps1`
- [ ] Actualizar `providers.tf` con nombre de storage
- [ ] Descomentar backend en `providers.tf`
- [ ] Ejecutar `terraform init -migrate-state`
- [ ] Verificar que `.tfstate` local se eliminó

### Paso 5: Primera Ejecución
- [ ] `terraform init`
- [ ] `terraform plan -var-file="environments/dev.tfvars"`
- [ ] Revisar plan cuidadosamente
- [ ] `terraform apply -var-file="environments/dev.tfvars"`

### Paso 6: Verificación
- [ ] Verificar recursos en Azure Portal
- [ ] Verificar secrets en Key Vault
- [ ] Probar conectividad a SQL
- [ ] Verificar Container App (cuando se despliegue imagen)
- [ ] Revisar Application Insights

---

## 🚀 COMANDOS ÚTILES

### Inicialización
```powershell
# Primera vez
terraform init

# Después de cambios en backend
terraform init -reconfigure

# Migrar state
terraform init -migrate-state
```

### Planning y Apply
```powershell
# Variables de entorno (ANTES de plan/apply)
$env:TF_VAR_sql_admin_password = "YourStrongP@ssw0rd!"
$env:TF_VAR_owner_email = "your.email@example.com"

# Plan para dev
terraform plan -var-file="environments/dev.tfvars" -out=tfplan

# Apply
terraform apply tfplan

# Apply directo (solo en dev)
terraform apply -var-file="environments/dev.tfvars" -auto-approve

# Destruir (¡CUIDADO!)
terraform destroy -var-file="environments/dev.tfvars"
```

### Outputs
```powershell
# Ver todos los outputs
terraform output

# Ver output específico
terraform output container_app_url

# Ver en JSON
terraform output -json > outputs.json
```

### Troubleshooting
```powershell
# Ver state
terraform show

# Listar resources
terraform state list

# Ver resource específico
terraform state show azurerm_resource_group.main

# Refrescar state
terraform refresh -var-file="environments/dev.tfvars"
```

---

## 📚 PRÓXIMOS PASOS

1. **Implementar archivos** según esta guía
2. **Probar en ambiente dev** primero
3. **Validar con herramientas** (tfsec, checkov)
4. **Documentar cualquier cambio** en README
5. **Crear PR** con los cambios
6. **Deploy a producción** después de validación

---

**Documento creado**: Noviembre 27, 2025  
**Relacionado con**: SECURITY-AUDIT-REPORT.md  
**Siguiente paso**: Implementar variables.tf y main.tf
