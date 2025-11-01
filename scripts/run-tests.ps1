# Script para ejecutar tests unitarios
# Uso: .\scripts\run-tests.ps1

Write-Host "🧪 Ejecutando tests unitarios..." -ForegroundColor Cyan

dotnet test --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Todos los tests pasaron correctamente" -ForegroundColor Green
} else {
    Write-Host "❌ Algunos tests fallaron" -ForegroundColor Red
    exit 1
}
