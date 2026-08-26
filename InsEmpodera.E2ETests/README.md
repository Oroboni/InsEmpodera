# Testes automáticos do InsEmpodera

Esta pasta contém jornadas end-to-end em Chromium com Playwright 1.62.0. A suíte abre a interface real, atravessa o pipeline ASP.NET Core e confirma o resultado no banco, em vez de simular os controllers.

## Execução recomendada

Na raiz do repositório, execute:

```powershell
.\testar.ps1
```

O runner restaura .NET e Node, compila em `Release`, instala de forma idempotente o Chromium compatível, executa os testes de backend com cobertura, os testes de eventos/contratos do DOM e, por fim, as jornadas no navegador.

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
- Resultados TRX e a cobertura do backend ficam em `TestResults`; o frontend grava o resultado estruturado em `Frontend.Tests/vitest-results.json`.

## Cobertura do navegador

- autenticação inválida/válida, sessão, logout e proteção de rotas;
- idioma automático do navegador e seleção explícita no login ou em Configurações;
- password reveal e validação visual da recuperação de senha;
- todos os links visíveis da sidebar, estado ativo e HTTP final;
- menu mobile por toque, Escape e backdrop, além de overflow desktop/mobile;
- usuários e perfis de acesso: validação, criação, busca, edição, permissões e persistência;
- comunidades, atores e atividades: criação, filtros, relações, edição e exclusão lógica/física;
- Diário de Campo: criação sem foto obrigatória, lista, detalhes, busca, filtro, edição, eixos,
  ações institucionais com vínculos de ator/eixo e exclusão;
- Ficha de 1º Contato: wizard de três etapas, seleções filhas, edição, conclusão, filtros e exclusão em cascata;
- Avaliação Pessoal: seleção do ator, eventos dos sliders, criação, busca, edição e exclusão.

Os testes de CRUD sempre conferem o banco após os eventos visíveis, inclusive tabelas de relacionamento. Seletores priorizam nomes, labels e atributos estáveis; não dependem de coordenadas, ordem aleatória ou dados externos.

## Limite conhecido

A recuperação de senha ainda é somente uma simulação visual em `wwwroot/js/forget.js`: o formulário usa `preventDefault`, não chama backend e não envia email. O E2E valida apenas a navegação e os estados visuais dessa tela; ele não afirma que houve recuperação real.

## CI

`.github/workflows/testes.yml` executa a bateria completa nas branches comuns e por acionamento manual, com caches de NuGet, npm e Playwright e upload dos artifacts mesmo em falha. O workflow legado `main.yml` foi preservado para as branches específicas de importação e publicação; os gatilhos são separados para não repetir a mesma bateria.
