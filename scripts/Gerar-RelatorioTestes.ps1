[CmdletBinding()]
param(
    [string]$BackendTrx,
    [string]$FrontendJson,
    [string]$NavegadorTrx,
    [string]$MySqlTrx,
    [string]$BuildLog,
    [string]$PipelineError,
    [Parameter(Mandatory)] [string]$Saida,
    [string]$SaidaJson,
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$registros = [System.Collections.Generic.List[object]]::new()
$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path

function Remove-AnsiEscape {
    param([AllowEmptyString()] [string]$Text)
    $safeText = if ($null -eq $Text) { '' } else { $Text }
    return [regex]::Replace($safeText, "`e\[[0-9;?]*[ -/]*[@-~]", '')
}

function Resolve-RepositoryPath {
    param([string]$RawPath)

    if ([string]::IsNullOrWhiteSpace($RawPath)) { return $null }
    $normalized = ($RawPath -replace '^file:///?', '') -replace '\\', '/'
    $rootNormalized = $resolvedProjectRoot -replace '\\', '/'
    if ($normalized.StartsWith($rootNormalized, [StringComparison]::OrdinalIgnoreCase)) {
        return $normalized.Substring($rootNormalized.Length).TrimStart('/')
    }

    $segments = $normalized.TrimStart('/').Split('/', [StringSplitOptions]::RemoveEmptyEntries)
    for ($index = 0; $index -lt $segments.Count; $index++) {
        $suffix = ($segments[$index..($segments.Count - 1)] -join '/')
        if (Test-Path -LiteralPath (Join-Path $resolvedProjectRoot $suffix)) {
            return $suffix
        }
    }

    return $null
}

function Find-SourceLocation {
    param([AllowEmptyString()] [string]$Text)

    $cleanText = Remove-AnsiEscape $Text
    $patterns = @(
        '(?<path>(?:[A-Za-z]:)?[/\\][^\r\n]*?\.(?:cs|cshtml)):line\s+(?<line>\d+)',
        '(?<path>(?:file:///?|[A-Za-z]:)?[/\\][^\r\n]*?\.(?:js|mjs|cjs|jsx|ts|tsx|cshtml)):(?<line>\d+)(?::(?<column>\d+))?'
    )

    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($cleanText, $pattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $relativePath = Resolve-RepositoryPath $match.Groups['path'].Value
            if ($relativePath) {
                $column = if ($match.Groups['column'].Success) { [int]$match.Groups['column'].Value } else { 1 }
                return [pscustomobject]@{
                    Arquivo = $relativePath
                    Linha = [int]$match.Groups['line'].Value
                    Coluna = $column
                }
            }
        }
    }

    return $null
}

function Find-AllSourceLocations {
    param([AllowEmptyString()] [string]$Text)

    $cleanText = Remove-AnsiEscape $Text
    $locationResults = [System.Collections.Generic.List[object]]::new()
    $patterns = @(
        '(?<path>(?:[A-Za-z]:)?[/\\][^\r\n]*?\.(?:cs|cshtml)):line\s+(?<line>\d+)',
        '(?<path>(?:file:///?|[A-Za-z]:)?[/\\][^\r\n]*?\.(?:js|mjs|cjs|jsx|ts|tsx|cshtml)):(?<line>\d+)(?::(?<column>\d+))?',
        '(?<path>(?:[A-Za-z]:)?[^\r\n:]*?\.(?:cs|cshtml))\((?<line>\d+),(?<column>\d+)\)'
    )

    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($cleanText, $pattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $relativePath = Resolve-RepositoryPath $match.Groups['path'].Value.Trim()
            if (-not $relativePath) { continue }
            $line = [int]$match.Groups['line'].Value
            $column = if ($match.Groups['column'].Success) { [int]$match.Groups['column'].Value } else { 1 }
            $key = "$relativePath`:$line`:$column"
            if (@($locationResults | Where-Object Chave -eq $key).Count -eq 0) {
                $kind = if ($relativePath -match '^(InsEmpodera\.Tests|InsEmpodera\.E2ETests|Frontend\.Tests)/') { 'Teste' } else { 'Aplicação' }
                $locationResults.Add([pscustomobject]@{
                    Chave = $key
                    Arquivo = $relativePath
                    Linha = $line
                    Coluna = $column
                    Tipo = $kind
                })
            }
        }
    }
    return @($locationResults)
}

function Protect-SensitiveText {
    param([AllowEmptyString()] [string]$Text)

    $value = Remove-AnsiEscape $Text
    $value = [regex]::Replace($value, '(?i)(Password|Senha|Pwd|Secret|ClientSecret|Token|AccessToken|RefreshToken|ApiKey|SessionId)\s*[:=]\s*([^;\s,&]+)', '$1=[valor protegido]')
    $value = [regex]::Replace($value, '(?i)(Authorization\s*:\s*(?:Bearer|Basic)\s+)[A-Za-z0-9._~+/=-]+', '$1[token protegido]')
    $value = [regex]::Replace($value, '(?i)(Cookie|Set-Cookie)\s*:\s*[^\r\n]+', '$1: [cookie protegido]')
    $value = [regex]::Replace($value, '(?i)\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b', '[email protegido]')
    $value = [regex]::Replace($value, '\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}(?:\.[A-Za-z0-9_-]{10,})?\b', '[token protegido]')
    return $value
}

function Get-ExpectedActual {
    param([AllowEmptyString()] [string]$Message)

    $clean = Protect-SensitiveText $Message
    $expected = ''
    $actual = ''
    $expectedMatch = [regex]::Match($clean, '(?im)^\s*(?:Expected|Esperado|Not found)\s*:\s*(?<value>.+)$')
    $actualMatch = [regex]::Match($clean, '(?im)^\s*(?:Actual|Received|Recebido|String)\s*:\s*(?<value>.+)$')
    if ($expectedMatch.Success) { $expected = $expectedMatch.Groups['value'].Value.Trim() }
    if ($actualMatch.Success) { $actual = $actualMatch.Groups['value'].Value.Trim() }
    if (-not $expected -and $clean -match '(?i)expected\s+(?<expected>.+?)\s+to\s+(?:be|equal|contain)\s+(?<actual>.+)') {
        $expected = $Matches['actual'].Trim()
        $actual = $Matches['expected'].Trim()
    }
    return [pscustomobject]@{ Esperado = $expected; Recebido = $actual }
}

function Get-FailureContext {
    param([object]$Failure)

    $text = "$($Failure.NomeTecnico)`n$($Failure.Mensagem)`n$($Failure.Detalhes)"
    $method = ''
    $controller = ''
    $route = ''
    $entity = ''
    $table = ''

    $methodMatch = [regex]::Match($text, '(?m)\bat\s+(?<method>(?:Empodera|InsEmpodera)\.[^(\r\n]+)\(')
    if ($methodMatch.Success) { $method = $methodMatch.Groups['method'].Value.Trim() }
    $controllerMatch = [regex]::Match($text, '(?<controller>[A-Za-zÀ-ÿ0-9_]+Controller)')
    if ($controllerMatch.Success) { $controller = $controllerMatch.Groups['controller'].Value }
    $routeMatch = [regex]::Match($text, '(?i)(?:\bpath|\broute|\brequest|\bGET|\bPOST|\bPUT|\bPATCH|\bDELETE)\s*[:=]?\s*["'']?(?<route>/[A-Za-z][A-Za-z0-9_./?=&%-]*)')
    if ($routeMatch.Success) { $route = $routeMatch.Groups['route'].Value.TrimEnd('.', ',', ')', '"', "'") }
    $tableMatch = [regex]::Match($text, '(?i)(?:table|tabela|references)\s+[`''"]?(?:[A-Za-z0-9_]+[`''"]?\.)?[`''"]?(?<table>[A-Za-z_][A-Za-z0-9_]*)')
    if ($tableMatch.Success) { $table = $tableMatch.Groups['table'].Value }
    $entities = @('Usuario', 'Perfil', 'Permissoes', 'Comunidade', 'Atores', 'AtorComunidade', 'DiarioCampo', 'FichaPrimeiroContato', 'AvaliacaoPessoal', 'Atividades', 'RedeRecursos', 'Vulnerabilidade')
    foreach ($candidate in $entities) {
        if ($text -match "(?i)\b$([regex]::Escape($candidate))s?\b") { $entity = $candidate; break }
    }
    return [pscustomobject]@{ Metodo = $method; Controller = $controller; Rota = $route; Entidade = $entity; Tabela = $table }
}

function Get-DiagnosticRule {
    param([object]$Failure)

    $text = "$($Failure.Suite)`n$($Failure.Area)`n$($Failure.NomeTecnico)`n$($Failure.Mensagem)`n$($Failure.Detalhes)"
    if ($Failure.Suite -eq 'Interface e JavaScript') {
        return [pscustomobject]@{ Id='INTERFACE'; Category='Interface'; Severity='Média'; Confidence=93; Title='Um comportamento da interface divergiu'; Explanation='Um evento, componente, seletor ou contrato de HTML não produziu o estado esperado.'; Action='Confira o arquivo JavaScript, a view relacionada e o trecho esperado versus recebido.'; Cascade=$false }
    }
    if ($Failure.Suite -eq 'Jornadas reais no Chromium' -and
        $text -notmatch 'Timeout|timed out|TaskCanceledException|ECONNRESET|connection reset|TargetClosed|browser has been closed|net::ERR_') {
        return [pscustomobject]@{ Id='BROWSER'; Category='Navegador'; Severity='Média'; Confidence=93; Title='Uma jornada real no navegador divergiu'; Explanation='O navegador não encontrou o estado visual, elemento ou resposta esperado durante a jornada.'; Action='Abra screenshot e trace, confira a URL atual, o seletor semântico e as requisições HTTP imediatamente anteriores.'; Cascade=$false }
    }
    $rules = @(
        @{ Id='MISSING_RESULT'; Pattern='não produziu arquivo de resultado|resultado obrigatório ausente'; Category='Infraestrutura'; Severity='Crítica'; Confidence=96; Title='Uma ou mais etapas não produziram resultado'; Explanation='O pipeline foi interrompido antes de concluir essas etapas ou uma ferramenta terminou sem gerar o arquivo obrigatório.'; Action='Corrija primeiro a falha de restauração, compilação ou preparação imediatamente anterior. Depois confirme se os arquivos TRX/JSON voltam a ser produzidos.'; Cascade=$true },
        @{ Id='PIPELINE_INFRASTRUCTURE'; Pattern='Pipeline interrompido|Pré-requisito ausente|Restaurar dependencias|Preparar Chromium|falhou com o codigo'; Category='Infraestrutura'; Severity='Crítica'; Confidence=98; Title='A infraestrutura interrompeu o pipeline'; Explanation='Uma ferramenta, restauração ou preparação obrigatória falhou antes que todas as suítes pudessem terminar.'; Action='Corrija a primeira mensagem desta causa. As etapas sem resultado são consequências esperadas da interrupção.'; Cascade=$true },
        @{ Id='COMPILATION'; Pattern='^Compilação|\b(?:CS|MSB)\d{4}\b|Build FAILED|Compilação falhou'; Category='Compilação'; Severity='Crítica'; Confidence=99; Title='O código não pôde ser compilado'; Explanation='O compilador encontrou um erro estrutural, de tipo, referência ou sintaxe e interrompeu as etapas posteriores.'; Action='Abra o arquivo e a linha indicados, corrija o primeiro erro de compilação e execute novamente; erros seguintes podem ser consequência dele.'; Cascade=$true },
        @{ Id='EF_MULTIPLE_PROVIDERS'; Pattern='database providers.+have been registered|Only a single database provider'; Category='Banco'; Severity='Crítica'; Confidence=99; Title='Mais de um provedor de banco foi registrado'; Explanation='A aplicação tentou inicializar o mesmo contexto com dois provedores de banco. O host não consegue iniciar, portanto os demais testes afetados são provavelmente falhas em cascata.'; Action='Revise a configuração do ApplicationDbContext e garanta que apenas um UseSqlite ou UseMySql seja executado por ambiente.'; Cascade=$true },
        @{ Id='MYSQL_UNKNOWN_COLUMN'; Pattern='Unknown column|no such column|invalid column name'; Category='Banco'; Severity='Alta'; Confidence=98; Title='A coluna esperada pela aplicação não existe no banco'; Explanation='O modelo da aplicação e o esquema efetivamente carregado estão diferentes.'; Action='Compare a propriedade indicada com Banco.txt e confirme que a base de teste foi recriada a partir da versão atual.'; Cascade=$true },
        @{ Id='FOREIGN_KEY'; Pattern='foreign key constraint|FOREIGN KEY|constraint failed|Cannot add or update a child row'; Category='Banco'; Severity='Alta'; Confidence=97; Title='Uma restrição de relacionamento do banco foi violada'; Explanation='A operação tentou gravar ou excluir dados incompatíveis com uma chave estrangeira.'; Action='Confira a entidade dependente, a chave informada e as regras OnDelete no modelo e no Banco.txt.'; Cascade=$false },
        @{ Id='DB_UPDATE'; Pattern='DbUpdateException|SqlException|MySqlException|SqliteException|SQLSTATE'; Category='Banco'; Severity='Alta'; Confidence=92; Title='O banco recusou uma operação de persistência'; Explanation='O Entity Framework enviou a operação, mas o provedor de banco a rejeitou.'; Action='Leia a exceção interna, a tabela e a restrição indicadas; compare o modelo do EF com Banco.txt.'; Cascade=$false },
        @{ Id='AUTHORIZATION'; Pattern='authorization|permission|permiss|forbidden|access denied|403|policy|BOLA|IDOR|antiforgery|CSRF'; Category='Autorização'; Severity='Alta'; Confidence=90; Title='Uma regra de acesso ou proteção de operação divergiu'; Explanation='O resultado observado indica que uma pessoa recebeu acesso indevido ou foi bloqueada quando deveria ter permissão.'; Action='Confira a política, o perfil, o módulo e a validação de propriedade do recurso no controller indicado.'; Cascade=$false },
        @{ Id='AUTHENTICATION'; Pattern='login|password|senha|Identity|authentication|cookie|sign.?in|401|Unauthenticated'; Category='Autenticação'; Severity='Alta'; Confidence=88; Title='O fluxo de autenticação não se comportou como esperado'; Explanation='A criação, validação ou preservação da identidade da pessoa usuária divergiu do contrato testado.'; Action='Confira AccountController, configuração do Identity, cookie de autenticação e estado Ativo do usuário.'; Cascade=$false },
        @{ Id='TIMEOUT_FLAKY'; Pattern='Timeout|timed out|TaskCanceledException|ECONNRESET|connection reset|TargetClosed|browser has been closed|net::ERR_'; Category='Instabilidade'; Severity='Média'; Confidence=86; Title='A execução foi interrompida por tempo ou conexão instável'; Explanation='A evidência aponta para indisponibilidade temporária, lentidão ou encerramento inesperado de um processo externo.'; Action='Verifique trace, screenshot, console do navegador e duração. Repita somente este teste para distinguir lentidão real de instabilidade.'; Cascade=$false },
        @{ Id='BROWSER'; Pattern='^Jornadas reais no Chromium|Playwright|Chromium|locator|selector|page\.|Screenshot|accessibility|axe-core'; Category='Navegador'; Severity='Média'; Confidence=88; Title='Uma jornada real no navegador divergiu'; Explanation='O navegador não encontrou o estado visual, elemento ou resposta esperado durante a jornada.'; Action='Abra screenshot e trace, confira a URL atual, o seletor semântico e as requisições HTTP imediatamente anteriores.'; Cascade=$false },
        @{ Id='INTERFACE'; Pattern='^Interface e JavaScript|Vitest|jsdom|TestingLibrary|HTMLElement|document\.|querySelector|toHaveClass|toBeVisible'; Category='Interface'; Severity='Média'; Confidence=88; Title='Um comportamento da interface divergiu'; Explanation='Um evento, componente, seletor ou contrato de HTML não produziu o estado esperado.'; Action='Confira o arquivo JavaScript, a view relacionada e o trecho esperado versus recebido.'; Cascade=$false },
        @{ Id='ASSERT_CONTAINS'; Pattern='Assert\.Contains|Sub-string not found|toContain'; Category='Validação'; Severity='Média'; Confidence=94; Title='Um conteúdo obrigatório não foi encontrado'; Explanation='A resposta foi produzida, mas não contém o texto, atributo ou elemento exigido pelo teste.'; Action='Compare o valor esperado e o recebido e confirme se a mudança é um defeito ou uma alteração intencional do contrato.'; Cascade=$false },
        @{ Id='ASSERT_EQUAL'; Pattern='Assert\.Equal|Expected:|Actual:|expected.+to (?:be|equal)'; Category='Validação'; Severity='Média'; Confidence=92; Title='O resultado recebido é diferente do esperado'; Explanation='A operação terminou, porém o valor, status ou estado final não corresponde ao contrato.'; Action='Compare esperado e recebido no relatório e siga a primeira localização da aplicação antes da asserção do teste.'; Cascade=$false },
        @{ Id='HTTP_SERVER_ERROR'; Pattern='\b500\b|Internal Server Error|Unhandled exception'; Category='Infraestrutura'; Severity='Alta'; Confidence=86; Title='A aplicação respondeu com erro interno'; Explanation='Uma exceção não tratada impediu a conclusão da requisição.'; Action='Siga a primeira linha da aplicação no rastreamento e examine a requisição, a rota e os dados usados.'; Cascade=$false }
    )

    foreach ($rule in $rules) {
        if ($text -match $rule.Pattern) { return [pscustomobject]$rule }
    }
    if ($Failure.Suite -eq 'Interface e JavaScript') {
        return [pscustomobject]@{ Id='INTERFACE_GENERIC'; Category='Interface'; Severity='Média'; Confidence=72; Title='Falha em contrato da interface'; Explanation='O teste de interface encontrou uma diferença ainda não classificada por uma regra específica.'; Action='Compare a mensagem, o trecho JavaScript e a view relacionada.'; Cascade=$false }
    }
    if ($Failure.Suite -eq 'Jornadas reais no Chromium') {
        return [pscustomobject]@{ Id='BROWSER_GENERIC'; Category='Navegador'; Severity='Média'; Confidence=72; Title='Falha em jornada do navegador'; Explanation='A jornada real terminou em um estado diferente do esperado.'; Action='Abra as evidências do Playwright e confira o primeiro arquivo da aplicação no rastreamento.'; Cascade=$false }
    }
    return [pscustomobject]@{ Id='UNCLASSIFIED'; Category='Validação'; Severity='Média'; Confidence=55; Title='Falha ainda não reconhecida pelo catálogo'; Explanation='Há evidência suficiente para localizar a falha, mas não para afirmar uma causa específica.'; Action='Use a mensagem, esperado versus recebido e rastreamento; depois adicione uma regra ao catálogo se o padrão for recorrente.'; Cascade=$false }
}

function Get-Fingerprint {
    param([object]$Failure, [object]$Rule)
    $message = (Get-PrimaryMessage $Failure.Mensagem).ToLowerInvariant()
    $message = [regex]::Replace($message, '(?:[A-Za-z]:)?[/\\][^\s:]+', '<caminho>')
    $message = [regex]::Replace($message, '\bline\s+\d+|:\d+(?::\d+)?', '<linha>')
    $message = [regex]::Replace($message, '\b[0-9a-f]{8}-[0-9a-f-]{27,}\b', '<id>', 'IgnoreCase')
    $message = [regex]::Replace($message, '"[^"\r\n]{0,80}"', '"<valor>"')
    $message = [regex]::Replace($message, '\s+', ' ').Trim()
    if ($message.Length -gt 260) { $message = $message.Substring(0, 260) }
    return "$($Rule.Id)|$message"
}

function Get-CodeExcerpt {
    param([string]$RelativePath, [int]$Line, [int]$Radius = 3)

    if (-not $RelativePath -or $Line -lt 1) { return @() }
    $fullPath = Join-Path $resolvedProjectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { return @() }

    $content = @(Get-Content -LiteralPath $fullPath)
    $start = [math]::Max(1, $Line - $Radius)
    $end = [math]::Min($content.Count, $Line + $Radius)
    $excerpt = [System.Collections.Generic.List[string]]::new()
    for ($current = $start; $current -le $end; $current++) {
        $marker = if ($current -eq $Line) { '>>' } else { '  ' }
        $excerpt.Add(('{0} {1,5} | {2}' -f $marker, $current, $content[$current - 1]))
    }
    return $excerpt
}

function Get-PrimaryMessage {
    param([AllowEmptyString()] [string]$Message)
    $line = @(Remove-AnsiEscape $Message -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
    if ($line.Count -eq 0) { return 'Falha sem mensagem detalhada' }
    return $line[0].Trim()
}

function Escape-MarkdownCell {
    param([AllowEmptyString()] [string]$Value)
    return ((Remove-AnsiEscape $Value) -replace '\|', '\|' -replace "`r?`n", ' ').Trim()
}

function Escape-GitHubCommandValue {
    param([AllowEmptyString()] [string]$Value, [switch]$Property)
    $safeValue = if ($null -eq $Value) { '' } else { $Value }
    $escaped = $safeValue -replace '%', '%25' -replace "`r", '%0D' -replace "`n", '%0A'
    if ($Property) { $escaped = $escaped -replace ':', '%3A' -replace ',', '%2C' }
    return $escaped
}

function Get-SourceLink {
    param([string]$RelativePath, [int]$Line)
    $label = "$RelativePath`:$Line"
    if ($env:GITHUB_SERVER_URL -and $env:GITHUB_REPOSITORY -and $env:GITHUB_SHA) {
        $encodedPath = (($RelativePath -replace '\\', '/') -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }) -join '/'
        return "[$label]($($env:GITHUB_SERVER_URL)/$($env:GITHUB_REPOSITORY)/blob/$($env:GITHUB_SHA)/$encodedPath#L$Line)"
    }
    return ('`' + $label + '`')
}

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
    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path)) { return }

    [xml]$document = Get-Content -LiteralPath $Path -Raw
    foreach ($result in $document.SelectNodes("//*[local-name()='UnitTestResult']")) {
        $duration = [TimeSpan]::Zero
        if ($result.duration) { [TimeSpan]::TryParse($result.duration, [ref]$duration) | Out-Null }
        $parts = $result.testName -split '\.'
        $className = if ($parts.Count -gt 1) { $parts[-2] } else { $AreaPadrao }
        $area = (Convert-ToFriendlyName ($className -replace 'Tests$', ''))
        $messageNode = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        $stackNode = $result.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='StackTrace']")
        $message = if ($messageNode) { $messageNode.InnerText.Trim() } else { '' }
        $details = if ($stackNode) { $stackNode.InnerText.Trim() } else { '' }
        $locations = @(Find-AllSourceLocations "$message`n$details")
        $location = $locations | Select-Object -First 1
        $registros.Add([pscustomobject]@{
            Suite = $Suite
            Area = $area
            NomeTecnico = [string]$result.testName
            Nome = Convert-ToFriendlyName $result.testName
            Resultado = Convert-Outcome $result.outcome
            DuracaoMs = [math]::Round($duration.TotalMilliseconds)
            Mensagem = $message
            Detalhes = $details
            Arquivo = if ($location) { $location.Arquivo } else { '' }
            Linha = if ($location) { $location.Linha } else { 0 }
            Coluna = if ($location) { $location.Coluna } else { 0 }
            Localizacoes = $locations
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
            $locations = @(Find-AllSourceLocations "$failure`n$($file.name)")
            $location = $locations | Select-Object -First 1
            $registros.Add([pscustomobject]@{
                Suite = 'Interface e JavaScript'
                Area = Convert-ToFriendlyName $area
                NomeTecnico = if ($test.fullName) { $test.fullName } else { $test.title }
                Nome = if ($test.fullName) { $test.fullName } else { $test.title }
                Resultado = Convert-Outcome $test.status
                DuracaoMs = [math]::Round($duration)
                Mensagem = $failure.Trim()
                Detalhes = $file.name
                Arquivo = if ($location) { $location.Arquivo } else {
                    $resolvedFrontendPath = Resolve-RepositoryPath $file.name
                    if ($resolvedFrontendPath) { $resolvedFrontendPath } else { '' }
                }
                Linha = if ($location) { $location.Linha } else { 0 }
                Coluna = if ($location) { $location.Coluna } else { 0 }
                Localizacoes = $locations
            })
        }
    }
}

function Add-BuildResults {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path)) { return }

    $content = Get-Content -LiteralPath $Path -Raw
    $pattern = '(?m)^(?<path>.+?\.(?:cs|cshtml|props|targets|csproj))\((?<line>\d+),(?<column>\d+)\):\s*error\s+(?<code>[A-Z]+\d+)\s*:\s*(?<message>.+?)(?:\s+\[[^\]]+\])?$'
    foreach ($match in [regex]::Matches($content, $pattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $relativePath = Resolve-RepositoryPath $match.Groups['path'].Value.Trim()
        $location = if ($relativePath) {
            [pscustomobject]@{ Chave="$relativePath`:$($match.Groups['line'].Value)"; Arquivo=$relativePath; Linha=[int]$match.Groups['line'].Value; Coluna=[int]$match.Groups['column'].Value; Tipo='Aplicação' }
        } else { $null }
        $message = "$($match.Groups['code'].Value): $($match.Groups['message'].Value.Trim())"
        $registros.Add([pscustomobject]@{
            Suite = 'Compilação'
            Area = 'Código fonte'
            NomeTecnico = $match.Groups['code'].Value
            Nome = "Erro de compilação $($match.Groups['code'].Value)"
            Resultado = 'Falhou'
            DuracaoMs = 0
            Mensagem = $message
            Detalhes = $match.Value
            Arquivo = if ($location) { $location.Arquivo } else { '' }
            Linha = if ($location) { $location.Linha } else { 0 }
            Coluna = if ($location) { $location.Coluna } else { 0 }
            Localizacoes = if ($location) { @($location) } else { @() }
        })
    }
}

if (-not [string]::IsNullOrWhiteSpace($PipelineError)) {
    $registros.Add([pscustomobject]@{
        Suite = 'Pipeline'
        Area = 'Infraestrutura'
        NomeTecnico = 'PIPELINE_INTERRUPTED'
        Nome = 'Pipeline interrompido antes da conclusão'
        Resultado = 'Falhou'
        DuracaoMs = 0
        Mensagem = Protect-SensitiveText "Pipeline interrompido: $PipelineError"
        Detalhes = ''
        Arquivo = ''
        Linha = 0
        Coluna = 0
        Localizacoes = @()
    })
}
Add-BuildResults -Path $BuildLog
Add-TrxResults -Path $BackendTrx -Suite 'Backend e integrações HTTP' -AreaPadrao 'Backend'
Add-FrontendResults -Path $FrontendJson
Add-TrxResults -Path $NavegadorTrx -Suite 'Jornadas reais no Chromium' -AreaPadrao 'Navegador'
Add-TrxResults -Path $MySqlTrx -Suite 'Compatibilidade com MySQL 8' -AreaPadrao 'MySQL'

$suitesEsperadas = @(
    if (-not [string]::IsNullOrWhiteSpace($BuildLog)) { [pscustomobject]@{ Nome = 'Compilação'; Arquivo = $BuildLog } }
    if (-not [string]::IsNullOrWhiteSpace($BackendTrx)) { [pscustomobject]@{ Nome = 'Backend e integrações HTTP'; Arquivo = $BackendTrx } }
    if (-not [string]::IsNullOrWhiteSpace($FrontendJson)) { [pscustomobject]@{ Nome = 'Interface e JavaScript'; Arquivo = $FrontendJson } }
    if (-not [string]::IsNullOrWhiteSpace($NavegadorTrx)) { [pscustomobject]@{ Nome = 'Jornadas reais no Chromium'; Arquivo = $NavegadorTrx } }
    if (-not [string]::IsNullOrWhiteSpace($MySqlTrx)) { [pscustomobject]@{ Nome = 'Compatibilidade com MySQL 8'; Arquivo = $MySqlTrx } }
)
$naoExecutadas = @($suitesEsperadas | Where-Object { -not (Test-Path -LiteralPath $_.Arquivo) })
foreach ($missing in $naoExecutadas) {
    $registros.Add([pscustomobject]@{
        Suite = $missing.Nome
        Area = 'Pipeline'
        NomeTecnico = "MISSING_RESULT_$($missing.Nome)"
        Nome = "Resultado ausente — $($missing.Nome)"
        Resultado = 'Falhou'
        DuracaoMs = 0
        Mensagem = 'A etapa não produziu arquivo de resultado obrigatório.'
        Detalhes = "Arquivo esperado: $($missing.Arquivo)"
        Arquivo = ''
        Linha = 0
        Coluna = 0
        Localizacoes = @()
    })
}
foreach ($record in $registros) {
    $record.Nome = Protect-SensitiveText $record.Nome
    $record.NomeTecnico = Protect-SensitiveText $record.NomeTecnico
}
$falhas = @($registros | Where-Object Resultado -eq 'Falhou')
$ignorados = @($registros | Where-Object Resultado -eq 'Ignorado')

foreach ($failure in $falhas) {
    $rule = Get-DiagnosticRule $failure
    $comparison = Get-ExpectedActual $failure.Mensagem
    $context = Get-FailureContext $failure
    $locations = @($failure.Localizacoes)
    $applicationLocation = $locations | Where-Object Tipo -eq 'Aplicação' | Select-Object -First 1
    $testLocation = $locations | Where-Object Tipo -eq 'Teste' | Select-Object -First 1
    $primaryLocation = if ($applicationLocation) { $applicationLocation } elseif ($testLocation) { $testLocation } else { $null }
    if ($primaryLocation) {
        $failure.Arquivo = $primaryLocation.Arquivo
        $failure.Linha = $primaryLocation.Linha
        $failure.Coluna = $primaryLocation.Coluna
    }
    $failure.Mensagem = Protect-SensitiveText $failure.Mensagem
    $failure.Detalhes = Protect-SensitiveText $failure.Detalhes
    $failure | Add-Member -NotePropertyName Categoria -NotePropertyValue $rule.Category
    $failure | Add-Member -NotePropertyName Severidade -NotePropertyValue $rule.Severity
    $failure | Add-Member -NotePropertyName Confianca -NotePropertyValue ([int]$rule.Confidence)
    $failure | Add-Member -NotePropertyName Regra -NotePropertyValue $rule.Id
    $failure | Add-Member -NotePropertyName TituloCausa -NotePropertyValue $rule.Title
    $failure | Add-Member -NotePropertyName Explicacao -NotePropertyValue $rule.Explanation
    $failure | Add-Member -NotePropertyName Recomendacao -NotePropertyValue $rule.Action
    $failure | Add-Member -NotePropertyName PodeGerarCascata -NotePropertyValue ([bool]$rule.Cascade)
    $failure | Add-Member -NotePropertyName Esperado -NotePropertyValue $comparison.Esperado
    $failure | Add-Member -NotePropertyName Recebido -NotePropertyValue $comparison.Recebido
    $failure | Add-Member -NotePropertyName Metodo -NotePropertyValue $context.Metodo
    $failure | Add-Member -NotePropertyName Controller -NotePropertyValue $context.Controller
    $failure | Add-Member -NotePropertyName Rota -NotePropertyValue (Protect-SensitiveText $context.Rota)
    $failure | Add-Member -NotePropertyName Entidade -NotePropertyValue $context.Entidade
    $failure | Add-Member -NotePropertyName Tabela -NotePropertyValue $context.Tabela
    $failure | Add-Member -NotePropertyName LocalAplicacao -NotePropertyValue $applicationLocation
    $failure | Add-Member -NotePropertyName LocalTeste -NotePropertyValue $testLocation
    $failure | Add-Member -NotePropertyName Fingerprint -NotePropertyValue (Get-Fingerprint $failure $rule)
}

$causas = @($falhas | Group-Object Fingerprint | ForEach-Object {
    $representative = $_.Group | Sort-Object @{ Expression={ if ($_.LocalAplicacao) { 0 } else { 1 } } }, DuracaoMs | Select-Object -First 1
    $confidence = [int]$representative.Confianca
    if ($_.Count -ge 3 -and $confidence -lt 97) { $confidence = [math]::Min(97, $confidence + 5) }
    [pscustomobject]@{
        Id = $representative.Regra
        Fingerprint = $_.Name
        Categoria = $representative.Categoria
        Severidade = $representative.Severidade
        Confianca = $confidence
        Titulo = $representative.TituloCausa
        Explicacao = $representative.Explicacao
        Recomendacao = $representative.Recomendacao
        Ocorrencias = $_.Count
        CascataProvavel = [bool]($representative.PodeGerarCascata -and $_.Count -gt 1)
        Representante = $representative
        Falhas = @($_.Group)
    }
} | Sort-Object @{ Expression={ switch ($_.Severidade) { 'Crítica' { 0 } 'Alta' { 1 } 'Média' { 2 } default { 3 } } } }, @{ Expression='Ocorrencias'; Descending=$true })

$resultadoGeral = if ($falhas.Count -gt 0 -or $naoExecutadas.Count -gt 0) { 'FALHOU' } else { 'APROVADO' }
$icone = if ($resultadoGeral -eq 'APROVADO') { '✅' } else { '❌' }
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $icone Relatório consolidado de testes — InsEmpodera")
$lines.Add('')
$lines.Add("**Resultado geral:** $resultadoGeral  ")
$lines.Add("**Gerado em:** $((Get-Date).ToString('dd/MM/yyyy HH:mm:ss zzz'))  ")
$executionEnvironment = if ($env:GITHUB_ACTIONS -eq 'true') { 'GitHub Actions' } else { 'Local' }
$commitReference = if ($env:GITHUB_SHA) { $env:GITHUB_SHA.Substring(0, [math]::Min(12, $env:GITHUB_SHA.Length)) } else { 'não informado' }
$lines.Add("**Ambiente:** $executionEnvironment — $([Runtime.InteropServices.RuntimeInformation]::OSDescription)  ")
$lines.Add("**PowerShell:** $($PSVersionTable.PSVersion) — **Commit:** $commitReference")
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
        $lines.Add("- ⚪ **$($missing.Nome):** não produziu arquivo de resultado. A etapa pode ter sido bloqueada por compilação, restauração ou infraestrutura.")
    }

    if ($causas.Count -gt 0) {
        $lines.Add('')
        $lines.Add("Foram encontradas **$($causas.Count) causas distintas** para **$($falhas.Count) testes com falha**. Falhas iguais foram reunidas para evitar diagnósticos repetidos.")
        $lines.Add('')
        $lines.Add('### Distribuição por categoria')
        $lines.Add('')
        $lines.Add('| Categoria | Causas distintas | Testes afetados |')
        $lines.Add('|---|---:|---:|')
        foreach ($category in @($causas | Group-Object Categoria | Sort-Object Count -Descending)) {
            $affected = ($category.Group | Measure-Object Ocorrencias -Sum).Sum
            $lines.Add("| $($category.Name) | $($category.Count) | $affected |")
        }

        $lines.Add('')
        $lines.Add('### Causas prováveis priorizadas')
        $lines.Add('')
        $lines.Add('| Prioridade | Categoria | Causa provável | Confiança | Impacto | Origem |')
        $lines.Add('|---:|---|---|---:|---:|---|')
        $priority = 0
        foreach ($cause in $causas) {
            $priority++
            $failure = $cause.Representante
            $source = if ($failure.Arquivo -and $failure.Linha -gt 0) { Get-SourceLink $failure.Arquivo $failure.Linha } else { 'Não identificada' }
            $cascade = if ($cause.CascataProvavel) { " — cascata provável" } else { '' }
            $lines.Add("| $priority | $($cause.Categoria) / $($cause.Severidade) | $(Escape-MarkdownCell $cause.Titulo)$cascade | $($cause.Confianca)% | $($cause.Ocorrencias) teste(s) | $source |")
        }

        $causeNumber = 0
        foreach ($cause in $causas) {
            $causeNumber++
            $failure = $cause.Representante
            $lines.Add('')
            $lines.Add("## ❌ Causa $causeNumber — $($cause.Titulo)")
            $lines.Add('')
            $lines.Add("- **Categoria:** $($cause.Categoria)")
            $lines.Add("- **Severidade:** $($cause.Severidade)")
            $lines.Add("- **Confiança da análise:** $($cause.Confianca)%")
            $lines.Add("- **Impacto:** $($cause.Ocorrencias) teste(s) afetado(s)")
            $lines.Add(('- **Regra de diagnóstico:** `{0}`' -f $cause.Id))
            if ($cause.CascataProvavel) {
                $lines.Add('- **Cascata provável:** sim. A falha acontece durante uma operação compartilhada; os testes afetados não devem ser tratados como defeitos independentes.')
            }
            $lines.Add('')
            $lines.Add('### Explicação em linguagem clara')
            $lines.Add('')
            $lines.Add($cause.Explicacao)
            $lines.Add('')
            $lines.Add('> Esta é uma causa provável baseada nas evidências abaixo. A porcentagem expressa a confiança da regra, não uma garantia absoluta.')

            $contextItems = [System.Collections.Generic.List[string]]::new()
            if ($failure.Metodo) { $contextItems.Add(('- **Método:** `{0}`' -f $failure.Metodo)) }
            if ($failure.Controller) { $contextItems.Add(('- **Controller:** `{0}`' -f $failure.Controller)) }
            if ($failure.Rota) { $contextItems.Add(('- **Rota:** `{0}`' -f $failure.Rota)) }
            if ($failure.Entidade) { $contextItems.Add(('- **Entidade:** `{0}`' -f $failure.Entidade)) }
            if ($failure.Tabela) { $contextItems.Add(('- **Tabela:** `{0}`' -f $failure.Tabela)) }
            if ($contextItems.Count -gt 0) {
                $lines.Add('')
                $lines.Add('### Contexto identificado')
                $lines.Add('')
                foreach ($contextItem in $contextItems) { $lines.Add($contextItem) }
            }

            if ($failure.Esperado -or $failure.Recebido) {
                $lines.Add('')
                $lines.Add('### Esperado versus recebido')
                $lines.Add('')
                $lines.Add('| Esperado | Recebido |')
                $lines.Add('|---|---|')
                $expected = if ($failure.Esperado) { Escape-MarkdownCell $failure.Esperado } else { 'Não informado pela biblioteca de testes' }
                $actual = if ($failure.Recebido) { Escape-MarkdownCell $failure.Recebido } else { 'Não informado pela biblioteca de testes' }
                $lines.Add("| $expected | $actual |")
            }

            $lines.Add('')
            $lines.Add('### Evidência original sanitizada')
            $lines.Add('')
            $lines.Add('```text')
            $safeMessage = ($failure.Mensagem -replace '```', "'''").Trim()
            if ($safeMessage.Length -gt 6000) { $safeMessage = $safeMessage.Substring(0, 6000) + "`n... mensagem reduzida; consulte o TRX/JSON completo." }
            $lines.Add($safeMessage)
            $lines.Add('```')

            foreach ($locationKind in @('Aplicação', 'Teste')) {
                $location = if ($locationKind -eq 'Aplicação') { $failure.LocalAplicacao } else { $failure.LocalTeste }
                if (-not $location) { continue }
                $lines.Add('')
                $lines.Add("### Código relacionado — $locationKind")
                $lines.Add('')
                $lines.Add("Local: $(Get-SourceLink $location.Arquivo $location.Linha)")
                $excerpt = @(Get-CodeExcerpt $location.Arquivo $location.Linha)
                if ($excerpt.Count -gt 0) {
                    $lines.Add('')
                    $lines.Add('```text')
                    foreach ($excerptLine in $excerpt) { $lines.Add($excerptLine) }
                    $lines.Add('```')
                }
            }

            $lines.Add('')
            $lines.Add('### Próximo passo recomendado')
            $lines.Add('')
            $lines.Add($cause.Recomendacao)

            $lines.Add('')
            $lines.Add("<details><summary>Testes afetados ($($cause.Ocorrencias))</summary>")
            $lines.Add('')
            foreach ($affectedFailure in @($cause.Falhas | Sort-Object Suite, Area, Nome)) {
                $affectedSource = if ($affectedFailure.LocalTeste) { " — $(Get-SourceLink $affectedFailure.LocalTeste.Arquivo $affectedFailure.LocalTeste.Linha)" } else { '' }
                $lines.Add("- **$($affectedFailure.Suite):** $($affectedFailure.Nome)$affectedSource")
            }
            $lines.Add('')
            $lines.Add('</details>')

            $stackLines = @($failure.Detalhes -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 16)
            if ($stackLines.Count -gt 0) {
                $lines.Add('')
                $lines.Add('<details><summary>Rastreamento técnico sanitizado (primeiras 16 linhas)</summary>')
                $lines.Add('')
                $lines.Add('```text')
                foreach ($stackLine in $stackLines) { $lines.Add(($stackLine -replace '```', "'''")) }
                $lines.Add('```')
                $lines.Add('</details>')
            }
        }
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
$lines.Add('- **Compatibilidade com MySQL 8:** executa as integrações usando o mesmo provedor relacional da produção.')
$lines.Add('- Arquivos `.trx` e `.json` continuam disponíveis para diagnóstico técnico e integração com o GitHub.')

$directory = Split-Path -Parent $Saida
if ($directory) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
[IO.File]::WriteAllLines($Saida, $lines, [Text.UTF8Encoding]::new($false))

if ([string]::IsNullOrWhiteSpace($SaidaJson)) {
    $SaidaJson = [IO.Path]::ChangeExtension($Saida, '.json')
}
$jsonDirectory = Split-Path -Parent $SaidaJson
if ($jsonDirectory) { New-Item -ItemType Directory -Path $jsonDirectory -Force | Out-Null }
$diagnosticJson = [pscustomobject]@{
    schemaVersion = '2.0'
    generatedAt = (Get-Date).ToUniversalTime().ToString('o')
    result = $resultadoGeral
    environment = [pscustomobject]@{
        execution = $executionEnvironment
        operatingSystem = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        powershell = $PSVersionTable.PSVersion.ToString()
        commit = $commitReference
        runId = if ($env:GITHUB_RUN_ID) { $env:GITHUB_RUN_ID } else { '' }
        runAttempt = if ($env:GITHUB_RUN_ATTEMPT) { $env:GITHUB_RUN_ATTEMPT } else { '' }
    }
    summary = [pscustomobject]@{
        executed = $registros.Count
        passed = @($registros | Where-Object Resultado -eq 'Aprovado').Count
        failed = $falhas.Count
        skipped = $ignorados.Count
        missingSuites = @($naoExecutadas | ForEach-Object Nome)
        distinctCauses = $causas.Count
    }
    categories = @($causas | Group-Object Categoria | ForEach-Object {
        [pscustomobject]@{ category=$_.Name; distinctCauses=$_.Count; affectedTests=(($_.Group | Measure-Object Ocorrencias -Sum).Sum) }
    })
    causes = @($causas | ForEach-Object {
        $cause = $_
        $representative = $cause.Representante
        [pscustomobject]@{
            id = $cause.Id
            category = $cause.Categoria
            severity = $cause.Severidade
            confidence = $cause.Confianca
            title = $cause.Titulo
            explanation = $cause.Explicacao
            recommendation = $cause.Recomendacao
            affectedTests = $cause.Ocorrencias
            probableCascade = $cause.CascataProvavel
            expected = $representative.Esperado
            received = $representative.Recebido
            context = [pscustomobject]@{
                method = $representative.Metodo
                controller = $representative.Controller
                route = $representative.Rota
                entity = $representative.Entidade
                table = $representative.Tabela
            }
            applicationLocation = $representative.LocalAplicacao
            testLocation = $representative.LocalTeste
            evidence = $representative.Mensagem
            tests = @($cause.Falhas | ForEach-Object {
                [pscustomobject]@{ suite=$_.Suite; area=$_.Area; name=$_.Nome; durationMs=$_.DuracaoMs; location=$_.LocalTeste }
            })
        }
    })
}
[IO.File]::WriteAllText($SaidaJson, ($diagnosticJson | ConvertTo-Json -Depth 12), [Text.UTF8Encoding]::new($false))

if ($env:GITHUB_ACTIONS -eq 'true') {
    $annotationLimit = 50
    foreach ($cause in @($causas | Select-Object -First $annotationLimit)) {
        $failure = $cause.Representante
        $title = Escape-GitHubCommandValue "$($cause.Categoria) — $($cause.Titulo)" -Property
        $cascadeText = if ($cause.CascataProvavel) { " Provável cascata afetando $($cause.Ocorrencias) testes." } else { " Impacto: $($cause.Ocorrencias) teste(s)." }
        $message = Escape-GitHubCommandValue "$($cause.Explicacao)$cascadeText Próximo passo: $($cause.Recomendacao)"
        if ($failure.Arquivo -and $failure.Linha -gt 0) {
            $file = Escape-GitHubCommandValue ($failure.Arquivo -replace '\\', '/') -Property
            Write-Output "::error file=$file,line=$($failure.Linha),col=$([math]::Max(1, $failure.Coluna)),title=$title::$message"
        }
        else {
            Write-Output "::error title=$title::$message"
        }
    }
    if ($causas.Count -gt $annotationLimit) {
        $remaining = $causas.Count - $annotationLimit
        Write-Output "::warning title=Diagnóstico resumido::$remaining causa(s) adicional(is) estão no relatório Markdown e no JSON. As anotações foram limitadas para manter a página legível."
    }
    foreach ($missing in $naoExecutadas) {
        $title = Escape-GitHubCommandValue $missing.Nome -Property
        Write-Output "::error title=$title::A etapa não produziu seu arquivo de resultado. Consulte o log de compilação ou inicialização imediatamente anterior."
    }
}

Write-Host "`n============================================================" -ForegroundColor DarkCyan
Write-Host " RESULTADO GERAL: $resultadoGeral" -ForegroundColor $(if ($resultadoGeral -eq 'APROVADO') { 'Green' } else { 'Red' })
Write-Host " Testes executados: $($registros.Count) | Falhas: $($falhas.Count) | Ignorados: $($ignorados.Count)"
Write-Host " Relatório fácil de ler: $Saida"
Write-Host " Diagnóstico para automações: $SaidaJson"
Write-Host "============================================================" -ForegroundColor DarkCyan
