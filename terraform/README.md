# Terraform - Infraestructura como Código (IaC)

Este directorio contiene la infraestructura como código para provisionar recursos en Azure.

## 📋 Prerequisitos

- **Azure CLI** instalado ([Descargar](https://docs.microsoft.com/cli/azure/install-azure-cli))
- **Terraform** >= 1.5.0 instalado ([Descargar](https://www.terraform.io/downloads))
- **Suscripción de Azure** activa
- **Permisos de Contributor** en la suscripción

## 🚀 Configuración Inicial (Primera vez)

### Paso 1: Crear Service Principal

El Service Principal es la identidad que Terraform usará para crear recursos en Azure.

```powershell
# Ejecutar desde el directorio terraform/
.\setup-service-principal.ps1
```

Este script creará un Service Principal y mostrará las credenciales. **GUÁRDALAS DE FORMA SEGURA**.

### Paso 2: Configurar Variables de Entorno

Configura las credenciales del Service Principal en tu sesión actual:

```powershell
.\configure-terraform-env.ps1
```

### Paso 3: Crear Backend para Terraform State

El estado de Terraform se almacenará en Azure Storage para trabajo colaborativo:

```powershell
.\setup-azure-backend.ps1
```

### Paso 4: Descomentar el Backend en providers.tf

Una vez creado el Storage Account, edita `providers.tf` y descomenta el bloque `backend`:

```hcl
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

## 🏗️ Paso a Paso Completo: Crear la Infraestructura

Esta sección te guía paso a paso desde cero hasta tener tu infraestructura desplegada en Azure.

### ✅ Pre-requisitos Verificados

Antes de comenzar, asegúrate de tener:

```powershell
# Verificar Azure CLI
az --version  # Mínimo: 2.40.0

# Verificar Terraform
terraform --version  # Mínimo: 1.5.0

# Login a Azure
az login

# Verificar suscripción activa
az account show
```

---

### 📝 PASO 1: Crear Service Principal

```powershell
cd c:\ATS\GIT\api-devops\terraform

# Ejecutar script de creación
.\setup-service-principal.ps1
```

**Durante la ejecución**:
1. Selecciona tu suscripción de Azure
2. El script mostrará las credenciales (App ID, Password, Tenant ID, Subscription ID)
3. **OPCIÓN RECOMENDADA**: Selecciona "1" para guardar en Key Vault
4. **Opción alternativa**: Selecciona "2" para archivo temporal

**⚠️ IMPORTANTE**: Si elegiste opción 2, **elimina el archivo** después de copiar las credenciales.

---

### 📝 PASO 2: Configurar Variables de Entorno

```powershell
# Opción A: Usar el script (recomendado)
.\configure-terraform-env.ps1

# Opción B: Configurar manualmente
$env:ARM_CLIENT_ID = "tu-app-id-aqui"
$env:ARM_CLIENT_SECRET = "tu-password-aqui"
$env:ARM_TENANT_ID = "tu-tenant-id-aqui"
$env:ARM_SUBSCRIPTION_ID = "tu-subscription-id-aqui"

# Verificar
echo $env:ARM_CLIENT_ID
```

**Nota**: Estas variables son para la sesión actual. Para hacerlas permanentes, agrégalas a tu perfil de PowerShell.

---

### 📝 PASO 3: Crear Backend Remoto

```powershell
# Ejecutar script de backend
.\setup-azure-backend.ps1
```

**El script hará**:
1. Crear Resource Group: `terraform-state-rg`
2. Crear Storage Account con nombre único (ej: `tfstate12345678`)
3. Crear Container: `tfstate`
4. Configurar RBAC para Azure AD authentication

**⚠️ MUY IMPORTANTE**: El script mostrará el nombre del Storage Account. **Cópialo**, lo necesitarás en el siguiente paso.

Ejemplo de salida:
```
Storage Account: tfstate12345678  ← COPIAR ESTE NOMBRE
```

---

### 📝 PASO 4: Actualizar providers.tf

Abre `providers.tf` y actualiza el bloque `backend`:

```hcl
# Buscar esta sección y descomentar:
backend "azurerm" {
  resource_group_name  = "terraform-state-rg"
  storage_account_name = "tfstate12345678"  # ⚠️ CAMBIAR por el nombre real
  container_name       = "tfstate"
  key                  = "api-devops.terraform.tfstate"
  use_azuread_auth     = true  # ✅ Usar Azure AD (más seguro)
}
```

---

### 📝 PASO 5: Migrar State al Backend

```powershell
# Inicializar y migrar state
terraform init -migrate-state
```

Cuando pregunte: `Do you want to copy existing state to the new backend?`
- Responde: **yes**

Deberías ver:
```
Successfully configured the backend "azurerm"!
```

---

### 📝 PASO 6: Crear Archivo de Variables

```powershell
# Copiar template de desarrollo
cp environments\dev.tfvars.example environments\dev.tfvars

# Editar con tus valores
notepad environments\dev.tfvars
```

**Valores CRÍTICOS a cambiar**:

```hcl
# 1. Tu email
owner_email = "tu.email@example.com"  # ⚠️ CAMBIAR

# 2. Tu IP pública (obtener en https://www.whatismyip.com/)
allowed_sql_ips = [
  {
    name = "MiOficina"
    ip   = "203.0.113.45"  # ⚠️ CAMBIAR por tu IP real
  }
]

key_vault_allowed_ips = ["203.0.113.45"]  # ⚠️ La misma IP
```

**⚠️ NUNCA pongas el password de SQL en este archivo**

---

### 📝 PASO 7: Configurar Password de SQL

```powershell
# Configurar como variable de entorno
$env:TF_VAR_sql_admin_password = "TuPassw0rd!Segur0"
$env:TF_VAR_owner_email = "tu.email@example.com"

# Verificar
echo $env:TF_VAR_sql_admin_password
```

**Requisitos del password**:
- ✅ Mínimo 8 caracteres
- ✅ Mayúsculas y minúsculas
- ✅ Números
- ✅ Caracteres especiales (`!@#$%^&*`)

---

### 📝 PASO 8: Validar Configuración

```powershell
# Formatear código
terraform fmt

# Validar sintaxis
terraform validate
```

Deberías ver:
```
Success! The configuration is valid.
```

---

### 📝 PASO 9: Ver el Plan de Ejecución

```powershell
# Ver qué recursos se van a crear
terraform plan -var-file="environments/dev.tfvars"
```

**Revisa cuidadosamente**:
- ✅ Recursos que se crearán (verde con `+`)
- ⚠️ Número total de recursos
- 🔍 Configuraciones de seguridad

**Recursos esperados** (si `main.tf` está completo):
- 1 Resource Group
- 1 Container Registry
- 1 Key Vault
- 1 SQL Server
- 1 SQL Database
- 1 Container App Environment
- 1 Container App
- 1 Log Analytics Workspace
- 1 Application Insights
- 1 Managed Identity
- Firewall rules
- Key Vault secrets

---

### 📝 PASO 10: Aplicar Cambios

```powershell
# Opción A: Guardar y aplicar plan
terraform plan -var-file="environments/dev.tfvars" -out=tfplan
terraform apply tfplan

# Opción B: Aplicar directamente (menos seguro)
terraform apply -var-file="environments/dev.tfvars"
```

Cuando pregunte: `Do you want to perform these actions?`
- Responde: **yes**

**⏱️ Tiempo estimado**: 5-10 minutos

---

### 📝 PASO 11: Verificar Outputs

```powershell
# Ver todos los outputs
terraform output

# Outputs importantes
terraform output resource_group_name
terraform output key_vault_name
terraform output sql_server_fqdn
terraform output container_app_url
terraform output acr_login_server
```

**Guarda estos valores**, los necesitarás para:
- Configurar CI/CD
- Desplegar tu aplicación
- Configurar connection strings

---

### 📝 PASO 12: Verificar en Azure Portal

1. Ir a [Azure Portal](https://portal.azure.com)
2. Buscar el Resource Group: `rg-api-devops-dev`
3. Verificar recursos creados:
   - ✅ Container Registry
   - ✅ Key Vault
   - ✅ SQL Server + Database
   - ✅ Container App + Environment
   - ✅ Log Analytics + App Insights

---

### 📝 PASO 13: Probar Conectividad

```powershell
# 1. Obtener outputs
$kvName = terraform output -raw key_vault_name
$sqlFqdn = terraform output -raw sql_server_fqdn

# 2. Ver secreto en Key Vault
az keyvault secret show --vault-name $kvName --name sql-connection-string

# 3. Probar SQL (requiere sqlcmd instalado)
$sqlPassword = $env:TF_VAR_sql_admin_password
sqlcmd -S $sqlFqdn -U sqladmin -P $sqlPassword -Q "SELECT @@VERSION"
```

---

### ✅ Checklist de Verificación

Antes de continuar, verifica que todo esté correcto:

- [ ] ✅ Service Principal creado y credenciales guardadas
- [ ] ✅ Variables de entorno configuradas (`ARM_*`)
- [ ] ✅ Backend remoto creado y configurado
- [ ] ✅ State migrado al backend (`terraform.tfstate` local eliminado)
- [ ] ✅ Archivo `dev.tfvars` creado con valores reales
- [ ] ✅ Password de SQL configurado como variable de entorno
- [ ] ✅ `terraform validate` exitoso
- [ ] ✅ `terraform apply` completado sin errores
- [ ] ✅ Outputs visibles con `terraform output`
- [ ] ✅ Recursos verificados en Azure Portal
- [ ] ✅ Key Vault accesible y con secretos

---

### 🚨 Errores Comunes y Soluciones

#### Error: "Backend configuration changed"
```powershell
terraform init -reconfigure
```

#### Error: "No valid credential sources"
```powershell
# Verificar variables
echo $env:ARM_CLIENT_ID

# Reconfigurar
.\configure-terraform-env.ps1
```

#### Error: "Storage account name already taken"
```powershell
# El script genera nombres únicos. Si falla, ejecuta nuevamente:
.\setup-azure-backend.ps1
```

#### Error: "Invalid SQL password"
```powershell
# Verificar que cumple requisitos
$env:TF_VAR_sql_admin_password = "NewP@ssw0rd123!"
```

#### Error: "main.tf not found"
```powershell
# Crear main.tf con el código de SOLUCIONES-IMPLEMENTACION.md
notepad main.tf
```

---

### 🎯 Próximos Pasos

Una vez desplegada la infraestructura:

1. **Desplegar tu aplicación**:
   ```powershell
   # Build y push de imagen Docker
   docker build -t miapp:latest .
   docker tag miapp:latest <acr-login-server>/miapp:latest
   az acr login --name <acr-name>
   docker push <acr-login-server>/miapp:latest
   ```

2. **Actualizar Container App** con la nueva imagen

3. **Configurar CI/CD** (GitHub Actions o Azure DevOps)

4. **Configurar monitoreo** en Application Insights

5. **Ejecutar migraciones** de base de datos

---

## 📁 Estructura del Proyecto

```
terraform/
├── providers.tf                      # Configuración de providers y backend
├── variables.tf                      # Variables parametrizables
├── main.tf                          # Recursos principales de Azure
├── outputs.tf                       # Outputs del deployment
├── .gitignore                       # Archivos a ignorar en Git
├── setup-service-principal.ps1      # Script para crear Service Principal
├── setup-azure-backend.ps1          # Script para crear backend remoto
├── configure-terraform-env.ps1      # Script para configurar variables de entorno
└── environments/
    ├── dev.tfvars                   # Configuración para desarrollo
    └── prod.tfvars                  # Configuración para producción
```

## 🔐 Recursos de Azure a Provisionar

- **Resource Group**: Contenedor de recursos
- **Key Vault**: Gestión centralizada de secretos
- **Container Registry (ACR)**: Registro de imágenes Docker
- **Container Apps Environment**: Entorno serverless
- **Container App**: Hosting del API
- **SQL Server**: Servidor de base de datos
- **SQL Database**: Base de datos relacional
- **Application Insights**: Monitoreo y telemetría
- **Log Analytics Workspace**: Logs centralizados
- **Managed Identity**: Identidad para acceso sin credenciales

## 💻 Comandos de Terraform

### Inicialización

```powershell
# Inicializar Terraform (primera vez)
terraform init

# Re-inicializar después de cambios en providers
terraform init -upgrade
```

### Planificación y Aplicación

```powershell
# Ver plan para desarrollo
terraform plan -var-file="environments/dev.tfvars"

# Aplicar cambios en desarrollo
terraform apply -var-file="environments/dev.tfvars"

# Aplicar sin confirmación (usar con precaución)
terraform apply -var-file="environments/dev.tfvars" -auto-approve

# Plan para producción
terraform plan -var-file="environments/prod.tfvars"

# Aplicar en producción
terraform apply -var-file="environments/prod.tfvars"
```

### Ver Outputs

```powershell
# Ver todos los outputs
terraform output

# Ver output específico
terraform output key_vault_name

# Ver output en JSON
terraform output -json

# Ver outputs sensibles (ejemplo: password hint)
terraform output sql_admin_password_hint
```

### Validación y Formato

```powershell
# Validar sintaxis
terraform validate

# Formatear archivos
terraform fmt

# Formatear recursivamente
terraform fmt -recursive
```

### Destrucción

```powershell
# Ver qué se destruiría
terraform plan -destroy -var-file="environments/dev.tfvars"

# Destruir recursos (¡CUIDADO!)
terraform destroy -var-file="environments/dev.tfvars"
```

## 🔒 Seguridad

### Variables de Entorno Necesarias

```powershell
$env:ARM_CLIENT_ID = "tu-client-id"
$env:ARM_CLIENT_SECRET = "tu-client-secret"
$env:ARM_SUBSCRIPTION_ID = "tu-subscription-id"
$env:ARM_TENANT_ID = "tu-tenant-id"
```

### Secretos en Key Vault

Todos los secretos se almacenan automáticamente en Azure Key Vault:

- `sql-admin-username`: Usuario administrador de SQL
- `sql-admin-password`: Password de SQL (generado automáticamente)
- `sql-connection-string`: Connection string completo
- `acr-username`: Usuario de Container Registry
- `acr-password`: Password de Container Registry

### Recuperar Secretos

```powershell
# Obtener nombre del Key Vault
$kvName = terraform output -raw key_vault_name

# Ver un secreto
az keyvault secret show --vault-name $kvName --name sql-admin-password --query value -o tsv

# Listar todos los secretos
az keyvault secret list --vault-name $kvName --query "[].name" -o table
```

## 🔄 Workflow Típico

### Para Desarrollo

```powershell
# 1. Configurar variables de entorno (si no están configuradas)
.\configure-terraform-env.ps1

# 2. Ver cambios
terraform plan -var-file="environments/dev.tfvars"

# 3. Aplicar cambios
terraform apply -var-file="environments/dev.tfvars"

# 4. Ver outputs
terraform output deployment_summary
```

### Para Producción

```powershell
# 1. Asegurarse de tener las credenciales correctas
.\configure-terraform-env.ps1

# 2. Ver plan detallado
terraform plan -var-file="environments/prod.tfvars" -out=tfplan

# 3. Revisar el plan
# ... revisión manual ...

# 4. Aplicar el plan aprobado
terraform apply tfplan

# 5. Verificar outputs
terraform output deployment_summary
```

## 🐛 Troubleshooting

### Error: Backend no inicializado

```powershell
# Solución: Ejecutar init nuevamente
terraform init -reconfigure
```

### Error: Credenciales inválidas

```powershell
# Verificar variables de entorno
echo $env:ARM_CLIENT_ID
echo $env:ARM_TENANT_ID

# Re-configurar
.\configure-terraform-env.ps1
```

### Error: State lock

```powershell
# Si el state está bloqueado por un proceso interrumpido
terraform force-unlock <LOCK_ID>
```

### Error: Provider no encontrado

```powershell
# Re-inicializar con upgrade
terraform init -upgrade
```

## 📚 Referencias

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure CLI Reference](https://docs.microsoft.com/cli/azure/)
- [Terraform Best Practices](https://www.terraform-best-practices.com/)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/)

## ⚠️ Notas Importantes

- **NUNCA** commitees archivos `.tfvars` con secretos a Git
- **NUNCA** expongas credenciales del Service Principal
- Usa Key Vault para todos los secretos
- El archivo `terraform.tfstate` contiene información sensible
- Siempre revisa el `plan` antes de hacer `apply`
- En producción, usa pipelines de CI/CD en lugar de ejecución manual
