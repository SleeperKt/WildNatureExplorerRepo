<#
.SYNOPSIS
  Prints image/container metrics for local reporting (sizes, creation time, optional build timing).

.PARAMETER Build
  Run docker compose build first and report elapsed time.

.PARAMETER Ps
  Run docker compose ps after metrics (stack should be running).

.EXAMPLE
  .\docker\scripts\show-docker-metrics.ps1
  .\docker\scripts\show-docker-metrics.ps1 -Build
  .\docker\scripts\show-docker-metrics.ps1 -Ps
#>
param(
    [switch]$Build,
    [switch]$Ps
)

function Format-DockerByteSize {
    param([long]$Bytes)
    if ($Bytes -lt 0) { return "$Bytes" }
    $units = @('B', 'KiB', 'MiB', 'GiB', 'TiB')
    $v = [double]$Bytes
    $idx = 0
    while ($v -ge 1024 -and $idx -lt ($units.Length - 1)) {
        $v /= 1024
        $idx++
    }
    if ($idx -eq 0) {
        return ('{0:N0} {1}' -f $Bytes, $units[0])
    }
    return ('{0:N2} {1}' -f $v, $units[$idx])
}

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $repoRoot

$composeFile = Join-Path $repoRoot "docker-compose.yml"
if (-not (Test-Path $composeFile)) {
    Write-Error "docker-compose.yml not found at $composeFile"
}

Write-Host ""
Write-Host "=== Wild Nature Explorer - Docker metrics ===" -ForegroundColor Cyan
Write-Host ("Repository: " + $repoRoot)
Write-Host ""

if ($Build) {
    Write-Host "--- docker compose build ---" -ForegroundColor Yellow
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    docker compose -f docker-compose.yml build
    $exit = $LASTEXITCODE
    $sw.Stop()
    Write-Host ("Build elapsed: " + $sw.Elapsed.ToString()) -ForegroundColor Green
    Write-Host ""
    if ($exit -ne 0) { exit $exit }
}

Write-Host "--- Compose images (service / repository / tag / size) ---" -ForegroundColor Yellow
Write-Host ""
docker compose -f docker-compose.yml images

Write-Host ""
Write-Host "--- Image inspect: created, size, architecture ---" -ForegroundColor Yellow
Write-Host ""

$tags = @(docker compose -f docker-compose.yml config --images 2>$null | Where-Object { $_ -match '\S' })
if (-not $tags -or $tags.Count -eq 0) {
    Write-Host "(Could not resolve images from compose config.)"
}
else {
    foreach ($tag in $tags) {
        $t = $tag.Trim()
        docker image inspect $t 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "(Not pulled/built yet: $t)"
            Write-Host "---"
            continue
        }
        Write-Host "Image reference: $t"
        $metaJson = docker image inspect $t --format '{{json .}}'
        $meta = $metaJson | ConvertFrom-Json
        $sz = [long]$meta.Size
        Write-Host ("ID: " + $meta.Id)
        Write-Host ("RepoTags: " + (($meta.RepoTags | ConvertTo-Json -Compress)))
        Write-Host ("Created: " + $meta.Created)
        Write-Host ("Size (bytes): " + $sz)
        Write-Host ("Size (human): " + (Format-DockerByteSize $sz))
        Write-Host ("Architecture: " + $meta.Architecture)
        Write-Host ("Os: " + $meta.Os)
        Write-Host "---"
    }
}

Write-Host ""
Write-Host "--- Reference: postgis/postgis ---" -ForegroundColor Yellow
Write-Host ""

docker images postgis/postgis --format "table {{.Repository}}\t{{.Tag}}\t{{.ID}}\t{{.CreatedSince}}\t{{.Size}}"

if ($Ps) {
    Write-Host ""
    Write-Host "--- docker compose ps ---" -ForegroundColor Yellow
    Write-Host ""
    docker compose -f docker-compose.yml ps
}

Write-Host ""
Write-Host 'Tip: cold-start - run Measure-Command { docker compose up -d }; then docker compose ps.' -ForegroundColor DarkGray
Write-Host 'Tip: runtime stats - docker stats --no-stream (stack running).' -ForegroundColor DarkGray
Write-Host ""
