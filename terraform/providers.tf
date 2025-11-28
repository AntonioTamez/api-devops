terraform {
  required_version = ">= 1.5.0, < 2.0.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"  # Permite patches de seguridad (3.80, 3.81, etc)
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"   # Permite patches de seguridad
    }
  }

  # Backend remoto en Azure Storage
  # IMPORTANTE: Comentar este bloque SOLO en la primera ejecución
  # Pasos:
  # 1. Ejecutar: .\setup-azure-backend.ps1
  # 2. Descomentar este bloque
  # 3. Ejecutar: terraform init -migrate-state
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "tfstate9cb26312suxd"
    container_name       = "tfstate"
    key                  = "api-devops.terraform.tfstate"
    
    # SEGURIDAD: Usar Azure AD authentication (mas seguro que access keys)
    use_azuread_auth = true
  }
}

provider "azurerm" {
  features {
    resource_group {
      # ✅ SEGURIDAD: Proteger Resource Groups en producción contra eliminación accidental
      # En desarrollo: false para permitir destroy completo
      # En producción: true para prevenir eliminación accidental
      prevent_deletion_if_contains_resources = false  # TODO: Cambiar a 'var.environment == "prod"' cuando se agregue variable
    }
    
    key_vault {
      # ✅ SEGURIDAD: NO hacer purge automático de Key Vault
      # purge_soft_delete_on_destroy = true es PELIGROSO
      # - Elimina permanentemente el Key Vault y TODOS sus secretos
      # - No hay recuperación posible
      # - Un 'terraform destroy' accidental causa pérdida de datos permanente
      purge_soft_delete_on_destroy    = false  # CAMBIADO DE true A false
      recover_soft_deleted_key_vaults = true
    }
  }
}
