# Integration-test coverage → ./TestResults-integration + ./coveragereport-integration (Html_Dark).
# Run from repo root:
#   powershell -ExecutionPolicy Bypass -File .\scripts\run-coverage-integration.ps1 [-OpenReport]
param([switch] $OpenReport)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $RepoRoot

Remove-Item -Recurse -Force ./TestResults-integration, ./coveragereport-integration -ErrorAction SilentlyContinue

dotnet test ./WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj `
    -c Release `
    --filter "Category=Integration" `
    --settings ./WildNatureExplorer.Tests/CodeCoverage.integration.runsettings `
    --results-directory ./TestResults-integration `
    --logger "trx;LogFileName=integration-results.trx"

$xml = Get-ChildItem -Path ./TestResults-integration -Recurse -Filter coverage.cobertura.xml |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not $xml) { throw "coverage.cobertura.xml not found under ./TestResults-integration" }

dotnet tool update --global dotnet-reportgenerator-globaltool 2>$null
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

& reportgenerator "-reports:$($xml.FullName)" "-targetdir:$RepoRoot/coveragereport-integration" "-reporttypes:Html_Dark"

Write-Host "HTML: $RepoRoot/coveragereport-integration/index.html"
if ($OpenReport) {
    Start-Process "$RepoRoot/coveragereport-integration/index.html"
}
