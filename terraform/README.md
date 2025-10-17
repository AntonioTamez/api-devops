# terraform/

Este directorio contiene la infraestructura como código (IaC) para provisionar recursos en Azure.

## Estructura

```
terraform/
├── main.tf              # Recursos principales de Azure
├── variables.tf         # Variables parametrizables
├── outputs.tf           # Outputs del deployment
├── providers.tf         # Configuración de providers
└── environments/        # Variables por ambiente
    ├── dev.tfvars      # Configuración para desarrollo
    └── prod.tfvars     # Configuración para producción
```

## Recursos de Azure a Provisionar

- **Resource Group**: Contenedor de recursos
- **Container Registry (ACR)**: Registro de imágenes Docker
- **Container Apps**: Hosting del API
- **SQL Database**: Base de datos SQL Server
- **Application Insights**: Monitoreo y telemetría
- **Key Vault**: Gestión de secrets
- **Log Analytics Workspace**: Logs centralizados

## Comandos Básicos

```bash
# Inicializar Terraform
terraform init

# Planificar cambios
terraform plan -var-file="environments/dev.tfvars"

# Aplicar cambios
terraform apply -var-file="environments/prod.tfvars"

# Ver outputs
terraform output
```

## Próximos pasos

Los archivos de Terraform se crearán en el Sprint 5.
