# Script para configurar Azure Backend para Terraform
# Este script crea el Resource Group y Storage Account para almacenar el Terraform state

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Azure Terraform Backend Setup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Azure CLI está instalado
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "OK: Azure CLI version $($azVersion.'azure-cli') detectado" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Azure CLI no esta instalado. Por favor instala Azure CLI primero." -ForegroundColor Red
    Write-Host "Descarga desde: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Login a Azure
Write-Host ""
Write-Host "Iniciando sesión en Azure..." -ForegroundColor Yellow
az login

# Seleccionar suscripción
if ($SubscriptionId) {
    Write-Host "Configurando suscripción: $SubscriptionId" -ForegroundColor Yellow
    az account set --subscription $SubscriptionId
} else {
    Write-Host ""
    Write-Host "Suscripciones disponibles:" -ForegroundColor Yellow
    az account list --output table
    Write-Host ""
    $SubscriptionId = Read-Host "Ingresa el ID de la suscripción a usar"
    az account set --subscription $SubscriptionId
}

$currentSub = az account show --output json | ConvertFrom-Json
Write-Host "OK: Usando suscripcion: $($currentSub.name)" -ForegroundColor Green

# ========================================
# CONFIGURACIÓN
# ========================================

# Resource Group y Container (fijos)
$resourceGroupName = "terraform-state-rg"
$containerName = "tfstate"

# Storage Account con nombre único basado en subscription
# Esto previene conflictos de nombres globalmente únicos en Azure
$subscriptionHash = ($currentSub.id -replace '-', '').Substring(0, 8).ToLower()
$storageAccountName = "tfstate$subscriptionHash"

# Validar disponibilidad del nombre
Write-Host "Validando disponibilidad del nombre: $storageAccountName" -ForegroundColor Yellow
$checkResult = az storage account check-name --name $storageAccountName --output json | ConvertFrom-Json

if ($checkResult.nameAvailable -eq $false) {
    Write-Host "ADVERTENCIA: Nombre no disponible: $($checkResult.reason)" -ForegroundColor Yellow
    Write-Host "ADVERTENCIA: Motivo: $($checkResult.message)" -ForegroundColor Yellow
    
    # Agregar sufijo aleatorio de 4 letras
    $randomSuffix = -join ((97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
    $storageAccountName = "tfstate$subscriptionHash$randomSuffix"
    
    Write-Host "Usando nombre alternativo: $storageAccountName" -ForegroundColor Cyan
    
    # Validar nuevamente
    $checkResult = az storage account check-name --name $storageAccountName --output json | ConvertFrom-Json
    if ($checkResult.nameAvailable -eq $false) {
        Write-Host "ERROR: No se pudo generar un nombre disponible. Intenta nuevamente." -ForegroundColor Red
        exit 1
    }
}

Write-Host "OK: Nombre validado y disponible: $storageAccountName" -ForegroundColor Green

Write-Host ""
Write-Host "Configuración:" -ForegroundColor Cyan
Write-Host "  Resource Group: $resourceGroupName" -ForegroundColor White
Write-Host "  Storage Account: $storageAccountName" -ForegroundColor White
Write-Host "  Container: $containerName" -ForegroundColor White
Write-Host "  Location: $Location" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "¿Continuar con la creación? (s/n)"
if ($confirm -ne 's' -and $confirm -ne 'S') {
    Write-Host "Operación cancelada." -ForegroundColor Yellow
    exit 0
}

# Crear Resource Group
Write-Host ""
Write-Host "Creando Resource Group..." -ForegroundColor Yellow
az group create --name $resourceGroupName --location $Location
if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Resource Group creado exitosamente" -ForegroundColor Green
} else {
    Write-Host "ERROR: Error al crear Resource Group" -ForegroundColor Red
    exit 1
}

# Crear Storage Account
Write-Host ""
Write-Host "Creando Storage Account..." -ForegroundColor Yellow
Write-Host "(Esto puede tomar un momento...)" -ForegroundColor Gray
az storage account create `
    --name $storageAccountName `
    --resource-group $resourceGroupName `
    --location $Location `
    --sku Standard_LRS `
    --encryption-services blob `
    --https-only true `
    --min-tls-version TLS1_2

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Storage Account creado exitosamente" -ForegroundColor Green
} else {
    Write-Host "ERROR: Error al crear Storage Account" -ForegroundColor Red
    exit 1
}

# Crear Container
Write-Host ""
Write-Host "Creando Blob Container..." -ForegroundColor Yellow
az storage container create `
    --name $containerName `
    --account-name $storageAccountName

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Blob Container creado exitosamente" -ForegroundColor Green
} else {
    Write-Host "ERROR: Error al crear Blob Container" -ForegroundColor Red
    exit 1
}

# ========================================
# CONFIGURAR RBAC PARA AZURE AD AUTH
# ========================================

Write-Host ""
Write-Host "Configurando Azure AD authentication para Terraform..." -ForegroundColor Yellow

# Intentar obtener el Service Principal si las variables de entorno están configuradas
if ($env:ARM_CLIENT_ID) {
    try {
        $spObjectId = az ad sp show --id $env:ARM_CLIENT_ID --query id -o tsv 2>$null
        
        if ($spObjectId) {
            Write-Host "Service Principal detectado: $($env:ARM_CLIENT_ID)" -ForegroundColor Gray
            
            # Dar permisos de Storage Blob Data Contributor
            Write-Host "Asignando rol 'Storage Blob Data Contributor'..." -ForegroundColor Yellow
            az role assignment create `
                --role "Storage Blob Data Contributor" `
                --assignee $spObjectId `
                --scope "/subscriptions/$SubscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Storage/storageAccounts/$storageAccountName" `
                --output none 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "OK: RBAC configurado para Service Principal" -ForegroundColor Green
            } else {
                Write-Host "ADVERTENCIA: No se pudo configurar RBAC automaticamente" -ForegroundColor Yellow
            }
        }
    } catch {
        Write-Host "ADVERTENCIA: No se pudo obtener el Service Principal" -ForegroundColor Yellow
    }
} else {
    Write-Host "ADVERTENCIA: Variable ARM_CLIENT_ID no encontrada" -ForegroundColor Yellow
    Write-Host "    Ejecuta configure-terraform-env.ps1 primero o configura RBAC manualmente:" -ForegroundColor Gray
    Write-Host "    az role assignment create --role 'Storage Blob Data Contributor' --assignee SP_OBJECT_ID --scope /subscriptions/$SubscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Storage/storageAccounts/$storageAccountName" -ForegroundColor Gray
}

# ========================================
# RESUMEN Y PRÓXIMOS PASOS
# ========================================

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "OK: Configuracion completada" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recursos creados:" -ForegroundColor Cyan
Write-Host "  Resource Group: $resourceGroupName" -ForegroundColor White
Write-Host "  Storage Account: $storageAccountName" -ForegroundColor White
Write-Host "  Container: $containerName" -ForegroundColor White
Write-Host "  Location: $Location" -ForegroundColor White
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Actualizar providers.tf con esta configuración:" -ForegroundColor White
Write-Host ""
Write-Host "   backend `"azurerm`" {" -ForegroundColor Cyan
Write-Host "     resource_group_name  = `"$resourceGroupName`"" -ForegroundColor Cyan
Write-Host "     storage_account_name = `"$storageAccountName`"" -ForegroundColor Cyan
Write-Host "     container_name       = `"$containerName`"" -ForegroundColor Cyan
Write-Host "     key                  = `"api-devops.terraform.tfstate`"" -ForegroundColor Cyan
Write-Host "     use_azuread_auth     = true" -ForegroundColor Green
Write-Host "   }" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Descomentar el bloque 'backend' en providers.tf" -ForegroundColor White
Write-Host ""
Write-Host "3. Inicializar Terraform:" -ForegroundColor White
Write-Host "   terraform init -migrate-state" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Cuando se solicite, confirma la migración del state al backend remoto" -ForegroundColor White
Write-Host ""
Write-Host "OK: SEGURIDAD - El state estara encriptado y respaldado en Azure Storage" -ForegroundColor Green
Write-Host ""
