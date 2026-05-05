# Generates Cobertura under ./TestResults and HTML under ./coveragereport (Html_Dark theme).
# Run from repo root:  powershell -ExecutionPolicy Bypass -File .\scripts\run-coverage.ps1 [-OpenReport]
# (PowerShell 7+:       pwsh .\scripts\run-coverage.ps1 [-OpenReport])
param([switch] $OpenReport)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $RepoRoot

dotnet test ./WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj `
    -c Release `
    --filter "Category=Unit" `
    --settings ./WildNatureExplorer.Tests/CodeCoverage.runsettings `
    --results-directory ./TestResults

$xml = Get-ChildItem -Path ./TestResults -Recurse -Filter coverage.cobertura.xml |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not $xml) { throw "coverage.cobertura.xml not found under ./TestResults" }

dotnet tool update --global dotnet-reportgenerator-globaltool 2>$null
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

& reportgenerator "-reports:$($xml.FullName)" "-targetdir:$RepoRoot/coveragereport" "-reporttypes:Html_Dark"

Write-Host "Opened summary: line-rate is in coverage.cobertura.xml; HTML: $RepoRoot/coveragereport/index.html"
if ($OpenReport) {
    Start-Process "$RepoRoot/coveragereport/index.html"
}
