# Testes e validação

[← Voltar ao README principal](../README.md)

## Sumário

- [Visão geral](#visão-geral)
- [Pré-requisitos](#pré-requisitos)
- [Testes de backend](#testes-de-backend)
- [Testes de interface](#testes-de-interface)
- [Testes no navegador (E2E)](#testes-no-navegador-e2e)
- [Executor completo](#executor-completo)
- [Testes de mutação](#testes-de-mutação)
- [Integração contínua](#integração-contínua)
- [Validação manual](#validação-manual)
- [Registro de execução desta revisão](#registro-de-execução-desta-revisão)

## Visão geral

| Tipo | Projeto / pasta | Ferramentas | O que verifica |
| --- | --- | --- | --- |
| Unidade | `InsEmpodera.Tests/Unit` | xUnit | Regras isoladas, como a escolha do idioma |
| Contratos de arquitetura | `InsEmpodera.Tests/Architecture` | xUnit | Presença de autorização e token CSRF em ações, configuração de sessão, cookies, logs, colunas do Identity e o checklist da revisão do DDL (`RevisaoDdlTests`) |
| CRUD | `InsEmpodera.Tests/Crud` | xUnit + EF Core | Regras dos controllers chamadas diretamente, com banco real |
| Integração HTTP | `InsEmpodera.Tests/Integration` | xUnit + `WebApplicationFactory` | Fluxos completos pela aplicação: login, cookies, model binding, views, exportação e backup |
| Segurança | `InsEmpodera.Tests/Security` | xUnit | Autorização por módulo, usuários inativos, recuperação de senha, uploads e cabeçalhos |
| Interface | `Frontend.Tests` | Vitest + jsdom | Scripts de `wwwroot/js` e contratos entre views e JavaScript |
| Ponta a ponta | `InsEmpodera.E2ETests` | Playwright (Chromium) + xUnit | Jornadas reais no navegador, conferindo o banco após cada ação |
| Mutação | `InsEmpodera.Tests` | Stryker.NET | Capacidade da suíte de detectar defeitos introduzidos |
| Compilação | Solução | `dotnet build` | Compilação da aplicação e dos testes |

A matriz de cobertura dos CRUDs está em [TESTES.md](../TESTES.md), e os detalhes da suíte E2E, em [InsEmpodera.E2ETests/README.md](../InsEmpodera.E2ETests/README.md).

## Pré-requisitos

| Suíte | Requer |
| --- | --- |
| Backend completo e E2E | .NET 9 e um servidor MySQL 8.4 ou posterior acessível (os bancos de teste são criados com `Database/Empodera.schema.sql`) |
| Subconjunto de arquitetura e unidade | .NET 9 |
| Interface | Node.js 22 e npm |
| E2E | Também PowerShell 7, para instalar o Chromium pelo script do Playwright |
| Executor completo | .NET 9, Node.js, npm, PowerShell 7 e MySQL 8.4 ou posterior |

Os testes que usam banco leem a variável `TEST_MYSQL_CONNECTION`. Informe **somente o servidor**, sem `Database`: cada teste cria um banco temporário com prefixo `insempodera_` e o apaga ao final. O usuário precisa de permissão para criar e excluir bancos. O banco de desenvolvimento não é acessado.

```bash
export TEST_MYSQL_CONNECTION="Server=127.0.0.1;Port=3306;User=root;Password=SUA_SENHA;Allow User Variables=true"
```

```powershell
$env:TEST_MYSQL_CONNECTION = "Server=127.0.0.1;Port=3306;User=root;Password=SUA_SENHA;Allow User Variables=true"
```

## Testes de backend

Suíte completa (requer `TEST_MYSQL_CONNECTION`):

```bash
dotnet test InsEmpodera.Tests/InsEmpodera.Tests.csproj
```

### Subconjunto sem banco

Os testes de arquitetura e de unidade não acessam o banco e podem ser executados sem MySQL:

```bash
dotnet test InsEmpodera.Tests/InsEmpodera.Tests.csproj --filter "FullyQualifiedName~InsEmpodera.Tests.Architecture|FullyQualifiedName~InsEmpodera.Tests.Unit"
```

Resultado esperado: a linha final informa `Com falha: 0` (ou `Failed: 0`).

## Testes de interface

```bash
npm ci --prefix Frontend.Tests
npm test --prefix Frontend.Tests
```

Para gerar a cobertura em HTML na pasta `Frontend.Tests/coverage`:

```bash
npm run coverage --prefix Frontend.Tests
```

## Testes no navegador (E2E)

Requer `TEST_MYSQL_CONNECTION` e PowerShell 7. A aplicação é iniciada automaticamente em uma porta livre.

```powershell
dotnet build InsEmpodera.E2ETests/InsEmpodera.E2ETests.csproj -c Release
./bin/InsEmpodera.E2ETests/Release/net10.0/playwright.ps1 install chromium
dotnet test InsEmpodera.E2ETests/InsEmpodera.E2ETests.csproj -c Release --no-build
```

Para acompanhar o navegador, defina `E2E_HEADED=true` e, opcionalmente, `E2E_SLOWMO=250`. Em caso de falha, capturas de tela e *traces* ficam em `InsEmpodera.E2ETests/TestResults/e2e-artifacts`.

## Executor de backend e banco

O script [`testar.ps1`](../testar.ps1) restaura as dependências, compila em `Release` e executa somente os testes de backend, integração HTTP e banco. Não instala Node.js ou Chromium e não executa Vitest nem Playwright. Se `TEST_MYSQL_CONNECTION` não estiver definida, ele usa a connection string guardada em *user secrets*.

```powershell
./testar.ps1
```

Saídas geradas em `TestResults/` (ignoradas pelo Git):

| Arquivo | Conteúdo |
| --- | --- |
| `RELATORIO-DE-TESTES.md` | Resumo legível da execução |
| `DIAGNOSTICO-DE-TESTES.json` | Diagnóstico estruturado das falhas |
| `01-backend/*.trx` | Resultados detalhados do xUnit |
| `00-compilacao/compilacao.log` | Log da compilação |

## Testes de mutação

Medem se a suíte detecta alterações defeituosas no código. A ferramenta está fixada em `.config/dotnet-tools.json`; na CI, o workflow instala também o .NET 10 para executá-la.

```bash
dotnet tool restore
cd InsEmpodera.Tests
dotnet stryker
```

## Integração contínua

| Workflow | Quando executa | Etapas |
| --- | --- | --- |
| [`testes.yml`](../.github/workflows/testes.yml) | Push e pull request em todas as branches, exceto `import_exel`/`Import_exel`; também manual | Job `testes`: executa `testar.ps1` com MySQL 8.4 LTS e publica `TestResults`, cobertura e evidências de falha. Job `banco-mysql`: executa a suíte de backend no MySQL 8.4 LTS e gera um relatório de compatibilidade |
| [`main.yml`](../.github/workflows/main.yml) | Push em `import_exel` e pull request para `Import_exel` | Restaura, compila, testa e publica o pacote como artefato |
| [`qualidade-periodica.yml`](../.github/workflows/qualidade-periodica.yml) | Domingos às 03h (UTC) e manual | Testes de mutação com Stryker.NET e resumo em português |

Os resultados podem ser consultados na aba **Actions** do repositório.

## Validação manual

Roteiro reproduzível no [modo de demonstração](instalacao-e-configuracao.md#opção-a--demonstração-com-sqlite), com dados fictícios. Os casos marcados com ✅ foram executados nesta revisão.

| ID | Cenário | Passos | Resultado esperado | Executado |
| --- | --- | --- | --- | :---: |
| CT-M01 | Proteção de rotas | Sem login, abrir `/` | Redirecionamento para `/Account` | ✅ |
| CT-M02 | Login válido | Entrar com `joao@email.com` e a senha de `Testing__AdminPassword` | Redirecionamento para a página inicial | ✅ |
| CT-M03 | Login inválido | Entrar com senha incorreta | Mensagem "E-mail ou senha inválidos." | — |
| CT-M04 | Navegação | Abrir cada item do menu lateral | Todas as páginas carregam (HTTP 200) | ✅ |
| CT-M05 | Cadastro de comunidade | Em **Comunidades**, cadastrar "Comunidade Exemplo" com endereço fictício | A comunidade aparece na listagem | ✅ |
| CT-M06 | Exportação | Em **Comunidades**, usar o botão de exportação | Download de `comunidades-empodera.xlsx` | ✅ |
| CT-M07 | Proteção CSRF | Enviar um `POST` sem `__RequestVerificationToken` | HTTP 400 | ✅ |
| CT-M08 | Cabeçalhos de segurança | Inspecionar a resposta de `/Account` | Presença de `Content-Security-Policy`, `X-Frame-Options` e demais cabeçalhos | ✅ |
| CT-M09 | Idioma | Em **Configurações**, escolher inglês | Interface exibida em inglês | — |
| CT-M10 | Ficha de 1º contato | Criar uma ficha para um ator e concluí-la | Situação `Concluida` na listagem | — |

## Registro de execução desta revisão

Execução em 14/10/2026, em macOS (arm64), com .NET SDK 10.0.201, runtime ASP.NET Core 10 (`DOTNET_ROLL_FORWARD=Major`, pois o runtime 9 não estava instalado) e uma instância MySQL 9.7.1 temporária.

| Verificação | Resultado |
| --- | --- |
| `dotnet build InsEmpodera.sln` | Sucesso, 0 erros e 0 avisos |
| Backend completo (`InsEmpodera.Tests`) | 220 aprovados, 0 falhas |
| Subconjunto de arquitetura e unidade | 39 aprovados, 0 falhas |
| Interface (`Frontend.Tests`) | 157 aprovados em 11 arquivos, 0 falhas |
| E2E (`InsEmpodera.E2ETests`) | Não executado (exige PowerShell e o download do Chromium) |
| `testar.ps1` | Não executado (PowerShell indisponível no ambiente) |
| Mutação (Stryker.NET) | Não executado |

Os testes rodam em MySQL 8.4 LTS na CI; nesta revisão passaram também localmente em MySQL 8.4.11 (Linux, via Docker) e 9.7.1 (macOS). O MariaDB deixou de ser suportado: o esquema usa o collation `utf8mb4_0900_ai_ci`, que só existe no MySQL.
