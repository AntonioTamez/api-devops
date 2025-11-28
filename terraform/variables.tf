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
  description = "List of allowed IPs for SQL Server access (use specific IPs, not 0.0.0.0-255.255.255.255)"
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
