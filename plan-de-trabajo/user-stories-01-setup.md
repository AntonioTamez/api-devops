# 📖 Historias de Usuario - Parte 1: Setup Inicial
**Identificador**: US-01-SETUP  
**Orden de lectura**: 1 de 8  
**Sprint**: Sprint 0 - Configuración Base  

---

## 🎯 Objetivo del Sprint 0
Establecer la base del proyecto con la estructura de directorios, configuración de Git y herramientas necesarias.

---

## US-001: Inicializar Repositorio Git ✅

**Estado**: ✅ **COMPLETADA**

**Como** arquitecto de software  
**Quiero** inicializar un repositorio Git con .gitignore configurado  
**Para** tener control de versiones y evitar subir archivos innecesarios

### Criterios de Aceptación
- ✅ Repositorio Git inicializado en `c:/ATS/GIT/api-devops`
- ✅ Archivo `.gitignore` creado con exclusiones de .NET, Docker, Terraform, IDE
- ✅ Commit inicial realizado
- ✅ Rama `master` como rama principal
- ✅ Rama `develop` creada para desarrollo

### Tareas Técnicas
1. Ejecutar `git init`
2. Crear `.gitignore` con plantillas de:
   - Visual Studio / Rider
   - .NET (bin/, obj/, *.user)
   - Terraform (.terraform/, *.tfstate, *.tfvars)
   - Docker (volumes, logs)
   - Secrets (appsettings.*.json excepto Development)
3. Commit inicial: "Initial commit: Project structure"
4. Crear rama develop

### Dependencias
- Ninguna

### Estimación
**Esfuerzo**: 1 punto (30 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- ✅ Repositorio inicializado
- ✅ `.gitignore` funcionando correctamente
- ✅ Ramas master y develop creadas
- ⏳ Documentado en README.md inicial (US-003)

---

## US-002: Crear Estructura de Directorios ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador  
**Quiero** tener la estructura de directorios del proyecto organizada  
**Para** mantener el código limpio y fácil de navegar

### Criterios de Aceptación
- ✅ Estructura de carpetas creada según plan.md
- ✅ Archivos placeholder (README.md por carpeta)
- ✅ Permisos correctos en archivos sensibles

### Estructura a Crear
```
api-devops/
├── src/                    # Código fuente del API
├── tests/                  # Tests unitarios e integración
├── terraform/              # IaC para Azure
│   └── environments/       # Configs por ambiente
├── .github/                # CI/CD workflows
│   └── workflows/
└── docs/                   # Documentación adicional
```

### Tareas Técnicas
1. Crear directorios principales: src, tests, terraform, .github/workflows, docs
2. Crear subdirectorio: terraform/environments
3. Crear README.md placeholder en cada carpeta raíz
4. Commit: "feat: Add project folder structure"

### Dependencias
- ✅ US-001 (Repositorio Git)

### Estimación
**Esfuerzo**: 1 punto (15 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- ✅ Todos los directorios creados
- ✅ README.md en carpetas principales
- ✅ Estructura commiteada

---

## US-003: Crear Archivo README.md Principal ✅

**Estado**: ✅ **COMPLETADA**

**Como** miembro del equipo  
**Quiero** un README.md completo en la raíz del proyecto  
**Para** entender rápidamente cómo usar el proyecto

### Criterios de Aceptación
- ✅ README.md con descripción del proyecto
- ✅ Badges de build status, version
- ✅ Prerequisitos claramente listados
- ✅ Instrucciones de setup local
- ✅ Comandos principales documentados
- ✅ Links a documentación adicional
- ✅ Información de contacto del equipo

### Contenido del README
1. **Título y descripción**
2. **Badges**: Build status, Coverage, License
3. **Stack tecnológico**: .NET 8, Azure, Terraform, Docker
4. **Prerequisitos**:
   - .NET 8 SDK
   - Docker Desktop
   - Terraform >= 1.5
   - Azure CLI
5. **Quick Start**: Comandos para levantar local
6. **Estructura del proyecto**
7. **Desarrollo local**
8. **Testing**
9. **Deployment**
10. **Troubleshooting**
11. **Contribución**
12. **Licencia y contacto**

### Tareas Técnicas
1. Crear README.md con template completo
2. Agregar placeholders para badges (se actualizarán con CI/CD)
3. Documentar comandos básicos
4. Incluir diagrama de arquitectura (ASCII art o link a diagrama)
5. Commit: "docs: Add comprehensive README"

### Dependencias
- ✅ US-001 (Repositorio Git)
- ✅ US-002 (Estructura de directorios)

### Estimación
**Esfuerzo**: 2 puntos (1 hora)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- ✅ README.md completo y actualizado
- ✅ Todas las secciones llenas
- ✅ Links funcionando
- ⏳ Revisado por al menos un miembro del equipo (en review)

---

## US-004: Configurar EditorConfig y Code Style ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador  
**Quiero** tener reglas de estilo de código consistentes  
**Para** mantener la calidad y legibilidad del código

### Criterios de Aceptación
- ✅ Archivo `.editorconfig` creado con reglas de C#
- ✅ Reglas de indentación, espacios, line endings
- ✅ Convenciones de nombres C# (PascalCase, camelCase)
- ✅ Reglas aplicables en todos los IDEs (VS, VSCode, Rider)

### Configuración Incluida
1. **General**:
   - Charset: UTF-8
   - End of line: CRLF (Windows)
   - Indent style: spaces
   - Indent size: 4
   - Trim trailing whitespace

2. **C# Específico**:
   - PascalCase para clases, métodos, propiedades
   - camelCase para variables locales, parámetros
   - _camelCase para campos privados
   - Usar `var` cuando es obvio
   - Preferir expression bodies cuando sea conciso
   - Requerir braces en if/else/for/while

3. **Otros archivos**:
   - YAML: 2 espacios
   - JSON: 2 espacios
   - Markdown: 2 espacios

### Tareas Técnicas
1. Crear `.editorconfig` en la raíz
2. Configurar reglas para C#, JSON, YAML, Markdown
3. Probar en Visual Studio / Rider
4. Commit: "chore: Add EditorConfig for code style consistency"

### Dependencias
- ✅ US-001 (Repositorio Git)

### Estimación
**Esfuerzo**: 1 punto (30 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- ✅ `.editorconfig` creado
- ✅ Reglas aplicándose correctamente en IDE
- ✅ Documentado en README.md

---

## 📋 Resumen Sprint 0

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-001 | Inicializar Repositorio Git | 🔴 Crítica | 1 pt | ✅ Completada |
| US-002 | Crear Estructura de Directorios | 🔴 Crítica | 1 pt | ✅ Completada |
| US-003 | Crear README.md Principal | 🟡 Alta | 2 pts | ✅ Completada |
| US-004 | EditorConfig y Code Style | 🟢 Media | 1 pt | ✅ Completada |

**Total Sprint 0**: 5 puntos (~3 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: N/A (Primera parte)
- **➡️ Siguiente**: [user-stories-02-api-base.md](./user-stories-02-api-base.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
