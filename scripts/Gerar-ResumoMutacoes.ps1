[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$PastaStryker,
    [Parameter(Mandatory)] [string]$Saida
)

$ErrorActionPreference = "Stop"
$reportFile = Get-ChildItem -LiteralPath $PastaStryker -Recurse -Filter 'qualidade-real-dos-testes.json' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not $reportFile) {
    throw "Nenhum relatório de mutation testing foi encontrado em '$PastaStryker'."
}

$report = Get-Content -LiteralPath $reportFile.FullName -Raw | ConvertFrom-Json
$mutants = @()
$areas = @()
foreach ($file in $report.files.PSObject.Properties) {
    $fileMutants = @($file.Value.mutants | Where-Object status -in @('Killed', 'Survived', 'NoCoverage', 'Timeout'))
    $mutants += $fileMutants
    if ($fileMutants.Count -gt 0) {
        $areas += [pscustomobject]@{
            Nome = Split-Path $file.Name -Leaf
            Detectados = @($fileMutants | Where-Object status -eq 'Killed').Count
            Sobreviventes = @($fileMutants | Where-Object status -eq 'Survived').Count
            SemCobertura = @($fileMutants | Where-Object status -eq 'NoCoverage').Count
            Timeouts = @($fileMutants | Where-Object status -eq 'Timeout').Count
        }
    }
}

$killed = @($mutants | Where-Object status -eq 'Killed').Count
$survived = @($mutants | Where-Object status -eq 'Survived').Count
$uncovered = @($mutants | Where-Object status -eq 'NoCoverage').Count
$timeouts = @($mutants | Where-Object status -eq 'Timeout').Count
$total = $killed + $survived + $uncovered + $timeouts
$score = if ($total -gt 0) { [math]::Round(100 * $killed / $total, 2) } else { 0 }
$classification = if ($score -ge 80) { 'Excelente' } elseif ($score -ge 60) { 'Aceitável' } elseif ($score -ge 55) { 'Atenção' } else { 'Insuficiente' }
$icon = if ($score -ge 60) { '✅' } elseif ($score -ge 55) { '⚠️' } else { '❌' }

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $icon Qualidade real dos testes — InsEmpodera")
$lines.Add('')
$lines.Add("**Índice de detecção:** $($score.ToString('0.00'))% — $classification  ")
$lines.Add("**Defeitos simulados detectados:** $killed de $total")
$lines.Add('')
$lines.Add('| Resultado da simulação | Quantidade | Interpretação |')
$lines.Add('|---|---:|---|')
$lines.Add("| ✅ Detectado | $killed | O teste falhou como deveria ao receber o defeito. |")
$lines.Add("| ❌ Sobreviveu | $survived | A alteração defeituosa não foi percebida. |")
$lines.Add("| ⚪ Sem cobertura | $uncovered | Nenhum teste executou esse trecho. |")
$lines.Add("| ⏱️ Tempo esgotado | $timeouts | O resultado não pôde ser determinado. |")
$lines.Add('')
$lines.Add('## Resultado por área crítica')
$lines.Add('')
$lines.Add('| Arquivo | Detectados | Sobreviventes | Sem cobertura | Timeouts |')
$lines.Add('|---|---:|---:|---:|---:|')
foreach ($area in $areas | Sort-Object Sobreviventes -Descending) {
    $lines.Add("| $($area.Nome) | $($area.Detectados) | $($area.Sobreviventes) | $($area.SemCobertura) | $($area.Timeouts) |")
}
$lines.Add('')
$lines.Add('O workflow falha se o índice cair abaixo de 55%, impedindo regressões graves na capacidade de detecção da suíte.')

$directory = Split-Path -Parent $Saida
if ($directory) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
[IO.File]::WriteAllLines($Saida, $lines, [Text.UTF8Encoding]::new($false))
Write-Host "Resumo legível do mutation testing: $Saida"
