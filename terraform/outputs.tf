# ========================================
# OUTPUTS GENERALES
# ========================================

output "deployment_summary" {
  description = "Summary of deployed resources"
  value = {
    environment         = var.environment
    resource_group_name = try(azurerm_resource_group.main.name, "not-created")
    location            = var.location
    deployment_time     = timestamp()
    terraform_version   = terraform.version
  }
}

# ========================================
# RESOURCE GROUP
# ========================================

output "resource_group_name" {
  description = "Name of the resource group"
  value       = try(azurerm_resource_group.main.name, null)
}

output "resource_group_id" {
  description = "ID of the resource group"
  value       = try(azurerm_resource_group.main.id, null)
}

# ========================================
# KEY VAULT
# ========================================

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = try(azurerm_key_vault.main.name, null)
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = try(azurerm_key_vault.main.vault_uri, null)
}

output "key_vault_id" {
  description = "ID of the Key Vault"
  value       = try(azurerm_key_vault.main.id, null)
}

# ========================================
# CONTAINER REGISTRY
# ========================================

output "acr_login_server" {
  description = "Login server for Azure Container Registry"
  value       = try(azurerm_container_registry.acr.login_server, null)
}

output "acr_name" {
  description = "Name of the Container Registry"
  value       = try(azurerm_container_registry.acr.name, null)
}

output "acr_id" {
  description = "ID of the Container Registry"
  value       = try(azurerm_container_registry.acr.id, null)
}

# ⚠️ NOTA: Solo disponibles si admin_enabled = true en ACR
# RECOMENDADO: Usar Managed Identity en lugar de admin credentials
output "acr_admin_username" {
  description = "ACR admin username (only if admin_enabled=true - NOT RECOMMENDED)"
  value       = try(azurerm_container_registry.acr.admin_username, "N/A - Admin disabled (recommended)")
  sensitive   = true
}

output "acr_admin_password" {
  description = "ACR admin password (only if admin_enabled=true - NOT RECOMMENDED)"
  value       = try(azurerm_container_registry.acr.admin_password, "N/A - Admin disabled (recommended)")
  sensitive   = true
}

# ========================================
# SQL SERVER
# ========================================

output "sql_server_fqdn" {
  description = "Fully qualified domain name of SQL Server"
  value       = try(azurerm_mssql_server.main.fully_qualified_domain_name, null)
}

output "sql_server_name" {
  description = "Name of SQL Server"
  value       = try(azurerm_mssql_server.main.name, null)
}

output "sql_database_name" {
  description = "Name of the SQL Database"
  value       = try(azurerm_mssql_database.main.name, null)
}

output "sql_database_id" {
  description = "ID of the SQL Database"
  value       = try(azurerm_mssql_database.main.id, null)
}

output "sql_admin_username" {
  description = "SQL Server admin username"
  value       = var.sql_admin_username
  sensitive   = true
}

output "sql_admin_password_hint" {
  description = "Hint about where to find SQL admin password"
  value       = "Stored in Key Vault as secret 'sql-admin-password' or in TF_VAR_sql_admin_password"
}

# ⚠️ NUNCA exponer password directamente, solo Key Vault reference
output "sql_connection_string_keyvault_ref" {
  description = "Key Vault reference for SQL connection string"
  value       = try("@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/sql-connection-string)", null)
}

# ========================================
# CONTAINER APP
# ========================================

output "container_app_url" {
  description = "URL of the deployed Container App"
  value       = try("https://${azurerm_container_app.api.latest_revision_fqdn}", "not-deployed-yet")
}

output "container_app_name" {
  description = "Name of the Container App"
  value       = try(azurerm_container_app.api.name, null)
}

output "container_app_id" {
  description = "ID of the Container App"
  value       = try(azurerm_container_app.api.id, null)
}

output "container_app_environment_id" {
  description = "ID of the Container App Environment"
  value       = try(azurerm_container_app_environment.main.id, null)
}

# ========================================
# APPLICATION INSIGHTS
# ========================================

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = try(azurerm_application_insights.main[0].instrumentation_key, null)
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = try(azurerm_application_insights.main[0].connection_string, null)
  sensitive   = true
}

output "application_insights_app_id" {
  description = "Application Insights application ID"
  value       = try(azurerm_application_insights.main[0].app_id, null)
}

output "log_analytics_workspace_id" {
  description = "ID of Log Analytics Workspace"
  value       = try(azurerm_log_analytics_workspace.main.id, null)
}

# ========================================
# MANAGED IDENTITY
# ========================================

output "container_app_identity_client_id" {
  description = "Client ID of Container App Managed Identity"
  value       = try(azurerm_user_assigned_identity.container_app.client_id, null)
}

output "container_app_identity_principal_id" {
  description = "Principal ID of Container App Managed Identity"
  value       = try(azurerm_user_assigned_identity.container_app.principal_id, null)
}

output "container_app_identity_id" {
  description = "ID of Container App Managed Identity"
  value       = try(azurerm_user_assigned_identity.container_app.id, null)
}

# ========================================
# PARA CI/CD
# ========================================

output "github_actions_vars" {
  description = "Variables to configure in GitHub Actions secrets"
  value = {
    ACR_LOGIN_SERVER              = try(azurerm_container_registry.acr.login_server, "not-created")
    AZURE_RESOURCE_GROUP          = try(azurerm_resource_group.main.name, "not-created")
    CONTAINER_APP_NAME            = try(azurerm_container_app.api.name, "not-created")
    KEY_VAULT_NAME                = try(azurerm_key_vault.main.name, "not-created")
    SQL_SERVER_FQDN               = try(azurerm_mssql_server.main.fully_qualified_domain_name, "not-created")
    APPLICATION_INSIGHTS_CONN_STR = "***SENSITIVE - Get from Key Vault or sensitive outputs***"
  }
}

# ========================================
# VERIFICACIÓN DE CONFIGURACIÓN
# ========================================

output "security_configuration" {
  description = "Security configuration summary"
  value = {
    environment                    = var.environment
    acr_admin_enabled              = var.acr_admin_enabled
    deletion_protection_enabled    = var.enable_deletion_protection
    key_vault_purge_protection     = var.key_vault_purge_protection
    monitoring_enabled             = var.enable_monitoring
    sql_firewall_rules_count       = length(var.allowed_sql_ips)
    key_vault_allowed_ips_count    = length(var.key_vault_allowed_ips)
  }
}
