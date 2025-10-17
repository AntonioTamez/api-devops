# .github/workflows/

Este directorio contiene los workflows de GitHub Actions para CI/CD automatizado.

## Workflows

### `build-test.yml`
Pipeline de build y testing que se ejecuta en Pull Requests.

**Triggers**: 
- Pull Request a `master` o `develop`

**Acciones**:
- Setup .NET 8
- Restore dependencies
- Build proyecto
- Run tests unitarios
- Code coverage report

### `deploy.yml`
Pipeline de deployment automatizado a Azure.

**Triggers**:
- Push a rama `master` (deploy a producción)
- Manual dispatch (workflow_dispatch)

**Jobs**:
1. **Build Docker Image**: Construye y sube imagen a ACR
2. **Terraform Apply**: Provisiona/actualiza infraestructura
3. **Deploy**: Despliega a Azure Container Apps
4. **Health Check**: Valida que el deployment fue exitoso

## Secrets Requeridos

Configurar en: **Settings → Secrets and variables → Actions**

- `AZURE_CREDENTIALS`: Credenciales del Service Principal
- `AZURE_SUBSCRIPTION_ID`: ID de suscripción de Azure
- `ACR_LOGIN_SERVER`: URL del Container Registry
- `ACR_USERNAME`: Usuario admin del ACR
- `ACR_PASSWORD`: Password del ACR
- `SQL_ADMIN_PASSWORD`: Password del SQL Server

## Flujo Completo

```
PR → Build + Test → Review → Merge → Build Docker → Terraform → Deploy → Health Check
```

## Próximos pasos

Los workflows se crearán en el Sprint 6.
