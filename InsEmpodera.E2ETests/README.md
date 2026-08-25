# Testes automáticos do InsEmpodera

Esta pasta contém jornadas end-to-end em Chromium com Playwright 1.62.0. A suíte abre a interface real, atravessa o pipeline ASP.NET Core e confirma o resultado no banco, em vez de simular os controllers.

## Execução recomendada

Na raiz do repositório, execute:

```powershell
.\testar.ps1
```

O runner restaura .NET e Node, compila em `Release`, instala de forma idempotente o Chromium compatível, executa os testes de backend, os testes de eventos/DOM com cobertura e, por fim, as jornadas no navegador.

Para acompanhar o navegador localmente:

```powershell
.\testar.ps1 -Headed -SlowMo 250
```

Para executar apenas esta suíte depois do primeiro build:

```powershell
dotnet build .\InsEmpodera.E2ETests\InsEmpodera.E2ETests.csproj -c Release
.\bin\InsEmpodera.E2ETests\Release\net9.0\playwright.ps1 install chromium
dotnet test .\InsEmpodera.E2ETests\InsEmpodera.E2ETests.csproj -c Release --no-build
```

Pré-requisitos: .NET SDK 9, Node.js/npm e PowerShell 7. O teste não exige MySQL nem usa o banco de desenvolvimento.

## Isolamento e evidências

- Cada execução sobe Kestrel em uma porta livre e cria um arquivo SQLite temporário exclusivo.
- O banco é criado a partir do modelo real, recebe apenas seeds conhecidos e é descartado ao final.
- Cada cenário cria dados com GUID e a coleção xUnit é serial, evitando colisões entre CRUDs.
- Requisições externas são bloqueadas. Qualquer erro JavaScript ou resposta local 4xx/5xx — inclusive CSS, scripts, imagens e fontes — reprova o cenário.
- Em falhas, a suíte salva screenshot e trace reproduzível em `InsEmpodera.E2ETests/TestResults/e2e-artifacts`.
- Resultados TRX e cobertura ficam em `TestResults`; o frontend grava HTML em `Frontend.Tests/coverage`.

## Cobertura do navegador

- autenticação inválida/válida, sessão, logout e proteção de rotas;
- idioma do navegador e preferência autenticada do usuário;
- password reveal e validação visual da recuperação de senha;
- todos os links visíveis da sidebar, estado ativo e HTTP final;
- menu mobile por toque, Escape e backdrop, além de overflow desktop/mobile;
- usuários e perfis de acesso: validação, criação, busca, edição, permissões e persistência;
- comunidades, atores e atividades: criação, filtros, relações, edição e exclusão lógica/física;
- Diário de Campo: lista, detalhes, busca, filtro, edição, eixos e exclusão;
- Ficha de 1º Contato: wizard de três etapas, seleções filhas, edição, conclusão, filtros e exclusão em cascata;
- Avaliação Pessoal: seleção do ator, eventos dos sliders, criação, busca, edição e exclusão.

Os testes de CRUD sempre conferem o banco após os eventos visíveis, inclusive tabelas de relacionamento. Seletores priorizam nomes, labels e atributos estáveis; não dependem de coordenadas, ordem aleatória ou dados externos.

## Diagnósticos conhecidos

Alguns cenários têm o trait `Diagnostic`. Eles registram precisamente lacunas existentes e passam enquanto o comportamento diagnosticado continuar igual; quando a aplicação for corrigida, o teste deve ser convertido para o novo fluxo esperado.

- `Ajuda` aparece na sidebar, mas não existe controller/view e retorna 404.
- `Configurações` chama uma view `Index` inexistente e retorna 500.
- perfis de acesso não expõem exclusão nem endpoint `Delete`, portanto o CRUD ainda não é completo.
- a criação do Diário de Campo não oferece o campo obrigatório `Foto`.
- ações institucionais dinâmicas são montadas na UI como `TempAcoes`, mas não são persistidas pelo endpoint atual.

A recuperação de senha é somente uma simulação visual em `wwwroot/js/forget.js`: o formulário usa `preventDefault`, não chama backend, não envia email e não possui action `ForgotPassword`. O E2E valida apenas navegação e estados visuais dessa tela.

## CI

`.github/workflows/testes.yml` executa a bateria completa nas branches comuns e por acionamento manual, com caches de NuGet, npm e Playwright e upload dos artifacts mesmo em falha. O workflow legado `main.yml` foi preservado para as branches específicas de importação e publicação; os gatilhos são separados para não repetir a mesma bateria.
