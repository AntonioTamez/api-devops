# 📖 Historias de Usuario - Parte 7: CI/CD con GitHub Actions
**Identificador**: US-07-CICD  
**Orden de lectura**: 7 de 8  
**Sprint**: Sprint 6 - Pipeline de Deployment Automatizado  

---

## 🎯 Objetivo del Sprint 6
Implementar pipeline completo de CI/CD con GitHub Actions para deployment automatizado en Azure.

---

## US-032: Configurar GitHub Secrets

**Como** DevOps engineer  
**Quiero** configurar secrets en GitHub  
**Para** permitir autenticación segura en Azure desde el pipeline

### Criterios de Aceptación
- ✅ Service Principal credentials configurados
- ✅ ACR credentials configurados
- ✅ SQL password configurado
- ✅ Secrets documentados en README
- ✅ Secrets NO commiteados en el repo

### Secrets a Configurar

En GitHub: **Settings → Secrets and variables → Actions → New repository secret**

1. **AZURE_CREDENTIALS**
   ```json
   {
     "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
     "clientSecret": "your-client-secret",
     "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
     "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
   }
   ```
   Obtener con:
   ```bash
   az ad sp create-for-rbac --name "sp-api-devops-ci" --role Contributor --scopes /subscriptions/{subscription-id} --sdk-auth
   ```

2. **AZURE_SUBSCRIPTION_ID**
   - ID de tu suscripción de Azure

3. **ACR_LOGIN_SERVER**
   - Valor del output de Terraform: `terraform output acr_login_server`

4. **ACR_USERNAME**
   - Valor del output: `terraform output acr_admin_username`

5. **ACR_PASSWORD**
   - Valor del output: `terraform output -raw acr_admin_password`

6. **SQL_ADMIN_PASSWORD**
   - Password del SQL Server (mismo usado en Terraform)

7. **TERRAFORM_BACKEND_RG** (opcional)
   - terraform-state-rg

8. **TERRAFORM_BACKEND_STORAGE** (opcional)
   - tfstatedevops

### Tareas Técnicas
1. Crear Service Principal si no existe
2. Configurar cada secret en GitHub
3. Crear documento `docs/SECRETS.md` con instrucciones (sin valores reales)
4. Verificar que secrets están disponibles en Actions
5. Commit: "docs: Add instructions for GitHub Secrets configuration"

### Dependencias
- ✅ US-026 (Service Principal creado para Terraform)
- ✅ US-030 (ACR creado con admin habilitado)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Todos los secrets configurados
- Service Principal con permisos correctos
- Documentación creada
- Verificado acceso desde workflow test

---

## US-033: Crear Workflow de Build y Test

**Como** desarrollador  
**Quiero** un workflow que ejecute build y tests en cada PR  
**Para** validar cambios antes de merge

### Criterios de Aceptación
- ✅ Workflow ejecuta en PRs a master
- ✅ .NET 8 instalado
- ✅ Restore, build y test ejecutados
- ✅ Code coverage reportado
- ✅ Resultados visibles en PR

### Workflow: .github/workflows/build-test.yml

```yaml
name: Build and Test

on:
  pull_request:
    branches:
      - master
      - develop
  push:
    branches:
      - develop

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET 8
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore src/DevOpsApi.csproj

    - name: Build
      run: dotnet build src/DevOpsApi.csproj --configuration Release --no-restore

    - name: Run unit tests
      run: dotnet test tests/DevOpsApi.UnitTests/DevOpsApi.UnitTests.csproj --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"

    - name: Code Coverage Report
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
        flags: unittests
        name: codecov-umbrella
        fail_ci_if_error: false

    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results
        path: '**/TestResults/**'
```

### Tareas Técnicas
1. Crear carpeta `.github/workflows/`
2. Crear `.github/workflows/build-test.yml`
3. Crear PR de prueba para validar workflow
4. Verificar que ejecuta correctamente
5. Ajustar paths si es necesario
6. Commit: "ci: Add build and test workflow for PRs"

### Dependencias
- ✅ US-022 (Tests implementados)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Workflow creado y funcionando
- Tests ejecutándose en PRs
- Coverage reportándose
- Fallos visibles en PR

---

## US-034: Crear Workflow de Build Docker Image

**Como** DevOps engineer  
**Quiero** un job que construya y suba la imagen Docker a ACR  
**Para** tener artefactos deployables

### Criterios de Aceptación
- ✅ Imagen Docker construida correctamente
- ✅ Tagged con SHA + timestamp
- ✅ Push a Azure Container Registry
- ✅ Multi-platform support (opcional)
- ✅ Cache de layers para velocidad

### Workflow: .github/workflows/deploy.yml (Parte 1)

```yaml
name: Deploy to Azure

on:
  push:
    branches:
      - master
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy'
        required: true
        default: 'dev'
        type: choice
        options:
          - dev
          - prod

env:
  IMAGE_NAME: api-devops
  
jobs:
  build-docker:
    runs-on: ubuntu-latest
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3

    - name: Log in to Azure Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ secrets.ACR_LOGIN_SERVER }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}

    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ secrets.ACR_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}
        tags: |
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
          type=raw,value={{date 'YYYYMMDD-HHmmss'}}

    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: ./src
        file: ./Dockerfile
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=registry,ref=${{ secrets.ACR_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:buildcache
        cache-to: type=registry,ref=${{ secrets.ACR_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:buildcache,mode=max
```

### Tareas Técnicas
1. Crear `.github/workflows/deploy.yml`
2. Implementar job build-docker
3. Probar build manual con workflow_dispatch
4. Verificar imagen en ACR
5. Commit: "ci: Add Docker build and push to ACR"

### Dependencias
- ✅ US-017 (Dockerfile creado)
- ✅ US-032 (Secrets configurados)

### Estimación
**Esfuerzo**: 3 puntos (1.5 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Imagen se construye correctamente
- Push a ACR exitoso
- Tags correctos
- Cache funcionando

---

## US-035: Crear Workflow de Terraform Apply

**Como** DevOps engineer  
**Quiero** un job que aplique cambios de Terraform  
**Para** actualizar infraestructura automáticamente

### Criterios de Aceptación
- ✅ Terraform init, plan y apply ejecutados
- ✅ Plan visible en PR (comentario)
- ✅ Apply solo en push a master
- ✅ Outputs guardados para siguiente job
- ✅ State remoto configurado

### deploy.yml (Parte 2 - Terraform)

```yaml
  terraform:
    runs-on: ubuntu-latest
    needs: build-docker
    outputs:
      container-app-name: ${{ steps.output.outputs.container-app-name }}
      resource-group: ${{ steps.output.outputs.resource-group }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup Terraform
      uses: hashicorp/setup-terraform@v3
      with:
        terraform_version: 1.5.0
        terraform_wrapper: false

    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Terraform Init
      working-directory: ./terraform
      run: |
        terraform init \
          -backend-config="resource_group_name=${{ secrets.TERRAFORM_BACKEND_RG }}" \
          -backend-config="storage_account_name=${{ secrets.TERRAFORM_BACKEND_STORAGE }}" \
          -backend-config="container_name=tfstate" \
          -backend-config="key=api-devops.terraform.tfstate"

    - name: Terraform Plan
      working-directory: ./terraform
      run: |
        terraform plan \
          -var-file="environments/${{ github.event.inputs.environment || 'prod' }}.tfvars" \
          -var="sql_admin_password=${{ secrets.SQL_ADMIN_PASSWORD }}" \
          -out=tfplan

    - name: Terraform Apply
      if: github.ref == 'refs/heads/master'
      working-directory: ./terraform
      run: terraform apply -auto-approve tfplan

    - name: Get Terraform Outputs
      id: output
      working-directory: ./terraform
      run: |
        echo "container-app-name=$(terraform output -raw container_app_name)" >> $GITHUB_OUTPUT
        echo "resource-group=$(terraform output -raw resource_group_name)" >> $GITHUB_OUTPUT
        echo "container-app-url=$(terraform output -raw container_app_url)" >> $GITHUB_OUTPUT
```

### Tareas Técnicas
1. Agregar job terraform a `deploy.yml`
2. Configurar backend secrets si no existen
3. Probar con workflow_dispatch
4. Verificar outputs
5. Commit: "ci: Add Terraform deployment job"

### Dependencias
- ✅ US-031 (Terraform completo)
- ✅ US-032 (Secrets configurados)

### Estimación
**Esfuerzo**: 4 puntos (2 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Terraform apply exitoso desde GitHub
- Infraestructura actualizándose
- Outputs accesibles
- Plan visible en logs

---

## US-036: Crear Workflow de Deployment a Container App

**Como** DevOps engineer  
**Quiero** actualizar Container App con nueva imagen  
**Para** deployar la aplicación automáticamente

### Criterios de Aceptación
- ✅ Container App actualizado con nueva imagen
- ✅ Migraciones de EF Core aplicadas
- ✅ Health check post-deployment
- ✅ Rollback automático si falla
- ✅ Notificación de resultado

### deploy.yml (Parte 3 - Deploy)

```yaml
  deploy:
    runs-on: ubuntu-latest
    needs: [build-docker, terraform]
    environment:
      name: production
      url: ${{ steps.deploy.outputs.url }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Get latest image tag
      id: image-tag
      run: |
        IMAGE_TAG="${{ secrets.ACR_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:master-${{ github.sha }}"
        echo "tag=$IMAGE_TAG" >> $GITHUB_OUTPUT

    - name: Update Container App
      id: deploy
      run: |
        az containerapp update \
          --name ${{ needs.terraform.outputs.container-app-name }} \
          --resource-group ${{ needs.terraform.outputs.resource-group }} \
          --image ${{ steps.image-tag.outputs.tag }} \
          --query properties.configuration.ingress.fqdn \
          --output tsv | tee fqdn.txt
        
        APP_URL="https://$(cat fqdn.txt)"
        echo "url=$APP_URL" >> $GITHUB_OUTPUT

    - name: Wait for deployment
      run: sleep 30

    - name: Health Check
      id: health-check
      run: |
        MAX_RETRIES=10
        RETRY_COUNT=0
        
        while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
          HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" ${{ steps.deploy.outputs.url }}/health)
          
          if [ $HTTP_CODE -eq 200 ]; then
            echo "✅ Health check passed!"
            exit 0
          fi
          
          echo "⏳ Health check failed (HTTP $HTTP_CODE). Retry $((RETRY_COUNT+1))/$MAX_RETRIES"
          RETRY_COUNT=$((RETRY_COUNT+1))
          sleep 10
        done
        
        echo "❌ Health check failed after $MAX_RETRIES attempts"
        exit 1

    - name: Run Database Migrations
      if: steps.health-check.outcome == 'success'
      run: |
        echo "Running EF Core migrations..."
        az containerapp exec \
          --name ${{ needs.terraform.outputs.container-app-name }} \
          --resource-group ${{ needs.terraform.outputs.resource-group }} \
          --command "dotnet ef database update --no-build" || true

    - name: Deployment Summary
      if: always()
      run: |
        echo "### Deployment Summary 🚀" >> $GITHUB_STEP_SUMMARY
        echo "" >> $GITHUB_STEP_SUMMARY
        echo "- **Environment**: Production" >> $GITHUB_STEP_SUMMARY
        echo "- **Image**: ${{ steps.image-tag.outputs.tag }}" >> $GITHUB_STEP_SUMMARY
        echo "- **URL**: ${{ steps.deploy.outputs.url }}" >> $GITHUB_STEP_SUMMARY
        echo "- **Status**: ${{ steps.health-check.outcome }}" >> $GITHUB_STEP_SUMMARY
        echo "" >> $GITHUB_STEP_SUMMARY
        echo "**Swagger**: ${{ steps.deploy.outputs.url }}/swagger" >> $GITHUB_STEP_SUMMARY

    - name: Rollback on Failure
      if: failure()
      run: |
        echo "⚠️ Deployment failed. Consider manual rollback."
        # Implementar lógica de rollback aquí
```

### Tareas Técnicas
1. Agregar job deploy a `deploy.yml`
2. Configurar environment "production" en GitHub
3. Probar deployment completo
4. Verificar health check
5. Commit: "ci: Add Container App deployment with health checks"

### Dependencias
- ✅ US-034 (Docker image push)
- ✅ US-035 (Terraform apply)

### Estimación
**Esfuerzo**: 5 puntos (2.5 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Deployment automático funcionando
- Health check validando
- URL accesible post-deployment
- Resumen visible en GitHub

---

## US-037: Documentar Pipeline CI/CD

**Como** miembro del equipo  
**Quiero** documentación completa del pipeline  
**Para** entender y mantener el proceso de deployment

### Criterios de Aceptación
- ✅ README actualizado con sección CI/CD
- ✅ Diagramas de flujo del pipeline
- ✅ Instrucciones para configurar secrets
- ✅ Troubleshooting de problemas comunes
- ✅ Proceso de rollback documentado

### Documentación a Crear

**README.md - Sección CI/CD**

```markdown
## 🚀 CI/CD Pipeline

### Workflow Overview

El proyecto utiliza GitHub Actions para automatizar el build, test y deployment.

#### Workflows

1. **Build and Test** (`.github/workflows/build-test.yml`)
   - Trigger: Pull Requests a `master` o `develop`
   - Acciones:
     - Build del proyecto .NET
     - Ejecución de tests unitarios
     - Reporte de code coverage

2. **Deploy to Azure** (`.github/workflows/deploy.yml`)
   - Trigger: Push a `master` o manual
   - Acciones:
     - Build y push de imagen Docker a ACR
     - Terraform apply (infraestructura)
     - Deploy a Azure Container Apps
     - Health checks post-deployment

### Pipeline Flow

```
┌─────────────┐
│ Push/PR     │
└──────┬──────┘
       │
       ├─────────────────────┐
       │                     │
┌──────▼──────┐       ┌──────▼──────────┐
│ Build & Test│       │ Build Docker     │
│ (on PR)     │       │ (on master)      │
└─────────────┘       └──────┬───────────┘
                             │
                      ┌──────▼──────────┐
                      │ Terraform Apply │
                      └──────┬──────────┘
                             │
                      ┌──────▼──────────┐
                      │ Deploy to Azure │
                      └──────┬──────────┘
                             │
                      ┌──────▼──────────┐
                      │ Health Check    │
                      └─────────────────┘
```

### Configuración de Secrets

Ver [SECRETS.md](./docs/SECRETS.md) para instrucciones detalladas.

Secrets requeridos:
- `AZURE_CREDENTIALS`
- `AZURE_SUBSCRIPTION_ID`
- `ACR_LOGIN_SERVER`
- `ACR_USERNAME`
- `ACR_PASSWORD`
- `SQL_ADMIN_PASSWORD`

### Manual Deployment

Para hacer un deployment manual:

1. Ve a **Actions** → **Deploy to Azure**
2. Click **Run workflow**
3. Selecciona el ambiente (dev/prod)
4. Click **Run workflow**

### Rollback

En caso de deployment fallido:

1. **Opción 1: Revert commit**
   ```bash
   git revert HEAD
   git push origin master
   ```

2. **Opción 2: Deployment manual de versión anterior**
   - Ir a Actions → Deploy to Azure
   - Seleccionar commit específico

3. **Opción 3: Azure Portal**
   - Container Apps → Revisions
   - Seleccionar revisión anterior
   - Activate

### Troubleshooting

**Deployment falla en health check**
- Verificar logs en Azure Portal
- Verificar connection string de SQL
- Verificar que migraciones se aplicaron

**Terraform apply falla**
- Verificar Service Principal permisos
- Revisar terraform state
- Ejecutar manualmente: `terraform plan`

**Imagen Docker no se construye**
- Verificar Dockerfile paths
- Revisar logs de Docker build
- Verificar ACR credentials
```

### Tareas Técnicas
1. Crear `docs/SECRETS.md` con instrucciones detalladas
2. Actualizar README.md con sección CI/CD
3. Crear diagramas (ASCII art o Mermaid)
4. Documentar troubleshooting común
5. Commit: "docs: Add comprehensive CI/CD documentation"

### Dependencias
- ✅ US-036 (Pipeline completo funcionando)

### Estimación
**Esfuerzo**: 3 puntos (1.5 horas)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- Documentación completa y clara
- Diagramas incluidos
- Troubleshooting útil
- Revisado por equipo

---

## 📋 Resumen Sprint 6

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-032 | Configurar GitHub Secrets | 🔴 Crítica | 2 pts | ⏳ Pendiente |
| US-033 | Workflow Build y Test | 🔴 Crítica | 3 pts | ⏳ Pendiente |
| US-034 | Workflow Build Docker | 🔴 Crítica | 3 pts | ⏳ Pendiente |
| US-035 | Workflow Terraform Apply | 🔴 Crítica | 4 pts | ⏳ Pendiente |
| US-036 | Workflow Deploy Container App | 🔴 Crítica | 5 pts | ⏳ Pendiente |
| US-037 | Documentar Pipeline CI/CD | 🟢 Media | 3 pts | ⏳ Pendiente |

**Total Sprint 6**: 20 puntos (~10 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-06-terraform.md](./user-stories-06-terraform.md)
- **➡️ Siguiente**: N/A (Última parte)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
