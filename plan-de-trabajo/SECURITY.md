# 🔒 Guía de Seguridad - API DevOps

**Documento**: SECURITY.md  
**Versión**: 1.0  
**Última actualización**: Noviembre 2025  

---

## 🎯 Propósito

Este documento establece las mejores prácticas de seguridad para el proyecto API DevOps, con enfoque especial en la gestión de secretos, credenciales y datos sensibles.

---

## ⚠️ REGLAS CRÍTICAS - NUNCA HACER

### 🚫 1. NO Commitear Secrets al Repositorio

**Nunca incluir en Git**:
- ❌ Contraseñas en texto plano
- ❌ API Keys
- ❌ Connection strings con credenciales
- ❌ Certificados privados
- ❌ Tokens de autenticación
- ❌ Service Principal credentials
- ❌ Archivos `.tfvars` con valores sensibles
- ❌ Archivos `.env` con valores reales

### 🚫 2. NO Hardcodear Credenciales

**Nunca en código o configuración**:
- ❌ Passwords en `docker-compose.yml`
- ❌ Passwords en `appsettings.json`
- ❌ Connection strings en código C#
- ❌ API keys en archivos de configuración

### 🚫 3. NO Exponer Secrets en Logs

**Evitar**:
- ❌ Passwords en comandos CLI que van a logs
- ❌ Secrets en outputs de Terraform
- ❌ Credenciales en logs de aplicación
- ❌ Connection strings en mensajes de error

---

## ✅ Gestión Segura de Secretos

### 1. Desarrollo Local

#### Usar .NET User Secrets
```bash
# Configurar secrets localmente (NO se commitean)
cd src
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=DevOpsDb;User Id=sa;Password=TuPassword123!;TrustServerCertificate=True"
```

#### Usar Variables de Entorno con .env
```bash
# Crear archivo .env (agregado a .gitignore)
cat > .env << EOF
SQL_SA_PASSWORD=TuPasswordSeguro123!
ASPNETCORE_ENVIRONMENT=Development
EOF

# Usar en docker-compose.yml
services:
  sqlserver:
    environment:
      - SA_PASSWORD=${SQL_SA_PASSWORD}
```

**Archivo .env.example** (SÍ commitear):
```bash
# .env.example - Template sin valores reales
SQL_SA_PASSWORD=YOUR_PASSWORD_HERE
ASPNETCORE_ENVIRONMENT=Development
```

### 2. Azure Key Vault (Producción)

#### Almacenar Secrets en Key Vault
```bash
# Crear secrets en Key Vault
az keyvault secret set \
  --vault-name "kv-api-devops-prod" \
  --name "sql-admin-password" \
  --value "TuPasswordSuperSeguro!"

az keyvault secret set \
  --vault-name "kv-api-devops-prod" \
  --name "sql-connection-string" \
  --value "Server=tcp:..."
```

#### Referenciar desde App Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://kv-api-devops-prod.vault.azure.net/secrets/sql-connection-string)"
  }
}
```

#### Usar Managed Identity
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 3. GitHub Secrets (CI/CD)

#### Configurar Secrets en GitHub
1. Ve a: **Settings → Secrets and variables → Actions**
2. Click: **New repository secret**
3. Agregar cada secret individualmente

#### Secrets Requeridos
```
AZURE_CREDENTIALS           # JSON con Service Principal
AZURE_SUBSCRIPTION_ID       # ID de suscripción
ACR_LOGIN_SERVER           # URL del Container Registry
ACR_USERNAME               # Admin username (o mejor: usar Managed Identity)
ACR_PASSWORD               # Admin password (o mejor: usar Managed Identity)
SQL_ADMIN_PASSWORD         # Password del SQL Server
TERRAFORM_BACKEND_RG       # Resource group del state
TERRAFORM_BACKEND_STORAGE  # Storage account del state
```

#### Usar Secrets en Workflows
```yaml
# ✅ CORRECTO - Como variable de entorno
- name: Terraform Plan
  env:
    TF_VAR_sql_admin_password: ${{ secrets.SQL_ADMIN_PASSWORD }}
  run: terraform plan -var-file="environments/prod.tfvars" -out=tfplan

# ❌ INCORRECTO - En argumentos (visible en logs)
- name: Terraform Plan
  run: terraform plan -var="sql_admin_password=${{ secrets.SQL_ADMIN_PASSWORD }}"
```

### 4. Terraform State Security

#### Backend Seguro en Azure Storage
```bash
# Crear storage account con seguridad máxima
az storage account create \
  --name tfstateapidevops \
  --resource-group terraform-state-rg \
  --location eastus \
  --sku Standard_LRS \
  --https-only true \
  --allow-blob-public-access false \
  --min-tls-version TLS1_2 \
  --encryption-services blob

# Habilitar soft delete
az storage blob service-properties delete-policy update \
  --days-retained 30 \
  --account-name tfstateapidevops \
  --enable true

# Configurar acceso solo desde IPs específicas
az storage account network-rule add \
  --account-name tfstateapidevops \
  --ip-address YOUR_IP_ADDRESS
```

#### Usar Azure Key Vault para Secrets de Terraform
```hcl
# ✅ Almacenar secrets en Key Vault, NO en variables
data "azurerm_key_vault_secret" "sql_password" {
  name         = "sql-admin-password"
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_mssql_server" "main" {
  administrator_login_password = data.azurerm_key_vault_secret.sql_password.value
}
```

#### Marcar Outputs Sensibles
```hcl
# outputs.tf
output "sql_connection_string" {
  value     = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;..."
  sensitive = true  # ✅ NO se muestra en logs
}
```

---

## 🔐 Configuración de Accesos

### Service Principal con Permisos Mínimos

#### ❌ EVITAR - Permisos Excesivos
```bash
# NO hacer esto
az ad sp create-for-rbac --role Contributor
```

#### ✅ RECOMENDADO - Rol Custom
```bash
# 1. Crear definición de rol custom
cat > terraform-deployer-role.json << EOF
{
  "Name": "Terraform Deployer",
  "Description": "Minimal permissions for Terraform deployment",
  "Actions": [
    "Microsoft.Resources/deployments/*",
    "Microsoft.Resources/subscriptions/resourceGroups/*",
    "Microsoft.ContainerRegistry/*",
    "Microsoft.App/*",
    "Microsoft.Sql/servers/*",
    "Microsoft.Sql/managedInstances/databases/*",
    "Microsoft.Insights/*",
    "Microsoft.KeyVault/vaults/read",
    "Microsoft.KeyVault/vaults/secrets/read"
  ],
  "NotActions": [],
  "AssignableScopes": [
    "/subscriptions/{subscription-id}"
  ]
}
EOF

# 2. Crear rol
az role definition create --role-definition terraform-deployer-role.json

# 3. Crear Service Principal con rol custom
az ad sp create-for-rbac \
  --name "sp-api-devops-terraform" \
  --role "Terraform Deployer" \
  --scopes /subscriptions/{subscription-id}
```

### Azure SQL Firewall Rules

#### ❌ NUNCA - Permitir Todo el Internet
```hcl
# PELIGROSO - NO USAR
resource "azurerm_mssql_firewall_rule" "allow_all" {
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"  # ¡TODO EL INTERNET!
}
```

#### ✅ CORRECTO - IPs Específicas
```hcl
# Solo IPs necesarias
variable "allowed_ips" {
  description = "Lista de IPs permitidas para acceso a SQL Server"
  type = list(object({
    name  = string
    ip    = string
  }))
  default = []
}

resource "azurerm_mssql_firewall_rule" "allowed_ips" {
  for_each = { for ip in var.allowed_ips : ip.name => ip }
  
  name             = each.value.name
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = each.value.ip
  end_ip_address   = each.value.ip
}

# Azure Services (solo si es necesario)
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"  # Esto solo permite servicios de Azure
}
```

### Container Registry - Usar Managed Identity

#### ❌ EVITAR - Admin Credentials
```hcl
# Menos seguro
resource "azurerm_container_registry" "acr" {
  admin_enabled = true
}
```

#### ✅ RECOMENDADO - Managed Identity
```hcl
resource "azurerm_container_registry" "acr" {
  admin_enabled = false  # ✅ No usar admin
}

# Dar permisos a Container App Managed Identity
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.container_app.principal_id
}

# En Container App, usar Managed Identity
resource "azurerm_container_app" "api" {
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_app.id]
  }
  
  # No necesita username/password para ACR
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.container_app.id
  }
}
```

---

## 📝 Checklist de Seguridad Pre-Deployment

### Antes de Commitear
- [ ] No hay passwords en archivos de código
- [ ] No hay API keys hardcodeadas
- [ ] `.env` está en `.gitignore`
- [ ] `*.tfvars` está en `.gitignore` (excepto `.example`)
- [ ] Connection strings usan variables de entorno
- [ ] Secrets locales usan dotnet user-secrets

### Antes de Crear PR
- [ ] No hay secrets en el diff del PR
- [ ] Terraform outputs sensibles marcados como `sensitive = true`
- [ ] Variables sensibles en Terraform marcadas como `sensitive = true`
- [ ] Logs no exponen información sensible

### Antes de Deploy a Azure
- [ ] Todos los secrets están en Azure Key Vault
- [ ] Service Principal tiene permisos mínimos necesarios
- [ ] SQL Server firewall configurado con IPs específicas
- [ ] Storage account de Terraform state tiene encriptación
- [ ] Container Registry usa Managed Identity (no admin)
- [ ] Key Vault tiene network rules configuradas
- [ ] Application Insights configurado para logs

### Post-Deployment
- [ ] Verificar que no hay secrets en logs de CI/CD
- [ ] Confirmar que Terraform state está encriptado
- [ ] Revisar access logs de Key Vault
- [ ] Verificar firewall rules de SQL Server
- [ ] Confirmar que Container App usa Managed Identity

---

## 🔄 Rotación de Credenciales

### Frecuencia Recomendada
- **Service Principal**: Cada 90 días
- **SQL Server Admin Password**: Cada 90 días
- **ACR Admin Password**: No usar (preferir Managed Identity)
- **GitHub Secrets**: Cuando haya cambios de personal

### Proceso de Rotación

#### 1. SQL Server Password
```bash
# 1. Generar nuevo password
NEW_PASSWORD=$(openssl rand -base64 32)

# 2. Actualizar en Key Vault
az keyvault secret set \
  --vault-name "kv-api-devops-prod" \
  --name "sql-admin-password" \
  --value "$NEW_PASSWORD"

# 3. Actualizar SQL Server
az sql server update \
  --name "sql-api-devops-prod" \
  --resource-group "rg-api-devops-prod" \
  --admin-password "$NEW_PASSWORD"

# 4. Actualizar GitHub Secret
# (Manual en GitHub UI)

# 5. Re-deploy aplicación para usar nuevo password
```

#### 2. Service Principal
```bash
# 1. Crear nuevo credential
az ad sp credential reset \
  --id <app-id> \
  --append

# 2. Actualizar GitHub Secret AZURE_CREDENTIALS

# 3. Eliminar credential antigua después de verificar
az ad sp credential delete \
  --id <app-id> \
  --key-id <old-key-id>
```

---

## 🚨 Respuesta a Incidentes

### Si se Expone un Secret

1. **Detección**: Confirmar qué secret fue expuesto
2. **Rotación Inmediata**: Cambiar el secret comprometido
3. **Auditoría**: Revisar logs de acceso
4. **Notificación**: Informar al equipo
5. **Prevención**: Actualizar procesos para evitar repetición

### Herramientas de Detección
- **GitHub Secret Scanning**: Detecta secrets en commits
- **git-secrets**: Pre-commit hook para detectar secrets
- **TruffleHog**: Escaneo de repositorio completo
- **Azure Security Center**: Alertas de configuraciones inseguras

---

## 📚 Recursos Adicionales

### Documentación Oficial
- [Azure Key Vault Best Practices](https://docs.microsoft.com/azure/key-vault/general/best-practices)
- [Managed Identity Best Practices](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Terraform Sensitive Data](https://www.terraform.io/docs/language/values/variables.html#suppressing-values-in-cli-output)
- [GitHub Encrypted Secrets](https://docs.github.com/actions/security-guides/encrypted-secrets)

### Herramientas Recomendadas
- **git-secrets**: https://github.com/awslabs/git-secrets
- **detect-secrets**: https://github.com/Yelp/detect-secrets
- **TruffleHog**: https://github.com/trufflesecurity/trufflehog
- **Azure CLI**: https://docs.microsoft.com/cli/azure/

---

## 🆘 Contacto de Seguridad

**Para reportar problemas de seguridad**:
- Email: security@example.com
- Slack: #security-incidents
- Proceso: No publicar en issues públicos

---

**Última revisión**: Noviembre 2025  
**Próxima revisión**: Febrero 2026  
**Responsable**: DevOps Security Team
