[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$projectRoot = $PSScriptRoot
$backendProject = Join-Path $projectRoot "InsEmpodera.Tests/InsEmpodera.Tests.csproj"
$resultsDirectory = Join-Path $projectRoot "TestResults"
$buildDirectory = Join-Path $resultsDirectory "00-compilacao"
$buildLog = Join-Path $buildDirectory "compilacao.log"
$buildBinaryLog = Join-Path $buildDirectory "compilacao.binlog"
$backendResult = Join-Path $resultsDirectory "01-backend/resultado-backend-e-integracoes.trx"
$friendlyReport = Join-Path $resultsDirectory "RELATORIO-DE-TESTES.md"
$diagnosticJson = Join-Path $resultsDirectory "DIAGNOSTICO-DE-TESTES.json"
$reportScript = Join-Path $projectRoot "scripts/Gerar-RelatorioTestes.ps1"
$failedStages = [System.Collections.Generic.List[string]]::new()
$pipelineFailure = $null
$reportFailure = $null
$injectedTestConnection = $false

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
    if ([string]::IsNullOrWhiteSpace($env:TEST_MYSQL_CONNECTION)) {
        $secretPrefix = 'ConnectionStrings:DefaultConnection = '
        $secretLine = & dotnet user-secrets list --project (Join-Path $projectRoot 'InsEmpodera.csproj') 2>$null |
            Where-Object { $_.StartsWith($secretPrefix, [StringComparison]::Ordinal) } |
            Select-Object -First 1
        if (-not $secretLine) {
            throw 'Defina TEST_MYSQL_CONNECTION ou ConnectionStrings:DefaultConnection no cofre local para executar os testes em MySQL.'
        }
        $env:TEST_MYSQL_CONNECTION = $secretLine.Substring($secretPrefix.Length)
        $injectedTestConnection = $true
    }

    New-Item -ItemType Directory -Path $buildDirectory -Force | Out-Null
    foreach ($currentResult in @($buildLog, $buildBinaryLog, $backendResult, $friendlyReport, $diagnosticJson)) {
        if (Test-Path -LiteralPath $currentResult) {
            Remove-Item -LiteralPath $currentResult -Force
        }
    }

    foreach ($commandName in @("dotnet")) {
        if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
            throw "Pré-requisito ausente: '$commandName' não foi encontrado no PATH."
        }
    }

    Invoke-Checked "Restaurar dependencias .NET" { dotnet restore $backendProject }
    Invoke-Checked "Compilar aplicacao e testes de backend" {
        dotnet build $backendProject --configuration Release --no-restore `
            -p:GenerateFullPaths=true `
            -bl:$buildBinaryLog 2>&1 | Tee-Object -FilePath $buildLog
    }

    Invoke-TestStage "Backend, banco e integrações HTTP" {
        dotnet test $backendProject --configuration Release --no-build --no-restore `
            --collect:"XPlat Code Coverage" `
            --logger:"console;verbosity=minimal" `
            --logger:"trx;LogFileName=$(Split-Path -Leaf $backendResult)" `
            --results-directory (Split-Path -Parent $backendResult)
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
    if ($injectedTestConnection) {
        Remove-Item Env:TEST_MYSQL_CONNECTION -ErrorAction SilentlyContinue
    }
    Pop-Location
}

if ($pipelineFailure -or $reportFailure -or $failedStages.Count -gt 0) {
    throw "Falharam $($failedStages.Count) etapa(s): $($failedStages -join '; '). Consulte '$friendlyReport' e '$diagnosticJson'."
}

Write-Host "`nTodos os testes passaram. O resumo está em '$friendlyReport' e o diagnóstico estruturado em '$diagnosticJson'." -ForegroundColor Green
