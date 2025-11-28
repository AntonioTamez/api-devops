# ========================================
# LOCAL VALUES
# ========================================

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
  vnet_name   = "vnet-${local.resource_prefix}"
  subnet_name = "snet-${local.resource_prefix}-apps"
  
  # Configuraciones de seguridad por ambiente
  sql_firewall_enabled    = var.environment != "prod"  # En prod, solo Azure Services
  key_vault_public_access = var.environment != "prod"  # En prod, deny all
}
