# 🔐 Sprint 1.5: Seguridad y Rate Limiting

**Sprint ID**: US-02.5-SECURITY  
**Objetivo**: Implementar medidas de seguridad para proteger el API  
**Puntos totales**: 3 (~1.5 horas)  
**Estado**: ✅ Completado

---

## 📋 Tabla de Contenidos

- [US-011: Implementar Rate Limiting](#us-011-implementar-rate-limiting)
- [Resumen Sprint 1.5](#-resumen-sprint-15)
- [Futuras Historias de Seguridad](#-futuras-historias-de-seguridad-backlog)

---

## US-011: Implementar Rate Limiting

**Como** administrador del API  
**Quiero** limitar el número de peticiones por cliente  
**Para** proteger el servicio contra abuso y garantizar disponibilidad para todos los usuarios

### Criterios de Aceptación
- ✅ Rate limiting configurado en todos los endpoints del API
- ✅ Límite de **10 peticiones por minuto** por IP en producción
- ✅ Headers de rate limit en las respuestas HTTP
- ✅ Código HTTP 429 (Too Many Requests) cuando se excede el límite
- ✅ Configuración flexible por ambiente (dev más permisivo, prod restrictivo)
- ✅ Diferentes políticas para endpoints públicos vs health checks

### Rate Limiting Policies

**Política General (Endpoints API):**
- **Development**: 100 peticiones/minuto (para facilitar desarrollo y testing)
- **Production**: 10 peticiones/minuto (protección contra abuso)

**Política Health Checks:**
- **Sin límite** en `/health`, `/health/ready`, `/health/live`
- Razón: Kubernetes/monitoring necesitan consultar frecuentemente

**Política por Endpoint (opcional):**
- Endpoints públicos: 10 req/min
- Endpoints administrativos: 5 req/min (futuro)

### Headers HTTP Incluidos

Cada respuesta incluirá headers informativos:

```http
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 7
X-RateLimit-Reset: 1634567890
Retry-After: 60
```

**Descripción de headers:**
- `X-RateLimit-Limit`: Número máximo de peticiones permitidas
- `X-RateLimit-Remaining`: Peticiones restantes en la ventana actual
- `X-RateLimit-Reset`: Timestamp cuando se resetea el contador
- `Retry-After`: Segundos a esperar antes de reintentar (solo en 429)

### Respuesta al Exceder el Límite

**HTTP 429 Too Many Requests:**
```json
{
  "statusCode": 429,
  "message": "Rate limit exceeded. Try again in 60 seconds.",
  "retryAfter": 60
}
```

**Ejemplo de respuesta completa:**
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1634567950
Retry-After: 60

{
  "statusCode": 429,
  "message": "Rate limit exceeded. Try again in 60 seconds.",
  "retryAfter": 60
}
```

### Tareas Técnicas

1. **Instalar paquete NuGet:**
   ```bash
   dotnet add package AspNetCoreRateLimit
   ```

2. **Configurar servicios en `Program.cs`:**
   ```csharp
   // Add Rate Limiting
   builder.Services.AddMemoryCache();
   
   builder.Services.Configure<IpRateLimitOptions>(
       builder.Configuration.GetSection("IpRateLimiting"));
   builder.Services.Configure<IpRateLimitPolicies>(
       builder.Configuration.GetSection("IpRateLimitPolicies"));
   
   builder.Services.AddInMemoryRateLimiting();
   builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
   ```

3. **Agregar middleware en `Program.cs`:**
   ```csharp
   // IMPORTANTE: Debe ir DESPUÉS de UseRouting y ANTES de UseEndpoints
   app.UseIpRateLimiting();
   ```

4. **Configurar en `appsettings.json`:**
   ```json
   {
     "IpRateLimiting": {
       "EnableEndpointRateLimiting": true,
       "StackBlockedRequests": false,
       "RealIpHeader": "X-Real-IP",
       "ClientIdHeader": "X-ClientId",
       "HttpStatusCode": 429,
       "IpWhitelist": [],
       "EndpointWhitelist": [
         "get:/health",
         "get:/health/ready",
         "get:/health/live"
       ],
       "ClientWhitelist": [],
       "GeneralRules": [
         {
           "Endpoint": "*",
           "Period": "1m",
           "Limit": 10
         }
       ]
     }
   }
   ```

5. **Configurar en `appsettings.Development.json`:**
   ```json
   {
     "IpRateLimiting": {
       "GeneralRules": [
         {
           "Endpoint": "*",
           "Period": "1m",
           "Limit": 100
         }
       ]
     }
   }
   ```

6. **Configurar en `appsettings.Production.json`:**
   ```json
   {
     "IpRateLimiting": {
       "GeneralRules": [
         {
           "Endpoint": "*",
           "Period": "1m",
           "Limit": 10
         }
       ]
     }
   }
   ```

7. **Actualizar StatusController para mostrar info de rate limiting:**
   ```csharp
   [HttpGet("info")]
   public IActionResult GetInfo()
   {
       return Ok(new
       {
           ApiName = "DevOps API",
           // ... otros campos ...
           Security = new
           {
               RateLimiting = new
               {
                   Enabled = true,
                   Limit = _configuration.GetValue<int>("IpRateLimiting:GeneralRules:0:Limit"),
                   Period = _configuration.GetValue<string>("IpRateLimiting:GeneralRules:0:Period")
               }
           }
       });
   }
   ```

8. **Crear script de prueba `test-rate-limit.ps1`:**
   ```powershell
   Write-Host "Testing Rate Limiting..." -ForegroundColor Cyan
   
   for ($i = 1; $i -le 15; $i++) {
       Write-Host "Request $i..." -NoNewline
       try {
           $response = Invoke-WebRequest -Uri "http://localhost:5065/api/status" `
               -Method Get -ErrorAction Stop
           
           $limit = $response.Headers["X-RateLimit-Limit"]
           $remaining = $response.Headers["X-RateLimit-Remaining"]
           
           Write-Host " OK (200) - Remaining: $remaining/$limit" -ForegroundColor Green
       } catch {
           if ($_.Exception.Response.StatusCode.value__ -eq 429) {
               $retryAfter = $_.Exception.Response.Headers["Retry-After"]
               Write-Host " RATE LIMITED (429) - Retry after: $retryAfter seconds" -ForegroundColor Yellow
           } else {
               Write-Host " ERROR" -ForegroundColor Red
           }
       }
       Start-Sleep -Milliseconds 100
   }
   ```

9. **Probar rate limiting:**
   - Ejecutar script de prueba
   - Verificar que peticiones 1-10 retornan 200
   - Verificar que petición 11+ retorna 429
   - Verificar headers en respuestas
   - Esperar 1 minuto y verificar reset

10. **Commit:** `"feat: Add rate limiting (10 req/min) with AspNetCoreRateLimit"`

### Configuración Avanzada (Opcional)

**Rate limiting por endpoint específico:**
```json
{
  "SpecificEndpointRules": [
    {
      "Endpoint": "post:/api/products",
      "Period": "1m",
      "Limit": 5
    },
    {
      "Endpoint": "get:/api/status",
      "Period": "1m",
      "Limit": 20
    }
  ]
}
```

**Whitelist de IPs (para CI/CD, monitoring):**
```json
{
  "IpWhitelist": [
    "127.0.0.1",
    "::1"
  ]
}
```

### Dependencias
- ✅ US-005 (Proyecto Web API creado)
- ✅ US-009 (StatusController con endpoints)
- ✅ US-010 (Logging configurado - para registrar rate limit events)

### Estimación
**Esfuerzo**: 3 puntos (~1.5 horas)  
**Prioridad**: 🟡 Alta (Seguridad)

### Definición de Hecho (DoD)
- Rate limiting funcionando en todos los endpoints
- Límite de 10 peticiones/minuto en producción
- Límite de 100 peticiones/minuto en desarrollo
- Headers HTTP correctos en respuestas
- Respuesta 429 con mensaje apropiado y Retry-After
- Health checks excluidos del rate limiting
- Configuración por ambiente validada
- Script de prueba ejecutado exitosamente
- Logs mostrando eventos de rate limiting
- Documentado en README
- Info de rate limiting visible en `/api/status/info`

---

## 📋 Resumen Sprint 1.5

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-011 | Implementar Rate Limiting | 🟡 Alta | 3 pts | ✅ Completado |

**Total Sprint 1.5**: 3 puntos (~1.5 horas)

**Objetivo**: Proteger el API contra abuso y garantizar disponibilidad

**Entregables**:
- ✅ Rate limiting configurado por IP
- ✅ Headers informativos en respuestas
- ✅ Políticas diferentes por ambiente
- ✅ Health checks excluidos
- ✅ Script de pruebas

---

## 🔮 Futuras Historias de Seguridad (Backlog)

Historias planificadas para futuros sprints de seguridad:

### **US-XXX: Implementar Autenticación JWT** (Sprint futuro)
- Autenticación basada en tokens JWT
- Login endpoint
- Validación de tokens en cada petición
- Refresh tokens

### **US-XXX: Implementar Autorización basada en Roles** (Sprint futuro)
- Roles: Admin, User, ReadOnly
- Políticas de autorización
- Claims-based authorization
- Atributos [Authorize(Roles = "Admin")]

### **US-XXX: Agregar HTTPS Obligatorio** (Sprint futuro)
- Forzar HTTPS en producción
- HSTS headers
- Certificados SSL/TLS
- Redirección HTTP → HTTPS

### **US-XXX: Implementar API Keys** (Sprint futuro)
- API Keys para clientes externos
- Gestión de keys en base de datos
- Rotación de keys
- Rate limiting por API key

### **US-XXX: Agregar Validación de Input** (Sprint futuro)
- FluentValidation
- Data Annotations
- Prevención de SQL Injection
- XSS protection

### **US-XXX: Implementar Auditoría de Seguridad** (Sprint futuro)
- Logging de accesos
- Tracking de cambios
- Detección de anomalías
- Reportes de seguridad

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-02-api-base.md](./user-stories-02-api-base.md)
- **➡️ Siguiente**: [user-stories-03-database.md](./user-stories-03-database.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
