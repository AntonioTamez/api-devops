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

## Próximos pasos

Los proyectos de testing se crearán en el Sprint 4.
