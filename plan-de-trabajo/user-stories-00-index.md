# 📚 Índice General de Historias de Usuario
## API DevOps - .NET 8 + Azure + Terraform + CI/CD

**Proyecto**: API REST con DevOps Automatizado  
**Stack**: .NET 8, Azure, SQL Server, Docker, Terraform, GitHub Actions  
**Total de Historias**: 37  
**Total de Puntos**: 104 (~52 horas de desarrollo)  

---

## 📖 Guía de Uso

Este documento es el **punto de entrada** para todas las historias de usuario del proyecto. Las historias están organizadas en **7 partes** que deben implementarse **en orden secuencial** para garantizar dependencias correctas.

### Convenciones

- **🔴 Crítica**: Debe completarse antes de continuar
- **🟡 Alta**: Importante pero puede priorizarse
- **🟢 Media**: Puede implementarse después si es necesario
- **⏳ Pendiente**: No iniciada
- **🚧 En progreso**: Actualmente en desarrollo
- **✅ Completada**: Historia terminada y probada

---

## 📋 Resumen Ejecutivo por Sprint

| Sprint | Nombre | Historias | Puntos | Tiempo Estimado |
|--------|--------|-----------|--------|-----------------|
| Sprint 0 | Setup Inicial | US-001 a US-004 | 5 pts | ~3 horas |
| Sprint 1 | API Base | US-005 a US-010 | 14 pts | ~7 horas |
| Sprint 2 | Base de Datos | US-011 a US-016 | 20 pts | ~10 horas |
| Sprint 3 | Containerización | US-017 a US-021 | 14 pts | ~7 horas |
| Sprint 4 | Testing | US-022 a US-025 | 14 pts | ~7 horas |
| Sprint 5 | Terraform/Azure | US-026 a US-031 | 17 pts | ~8.5 horas |
| Sprint 6 | CI/CD | US-032 a US-037 | 20 pts | ~10 horas |
| **TOTAL** | | **37 historias** | **104 pts** | **~52 horas** |

---

## 🗂️ Estructura de Archivos

```
api-devops/
└── plan-de-trabajo/
    ├── plan.md                           # Plan maestro del proyecto
    ├── user-stories-00-index.md          # Este archivo (índice)
    ├── user-stories-01-setup.md          # Sprint 0: Setup
    ├── user-stories-02-api-base.md       # Sprint 1: API Base
    ├── user-stories-03-database.md       # Sprint 2: Base de Datos
    ├── user-stories-04-docker.md         # Sprint 3: Docker
    ├── user-stories-05-testing.md        # Sprint 4: Testing
    ├── user-stories-06-terraform.md      # Sprint 5: Terraform
    └── user-stories-07-cicd.md           # Sprint 6: CI/CD
```

---

## 📦 Sprint 0: Setup Inicial (US-01-SETUP)

**Archivo**: [user-stories-01-setup.md](./user-stories-01-setup.md)  
**Objetivo**: Establecer base del proyecto  
**Puntos**: 5 (~3 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-001 | Inicializar Repositorio Git | 🔴 | 1 pt | ✅ |
| US-002 | Crear Estructura de Directorios | 🔴 | 1 pt | ✅ |
| US-003 | Crear README.md Principal | 🟡 | 2 pts | ✅ |
| US-004 | Configurar EditorConfig | 🟢 | 1 pt | ⏳ |

**Entregables**:
- ✅ Repositorio Git inicializado
- ✅ Estructura de carpetas completa
- ✅ README.md documentado
- ✅ .gitignore y .editorconfig configurados

---

## 🔌 Sprint 1: API Base .NET 8 (US-02-API-BASE)

**Archivo**: [user-stories-02-api-base.md](./user-stories-02-api-base.md)  
**Objetivo**: Crear API funcional con Swagger  
**Puntos**: 14 (~7 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-005 | Crear Proyecto Web API .NET 8 | 🔴 | 2 pts | ⏳ |
| US-006 | Configurar Swagger/OpenAPI | 🔴 | 2 pts | ⏳ |
| US-007 | Implementar Health Checks | 🟡 | 3 pts | ⏳ |
| US-008 | Configurar CORS | 🟢 | 2 pts | ⏳ |
| US-009 | Crear Endpoints Minimal API | 🟢 | 2 pts | ⏳ |
| US-010 | Configurar Logging (Serilog) | 🟡 | 3 pts | ⏳ |

**Entregables**:
- ✅ API .NET 8 ejecutable
- ✅ Swagger UI accesible en raíz
- ✅ Health checks funcionando
- ✅ Endpoints de ejemplo (status, info)
- ✅ Logging estructurado configurado

---

## 🗄️ Sprint 2: Base de Datos (US-03-DATABASE)

**Archivo**: [user-stories-03-database.md](./user-stories-03-database.md)  
**Objetivo**: Integrar SQL Server con EF Core  
**Puntos**: 20 (~10 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-011 | Configurar Entity Framework Core | 🔴 | 3 pts | ⏳ |
| US-012 | Crear Modelo Product | 🔴 | 3 pts | ⏳ |
| US-013 | Crear Migraciones EF Core | 🔴 | 2 pts | ⏳ |
| US-014 | Repository y Service Layer | 🔴 | 5 pts | ⏳ |
| US-015 | ProductsController CRUD | 🔴 | 5 pts | ⏳ |
| US-016 | Health Check SQL Server | 🟡 | 2 pts | ⏳ |

**Entregables**:
- ✅ EF Core configurado con SQL Server
- ✅ Modelo Product con configuración Fluent API
- ✅ Migraciones creadas
- ✅ Service layer implementado
- ✅ CRUD completo de productos
- ✅ Health check de base de datos

---

## 🐳 Sprint 3: Containerización (US-04-DOCKER)

**Archivo**: [user-stories-04-docker.md](./user-stories-04-docker.md)  
**Objetivo**: Contenedorizar aplicación  
**Puntos**: 14 (~7 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-017 | Crear Dockerfile Multi-Stage | 🔴 | 3 pts | ⏳ |
| US-018 | Crear .dockerignore | 🟢 | 1 pt | ⏳ |
| US-019 | Docker Compose Local | 🔴 | 5 pts | ⏳ |
| US-020 | Auto-migration en Startup | 🟡 | 3 pts | ⏳ |
| US-021 | Documentar Docker | 🟢 | 2 pts | ⏳ |

**Entregables**:
- ✅ Dockerfile optimizado (< 200MB)
- ✅ docker-compose.yml funcional (API + SQL Server)
- ✅ Migraciones automáticas al iniciar
- ✅ Ambiente local completo con un comando
- ✅ Documentación de comandos Docker

---

## 🧪 Sprint 4: Testing (US-05-TESTING)

**Archivo**: [user-stories-05-testing.md](./user-stories-05-testing.md)  
**Objetivo**: Implementar tests unitarios  
**Puntos**: 14 (~7 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-022 | Crear Proyecto Tests Unitarios | 🟡 | 2 pts | ⏳ |
| US-023 | Tests ProductService | 🟡 | 5 pts | ⏳ |
| US-024 | Tests ProductsController | 🟡 | 5 pts | ⏳ |
| US-025 | Configurar Code Coverage | 🟢 | 2 pts | ⏳ |

**Entregables**:
- ✅ Proyecto xUnit creado
- ✅ Tests de ProductService (> 80% coverage)
- ✅ Tests de ProductsController
- ✅ Reportes de coverage en HTML
- ✅ Tests ejecutables en CI

---

## ☁️ Sprint 5: Terraform/Azure (US-06-TERRAFORM)

**Archivo**: [user-stories-06-terraform.md](./user-stories-06-terraform.md)  
**Objetivo**: Infraestructura como código  
**Puntos**: 17 (~8.5 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-026 | Configurar Terraform Providers | 🔴 | 3 pts | ⏳ |
| US-027 | Crear Variables Terraform | 🔴 | 2 pts | ⏳ |
| US-028 | Recursos Base Azure | 🔴 | 3 pts | ⏳ |
| US-029 | SQL Server y Database | 🔴 | 3 pts | ⏳ |
| US-030 | Container Apps | 🔴 | 5 pts | ⏳ |
| US-031 | Terraform Outputs | 🟡 | 1 pt | ⏳ |

**Entregables**:
- ✅ Terraform configurado con Azure
- ✅ Variables por ambiente (dev, prod)
- ✅ Resource Group y ACR creados
- ✅ Azure SQL Database provisionado
- ✅ Container Apps configurado
- ✅ Outputs para CI/CD

**Recursos Azure Creados**:
1. Resource Group
2. Container Registry (ACR)
3. Container App Environment
4. Container App
5. SQL Server
6. SQL Database
7. Application Insights
8. Log Analytics Workspace

---

## 🚀 Sprint 6: CI/CD (US-07-CICD)

**Archivo**: [user-stories-07-cicd.md](./user-stories-07-cicd.md)  
**Objetivo**: Pipeline automatizado  
**Puntos**: 20 (~10 horas)

| ID | Historia | Prioridad | Puntos | Estado |
|---|---|---|---|---|
| US-032 | Configurar GitHub Secrets | 🔴 | 2 pts | ⏳ |
| US-033 | Workflow Build y Test | 🔴 | 3 pts | ⏳ |
| US-034 | Workflow Build Docker | 🔴 | 3 pts | ⏳ |
| US-035 | Workflow Terraform Apply | 🔴 | 4 pts | ⏳ |
| US-036 | Workflow Deploy Container App | 🔴 | 5 pts | ⏳ |
| US-037 | Documentar Pipeline CI/CD | 🟢 | 3 pts | ⏳ |

**Entregables**:
- ✅ GitHub Secrets configurados
- ✅ Workflow de build/test en PRs
- ✅ Workflow de deployment en master
- ✅ Docker image push a ACR
- ✅ Terraform apply automatizado
- ✅ Deploy a Container Apps con health check
- ✅ Documentación completa de CI/CD

**Flujo Automatizado**:
```
PR → Build + Test → Merge → Build Docker → Terraform Apply → Deploy → Health Check
```

---

## 🎯 Hoja de Ruta de Implementación

### Semana 1: Fundamentos
- ✅ Sprint 0: Setup (Día 1)
- ✅ Sprint 1: API Base (Días 2-3)

### Semana 2: Datos y Contenedores
- ✅ Sprint 2: Base de Datos (Días 4-6)
- ✅ Sprint 3: Docker (Día 7)

### Semana 3: Calidad e Infraestructura
- ✅ Sprint 4: Testing (Día 8)
- ✅ Sprint 5: Terraform (Días 9-10)

### Semana 4: Automatización
- ✅ Sprint 6: CI/CD (Días 11-13)
- ✅ Refinamiento y documentación (Días 14-15)

---

## 📊 Métricas del Proyecto

### Por Prioridad
- 🔴 **Crítica**: 20 historias (54%)
- 🟡 **Alta**: 10 historias (27%)
- 🟢 **Media**: 7 historias (19%)

### Por Sprint
- **Sprint 0** (Setup): 4 historias, 5 puntos
- **Sprint 1** (API): 6 historias, 14 puntos
- **Sprint 2** (DB): 6 historias, 20 puntos
- **Sprint 3** (Docker): 5 historias, 14 puntos
- **Sprint 4** (Testing): 4 historias, 14 puntos
- **Sprint 5** (Terraform): 6 historias, 17 puntos
- **Sprint 6** (CI/CD): 6 historias, 20 puntos

### Distribución de Esfuerzo
```
Sprint 2 (DB):      ████████████████████  20 pts
Sprint 6 (CI/CD):   ████████████████████  20 pts
Sprint 5 (TF):      █████████████████     17 pts
Sprint 1 (API):     ██████████████        14 pts
Sprint 3 (Docker):  ██████████████        14 pts
Sprint 4 (Testing): ██████████████        14 pts
Sprint 0 (Setup):   █████                  5 pts
```

---

## ✅ Checklist de Completitud

### Prerequisites
- [ ] Azure Subscription activa
- [ ] GitHub Repository creado
- [ ] Azure CLI instalado
- [ ] Terraform instalado (>= 1.5.0)
- [ ] Docker Desktop instalado
- [ ] .NET 8 SDK instalado

### Sprint 0: Setup
- [x] US-001: Git inicializado
- [x] US-002: Estructura de carpetas
- [x] US-003: README.md completo
- [ ] US-004: EditorConfig configurado

### Sprint 1: API Base
- [ ] US-005: Proyecto .NET 8 creado
- [ ] US-006: Swagger configurado
- [ ] US-007: Health checks implementados
- [ ] US-008: CORS configurado
- [ ] US-009: Endpoints de ejemplo
- [ ] US-010: Logging configurado

### Sprint 2: Base de Datos
- [ ] US-011: EF Core configurado
- [ ] US-012: Modelo Product creado
- [ ] US-013: Migraciones creadas
- [ ] US-014: Service layer implementado
- [ ] US-015: CRUD completo
- [ ] US-016: Health check SQL

### Sprint 3: Docker
- [ ] US-017: Dockerfile multi-stage
- [ ] US-018: .dockerignore creado
- [ ] US-019: docker-compose funcionando
- [ ] US-020: Auto-migrations
- [ ] US-021: Documentación Docker

### Sprint 4: Testing
- [ ] US-022: Proyecto tests creado
- [ ] US-023: Tests ProductService
- [ ] US-024: Tests ProductsController
- [ ] US-025: Code coverage configurado

### Sprint 5: Terraform
- [ ] US-026: Providers configurados
- [ ] US-027: Variables creadas
- [ ] US-028: Recursos base provisioned
- [ ] US-029: SQL Database creado
- [ ] US-030: Container Apps creado
- [ ] US-031: Outputs configurados

### Sprint 6: CI/CD
- [ ] US-032: Secrets configurados
- [ ] US-033: Workflow build/test
- [ ] US-034: Workflow Docker build
- [ ] US-035: Workflow Terraform
- [ ] US-036: Workflow deployment
- [ ] US-037: Documentación CI/CD

---

## 🔍 Dependencias entre Sprints

```
Sprint 0 (Setup)
    ↓
Sprint 1 (API Base)
    ↓
Sprint 2 (Database) ←─────┐
    ↓                     │
Sprint 3 (Docker) ────────┤
    ↓                     │
Sprint 4 (Testing)        │
    ↓                     │
Sprint 5 (Terraform) ─────┘
    ↓
Sprint 6 (CI/CD)
```

**Nota**: Sprint 4 (Testing) puede implementarse en paralelo con Sprint 3 si se desea.

---

## 📞 Soporte y Contacto

- **Documentación**: [plan.md](./plan.md)
- **Issues**: GitHub Issues del proyecto
- **Email**: devops@example.com

---

## 📝 Notas Importantes

1. **Orden Secuencial**: Seguir el orden de sprints para evitar problemas de dependencias
2. **Verificación**: Cada historia debe pasar su "Definición de Hecho" antes de continuar
3. **Commits**: Usar mensajes de commit descriptivos según convenciones
4. **Branches**: Crear feature branches para cada historia (ej: `feature/US-001-git-init`)
5. **Testing**: Probar cada componente antes de integrar
6. **Documentación**: Actualizar README.md conforme se avanza

---

## 🔄 Control de Versiones

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | Oct 2025 | Versión inicial completa |

---

**Última actualización**: Octubre 2025  
**Total de historias**: 37  
**Estado del proyecto**: 🟢 Listo para iniciar implementación

---

## 🚀 ¡Comencemos!

Para iniciar el proyecto, dirígete a:  
👉 **[user-stories-01-setup.md](./user-stories-01-setup.md)**
