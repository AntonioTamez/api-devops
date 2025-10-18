# Script para iniciar SQL Server en Docker para desarrollo

Write-Host "🚀 Iniciando SQL Server en Docker..." -ForegroundColor Cyan

# Verificar si el contenedor ya existe
$existingContainer = docker ps -a --filter "name=sqlserver-dev" --format "{{.Names}}"

if ($existingContainer -eq "sqlserver-dev") {
    Write-Host "📦 Contenedor existente encontrado. Iniciando..." -ForegroundColor Yellow
    docker start sqlserver-dev
} else {
    Write-Host "📦 Creando nuevo contenedor SQL Server..." -ForegroundColor Green
    docker run -e "ACCEPT_EULA=Y" `
        -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" `
        -p 1433:1433 `
        --name sqlserver-dev `
        -d mcr.microsoft.com/mssql/server:2022-latest
}

# Esperar a que SQL Server esté listo
Write-Host "⏳ Esperando a que SQL Server esté listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Verificar el estado
$status = docker ps --filter "name=sqlserver-dev" --format "{{.Status}}"
if ($status) {
    Write-Host "✅ SQL Server está corriendo: $status" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Detalles de conexión:" -ForegroundColor Cyan
    Write-Host "   Server: localhost,1433" -ForegroundColor White
    Write-Host "   User: sa" -ForegroundColor White
    Write-Host "   Password: YourStrong!Passw0rd" -ForegroundColor White
    Write-Host ""
    Write-Host "🔌 Puedes conectarte ahora con Azure Data Studio o SQL Server Management Studio" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error al iniciar SQL Server" -ForegroundColor Red
    docker logs sqlserver-dev
}
