# 📖 Historias de Usuario - Parte 3: Base de Datos y Entity Framework
**Identificador**: US-03-DATABASE  
**Orden de lectura**: 3 de 8  
**Sprint**: Sprint 2 - Integración con SQL Server  

---

## 🎯 Objetivo del Sprint 2
Integrar Entity Framework Core con SQL Server, crear el modelo de datos inicial y implementar CRUD básico.

---

## US-012: Configurar Entity Framework Core ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador backend  
**Quiero** configurar Entity Framework Core con SQL Server  
**Para** gestionar la persistencia de datos de manera eficiente

### Criterios de Aceptación
- ✅ Paquetes NuGet de EF Core instalados
- ✅ DbContext creado y configurado
- ✅ Connection string parametrizado por ambiente
- ✅ EF Core Tools instalado para migrations
- ✅ Configuración de pooling y timeouts

### Tareas Técnicas
1. Instalar paquetes NuGet:
   ```bash
   cd src
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
   dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
   dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
   ```

2. Crear carpeta `Models/` en src

3. Crear `Models/ApiDbContext.cs`:
   ```csharp
   using Microsoft.EntityFrameworkCore;
   
   namespace DevOpsApi.Models;
   
   public class ApiDbContext : DbContext
   {
       public ApiDbContext(DbContextOptions<ApiDbContext> options) 
           : base(options)
       {
       }
   
       // DbSets se agregarán aquí
       public DbSet<Product> Products => Set<Product>();
   
       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           base.OnModelCreating(modelBuilder);
           
           // Configuraciones de entidades
           modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
       }
   }
   ```

4. Configurar en `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<ApiDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("DefaultConnection"),
           sqlOptions =>
           {
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 5,
                   maxRetryDelay: TimeSpan.FromSeconds(30),
                   errorNumbersToAdd: null);
               sqlOptions.CommandTimeout(60);
           }));
   ```

5. Agregar connection string en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=DevOpsDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

6. Commit: "feat: Configure Entity Framework Core with SQL Server"

### Dependencias
- ✅ US-005 (Proyecto Web API creado)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- EF Core configurado correctamente
- DbContext inyectable
- Connection string parametrizado
- Documentado en README

---

## US-013: Crear Modelo de Entidad Product ✅

**Estado**: ✅ **COMPLETADA**

**Como** desarrollador  
**Quiero** una entidad Product con sus configuraciones  
**Para** tener un ejemplo completo de modelo de datos

### Criterios de Aceptación
- ✅ Clase `Product` creada con propiedades básicas
- ✅ Data annotations para validación
- ✅ Configuración Fluent API
- ✅ Índices y constraints definidos
- ✅ Timestamps de auditoría (CreatedAt, UpdatedAt)

### Modelo Product

**Product.cs**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevOpsApi.Models;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Required]
    public int Stock { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}
```

**ProductConfiguration.cs** (Fluent API)
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsApi.Models.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
        
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Category);
        
        // Seed data
        builder.HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Product Sample", 
                Description = "Sample product for testing",
                Price = 99.99M, 
                Stock = 100,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
```

### Tareas Técnicas
1. Crear `Models/Product.cs`
2. Crear carpeta `Models/Configurations/`
3. Crear `Models/Configurations/ProductConfiguration.cs`
4. Actualizar `ApiDbContext` para incluir `Products` DbSet
5. Verificar que compila
6. Commit: "feat: Add Product entity with Fluent API configuration"

### Dependencias
- ✅ US-011 (EF Core configurado)

### Estimación
**Esfuerzo**: 3 puntos (1 hora)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Entidad Product creada
- Configuración Fluent API completa
- Seed data incluido
- Compilando sin errores

---

## US-014: Crear y Aplicar Migraciones de EF Core

**Como** desarrollador  
**Quiero** crear migraciones de base de datos  
**Para** versionar y aplicar cambios de esquema de forma controlada

### Criterios de Aceptación
- ✅ Migración inicial creada
- ✅ Scripts SQL generados
- ✅ Migración aplicable a base de datos local
- ✅ Rollback funcional
- ✅ Documentación de comandos de migration

### Tareas Técnicas
1. Crear migración inicial:
   ```bash
   cd src
   dotnet ef migrations add InitialCreate
   ```
   
2. Revisar archivos generados en `Migrations/`:
   - `xxxxxx_InitialCreate.cs`
   - `xxxxxx_InitialCreate.Designer.cs`
   - `ApiDbContextModelSnapshot.cs`

3. Generar script SQL (opcional):
   ```bash
   dotnet ef migrations script --output ../scripts/initial-schema.sql
   ```

4. Crear carpeta `scripts/` para SQL scripts

5. Aplicar migración (se hará con Docker después):
   ```bash
   dotnet ef database update
   ```

6. Verificar comandos de rollback:
   ```bash
   dotnet ef database update 0  # Rollback completo
   dotnet ef migrations remove  # Remover última migración
   ```

7. Actualizar `.gitignore` para incluir `Migrations/` (NO ignorar)

8. Commit: "feat: Add initial EF Core migration"

### Dependencias
- ✅ US-011 (EF Core configurado)
- ✅ US-012 (Entidad Product creada)

### Estimación
**Esfuerzo**: 2 puntos (30 minutos)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Migración creada y commiteada
- Script SQL generado
- Comandos documentados en README
- Migración probada localmente

---

## US-015: Crear Repositorio y Service Layer

**Como** arquitecto de software  
**Quiero** implementar el patrón Repository y Service  
**Para** separar la lógica de negocio del acceso a datos

### Criterios de Aceptación
- ✅ Interface `IProductService` definida
- ✅ Implementación `ProductService` con lógica de negocio
- ✅ Inyección de dependencias configurada
- ✅ Métodos CRUD básicos implementados
- ✅ Manejo de excepciones

### Arquitectura

**Interfaces (Services/IProductService.cs)**
```csharp
namespace DevOpsApi.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Implementación (Services/ProductService.cs)**
```csharp
using Microsoft.EntityFrameworkCore;
using DevOpsApi.Models;

namespace DevOpsApi.Services;

public class ProductService : IProductService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApiDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all products");
        return await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _logger.LogInformation("Creating new product: {ProductName}", product.Name);
        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product product)
    {
        var existing = await _context.Products.FindAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return null;
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.Category = product.Category;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product updated: {ProductId}", id);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product deleted (soft): {ProductId}", id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id && p.IsActive);
    }
}
```

### Tareas Técnicas
1. Crear carpeta `Services/`
2. Crear `Services/IProductService.cs`
3. Crear `Services/ProductService.cs`
4. Registrar en `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IProductService, ProductService>();
   ```
5. Commit: "feat: Implement Repository and Service layer for Products"

### Dependencias
- ✅ US-011 (EF Core configurado)
- ✅ US-012 (Entidad Product creada)

### Estimación
**Esfuerzo**: 5 puntos (2 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Service layer implementado
- Inyección de dependencias configurada
- Código compilando
- Logs implementados

---

## US-016: Crear ProductsController con CRUD Completo

**Como** consumidor de la API  
**Quiero** endpoints RESTful para gestionar productos  
**Para** realizar operaciones CRUD desde aplicaciones cliente

### Criterios de Aceptación
- ✅ Endpoints CRUD completos (GET, POST, PUT, DELETE)
- ✅ DTOs para request/response
- ✅ Validación de datos con Data Annotations
- ✅ Códigos HTTP apropiados (200, 201, 404, 400)
- ✅ Documentado en Swagger con comentarios XML
- ✅ Paginación en GET All
- ✅ Filtros básicos (por categoría, activos)

### Endpoints a Implementar

- `GET /api/products` - Listar todos (con paginación)
- `GET /api/products/{id}` - Obtener por ID
- `POST /api/products` - Crear nuevo
- `PUT /api/products/{id}` - Actualizar existente
- `DELETE /api/products/{id}` - Eliminar (soft delete)
- `GET /api/products/category/{category}` - Filtrar por categoría

### DTOs

**CreateProductDto.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace DevOpsApi.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
}
```

**UpdateProductDto.cs** (similar a Create)

**ProductResponseDto.cs**
```csharp
namespace DevOpsApi.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Controller

**Controllers/ProductsController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using DevOpsApi.Services;
using DevOpsApi.DTOs;
using DevOpsApi.Models;

namespace DevOpsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        var response = products.Select(p => MapToDto(p));
        return Ok(response);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }
        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category
        };

        var created = await _productService.CreateAsync(product);
        var response = MapToDto(created);
        
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category
        };

        var updated = await _productService.UpdateAsync(id, product);
        if (updated == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(MapToDto(updated));
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    private static ProductResponseDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        Category = product.Category,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
```

### Tareas Técnicas
1. Crear carpeta `DTOs/`
2. Crear DTOs (CreateProductDto, UpdateProductDto, ProductResponseDto)
3. Crear `Controllers/ProductsController.cs`
4. Eliminar `WeatherForecastController.cs` (si existe)
5. Probar todos los endpoints en Swagger
6. Commit: "feat: Add ProductsController with full CRUD operations"

### Dependencias
- ✅ US-014 (Service layer implementado)

### Estimación
**Esfuerzo**: 5 puntos (2.5 horas)  
**Prioridad**: 🔴 Crítica

### Definición de Hecho (DoD)
- Todos los endpoints CRUD funcionando
- DTOs con validación
- Documentado en Swagger
- Probado manualmente
- Ejemplos en README

---

## US-017: Agregar Health Check de SQL Server

**Como** ingeniero de SRE  
**Quiero** que el health check valide la conexión a SQL Server  
**Para** detectar problemas de base de datos rápidamente

### Criterios de Aceptación
- ✅ Health check de SQL Server agregado
- ✅ Timeout configurado (5 segundos)
- ✅ Falla si SQL Server no está accesible
- ✅ Incluido en endpoint `/health`
- ✅ Tag "ready" para readiness probe

### Tareas Técnicas
1. Instalar paquete:
   ```bash
   dotnet add package AspNetCore.HealthChecks.SqlServer
   ```

2. Configurar en `Program.cs`:
   ```csharp
   builder.Services.AddHealthChecks()
       .AddSqlServer(
           connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
           healthQuery: "SELECT 1;",
           name: "sql-server",
           failureStatus: HealthStatus.Unhealthy,
           tags: new[] { "ready", "db" },
           timeout: TimeSpan.FromSeconds(5));
   ```

3. Probar endpoint `/health`
4. Verificar que falla si SQL Server está apagado
5. Commit: "feat: Add SQL Server health check"

### Dependencias
- ✅ US-007 (Health checks básicos)
- ✅ US-011 (EF Core configurado)

### Estimación
**Esfuerzo**: 2 puntos (30 minutos)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Health check SQL funcionando
- Probado con SQL Server up/down
- Documentado en README

---

## 📋 Resumen Sprint 2

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-012 | Configurar Entity Framework Core | 🔴 Crítica | 3 pts | ✅ Completado |
| US-013 | Crear Modelo Product | 🔴 Crítica | 3 pts | ✅ Completado |
| US-014 | Crear y Aplicar Migraciones | 🔴 Crítica | 2 pts | ⏳ Pendiente |
| US-015 | Repository y Service Layer | 🔴 Crítica | 5 pts | ⏳ Pendiente |
| US-016 | ProductsController CRUD | 🔴 Crítica | 5 pts | ⏳ Pendiente |
| US-017 | Health Check SQL Server | 🟡 Alta | 2 pts | ⏳ Pendiente |

**Total Sprint 2**: 20 puntos (~10 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-02-api-base.md](./user-stories-02-api-base.md)
- **➡️ Siguiente**: [user-stories-04-docker.md](./user-stories-04-docker.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
