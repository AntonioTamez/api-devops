# Terraform Environments

Este directorio contiene las configuraciones específicas de cada ambiente.

## Estructura

```
environments/
├── README.md              # Este archivo
├── dev.tfvars.example     # Plantilla para desarrollo
├── prod.tfvars.example    # Plantilla para producción
├── dev.tfvars            # NO COMMITEAR - Valores reales de dev
└── prod.tfvars           # NO COMMITEAR - Valores reales de prod
```

## Configuración Inicial

## Uso

```bash
# Deploy a desarrollo
terraform apply -var-file="environments/dev.tfvars"

# Deploy a producción
terraform apply -var-file="environments/prod.tfvars"
```

## 🚀 Configuración Inicial

### 1. Crear archivos de configuración

```powershell
# Para desarrollo
cp dev.tfvars.example dev.tfvars

# Para producción
cp prod.tfvars.example prod.tfvars
```

### 2. Editar con valores reales

Edita `dev.tfvars` y `prod.tfvars` con tus valores reales:
- ✅ Cambiar `YOUR_EMAIL@example.com` con tu email
- ✅ Cambiar `YOUR_IP_HERE` con tu IP real
- ✅ Ajustar configuraciones según necesidad

### 3. Configurar password de SQL

⚠️ **IMPORTANTE**: NUNCA poner el password en archivos `.tfvars`

```powershell
# Variable de entorno (Recomendado)
$env:TF_VAR_sql_admin_password = "YourStrongP@ssw0rd!"
```

## 🔒 Seguridad

### Archivos que NO se deben commitear

Los siguientes archivos están en `.gitignore`:
- ❌ `*.tfvars` (excepto `.example`)
- ❌ `*.tfstate`
- ❌ `.terraform/`

## 📚 Referencias

- [SECURITY.md](../../plan-de-trabajo/SECURITY.md) - Guía de seguridad
- [SECURITY-AUDIT-REPORT.md](../SECURITY-AUDIT-REPORT.md) - Auditoría
