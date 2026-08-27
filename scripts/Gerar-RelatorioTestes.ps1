[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$BackendTrx,
    [Parameter(Mandatory)] [string]$FrontendJson,
    [Parameter(Mandatory)] [string]$NavegadorTrx,
    [Parameter(Mandatory)] [string]$Saida
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$registros = [System.Collections.Generic.List[object]]::new()

function Convert-ToFriendlyName {
    param([string]$Name)

    $shortName = ($Name -split '\.')[-1]
    $shortName = $shortName -replace '_', ' '
    $shortName = [regex]::Replace($shortName, '(?<=[a-zá-ú0-9])(?=[A-Z])', ' ')
    $shortName = [regex]::Replace($shortName, '\s+', ' ').Trim()
    return $shortName
}

function Convert-Outcome {
    param([string]$Outcome)
    switch -Regex ($Outcome) {
        '^(Passed|pass)$' { 'Aprovado'; break }
        '^(Failed|fail)$' { 'Falhou'; break }
        '^(Skipped|NotExecuted|Pending|Todo)$' { 'Ignorado'; break }
        default { if ([string]::IsNullOrWhiteSpace($Outcome)) { 'Não executado' } else { $Outcome } }
    }
}

function Add-TrxResults {
    param([string]$Path, [string]$Suite, [string]$AreaPadrao)
    if (-not (Test-Path -LiteralPath $Path)) { return }

    [xml]$document = Get-Content -LiteralPath $Path -Raw
    foreach ($result in $document.SelectNodes("//*[local-name()='UnitTestResult']")) {
        $duration = [TimeSpan]::Zero
        if ($result.duration) { [TimeSpan]::TryParse($result.duration, [ref]$duration) | Out-Null }
        $parts = $result.testName -split '\.'
        $className = if ($parts.Count -gt 1) { $parts[-2] } else { $AreaPadrao }
        $area = (Convert-ToFriendlyName ($className -replace 'Tests$', ''))
        $messageNode = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        $stackNode = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='StackTrace']")
        $registros.Add([pscustomobject]@{
            Suite = $Suite
            Area = $area
            Nome = Convert-ToFriendlyName $result.testName
            Resultado = Convert-Outcome $result.outcome
            DuracaoMs = [math]::Round($duration.TotalMilliseconds)
            Mensagem = if ($messageNode) { $messageNode.InnerText.Trim() } else { '' }
            Detalhes = if ($stackNode) { $stackNode.InnerText.Trim() } else { '' }
        })
    }
}

function Add-FrontendResults {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return }

    $document = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    foreach ($file in @($document.testResults)) {
        $area = [IO.Path]::GetFileNameWithoutExtension(($file.name -replace '\.test$', ''))
        foreach ($test in @($file.assertionResults)) {
            $failure = @($test.failureMessages) -join "`n"
            $duration = if ($null -ne $test.duration) { [double]$test.duration } else { 0 }
            $registros.Add([pscustomobject]@{
                Suite = 'Interface e JavaScript'
                Area = Convert-ToFriendlyName $area
                Nome = if ($test.fullName) { $test.fullName } else { $test.title }
                Resultado = Convert-Outcome $test.status
                DuracaoMs = [math]::Round($duration)
                Mensagem = $failure.Trim()
                Detalhes = ''
            })
        }
    }
}

Add-TrxResults -Path $BackendTrx -Suite 'Backend e integrações HTTP' -AreaPadrao 'Backend'
Add-FrontendResults -Path $FrontendJson
Add-TrxResults -Path $NavegadorTrx -Suite 'Jornadas reais no Chromium' -AreaPadrao 'Navegador'

$suitesEsperadas = @(
    [pscustomobject]@{ Nome = 'Backend e integrações HTTP'; Arquivo = $BackendTrx },
    [pscustomobject]@{ Nome = 'Interface e JavaScript'; Arquivo = $FrontendJson },
    [pscustomobject]@{ Nome = 'Jornadas reais no Chromium'; Arquivo = $NavegadorTrx }
)
$falhas = @($registros | Where-Object Resultado -eq 'Falhou')
$ignorados = @($registros | Where-Object Resultado -eq 'Ignorado')
$naoExecutadas = @($suitesEsperadas | Where-Object { -not (Test-Path -LiteralPath $_.Arquivo) })
$resultadoGeral = if ($falhas.Count -gt 0 -or $naoExecutadas.Count -gt 0) { 'FALHOU' } else { 'APROVADO' }
$icone = if ($resultadoGeral -eq 'APROVADO') { '✅' } else { '❌' }
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $icone Relatório consolidado de testes — InsEmpodera")
$lines.Add('')
$lines.Add("**Resultado geral:** $resultadoGeral  ")
$lines.Add("**Gerado em:** $((Get-Date).ToString('dd/MM/yyyy HH:mm:ss zzz'))")
$lines.Add('')
$lines.Add('## Resumo rápido')
$lines.Add('')
$lines.Add('| Etapa | Resultado | Aprovados | Falhas | Ignorados | Duração |')
$lines.Add('|---|---:|---:|---:|---:|---:|')

foreach ($suite in $suitesEsperadas) {
    $items = @($registros | Where-Object Suite -eq $suite.Nome)
    $approved = @($items | Where-Object Resultado -eq 'Aprovado').Count
    $failed = @($items | Where-Object Resultado -eq 'Falhou').Count
    $skipped = @($items | Where-Object Resultado -eq 'Ignorado').Count
    $duration = [TimeSpan]::FromMilliseconds(($items | Measure-Object DuracaoMs -Sum).Sum)
    $status = if (-not (Test-Path -LiteralPath $suite.Arquivo)) { '⚪ Não executado' } elseif ($failed -gt 0) { '❌ Falhou' } else { '✅ Aprovado' }
    $lines.Add("| $($suite.Nome) | $status | $approved | $failed | $skipped | $($duration.ToString('hh\:mm\:ss')) |")
}

$lines.Add('')
$lines.Add('## O que precisa de atenção')
$lines.Add('')
if ($falhas.Count -eq 0 -and $naoExecutadas.Count -eq 0) {
    $lines.Add('Nenhuma falha foi encontrada. Todas as etapas obrigatórias foram executadas.')
}
else {
    foreach ($missing in $naoExecutadas) {
        $lines.Add("- **$($missing.Nome):** não produziu arquivo de resultado; consulte a saída da etapa anterior.")
    }
    foreach ($failure in $falhas) {
        $lines.Add('')
        $lines.Add("### ❌ $($failure.Area) — $($failure.Nome)")
        $lines.Add('')
        $lines.Add("- Etapa: $($failure.Suite)")
        $lines.Add("- Duração: $($failure.DuracaoMs) ms")
        $lines.Add('')
        $lines.Add('```text')
        $lines.Add(($failure.Mensagem -replace '```', "'''"))
        $lines.Add('```')
    }
}

$lines.Add('')
$lines.Add('## Testes mais demorados')
$lines.Add('')
$lines.Add('| Etapa | Área | Teste | Duração |')
$lines.Add('|---|---|---|---:|')
foreach ($item in @($registros | Sort-Object DuracaoMs -Descending | Select-Object -First 10)) {
    $safeName = $item.Nome -replace '\|', '\|'
    $lines.Add("| $($item.Suite) | $($item.Area) | $safeName | $($item.DuracaoMs) ms |")
}

$lines.Add('')
$lines.Add('## Como interpretar')
$lines.Add('')
$lines.Add('- **Backend e integrações HTTP:** regras de negócio, banco, segurança, permissões e endpoints.')
$lines.Add('- **Interface e JavaScript:** eventos, formulários, componentes e contratos do HTML.')
$lines.Add('- **Jornadas reais no Chromium:** ações completas de uma pessoa usando o sistema no navegador.')
$lines.Add('- Arquivos `.trx` e `.json` continuam disponíveis para diagnóstico técnico e integração com o GitHub.')

$directory = Split-Path -Parent $Saida
if ($directory) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
[IO.File]::WriteAllLines($Saida, $lines, [Text.UTF8Encoding]::new($false))

Write-Host "`n============================================================" -ForegroundColor DarkCyan
Write-Host " RESULTADO GERAL: $resultadoGeral" -ForegroundColor $(if ($resultadoGeral -eq 'APROVADO') { 'Green' } else { 'Red' })
Write-Host " Testes executados: $($registros.Count) | Falhas: $($falhas.Count) | Ignorados: $($ignorados.Count)"
Write-Host " Relatório fácil de ler: $Saida"
Write-Host "============================================================" -ForegroundColor DarkCyan
