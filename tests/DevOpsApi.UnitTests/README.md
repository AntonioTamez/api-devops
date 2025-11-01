# DevOpsApi.UnitTests

Proyecto de tests unitarios para la API DevOps usando xUnit.

## 🎯 Objetivo

Verificar la lógica de negocio de la aplicación de forma aislada mediante tests unitarios.

## 🛠️ Tecnologías

- **xUnit**: Framework de testing
- **Moq**: Librería de mocking
- **FluentAssertions**: Assertions legibles
- **EF Core InMemory**: Base de datos en memoria para tests

## 📁 Estructura

```
DevOpsApi.UnitTests/
├── Controllers/        # Tests de controladores
├── Services/          # Tests de servicios
├── Models/            # Tests de modelos
├── Helpers/           # Clases helper para tests
│   └── TestBase.cs    # Clase base con utilidades comunes
└── README.md
```

## 🚀 Ejecutar Tests

### Ejecutar todos los tests
```bash
dotnet test
```

### Ejecutar con detalles
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Ejecutar tests específicos
```bash
dotnet test --filter "FullyQualifiedName~ProductService"
```

### Generar reporte de coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Convenciones

### Nomenclatura
- Clases de test: `{ClaseAProbar}Tests.cs`
- Métodos de test: `{Método}_{Escenario}_{ResultadoEsperado}`

### Ejemplo
```csharp
[Fact]
public async Task GetById_WithValidId_ShouldReturnProduct()
{
    // Arrange
    var productId = 1;
    
    // Act
    var result = await _service.GetByIdAsync(productId);
    
    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(productId);
}
```

## 🎯 Objetivos de Coverage

- **Mínimo**: 80% de cobertura
- **Objetivo**: 90% de cobertura
- **Ideal**: 95%+ de cobertura

## 📚 Recursos

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
