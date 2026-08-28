[CmdletBinding()]
param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$resultsRoot = Join-Path $resolvedRoot 'TestResults'
$temporaryDirectory = Join-Path $resultsRoot ('.diagnostico-selftest-' + [Guid]::NewGuid().ToString('N'))
$generator = Join-Path $resolvedRoot 'scripts/Gerar-RelatorioTestes.ps1'

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw "Autoteste do diagnóstico falhou: $Message" }
}

function Write-Utf8File {
    param([string]$Path, [string]$Content)
    [IO.File]::WriteAllText($Path, $Content, [Text.UTF8Encoding]::new($false))
}

New-Item -ItemType Directory -Path $temporaryDirectory -Force | Out-Null
try {
    $backendTrx = Join-Path $temporaryDirectory 'backend.trx'
    $browserTrx = Join-Path $temporaryDirectory 'browser.trx'
    $frontendJson = Join-Path $temporaryDirectory 'frontend.json'
    $buildLog = Join-Path $temporaryDirectory 'build.log'
    $report = Join-Path $temporaryDirectory 'report.md'
    $diagnostic = Join-Path $temporaryDirectory 'diagnostic.json'

    $providerFailure = @'
<UnitTestResult testName="DatabaseStartup_Fails" outcome="Failed" duration="00:00:00.100">
  <Output><ErrorInfo>
    <Message>System.InvalidOperationException: Services for database providers 'Microsoft.EntityFrameworkCore.Sqlite', 'Pomelo.EntityFrameworkCore.MySql' have been registered. Only a single database provider can be registered.</Message>
    <StackTrace>at Program.&lt;Main&gt;$(String[] args) in /home/runner/work/InsEmpodera/InsEmpodera/Program.cs:line 167
at InsEmpodera.Tests.Integration.ApplicationSmokeTests.DatabaseStartup_Fails() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.Tests/Integration/ApplicationSmokeTests.cs:line 22</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
'@
    $backend = @"
<?xml version="1.0" encoding="utf-8"?>
<TestRun><Results>
$providerFailure
$providerFailure
$providerFailure
<UnitTestResult testName="Login_WithValidCredentials_ReturnsRedirect(email: &quot;name-leak@example.com&quot;, password: &quot;NameLeak123&quot;)" outcome="Failed" duration="00:00:00.050">
  <Output><ErrorInfo>
    <Message>Assert.Equal() Failure: Values differ`nExpected: 302`nActual: 200`nEmail=person@example.com Password=Segredo123 path: "/Account/Reset?token=RouteLeak123"</Message>
    <StackTrace>at InsEmpodera.Tests.Integration.AuthenticationSessionFlowTests.Login_WithValidCredentials_ReturnsRedirect() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.Tests/Integration/AuthenticationSessionFlowTests.cs:line 33</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
<UnitTestResult testName="Permission_ReadOnlyProfile_CannotDelete" outcome="Failed" duration="00:00:00.040">
  <Output><ErrorInfo>
    <Message>Assert.Equal() Failure: authorization status differs`nExpected: 403`nActual: 200</Message>
    <StackTrace>at InsEmpodera.Tests.Security.CrudAuthorizationTests.Permission_ReadOnlyProfile_CannotDelete() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.Tests/Security/CrudAuthorizationTests.cs:line 40</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
<UnitTestResult testName="RequiredField_RejectsEmptyName" outcome="Failed" duration="00:00:00.030">
  <Output><ErrorInfo>
    <Message>Validation failed: required value was accepted</Message>
    <StackTrace>at InsEmpodera.Tests.Unit.UserCultureServiceTests.RequiredField_RejectsEmptyName() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.Tests/Unit/UserCultureServiceTests.cs:line 40</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
</Results></TestRun>
"@
    Write-Utf8File $backendTrx $backend

    $browser = @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun><Results>
<UnitTestResult testName="Comunidades_CriarEditarExcluir" outcome="Failed" duration="00:00:30">
  <Output><ErrorInfo>
    <Message>Microsoft.Playwright.PlaywrightException: Timeout 30000ms exceeded while waiting for locator getByRole('button')</Message>
    <StackTrace>at InsEmpodera.E2ETests.Tests.UsersAndCommunitiesUiTests.Comunidades_CriarEditarExcluir() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.E2ETests/Tests/UsersAndCommunitiesUiTests.cs:line 40</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
<UnitTestResult testName="Login_ExibeBotaoDeEntrar" outcome="Failed" duration="00:00:01">
  <Output><ErrorInfo>
    <Message>Microsoft.Playwright.PlaywrightException: locator getByRole('button', name='Entrar') did not match the visible page</Message>
    <StackTrace>at InsEmpodera.E2ETests.Tests.AuthenticationAndNavigationTests.Login_ExibeBotaoDeEntrar() in /home/runner/work/InsEmpodera/InsEmpodera/InsEmpodera.E2ETests/Tests/AuthenticationAndNavigationTests.cs:line 40</StackTrace>
  </ErrorInfo></Output>
</UnitTestResult>
</Results></TestRun>
'@
    Write-Utf8File $browserTrx $browser

    $frontend = [pscustomobject]@{
        testResults = @([pscustomobject]@{
            name = (Join-Path $resolvedRoot 'Frontend.Tests/tests/authentication-ui.test.js')
            assertionResults = @([pscustomobject]@{
                fullName = 'interface de login aplica o idioma selecionado'
                title = 'aplica o idioma selecionado'
                status = 'failed'
                duration = 12
                failureMessages = @("AssertionError: expected 'pt-BR' to equal 'en'`n    at $resolvedRoot/Frontend.Tests/tests/authentication-ui.test.js:42:7")
            })
        })
    }
    Write-Utf8File $frontendJson ($frontend | ConvertTo-Json -Depth 8)

    $buildPath = Join-Path $resolvedRoot 'Program.cs'
    Write-Utf8File $buildLog "$buildPath(42,18): error CS1002: ; expected Password=nao-pode-vazar [InsEmpodera.csproj]"

    $oldGitHubActions = $env:GITHUB_ACTIONS
    $oldGitHubServer = $env:GITHUB_SERVER_URL
    $oldGitHubRepository = $env:GITHUB_REPOSITORY
    $oldGitHubSha = $env:GITHUB_SHA
    $env:GITHUB_ACTIONS = 'true'
    $env:GITHUB_SERVER_URL = 'https://github.com'
    $env:GITHUB_REPOSITORY = 'teste/InsEmpodera'
    $env:GITHUB_SHA = '0123456789abcdef'
    try {
        $generatorOutput = & $generator -BuildLog $buildLog -BackendTrx $backendTrx -FrontendJson $frontendJson -NavegadorTrx $browserTrx -MySqlTrx (Join-Path $temporaryDirectory 'resultado-mysql-ausente.trx') -PipelineError 'Preparar Chromium falhou com o codigo 1 Password=PipelineLeak123' -Saida $report -SaidaJson $diagnostic *>&1 | Out-String
    }
    finally {
        $env:GITHUB_ACTIONS = $oldGitHubActions
        $env:GITHUB_SERVER_URL = $oldGitHubServer
        $env:GITHUB_REPOSITORY = $oldGitHubRepository
        $env:GITHUB_SHA = $oldGitHubSha
    }

    $markdown = Get-Content -LiteralPath $report -Raw
    $json = Get-Content -LiteralPath $diagnostic -Raw | ConvertFrom-Json

    Assert-True ($markdown -match 'Categoria:\*\* Compilação') 'não classificou erro de compilação.'
    Assert-True ($markdown -match 'Categoria:\*\* Banco') 'não classificou conflito de banco.'
    Assert-True ($markdown -match 'Categoria:\*\* Autenticação') 'não classificou falha de autenticação.'
    Assert-True ($markdown -match 'Categoria:\*\* Autorização') 'não classificou falha de autorização.'
    Assert-True ($markdown -match 'Categoria:\*\* Validação') 'não classificou falha de validação.'
    Assert-True ($markdown -match 'Categoria:\*\* Interface') 'não classificou falha da interface.'
    Assert-True ($markdown -match 'Categoria:\*\* Navegador') 'não classificou falha do navegador.'
    Assert-True ($markdown -match 'Categoria:\*\* Instabilidade') 'não classificou timeout como instabilidade.'
    Assert-True ($markdown -match 'Categoria:\*\* Infraestrutura') 'não classificou resultado ausente como infraestrutura.'
    Assert-True ($markdown -match 'Cascata provável:\*\* sim') 'não marcou a causa compartilhada como cascata.'
    Assert-True ($markdown -match 'Esperado versus recebido') 'não exibiu a comparação de valores.'
    Assert-True ($markdown -match 'Código relacionado — Aplicação') 'não exibiu trecho da aplicação.'
    Assert-True ($markdown -match 'Código relacionado — Teste') 'não exibiu trecho do teste.'
    Assert-True ($markdown -match '\[valor protegido\]') 'não sinalizou o valor sensível removido.'
    $secretPattern = 'nao-pode-vazar|Segredo123|person@example.com|name-leak@example.com|NameLeak123|RouteLeak123|PipelineLeak123'
    Assert-True ($markdown -notmatch $secretPattern) 'um segredo ou e-mail permaneceu no Markdown.'
    Assert-True ((Get-Content -LiteralPath $diagnostic -Raw) -notmatch $secretPattern) 'um segredo ou e-mail permaneceu no JSON.'
    Assert-True ($json.summary.distinctCauses -eq 10) "esperava 10 causas distintas, recebeu $($json.summary.distinctCauses)."
    $databaseCause = @($json.causes | Where-Object id -eq 'EF_MULTIPLE_PROVIDERS')
    Assert-True ($databaseCause.Count -eq 1) 'não deduplicou o conflito de provedores.'
    Assert-True ($databaseCause[0].affectedTests -eq 3) 'não contabilizou os três testes da cascata.'
    Assert-True ($databaseCause[0].probableCascade -eq $true) 'JSON não marcou a cascata provável.'
    Assert-True (@($json.causes | Where-Object id -eq 'PIPELINE_INFRASTRUCTURE').Count -eq 1) 'não preservou a falha original de infraestrutura.'
    Assert-True ($generatorOutput -match '::error file=Program.cs,line=167') 'não gerou anotação GitHub com arquivo e linha.'

    Write-Host 'Autoteste do diagnóstico aprovado: classificação, causa raiz, cascata, contexto, sanitização, Markdown, JSON e anotações.' -ForegroundColor Green
}
finally {
    $resolvedResultsRoot = (Resolve-Path -LiteralPath $resultsRoot).Path
    $resolvedTemporary = (Resolve-Path -LiteralPath $temporaryDirectory -ErrorAction SilentlyContinue).Path
    if ($resolvedTemporary -and $resolvedTemporary.StartsWith($resolvedResultsRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $resolvedTemporary -Recurse -Force
    }
}
