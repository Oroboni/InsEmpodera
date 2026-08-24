# Testes automáticos

A suíte valida a compilação, a seleção de idioma, as páginas públicas, o redirecionamento de sessão, o login, as páginas principais e os ciclos CRUD da aplicação.

Os testes de integração usam um banco SQLite temporário em memória. O MySQL configurado para desenvolvimento ou produção não é acessado nem alterado.

## Executar no Windows

Na pasta do projeto, execute:

```powershell
.\testar.ps1
```

Também é possível executar somente os testes:

```powershell
dotnet test InsEmpodera.sln
```

O resultado fica na pasta `TestResults`. A cobertura é gerada no formato Cobertura.

## Matriz de cobertura CRUD

Os testes exercitam usuários, perfis de acesso, comunidades, atores, atividades, diários de campo, fichas de primeiro contato e avaliações pessoais.

Para cada CRUD aplicável, a suíte verifica:

- criação e persistência de campos automáticos;
- leitura, filtros e retorno `NotFound` para IDs inexistentes;
- edição e preservação de identidade e auditoria;
- exclusão física ou lógica e tratamento de vínculos dependentes;
- entradas inválidas, duplicidades e inconsistência entre IDs;
- autenticação, permissão por perfil e proteção da conta conectada;
- token antifalsificação em todos os formulários POST;
- inexistência de rotas POST ambíguas;
- execução HTTP ponta a ponta com login, cookies, model binding, views e SQLite isolado.

Os testes em `Crud` verificam diretamente as regras dos controladores. Os testes em `Integration` repetem os fluxos pela aplicação HTTP real. Os testes em `Architecture` impedem que novos endpoints CRUD sejam adicionados sem as proteções obrigatórias.

## Execução contínua

O arquivo `.github/workflows/testes.yml` executa a suíte automaticamente em todo envio de código e em toda pull request. O relatório é anexado à execução do GitHub Actions, inclusive quando houver falha.

## Como ampliar a cobertura

- Adicione testes de regras isoladas em `InsEmpodera.Tests/Unit`.
- Adicione testes de rotas e fluxos completos em `InsEmpodera.Tests/Integration`.
- Toda correção de defeito deve incluir um teste que reproduza o problema antes da correção.
