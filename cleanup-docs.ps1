# Script para limpiar archivos innecesarios del proyecto
Write-Host "?? Limpiando archivos innecesarios..." -ForegroundColor Cyan
Write-Host ""

# Archivos a eliminar
$archivosParaEliminar = @(
    "COVERAGE_SCRIPTS.md",
    "FIXES_SUMMARY.md",
    "SECURITY_COVERAGE_FIXES.md",
    "NEW_COVERAGE_TESTS.md",
    "QUICK_START.md",
    "README_BADGES.md",
    "verify-fixes.ps1",
    "verify-new-tests.ps1"
)

# Contador
$eliminados = 0
$noEncontrados = 0

foreach ($archivo in $archivosParaEliminar) {
    if (Test-Path $archivo) {
        Write-Host "???  Eliminando: $archivo" -ForegroundColor Yellow
        Remove-Item $archivo -Force
        $eliminados++
    } else {
        Write-Host "??  No encontrado: $archivo" -ForegroundColor Gray
        $noEncontrados++
    }
}

Write-Host ""
Write-Host "? Limpieza completada!" -ForegroundColor Green
Write-Host "   Archivos eliminados: $eliminados" -ForegroundColor Green
Write-Host "   Archivos no encontrados: $noEncontrados" -ForegroundColor Gray
Write-Host ""

# Mostrar archivos que quedan
Write-Host "?? Archivos de documentación restantes:" -ForegroundColor Cyan
$docsRestantes = @(
    "README.md",
    "README_TESTING.md",
    "TESTING_BEST_PRACTICES.md",
    "RESUMEN_EJECUTIVO.md",
    "FINAL_SUMMARY.md"
)

foreach ($doc in $docsRestantes) {
    if (Test-Path $doc) {
        $size = (Get-Item $doc).Length / 1KB
        Write-Host "   ? $doc ($([math]::Round($size, 1)) KB)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "?? Scripts restantes:" -ForegroundColor Cyan
$scriptsRestantes = @(
    "run-tests-coverage.ps1",
    "run-sonarqube-analysis.ps1"
)

foreach ($script in $scriptsRestantes) {
    if (Test-Path $script) {
        Write-Host "   ? $script" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "?? Tip: Los archivos eliminados eran temporales y redundantes." -ForegroundColor Cyan
Write-Host "   Toda la información importante está en README.md y otros docs principales." -ForegroundColor Cyan
Write-Host ""
