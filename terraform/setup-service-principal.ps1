# Script para crear Service Principal para Terraform
# El Service Principal es la identidad que Terraform usará para crear recursos en Azure

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePrincipalName = "sp-api-devops-terraform"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Service Principal Setup para Terraform" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Azure CLI
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "OK: Azure CLI detectado" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Azure CLI no esta instalado" -ForegroundColor Red
    exit 1
}

# Login
Write-Host ""
Write-Host "Iniciando sesión en Azure..." -ForegroundColor Yellow
az login

# Seleccionar suscripción
if ($SubscriptionId) {
    az account set --subscription $SubscriptionId
} else {
    Write-Host ""
    Write-Host "Suscripciones disponibles:" -ForegroundColor Yellow
    az account list --output table
    Write-Host ""
    $SubscriptionId = Read-Host "Ingresa el ID de la suscripción"
    az account set --subscription $SubscriptionId
}

$currentSub = az account show --output json | ConvertFrom-Json
Write-Host "OK: Usando suscripcion: $($currentSub.name)" -ForegroundColor Green
Write-Host "  Subscription ID: $($currentSub.id)" -ForegroundColor White
Write-Host ""

# Generar hash único de subscription para nombres de recursos
$subscriptionHash = ($currentSub.id -replace '-', '').Substring(0, 8).ToLower()

# Crear rol custom si no existe
Write-Host "Verificando rol custom 'Terraform Deployer'..." -ForegroundColor Yellow
$roleExists = az role definition list --name "Terraform Deployer" --output json | ConvertFrom-Json

if ($roleExists.Length -eq 0) {
    Write-Host "Creando rol custom 'Terraform Deployer'..." -ForegroundColor Yellow
    
    # Actualizar SUBSCRIPTION_ID en el archivo JSON
    $roleDefinition = Get-Content "terraform-deployer-role.json" -Raw
    $roleDefinition = $roleDefinition -replace '\{SUBSCRIPTION_ID\}', $currentSub.id
    $roleDefinition | Out-File "terraform-deployer-role-temp.json" -Encoding UTF8
    
    az role definition create --role-definition "terraform-deployer-role-temp.json"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Rol custom creado exitosamente" -ForegroundColor Green
        Remove-Item "terraform-deployer-role-temp.json"
    } else {
        Write-Host "ERROR: Error al crear rol custom" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "OK: Rol custom ya existe" -ForegroundColor Green
}

Write-Host ""

# Crear Service Principal con rol custom
Write-Host "Creando Service Principal: $ServicePrincipalName" -ForegroundColor Yellow
Write-Host "NOTA: Usando rol custom 'Terraform Deployer' (permisos minimos)" -ForegroundColor Cyan
Write-Host "(Esto puede tomar un momento...)" -ForegroundColor Gray
Write-Host ""

$sp = az ad sp create-for-rbac `
    --name $ServicePrincipalName `
    --role "Terraform Deployer" `
    --scopes "/subscriptions/$($currentSub.id)" `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK: Service Principal creado exitosamente con rol custom" -ForegroundColor Green
} else {
    Write-Host "ERROR: Error al crear Service Principal" -ForegroundColor Red
    exit 1
}

# Mostrar información
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "OK: Configuracion completada" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: Guarda esta información de forma segura" -ForegroundColor Yellow
Write-Host "Esta información NO se puede recuperar después" -ForegroundColor Red
Write-Host ""
Write-Host "Credenciales del Service Principal:" -ForegroundColor Cyan
Write-Host "-----------------------------------" -ForegroundColor Gray
Write-Host "App ID (Client ID):     $($sp.appId)" -ForegroundColor White
Write-Host "Password (Client Secret): $($sp.password)" -ForegroundColor White
Write-Host "Tenant ID:              $($sp.tenant)" -ForegroundColor White
Write-Host "Subscription ID:        $($currentSub.id)" -ForegroundColor White
Write-Host ""

# ========================================
# GUARDAR CREDENCIALES DE FORMA SEGURA
# ========================================

Write-Host ""
Write-Host "¿Cómo deseas guardar las credenciales?" -ForegroundColor Yellow
Write-Host "1. En Azure Key Vault (RECOMENDADO - Más seguro)" -ForegroundColor Green
Write-Host "2. En archivo local temporal (requiere eliminación manual)" -ForegroundColor Yellow
Write-Host ""
$saveOption = Read-Host "Selecciona una opción (1 o 2)"

if ($saveOption -eq "1") {
    # OPCIÓN SEGURA: Guardar en Key Vault
    Write-Host ""
    Write-Host "Buscando Key Vaults en la suscripción..." -ForegroundColor Yellow
    
    $keyVaults = az keyvault list --output json | ConvertFrom-Json
    
    if ($keyVaults.Count -eq 0) {
        Write-Host "ADVERTENCIA: No se encontraron Key Vaults en la suscripcion" -ForegroundColor Yellow
        Write-Host "Creando Key Vault para Terraform..." -ForegroundColor Yellow
        
        $kvName = "kv-terraform-sp-$subscriptionHash"
        $kvRg = "rg-terraform-secrets"
        
        # Crear resource group si no existe
        az group create --name $kvRg --location eastus --output none
        
        # Crear Key Vault
        az keyvault create `
            --name $kvName `
            --resource-group $kvRg `
            --location eastus `
            --enable-rbac-authorization false `
            --output none
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK: Key Vault creado: $kvName" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Error al crear Key Vault" -ForegroundColor Red
            $saveOption = "2"  # Fallback a archivo
        }
    } else {
        # Mostrar Key Vaults disponibles
        Write-Host "Key Vaults disponibles:" -ForegroundColor Cyan
        $keyVaults | ForEach-Object { Write-Host "  - $($_.name)" -ForegroundColor White }
        Write-Host ""
        $kvName = Read-Host "Nombre del Key Vault a usar"
    }
    
    if ($kvName) {
        Write-Host "Guardando credenciales en Key Vault: $kvName" -ForegroundColor Yellow
        
        az keyvault secret set --vault-name $kvName --name "terraform-client-id" --value $sp.appId --output none
        az keyvault secret set --vault-name $kvName --name "terraform-client-secret" --value $sp.password --output none
        az keyvault secret set --vault-name $kvName --name "terraform-tenant-id" --value $sp.tenant --output none
        az keyvault secret set --vault-name $kvName --name "terraform-subscription-id" --value $currentSub.id --output none
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK: Credenciales guardadas en Key Vault" -ForegroundColor Green
            Write-Host ""
            Write-Host "Para recuperar las credenciales:" -ForegroundColor Cyan
            Write-Host "`$env:ARM_CLIENT_ID = `$(az keyvault secret show --vault-name $kvName --name terraform-client-id --query value -o tsv)" -ForegroundColor Gray
            Write-Host "`$env:ARM_CLIENT_SECRET = `$(az keyvault secret show --vault-name $kvName --name terraform-client-secret --query value -o tsv)" -ForegroundColor Gray
            Write-Host "`$env:ARM_TENANT_ID = `$(az keyvault secret show --vault-name $kvName --name terraform-tenant-id --query value -o tsv)" -ForegroundColor Gray
            Write-Host "`$env:ARM_SUBSCRIPTION_ID = `$(az keyvault secret show --vault-name $kvName --name terraform-subscription-id --query value -o tsv)" -ForegroundColor Gray
        } else {
            Write-Host "ERROR: Error al guardar en Key Vault. Usando archivo local..." -ForegroundColor Red
            $saveOption = "2"
        }
    }
}

if ($saveOption -eq "2" -or $saveOption -ne "1") {
    # OPCIÓN ARCHIVO: Guardar en archivo temporal con timestamp
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $outputFile = "terraform-sp-credentials-$timestamp.txt"
    
    $content = @"
# ========================================
# Credenciales del Service Principal para Terraform
# ========================================
# ADVERTENCIA: Este archivo contiene informacion MUY SENSIBLE
# ADVERTENCIA: Copia estas credenciales a un lugar seguro y ELIMINA este archivo INMEDIATAMENTE
# ADVERTENCIA: NO commitear este archivo a Git
# ========================================

ARM_CLIENT_ID=$($sp.appId)
ARM_CLIENT_SECRET=$($sp.password)
ARM_SUBSCRIPTION_ID=$($currentSub.id)
ARM_TENANT_ID=$($sp.tenant)

# ========================================
# CONFIGURAR VARIABLES DE ENTORNO
# ========================================

# Windows PowerShell:
`$env:ARM_CLIENT_ID="$($sp.appId)"
`$env:ARM_CLIENT_SECRET="$($sp.password)"
`$env:ARM_SUBSCRIPTION_ID="$($currentSub.id)"
`$env:ARM_TENANT_ID="$($sp.tenant)"

# Linux/macOS:
export ARM_CLIENT_ID="$($sp.appId)"
export ARM_CLIENT_SECRET="$($sp.password)"
export ARM_SUBSCRIPTION_ID="$($currentSub.id)"
export ARM_TENANT_ID="$($sp.tenant)"

# ========================================
# PARA GITHUB ACTIONS / AZURE DEVOPS
# ========================================
# Crear estos secretos en tu repositorio:

AZURE_CLIENT_ID: $($sp.appId)
AZURE_CLIENT_SECRET: $($sp.password)
AZURE_SUBSCRIPTION_ID: $($currentSub.id)
AZURE_TENANT_ID: $($sp.tenant)

# ========================================
# PARA GUARDAR EN KEY VAULT (RECOMENDADO)
# ========================================

az keyvault secret set --vault-name <KV_NAME> --name "terraform-client-id" --value "$($sp.appId)"
az keyvault secret set --vault-name <KV_NAME> --name "terraform-client-secret" --value "$($sp.password)"
az keyvault secret set --vault-name <KV_NAME> --name "terraform-tenant-id" --value "$($sp.tenant)"
az keyvault secret set --vault-name <KV_NAME> --name "terraform-subscription-id" --value "$($currentSub.id)"
"@

    $content | Out-File -FilePath $outputFile -Encoding UTF8
    
    Write-Host ""
    Write-Host "ADVERTENCIA: Credenciales guardadas en archivo: $outputFile" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Proximos pasos:" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Copia las credenciales a un lugar seguro" -ForegroundColor White
Write-Host "   - Key Vault (recomendado)" -ForegroundColor Gray
Write-Host "   - Password Manager" -ForegroundColor Gray
Write-Host "   - GitHub/Azure DevOps Secrets" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Configura las variables de entorno:" -ForegroundColor White
Write-Host "   .\configure-terraform-env.ps1" -ForegroundColor Cyan
Write-Host ""
if ($saveOption -eq "2") {
    Write-Host "3. ELIMINA el archivo: $outputFile" -ForegroundColor Red
    Write-Host "   Remove-Item `"$outputFile`" -Force" -ForegroundColor Gray
    Write-Host ""
}
Write-Host "4. Configura el backend de Terraform:" -ForegroundColor White
Write-Host "   .\setup-azure-backend.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "5. Para CI/CD, guarda las credenciales como secretos" -ForegroundColor White
Write-Host ""
