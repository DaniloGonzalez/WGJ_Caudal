param(
    [string]$ProjectPath = (Split-Path -Parent $PSScriptRoot)
)

# Compatibilidad local entre Render Pipelines 17.7.0 y Pipeline 0.8.
# No cambia versiones, manifest, lockfile ni archivos bajo Assets.
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $ProjectPath).Path
$cachePath = Join-Path $project 'Library/PackageCache'
$corePackages = @(Get-ChildItem -LiteralPath $cachePath -Directory -Filter 'com.unity.render-pipelines.core@*' |
    Where-Object {
        $packageJson = Join-Path $_.FullName 'package.json'
        (Get-Content -Raw -LiteralPath $packageJson | ConvertFrom-Json).version -eq '17.7.0'
    })
if ($corePackages.Count -ne 1) {
    throw 'Se esperaba exactamente una caché de Render Pipelines Core 17.7.0.'
}
$pipelinePackages = @(Get-ChildItem -LiteralPath $cachePath -Directory -Filter 'com.unity.pipeline@*' |
    Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'Runtime/Attributes/Unity.Pipeline.Attributes.asmdef') })
if ($pipelinePackages.Count -eq 0) {
    throw 'Unity.Pipeline.Attributes no está instalado. Resuelve los paquetes primero.'
}
$assemblyPath = Join-Path $corePackages[0].FullName 'Editor/CLI/Unity.RenderPipelines.Core.Editor.CLI.asmdef'
$assembly = Get-Content -Raw -LiteralPath $assemblyPath | ConvertFrom-Json
if ($assembly.name -ne 'Unity.RenderPipelines.Core.Editor.CLI') { throw 'Ensamblado inesperado.' }
if ($assembly.references -contains 'Unity.Pipeline.Attributes') {
    Write-Output 'La referencia de compatibilidad ya existe.'
} else {
    $assembly.references = @($assembly.references) + 'Unity.Pipeline.Attributes'
    $assembly | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $assemblyPath -Encoding utf8
    Write-Output 'Referencia añadida. Actualiza el proyecto y recompila desde Unity.'
}
