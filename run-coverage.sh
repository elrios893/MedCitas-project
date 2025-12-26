#!/bin/bash

# Script para generar reporte de coverage (Linux/Mac)

echo "?? Ejecutando tests con coverage..."

# Limpiar reportes anteriores
rm -f coverage.opencover.xml
rm -rf coverage-report

# Ejecutar tests con coverage
dotnet test MedCitas.Tests/MedCitas.Tests.csproj \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput="../coverage.opencover.xml" \
    /p:Exclude="[xunit.*]*%2c[*.Tests]*"

if [ $? -ne 0 ]; then
    echo "? Los tests fallaron"
    exit 1
fi

echo "? Tests ejecutados exitosamente"

# Verificar si existe reportgenerator
if ! command -v reportgenerator &> /dev/null; then
    echo "??  reportgenerator no está instalado. Instalando..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generar reporte HTML
echo "?? Generando reporte HTML..."

reportgenerator \
    -reports:"coverage.opencover.xml" \
 -targetdir:"coverage-report" \
    -reporttypes:"Html;TextSummary"

if [ $? -eq 0 ]; then
    echo "? Reporte generado exitosamente"
    
    # Mostrar resumen
    if [ -f "coverage-report/Summary.txt" ]; then
        echo ""
        echo "?? Resumen de Coverage:"
        cat "coverage-report/Summary.txt"
    fi
    
    # Abrir reporte en navegador
    echo ""
    echo "?? Abriendo reporte en navegador..."
    
    if command -v xdg-open &> /dev/null; then
 xdg-open coverage-report/index.html
    elif command -v open &> /dev/null; then
        open coverage-report/index.html
    else
        echo "Por favor abre manualmente: coverage-report/index.html"
    fi
else
    echo "? Error al generar reporte"
    exit 1
fi

echo ""
echo "?? Proceso completado!"
