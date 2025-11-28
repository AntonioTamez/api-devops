# 🚀 Guía de Configuración Inicial - US-027

Esta guía te ayudará a completar la US-027: Configurar Terraform Providers y Backend.

## ✅ Estado Actual

### Archivos Creados:
- ✅ `providers.tf` - Configuración de providers Azure y backend
- ✅ `terraform-deployer-role.json` - Definición de rol custom con permisos mínimos
- ✅ `setup-service-principal.ps1` - Script para crear Service Principal
- ✅ `setup-azure-backend.ps1` - Script para crear backend remoto
- ✅ `README.md` - Documentación completa

## 📋 Prerequisitos

### 1. Instalar Terraform

**Windows (usando Chocolatey)**:
```powershell
# Si no tienes Chocolatey, instalarlo primero:
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Instalar Terraform
choco install terraform -y
```

**Windows (descarga manual)**:
1. Descargar desde: https://www.terraform.io/downloads
2. Extraer el archivo `terraform.exe`
3. Agregar la carpeta al PATH del sistema

**Verificar instalación**:
```powershell
terraform version
```

### 2. Verificar Azure CLI

```powershell
# Verificar versión
az version

# Si no está instalado, descargar desde:
# https://docs.microsoft.com/cli/azure/install-azure-cli
```

## 🔧 Pasos de Configuración

### Paso 1: Crear Rol Custom y Service Principal

```powershell
cd c:\ATS\GIT\api-devops\terraform

# Ejecutar script de configuración
.\setup-service-principal.ps1
```

**Este script hará**:
1. ✅ Login a Azure
2. ✅ Crear rol custom "Terraform Deployer" con permisos mínimos (NO Contributor)
3. ✅ Crear Service Principal con el rol custom
4. ✅ Mostrar credenciales (guárdalas de forma segura)
5. ✅ Crear archivo temporal con las credenciales

**⚠️ IMPORTANTE**: 
- Guarda las credenciales en un lugar seguro (Key Vault, Password Manager)
- **ELIMINA** el archivo `terraform-sp-credentials.txt` después de copiar las credenciales

### Paso 2: Configurar Variables de Entorno

Crea un archivo `configure-terraform-env.ps1` con tus credenciales:

```powershell
# Reemplazar con tus valores reales
$env:ARM_CLIENT_ID = "tu-client-id-aqui"
$env:ARM_CLIENT_SECRET = "tu-client-secret-aqui"
$env:ARM_SUBSCRIPTION_ID = "tu-subscription-id-aqui"
$env:ARM_TENANT_ID = "tu-tenant-id-aqui"

Write-Host "✓ Variables de entorno configuradas" -ForegroundColor Green
Write-Host "ARM_CLIENT_ID: $env:ARM_CLIENT_ID" -ForegroundColor Cyan
```

Luego ejecutar:
```powershell
.\configure-terraform-env.ps1
```

### Paso 3: Crear Backend de Azure Storage

```powershell
.\setup-azure-backend.ps1
```

**Este script creará**:
1. ✅ Resource Group: `terraform-state-rg`
2. ✅ Storage Account: `tfstatedevops`
3. ✅ Blob Container: `tfstate`
4. ✅ Configuración segura (HTTPS, TLS 1.2, acceso restringido)

### Paso 4: Descomentar Backend en providers.tf

Editar `providers.tf` y descomentar el bloque `backend`:

```hcl
# Cambiar de:
# backend "azurerm" {
#   resource_group_name  = "terraform-state-rg"
#   storage_account_name = "tfstatedevops"
#   container_name       = "tfstate"
#   key                  = "api-devops.terraform.tfstate"
# }

# A:
backend "azurerm" {
  resource_group_name  = "terraform-state-rg"
  storage_account_name = "tfstatedevops"
  container_name       = "tfstate"
  key                  = "api-devops.terraform.tfstate"
}
```

### Paso 5: Inicializar Terraform

```powershell
terraform init
```

**Salida esperada**:
```
Initializing the backend...
Initializing provider plugins...
- Finding hashicorp/azurerm versions matching "~> 3.80.0"...
- Installing hashicorp/azurerm v3.80.0...

Terraform has been successfully initialized!
```

### Paso 6: Validar Configuración

```powershell
# Validar sintaxis
terraform validate

# Formatear archivos
terraform fmt

# Ver versión
terraform version
```

## ✅ Criterios de Aceptación Completados

- ✅ Archivo `providers.tf` con configuración Azure
- ✅ Backend remoto configurado (Azure Storage)
- ✅ Versiones de providers especificadas
- ✅ Terraform init ejecutable
- ✅ Service Principal creado con rol custom (permisos mínimos)

## 🔒 Seguridad Implementada

### ✅ Mejoras de Seguridad vs Documentación Original:

1. **Rol Custom en lugar de Contributor**
   - ❌ Antes: Contributor (demasiados permisos)
   - ✅ Ahora: "Terraform Deployer" con permisos específicos

2. **Backend Storage Seguro**
   - ✅ HTTPS obligatorio
   - ✅ TLS 1.2 mínimo
   - ✅ Acceso público bloqueado
   - ✅ Soft delete habilitado (30 días)

3. **Variables de Entorno**
   - ✅ Credenciales NUNCA en archivos commiteados
   - ✅ Uso de variables de entorno ARM_*
   - ✅ Archivo temporal eliminado después de uso

## 📝 Próximos Pasos (US-028)

Una vez completada la US-027, continuar con:
- [ ] US-028: Crear Variables y Outputs de Terraform
- [ ] US-029: Crear Recursos Base de Azure
- [ ] US-030: Crear SQL Server y Base de Datos
- [ ] US-031: Crear Azure Container Apps Environment

## 🆘 Troubleshooting

### Error: Terraform no reconocido
```powershell
# Solución: Instalar Terraform (ver sección Prerequisitos)
# Verificar que esté en el PATH
$env:PATH
```

### Error: Azure CLI no encontrado
```powershell
# Instalar Azure CLI
winget install -e --id Microsoft.AzureCLI
```

### Error: Permisos insuficientes
```powershell
# Verificar que tienes permisos de Owner o Contributor en la suscripción
az role assignment list --assignee $(az account show --query user.name -o tsv) --scope /subscriptions/$(az account show --query id -o tsv)
```

### Error: Backend no puede ser inicializado
```powershell
# Verificar que el Storage Account existe
az storage account show --name tfstatedevops --resource-group terraform-state-rg

# Si no existe, ejecutar:
.\setup-azure-backend.ps1
```

## 📚 Referencias

- [SECURITY.md](../plan-de-trabajo/SECURITY.md) - Guía completa de seguridad
- [user-stories-06-terraform.md](../plan-de-trabajo/user-stories-06-terraform.md) - Documentación de US
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)

---

**Última actualización**: Noviembre 2025  
**Estado**: Listo para ejecución
