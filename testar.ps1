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
$buildDirectory = Join-Path $resultsDirectory "00-compilacao"
$buildLog = Join-Path $buildDirectory "compilacao.log"
$buildBinaryLog = Join-Path $buildDirectory "compilacao.binlog"
$playwrightScript = Join-Path $projectRoot "bin/InsEmpodera.E2ETests/Release/net9.0/playwright.ps1"
$backendResult = Join-Path $resultsDirectory "01-backend/resultado-backend-e-integracoes.trx"
$frontendResult = Join-Path $resultsDirectory "02-interface/resultado-interface-javascript.json"
$e2eResult = Join-Path $resultsDirectory "03-navegador/resultado-jornadas-chromium.trx"
$friendlyReport = Join-Path $resultsDirectory "RELATORIO-DE-TESTES.md"
$diagnosticJson = Join-Path $resultsDirectory "DIAGNOSTICO-DE-TESTES.json"
$reportScript = Join-Path $projectRoot "scripts/Gerar-RelatorioTestes.ps1"
$diagnosticSelfTest = Join-Path $projectRoot "scripts/Testar-DiagnosticoPipeline.ps1"
$failedStages = [System.Collections.Generic.List[string]]::new()
$pipelineFailure = $null
$reportFailure = $null

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

function Invoke-TestStage {
    param(
        [Parameter(Mandatory)] [string]$Label,
        [Parameter(Mandatory)] [scriptblock]$Command
    )

    Write-Host "`n==> $Label" -ForegroundColor Cyan
    & $Command
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $script:failedStages.Add($Label)
        Write-Host "A etapa falhou, mas as demais verificações continuarão para produzir um diagnóstico completo." -ForegroundColor Red
    }
    else {
        Write-Host "Etapa concluída com sucesso." -ForegroundColor Green
    }
}

Push-Location $projectRoot
try {
    New-Item -ItemType Directory -Path $buildDirectory -Force | Out-Null
    foreach ($currentResult in @($buildLog, $buildBinaryLog, $backendResult, $frontendResult, $e2eResult, $friendlyReport, $diagnosticJson)) {
        if (Test-Path -LiteralPath $currentResult) {
            Remove-Item -LiteralPath $currentResult -Force
        }
    }

    foreach ($commandName in @("dotnet", "node", "npm")) {
        if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
            throw "Pré-requisito ausente: '$commandName' não foi encontrado no PATH."
        }
    }

    Invoke-Checked "Restaurar dependencias .NET" { dotnet restore $solution }
    Invoke-Checked "Restaurar dependencias do frontend" { npm ci --prefix $frontendDirectory }
    Invoke-Checked "Compilar aplicacao e testes" {
        dotnet build $solution --configuration Release --no-restore `
            -p:GenerateFullPaths=true `
            -bl:$buildBinaryLog 2>&1 | Tee-Object -FilePath $buildLog
    }

    Invoke-Checked "Validar o analisador de falhas do pipeline" {
        & $diagnosticSelfTest -ProjectRoot $projectRoot
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

    Invoke-TestStage "1/3 — Regras, banco, segurança e integrações HTTP" {
        dotnet test $backendProject --configuration Release --no-build --no-restore `
            --collect:"XPlat Code Coverage" `
            --logger:"console;verbosity=minimal" `
            --logger:"trx;LogFileName=$(Split-Path -Leaf $backendResult)" `
            --results-directory (Split-Path -Parent $backendResult)
    }

    Invoke-TestStage "2/3 — Interface, eventos e contratos do HTML" {
        $env:VITEST_RESULT_PATH = $frontendResult
        try {
            npm test --prefix $frontendDirectory
        }
        finally {
            Remove-Item Env:VITEST_RESULT_PATH -ErrorAction SilentlyContinue
        }
    }

    Invoke-TestStage "3/3 — Jornadas reais de uso no Chromium" {
        dotnet test $e2eProject --configuration Release --no-build --no-restore `
            --logger:"console;verbosity=minimal" `
            --logger:"trx;LogFileName=$(Split-Path -Leaf $e2eResult)" `
            --results-directory (Split-Path -Parent $e2eResult)
    }

}
catch {
    $pipelineFailure = $_
    $failedStages.Add("Pipeline interrompido: $($_.Exception.Message)")
    Write-Host "`nO pipeline foi interrompido. O diagnóstico ainda será gerado com todas as evidências disponíveis." -ForegroundColor Red
}
finally {
    try {
        $pipelineErrorText = if ($pipelineFailure) { $pipelineFailure.Exception.Message } else { '' }
        & $reportScript `
            -BuildLog $buildLog `
            -BackendTrx $backendResult `
            -FrontendJson $frontendResult `
            -NavegadorTrx $e2eResult `
            -PipelineError $pipelineErrorText `
            -Saida $friendlyReport `
            -SaidaJson $diagnosticJson

        if (-not (Test-Path -LiteralPath $friendlyReport) -or
            -not (Select-String -LiteralPath $friendlyReport -SimpleMatch '**Resultado geral:** APROVADO' -Quiet)) {
            $failedStages.Add("Diagnóstico consolidado indicou falha")
        }

        if ($env:GITHUB_STEP_SUMMARY -and (Test-Path -LiteralPath $friendlyReport)) {
            Get-Content -LiteralPath $friendlyReport -Raw | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
        }
    }
    catch {
        $reportFailure = $_
        $fallbackLines = @(
            '# ❌ O gerador de diagnóstico também falhou',
            '',
            "**Falha original do pipeline:** $($pipelineFailure.Exception.Message)",
            '',
            "**Falha do relatório:** $($_.Exception.Message)",
            '',
            'Consulte `TestResults/00-compilacao/compilacao.log` e os arquivos TRX/JSON disponíveis.'
        )
        [IO.File]::WriteAllLines($friendlyReport, $fallbackLines, [Text.UTF8Encoding]::new($false))
        Write-Host "O gerador de diagnóstico falhou; um relatório mínimo de emergência foi criado." -ForegroundColor Red
    }
    Pop-Location
}

if ($pipelineFailure -or $reportFailure -or $failedStages.Count -gt 0) {
    throw "Falharam $($failedStages.Count) etapa(s): $($failedStages -join '; '). Consulte '$friendlyReport' e '$diagnosticJson'."
}

Write-Host "`nTodos os testes passaram. O resumo está em '$friendlyReport' e o diagnóstico estruturado em '$diagnosticJson'." -ForegroundColor Green
