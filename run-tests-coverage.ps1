# Script para ejecutar tests con coverage
Write-Host "?? MedCitas - Tests con Coverage" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Limpiar coverages anteriores
Write-Host "?? Limpiando reportes anteriores..." -ForegroundColor Yellow
if (Test-Path "coverage-report") {
    Remove-Item -Recurse -Force "coverage-report"
}
if (Test-Path "MedCitas.Tests/coverage") {
    Remove-Item -Recurse -Force "MedCitas.Tests/coverage"
}

# Ejecutar tests
Write-Host ""
Write-Host "?? Ejecutando tests..." -ForegroundColor Yellow
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/ /p:Threshold=70 /p:ThresholdType=line

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Tests fallaron o coverage bajo del umbral!" -ForegroundColor Red
    Write-Host "   Revisa los errores arriba." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "? Tests completados exitosamente!" -ForegroundColor Green

# Verificar si existe el archivo de coverage
if (-not (Test-Path "MedCitas.Tests/coverage/coverage.opencover.xml")) {
    Write-Host ""
    Write-Host "??  Archivo de coverage no encontrado." -ForegroundColor Yellow
    Write-Host "   Ubicación esperada: MedCitas.Tests/coverage/coverage.opencover.xml" -ForegroundColor Yellow
    exit 1
}

# Generar reporte
Write-Host ""
Write-Host "?? Generando reporte de coverage..." -ForegroundColor Yellow

# Verificar si reportgenerator está instalado
$reportGeneratorExists = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $reportGeneratorExists) {
    Write-Host "??  ReportGenerator no está instalado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

reportgenerator "-reports:MedCitas.Tests/coverage/coverage.opencover.xml" "-targetdir:coverage-report" "-reporttypes:Html;Badges;TextSummary" "-verbosity:Warning"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Error al generar el reporte!" -ForegroundColor Red
    exit 1
}

# Mostrar resumen
Write-Host ""
Write-Host "?? Resumen de Coverage:" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
if (Test-Path "coverage-report/Summary.txt") {
    Get-Content "coverage-report/Summary.txt"
} else {
    Write-Host "??  Archivo de resumen no encontrado" -ForegroundColor Yellow
}

# Abrir reporte
Write-Host ""
Write-Host "?? Abriendo reporte en el navegador..." -ForegroundColor Green
if (Test-Path "coverage-report/index.html") {
    Start-Process "coverage-report/index.html"
} else {
    Write-Host "??  index.html no encontrado" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "? ¡Proceso completado!" -ForegroundColor Green
Write-Host "   Reporte guardado en: coverage-report/" -ForegroundColor Gray
Write-Host ""
