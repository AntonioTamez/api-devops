# 📖 Historias de Usuario - Parte 5: Testing y Calidad
**Identificador**: US-05-TESTING  
**Orden de lectura**: 5 de 8  
**Sprint**: Sprint 4 - Testing  

---

## 🎯 Objetivo del Sprint 4
Implementar tests unitarios y de integración para garantizar la calidad del código.

---

## US-022: Crear Proyecto de Tests Unitarios

**Como** desarrollador  
**Quiero** un proyecto de tests unitarios con xUnit  
**Para** verificar la lógica de negocio aisladamente

### Criterios de Aceptación
- ✅ Proyecto xUnit creado en carpeta `tests/`
- ✅ Referencias al proyecto principal configuradas
- ✅ Librerías de mocking instaladas (Moq, FluentAssertions)
- ✅ Estructura de carpetas espejo del proyecto principal
- ✅ Proyecto compila correctamente

### Tareas Técnicas
1. Crear proyecto de tests:
   ```bash
   cd c:/ATS/GIT/api-devops
   dotnet new xunit -n DevOpsApi.UnitTests -o tests/DevOpsApi.UnitTests
   ```

2. Agregar referencia al proyecto principal:
   ```bash
   cd tests/DevOpsApi.UnitTests
   dotnet add reference ../../src/DevOpsApi.csproj
   ```

3. Instalar paquetes necesarios:
   ```bash
   dotnet add package Moq
   dotnet add package FluentAssertions
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   ```

4. Crear estructura de carpetas:
   ```
   tests/DevOpsApi.UnitTests/
   ├── Controllers/
   ├── Services/
   ├── Models/
   └── Helpers/
   ```

5. Eliminar archivo UnitTest1.cs default

6. Crear archivo base `TestBase.cs` con helpers comunes

7. Commit: "test: Initialize unit tests project with xUnit"

### Dependencias
- ✅ US-005 (Proyecto API creado)

### Estimación
**Esfuerzo**: 2 puntos (30 minutos)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Proyecto tests creado
- Compila correctamente
- Estructura de carpetas lista
- README en carpeta tests

---

## US-023: Implementar Tests para ProductService

**Como** desarrollador  
**Quiero** tests unitarios para ProductService  
**Para** asegurar que la lógica de negocio funciona correctamente

### Criterios de Aceptación
- ✅ Tests para todos los métodos de ProductService
- ✅ Mock de DbContext con InMemory database
- ✅ Tests de casos exitosos y de error
- ✅ Coverage > 80% en ProductService
- ✅ Uso de FluentAssertions para legibilidad

### Tests a Implementar

**ProductServiceTests.cs**
```csharp
using DevOpsApi.Models;
using DevOpsApi.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DevOpsApi.UnitTests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly ApiDbContext _context;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApiDbContext(options);
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_context, _loggerMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Test Product 1", 
                Price = 10.99M, 
                Stock = 100,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Id = 2, 
                Name = "Test Product 2", 
                Price = 20.99M, 
                Stock = 50,
                Category = "Books",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Id = 3, 
                Name = "Inactive Product", 
                Price = 5.99M, 
                Stock = 0,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveProducts()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
        result.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product 1");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _service.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveProduct_ShouldReturnNull()
    {
        // Arrange
        var inactiveId = 3;

        // Act
        var result = await _service.GetByIdAsync(inactiveId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidProduct_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Description = "Test description",
            Price = 15.99M,
            Stock = 75,
            Category = "Test"
        };

        // Act
        var result = await _service.CreateAsync(newProduct);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Product");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verify in database
        var inDb = await _context.Products.FindAsync(result.Id);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateProduct()
    {
        // Arrange
        var productId = 1;
        var updatedProduct = new Product
        {
            Name = "Updated Name",
            Description = "Updated description",
            Price = 99.99M,
            Stock = 200,
            Category = "Updated Category"
        };

        // Act
        var result = await _service.UpdateAsync(productId, updatedProduct);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(99.99M);
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;
        var product = new Product { Name = "Test" };

        // Act
        var result = await _service.UpdateAsync(invalidId, product);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var result = await _service.DeleteAsync(productId);

        // Assert
        result.Should().BeTrue();
        
        // Verify soft delete
        var product = await _context.Products.FindAsync(productId);
        product.Should().NotBeNull();
        product!.IsActive.Should().BeFalse();
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _service.DeleteAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithValidActiveId_ShouldReturnTrue()
    {
        // Arrange
        var productId = 1;

        // Act
        var result = await _service.ExistsAsync(productId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _service.ExistsAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveId_ShouldReturnFalse()
    {
        // Arrange
        var inactiveId = 3;

        // Act
        var result = await _service.ExistsAsync(inactiveId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### Tareas Técnicas
1. Crear `Services/ProductServiceTests.cs`
2. Implementar todos los tests listados
3. Ejecutar tests:
   ```bash
   dotnet test
   ```
4. Verificar coverage:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```
5. Commit: "test: Add comprehensive unit tests for ProductService"

### Dependencias
- ✅ US-022 (Proyecto tests creado)
- ✅ US-014 (ProductService implementado)

### Estimación
**Esfuerzo**: 5 puntos (2.5 horas)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Todos los tests pasando
- Coverage > 80%
- Tests legibles y mantenibles
- CI puede ejecutarlos

---

## US-024: Implementar Tests para ProductsController

**Como** desarrollador  
**Quiero** tests para ProductsController  
**Para** verificar que los endpoints responden correctamente

### Criterios de Aceptación
- ✅ Tests para todos los endpoints
- ✅ Mock de IProductService
- ✅ Validación de códigos HTTP
- ✅ Validación de respuestas JSON
- ✅ Tests de validación de DTOs

### Tests a Implementar

**ProductsControllerTests.cs**
```csharp
using DevOpsApi.Controllers;
using DevOpsApi.DTOs;
using DevOpsApi.Models;
using DevOpsApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DevOpsApi.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10M, Stock = 100 },
            new Product { Id = 2, Name = "Product 2", Price = 20M, Stock = 50 }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductResponseDto>>().Subject;
        returnedProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOkWithProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10M, Stock = 100 };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductResponseDto>().Subject;
        returnedProduct.Id.Should().Be(1);
        returnedProduct.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Description",
            Price = 15.99M,
            Stock = 50,
            Category = "Test"
        };
        
        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Stock = createDto.Stock,
            Category = createDto.Category
        };
        
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
        var returnedProduct = createdResult.Value.Should().BeOfType<ProductResponseDto>().Subject;
        returnedProduct.Name.Should().Be("New Product");
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnOkWithUpdatedProduct()
    {
        // Arrange
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Price = 25.99M,
            Stock = 75
        };
        
        var updatedProduct = new Product
        {
            Id = 1,
            Name = updateDto.Name,
            Price = updateDto.Price,
            Stock = updateDto.Stock
        };
        
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<Product>())).ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductResponseDto>().Subject;
        returnedProduct.Name.Should().Be("Updated Product");
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Test", Price = 10M, Stock = 10 };
        _serviceMock.Setup(s => s.UpdateAsync(999, It.IsAny<Product>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
```

### Tareas Técnicas
1. Crear `Controllers/ProductsControllerTests.cs`
2. Implementar todos los tests
3. Ejecutar: `dotnet test`
4. Commit: "test: Add unit tests for ProductsController"

### Dependencias
- ✅ US-022 (Proyecto tests creado)
- ✅ US-015 (ProductsController implementado)

### Estimación
**Esfuerzo**: 5 puntos (2.5 horas)  
**Prioridad**: 🟡 Alta

### Definición de Hecho (DoD)
- Todos los tests pasando
- Todos los endpoints cubiertos
- Códigos HTTP verificados

---

## US-025: Configurar Code Coverage

**Como** tech lead  
**Quiero** reportes de code coverage  
**Para** medir la calidad de los tests

### Criterios de Aceptación
- ✅ Coverage configurado con Coverlet
- ✅ Reportes generados en formato HTML
- ✅ Coverage mínimo definido (80%)
- ✅ Integrado en pipeline CI

### Tareas Técnicas
1. Instalar Coverlet:
   ```bash
   cd tests/DevOpsApi.UnitTests
   dotnet add package coverlet.collector
   ```

2. Instalar ReportGenerator (global):
   ```bash
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```

3. Ejecutar tests con coverage:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. Generar reporte HTML:
   ```bash
   reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
   ```

5. Crear script `scripts/coverage.ps1`:
   ```powershell
   dotnet test --collect:"XPlat Code Coverage"
   reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
   Start-Process "coverage-report/index.html"
   ```

6. Agregar a `.gitignore`:
   ```
   coverage-report/
   **/TestResults/
   ```

7. Commit: "test: Configure code coverage with Coverlet"

### Dependencias
- ✅ US-023 (Tests implementados)

### Estimación
**Esfuerzo**: 2 puntos (45 minutos)  
**Prioridad**: 🟢 Media

### Definición de Hecho (DoD)
- Coverage reportándose correctamente
- Reporte HTML legible
- Script automatizado creado

---

## 📋 Resumen Sprint 4

| ID | Historia | Prioridad | Esfuerzo | Estado |
|---|---|---|---|---|
| US-022 | Crear Proyecto Tests Unitarios | 🟡 Alta | 2 pts | ⏳ Pendiente |
| US-023 | Tests ProductService | 🟡 Alta | 5 pts | ⏳ Pendiente |
| US-024 | Tests ProductsController | 🟡 Alta | 5 pts | ⏳ Pendiente |
| US-025 | Configurar Code Coverage | 🟢 Media | 2 pts | ⏳ Pendiente |

**Total Sprint 4**: 14 puntos (~7 horas)

---

## 🔗 Navegación

- **⬅️ Anterior**: [user-stories-04-docker.md](./user-stories-04-docker.md)
- **➡️ Siguiente**: [user-stories-06-terraform.md](./user-stories-06-terraform.md)
- **🏠 Índice**: [user-stories-00-index.md](./user-stories-00-index.md)

---

**Última actualización**: Octubre 2025  
**Estado del documento**: ✅ Listo para implementación
