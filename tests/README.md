# tests/

Este directorio contiene los tests unitarios y de integración del proyecto.

## Estructura

```
tests/
├── DevOpsApi.UnitTests/         # Tests unitarios con xUnit
│   ├── Controllers/
│   ├── Services/
│   └── Models/
└── DevOpsApi.IntegrationTests/  # Tests de integración
    └── ApiTests.cs
```

## Frameworks de Testing

- **xUnit**: Framework de testing principal
- **Moq**: Librería para mocking
- **FluentAssertions**: Assertions más legibles
- **Coverlet**: Code coverage

## Estado

- ✅ **DevOpsApi.UnitTests**: Proyecto creado y configurado
- ⏳ **DevOpsApi.IntegrationTests**: Pendiente

## Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar solo tests unitarios
dotnet test tests/DevOpsApi.UnitTests

# Ejecutar con coverage
dotnet test --collect:"XPlat Code Coverage"
```
