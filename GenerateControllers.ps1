# Script para generar controladores y vistas automáticamente
# Ejecutar desde la carpeta raíz de la solución

$projectPath = ".\cmcookies"

# Lista de modelos para generar controladores
$models = @(
    @{Name="Cookie"; Plural="Cookies"},
    @{Name="Customer"; Plural="Customers"},
    @{Name="Order"; Plural="Orders"},
    @{Name="Material"; Plural="Materials"},
    @{Name="Batch"; Plural="Batches"},
    @{Name="OrderDetail"; Plural="OrderDetails"},
    @{Name="User"; Plural="Users"},
    @{Name="Role"; Plural="Roles"},
    @{Name="Billing"; Plural="Billings"},
    @{Name="Shipping"; Plural="Shippings"}
)

Write-Host "=== Generando Controladores y Vistas ===" -ForegroundColor Green
Write-Host ""

foreach ($model in $models) {
    $modelName = $model.Name
    $controllerName = "$($model.Plural)Controller"
    
    Write-Host "Generando $controllerName..." -ForegroundColor Yellow
    
    # Comando para generar el controlador
    $command = "dotnet aspnet-codegenerator controller " +
               "-name $controllerName " +
               "-m $modelName " +
               "-dc CmcDBContext " +
               "--relativeFolderPath Controllers " +
               "--useDefaultLayout " +
               "--referenceScriptLibraries"
    
    try {
        Set-Location $projectPath
        Invoke-Expression $command
        Write-Host "✓ $controllerName generado exitosamente" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Error al generar $controllerName : $_" -ForegroundColor Red
    }
    finally {
        Set-Location ..
    }
    
    Write-Host ""
}

Write-Host "=== Proceso Completado ===" -ForegroundColor Green
Write-Host ""
Write-Host "Recuerda actualizar tu _Layout.cshtml para agregar enlaces a los nuevos controladores" -ForegroundColor Cyan
