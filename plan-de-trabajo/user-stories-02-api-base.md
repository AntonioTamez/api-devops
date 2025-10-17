# 📖 Historias de Usuario - Parte 2: API Base .NET 8
**Identificador**: US-02-API-BASE  
**Orden de lectura**: 2 de 8  
**Sprint**: Sprint 1 - API Básico  

---

## 🎯 Objetivo del Sprint 1
Crear el proyecto base de API .NET 8 con Swagger, configuración básica y endpoints de prueba.

---

## US-005: Crear Proyecto Web API .NET 8

**Como** desarrollador backend  
**Quiero** crear el proyecto base de Web API en .NET 8  
**Para** tener la estructura inicial del servicio REST

### Criterios de Aceptación
- ✅ Proyecto .NET 8 Web API creado en carpeta `src/`
- ✅ Template Web API con controllers
- ✅ Swagger/OpenAPI habilitado por defecto
- ✅ Proyecto compila sin errores
- ✅ Proyecto ejecutable localmente

### Tareas Técnicas
1. Ejecutar comando:
   ```bash
   cd c:/ATS/GIT/api-devops
   dotnet new webapi -n DevOpsApi -o src --framework net8.0
   ```
2. Verificar estructura creada:
   - Program.cs
   - DevOpsApi.csproj
   - Properties/launchSettings.json
   - appsettings.json
   - WeatherForecastController (eliminar después)
3. Ejecutar: `dotnet build`
4. Ejecutar: `dotnet run` y verificar que levanta
5. Verificar Swagger en: https://localhost:5001/swagger
6. Commit: "feat: Initialize .NET 8 Web API project"

### Dependencias
- ✅ US-002 (Estructura de directorios)

### Estimación
**Esfuerzo**: 2 puntos (30 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Proyecto creado y compilando
- Ejecutable localmente
- Swagger accesible
- README actualizado con comandos de ejecución

---

## US-006: Configurar Swagger/OpenAPI

**Como** consumidor de la API  
**Quiero** documentación interactiva con Swagger  
**Para** explorar y probar los endpoints fácilmente

### Criterios de Aceptación
- ✅ Swagger UI personalizado con información del proyecto
- ✅ Swagger disponible en raíz (`/`) en desarrollo
- ✅ Swagger también disponible en producción (configurable)
- ✅ Título, versión y descripción personalizados
- ✅ Información de contacto del equipo
- ✅ Documentación XML habilitada (comentarios C#)

### Configuración de Swagger
```csharp
SwaggerDoc("v1", new OpenApiInfo
{
    Title = "DevOps API",
    Version = "v1",
    Description = "API REST automatizada con CI/CD y Terraform",
    Contact = new OpenApiContact
    {
        Name = "DevOps Team",
        Email = "devops@example.com",
        Url = new Uri("https://github.com/your-org/api-devops")
    },
    License = new OpenApiLicense
    {
        Name = "Uso Interno",
    }
});
```

### Tareas Técnicas
1. Modificar `Program.cs`:
   - Personalizar `SwaggerDoc`
   - Configurar `SwaggerUI` en raíz: `c.RoutePrefix = string.Empty;`
   - Habilitar en producción (condicionalmente)
2. Habilitar comentarios XML:
   - En `.csproj`: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
   - Configurar Swagger para usar XML comments
3. Probar navegando a http://localhost:5000
4. Commit: "feat: Configure Swagger with custom metadata"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Swagger UI personalizado funcionando
- Accesible en raíz
- Comentarios XML visibles en Swagger
- Screenshot en documentación

---

## US-007: Implementar Health Checks

**Como** ingeniero de SRE  
**Quiero** endpoints de health check  
**Para** monitorear el estado del API y sus dependencias

### Criterios de Aceptación
- ✅ Endpoint `/health` retorna estado general
- ✅ Endpoint `/health/ready` para readiness probe
- ✅ Endpoint `/health/live` para liveness probe
- ✅ Health checks incluyen: memoria, disco (preparados para SQL después)
- ✅ Respuestas en formato JSON con detalles
- ✅ Códigos HTTP apropiados (200, 503)

### Endpoints de Health Check

**GET /health**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "memory": {
      "status": "Healthy",
      "description": "Memory usage is normal"
    }
  }
}
```

**GET /health/ready**
- Verifica que el API está listo para recibir tráfico
- Status 200 si está ready, 503 si no

**GET /health/live**
- Verifica que el proceso está vivo
- Status 200 si está vivo, 503 si debe reiniciarse

### Tareas Técnicas
1. Agregar paquete NuGet:
   ```bash
   dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
   ```
2. Configurar en `Program.cs`:
   ```csharp
   builder.Services.AddHealthChecks();
   
   app.MapHealthChecks("/health");
   app.MapHealthChecks("/health/ready", new HealthCheckOptions
   {
       Predicate = check => check.Tags.Contains("ready")
   });
   app.MapHealthChecks("/health/live", new HealthCheckOptions
   {
       Predicate = _ => false // Solo verifica que el proceso está vivo
   });
   ```
3. Crear `HealthController.cs` con endpoint detallado (opcional)
4. Probar los 3 endpoints
5. Commit: "feat: Add health check endpoints"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- 3 endpoints de health check funcionando
- Respuestas JSON válidas
- Documentado en Swagger
- README actualizado con endpoints

---

## US-008: Configurar CORS

**Como** desarrollador frontend  
**Quiero** que el API permita CORS  
**Para** poder consumirlo desde aplicaciones web

### Criterios de Aceptación
- ✅ CORS configurado en `Program.cs`
- ✅ Política permisiva en desarrollo (AllowAny)
- ✅ Política restrictiva en producción (origins específicos)
- ✅ Permite todos los métodos HTTP necesarios
- ✅ Permite headers comunes (Content-Type, Authorization)
- ✅ Configuración por ambiente (appsettings)

### Configuración CORS

**appsettings.Development.json**
```json
{
  "Cors": {
    "AllowedOrigins": ["*"]
  }
}
```

**appsettings.Production.json**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://yourdomain.com",
      "https://app.yourdomain.com"
    ]
  }
}
```

### Tareas Técnicas
1. Configurar CORS en `Program.cs`:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("ApiPolicy", builder =>
       {
           var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
           if (origins?.Contains("*") == true)
           {
               builder.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
           }
           else
           {
               builder.WithOrigins(origins ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
           }
       });
   });
   
   app.UseCors("ApiPolicy");
   ```
2. Agregar configuración en appsettings
3. Probar con CORS desde navegador (fetch desde consola)
4. Commit: "feat: Configure CORS with environment-specific policies"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- CORS funcionando en dev y prod
- Configuración por ambiente
- Probado con fetch desde navegador
- Documentado en README

---

## US-009: Crear StatusController con Endpoints de Ejemplo

**Como** desarrollador  
**Quiero** endpoints de ejemplo usando Controllers  
**Para** demostrar la funcionalidad básica del servicio

### Criterios de Aceptación
- ✅ Endpoint `GET /api/status` - Retorna estado del API
- ✅ Endpoint `GET /api/info` - Retorna información del proyecto
- ✅ Respuestas en formato JSON
- ✅ Documentados en Swagger automáticamente
- ✅ Incluyen timestamp y metadata útil
- ✅ Controller con atributos `[ApiController]` y `[Route]`

### Endpoints a Crear

**GET /api/status**
```json
{
  "status": "Running",
  "timestamp": "2025-10-15T20:00:00Z",
  "environment": "Development",
  "version": "1.0.0",
  "uptime": "00:15:23"
}
```

**GET /api/info**
```json
{
  "apiName": "DevOps API",
  "description": "API con CI/CD automatizado",
  "features": [
    "Swagger",
    "Docker",
    "Terraform",
    "GitHub Actions",
    "SQL Server"
  ],
  "repository": "https://github.com/your-org/api-devops",
  "documentation": "https://localhost:5000"
}
```

### Tareas Técnicas
1. Crear carpeta `Controllers/` en el proyecto
2. Crear `StatusController.cs`:
   ```csharp
   using Microsoft.AspNetCore.Mvc;
   
   namespace DevOpsApi.Controllers;
   
   [ApiController]
   [Route("api/[controller]")]
   public class StatusController : ControllerBase
   {
       private readonly IHostEnvironment _environment;
       
       public StatusController(IHostEnvironment environment)
       {
           _environment = environment;
       }
       
       /// <summary>
       /// Obtiene el estado actual del API
       /// </summary>
       [HttpGet]
       [ProducesResponseType(StatusCodes.Status200OK)]
       public IActionResult GetStatus()
       {
           return Ok(new
           {
               Status = "Running",
               Timestamp = DateTime.UtcNow,
               Environment = _environment.EnvironmentName,
               Version = "1.0.0"
           });
       }
       
       /// <summary>
       /// Obtiene información del proyecto
       /// </summary>
       [HttpGet("info")]
       [ProducesResponseType(StatusCodes.Status200OK)]
       public IActionResult GetInfo()
       {
           return Ok(new
           {
               ApiName = "DevOps API",
               Description = "API con CI/CD automatizado",
               Features = new[] { "Swagger", "Docker", "Terraform", "GitHub Actions" },
               Repository = "https://github.com/your-org/api-devops",
               Documentation = "https://localhost:5000",
               Timestamp = DateTime.UtcNow
           });
       }
   }
   ```
3. Probar endpoints en Swagger
4. Verificar respuestas JSON
5. Commit: "feat: Add StatusController with example endpoints"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)
- ✅ US-006 (Swagger configurado)

### Estimación
**Esfuerzo**: 2 puntos (30 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- Endpoints funcionando
- Documentados en Swagger
- Respuestas JSON válidas
- Ejemplos en README

---

## US-010: Configurar Logging Estructurado

**Como** ingeniero de SRE  
**Quiero** logs estructurados con Serilog  
**Para** facilitar el debugging y monitoreo en producción

### Criterios de Aceptación
- ✅ Serilog configurado como provider de logging
- ✅ Logs en consola con formato legible en desarrollo
- ✅ Logs en formato JSON en producción
- ✅ Niveles de log configurables por ambiente
- ✅ Correlación de requests con Request ID
- ✅ Logs enriquecidos con contexto (environment, version)

### Configuración de Serilog

**Niveles de Log por Ambiente:**
- Development: Debug
- Production: Information

**Sinks:**
- Console (desarrollo): formato simple
- Console (producción): formato JSON
- (Preparado para) Application Insights

### Tareas Técnicas
1. Instalar paquetes NuGet:
   ```bash
   dotnet add package Serilog.AspNetCore
   dotnet add package Serilog.Sinks.Console
   dotnet add package Serilog.Enrichers.Environment
   ```
2. Configurar en `Program.cs`:
   ```csharp
   builder.Host.UseSerilog((context, configuration) =>
   {
       configuration
           .ReadFrom.Configuration(context.Configuration)
           .Enrich.FromLogContext()
           .Enrich.WithEnvironmentName()
           .Enrich.WithMachineName()
           .WriteTo.Console();
   });
   ```
3. Configurar en `appsettings.json`:
   ```json
   {
     "Serilog": {
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft": "Warning",
           "System": "Warning"
         }
       }
     }
   }
   ```
4. Agregar middleware de request logging:
   ```csharp
   app.UseSerilogRequestLogging();
   ```
5. Probar logs en diferentes niveles
6. Commit: "feat: Configure Serilog for structured logging"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)

### Estimación
**Esfuerzo**: 3 puntos (1.5 horas)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Serilog configurado y funcionando
- Logs visibles en consola
- Configuración por ambiente
- Ejemplos de uso en código
- Documentado en README

---

## 📋 Resumen Sprint 1

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-005 | Crear Proyecto Web API .NET 8 | 🔴 Crítica | 2 pts | ⏳ Pendiente |
| US-006 | Configurar Swagger/OpenAPI | 🔴 Crítica | 2 pts | ⏳ Pendiente |
| US-007 | Implementar Health Checks | 🟡 Alta | 3 pts | ⏳ Pendiente |
| US-008 | Configurar CORS | 🟢 Media | 2 pts | ⏳ Pendiente |
| US-009 | Crear StatusController | 🟢 Media | 2 pts | ⏳ Pendiente |
| US-010 | Configurar Logging | 🟡 Alta | 3 pts | ⏳ Pendiente |

**Total Sprint 1**: 14 puntos (~7 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-01-setup.md](./user-stories-01-setup.md)
- **➡️ Siguiente**: [user-stories-03-database.md](./user-stories-03-database.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
