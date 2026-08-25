[CmdletBinding()]
param(
    [switch]$Headed,
    [ValidateRange(0, 5000)]
    [int]$SlowMo = 0
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$projectRoot = $PSScriptRoot
$solution = Join-Path $projectRoot "InsEmpodera.sln"
$backendProject = Join-Path $projectRoot "InsEmpodera.Tests/InsEmpodera.Tests.csproj"
$frontendDirectory = Join-Path $projectRoot "Frontend.Tests"
$e2eProject = Join-Path $projectRoot "InsEmpodera.E2ETests/InsEmpodera.E2ETests.csproj"
$resultsDirectory = Join-Path $projectRoot "TestResults"
$playwrightScript = Join-Path $projectRoot "bin/InsEmpodera.E2ETests/Release/net9.0/playwright.ps1"

function Invoke-Checked {
    param(
        [Parameter(Mandatory)] [string]$Label,
        [Parameter(Mandatory)] [scriptblock]$Command
    )

    Write-Host "`n==> $Label" -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label falhou com o codigo $LASTEXITCODE."
    }
}

foreach ($commandName in @("dotnet", "node", "npm")) {
    if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
        throw "Pre-requisito ausente: '$commandName' nao foi encontrado no PATH."
    }
}

Push-Location $projectRoot
try {
    Invoke-Checked "Restaurar dependencias .NET" { dotnet restore $solution }
    Invoke-Checked "Restaurar dependencias do frontend" { npm ci --prefix $frontendDirectory }
    Invoke-Checked "Compilar aplicacao e testes" {
        dotnet build $solution --configuration Release --no-restore
    }

    if (-not (Test-Path -LiteralPath $playwrightScript)) {
        throw "O instalador do Playwright não foi gerado em '$playwrightScript'."
    }

    if ($env:CI) {
        Invoke-Checked "Preparar Chromium e dependencias do sistema" {
            & $playwrightScript install --with-deps chromium
        }
    }
    else {
        Invoke-Checked "Preparar Chromium" { & $playwrightScript install chromium }
    }

    if ($Headed) { $env:E2E_HEADED = "true" } else { Remove-Item Env:E2E_HEADED -ErrorAction SilentlyContinue }
    if ($SlowMo -gt 0) { $env:E2E_SLOWMO = $SlowMo.ToString() } else { Remove-Item Env:E2E_SLOWMO -ErrorAction SilentlyContinue }

    Invoke-Checked "Testar backend e integracoes HTTP" {
        dotnet test $backendProject --configuration Release --no-build --no-restore `
            --collect:"XPlat Code Coverage" --logger:"trx;LogFileName=backend.trx" `
            --results-directory (Join-Path $resultsDirectory "backend")
    }

    Invoke-Checked "Testar eventos e DOM do frontend com cobertura" {
        npm run coverage --prefix $frontendDirectory -- `
            --reporter=default --reporter=json --outputFile=vitest-results.json
    }

    Invoke-Checked "Testar jornadas reais no Chromium" {
        dotnet test $e2eProject --configuration Release --no-build --no-restore `
            --logger:"trx;LogFileName=e2e.trx" `
            --results-directory (Join-Path $resultsDirectory "e2e")
    }

    Write-Host "`nTodos os testes passaram: backend, frontend e navegador." -ForegroundColor Green
}
finally {
    Pop-Location
}
