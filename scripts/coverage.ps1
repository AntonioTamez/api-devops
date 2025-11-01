# Script para generar reportes de code coverage
# Uso: .\scripts\coverage.ps1

Write-Host "Ejecutando tests con code coverage..." -ForegroundColor Cyan

# Ejecutar tests con coverage
dotnet test --collect:'XPlat Code Coverage' --results-directory:TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al ejecutar los tests" -ForegroundColor Red
    exit 1
}

Write-Host "Tests ejecutados correctamente" -ForegroundColor Green

# Generar reporte HTML
Write-Host "Generando reporte HTML de coverage..." -ForegroundColor Cyan

reportgenerator -reports:TestResults\**\coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al generar el reporte" -ForegroundColor Red
    exit 1
}

Write-Host "Reporte generado en: coverage-report\index.html" -ForegroundColor Green

# Abrir reporte en el navegador
Write-Host "Abriendo reporte en el navegador..." -ForegroundColor Cyan
Start-Process coverage-report\index.html

Write-Host "Proceso completado!" -ForegroundColor Green
