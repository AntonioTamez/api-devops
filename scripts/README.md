# Scripts de Automatización

Scripts de PowerShell para tareas comunes del proyecto.

## 📜 Scripts Disponibles

### `run-tests.ps1`
Ejecuta todos los tests unitarios del proyecto.

**Uso:**
```powershell
.\scripts\run-tests.ps1
```

**Características:**
- Ejecuta todos los tests con verbosidad normal
- Muestra resultados en consola
- Retorna código de salida apropiado para CI/CD

---

### `coverage.ps1`
Ejecuta tests y genera reportes de code coverage.

**Uso:**
```powershell
.\scripts\coverage.ps1
```

**Características:**
- Ejecuta tests con recolección de coverage
- Genera reporte HTML con ReportGenerator
- Abre automáticamente el reporte en el navegador
- Guarda resultados en `coverage-report/`

**Salida:**
- `coverage-report/index.html` - Reporte principal
- `TestResults/` - Datos raw de coverage

---

## 🔧 Requisitos

### ReportGenerator
El script `coverage.ps1` requiere ReportGenerator instalado globalmente:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Para verificar la instalación:
```bash
reportgenerator --version
```

---

## 📊 Interpretando Reportes de Coverage

El reporte HTML muestra:

- **Line Coverage**: Porcentaje de líneas ejecutadas
- **Branch Coverage**: Porcentaje de ramas (if/else) cubiertas
- **Method Coverage**: Porcentaje de métodos probados

### Objetivos de Coverage
- ✅ **Mínimo aceptable**: 80%
- 🎯 **Objetivo**: 90%
- 🌟 **Excelente**: 95%+

---

## 🚀 Integración con CI/CD

Estos scripts están diseñados para integrarse con pipelines de CI/CD:

```yaml
# Ejemplo GitHub Actions
- name: Run Tests
  run: .\scripts\run-tests.ps1

- name: Generate Coverage
  run: .\scripts\coverage.ps1
```

---

## 📝 Notas

- Los reportes de coverage son ignorados por Git (`.gitignore`)
- Los tests deben pasar antes de generar reportes
- El coverage se calcula sobre el código de `src/`, no de `tests/`
