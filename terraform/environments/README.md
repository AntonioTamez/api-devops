# terraform/environments/

Este directorio contiene las variables de configuración por ambiente.

## Archivos

- **`dev.tfvars`**: Configuración para ambiente de desarrollo
  - SKU básicos para reducir costos
  - Réplicas mínimas
  - Base de datos pequeña

- **`prod.tfvars`**: Configuración para ambiente de producción
  - SKU estándar o premium
  - Alta disponibilidad
  - Auto-scaling configurado
  - Backups automáticos

## Uso

```bash
# Deploy a desarrollo
terraform apply -var-file="environments/dev.tfvars"

# Deploy a producción
terraform apply -var-file="environments/prod.tfvars"
```

## Próximos pasos

Los archivos `.tfvars` se crearán en el Sprint 5.
