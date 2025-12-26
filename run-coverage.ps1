# Script para generar reporte de coverage

Write-Host "?? Ejecutando tests con coverage..." -ForegroundColor Cyan

# Limpiar reportes anteriores
if (Test-Path "coverage.opencover.xml") {
    Remove-Item "coverage.opencover.xml"
}
if (Test-Path "coverage-report") {
    Remove-Item "coverage-report" -Recurse
}

# Ejecutar tests con coverage
dotnet test MedCitas.Tests/MedCitas.Tests.csproj `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=opencover `
    /p:CoverletOutput="../coverage.opencover.xml" `
    /p:Exclude="[xunit.*]*%2c[*.Tests]*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Los tests fallaron" -ForegroundColor Red
    exit 1
}

Write-Host "? Tests ejecutados exitosamente" -ForegroundColor Green

# Verificar si existe reportgenerator
$reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue

if (-not $reportGenerator) {
    Write-Host "??  reportgenerator no está instalado. Instalando..." -ForegroundColor Yellow
  dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generar reporte HTML
Write-Host "?? Generando reporte HTML..." -ForegroundColor Cyan

reportgenerator `
    -reports:"coverage.opencover.xml" `
  -targetdir:"coverage-report" `
    -reporttypes:"Html;TextSummary"

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Reporte generado exitosamente" -ForegroundColor Green
    
  # Mostrar resumen
    if (Test-Path "coverage-report/Summary.txt") {
        Write-Host "`n?? Resumen de Coverage:" -ForegroundColor Cyan
        Get-Content "coverage-report/Summary.txt"
    }
    
  # Abrir reporte en navegador
    $reportPath = Join-Path (Get-Location) "coverage-report/index.html"
    Write-Host "`n?? Abriendo reporte en navegador..." -ForegroundColor Cyan
    Start-Process $reportPath
} else {
Write-Host "? Error al generar reporte" -ForegroundColor Red
    exit 1
}

Write-Host "`n?? Proceso completado!" -ForegroundColor Green
