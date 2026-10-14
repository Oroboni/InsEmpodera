# InsEmpodera — Sistema de Gestão de Comunidades do Instituto Empodera

Aplicação web desenvolvida em ASP.NET Core MVC para apoiar o Instituto Empodera no registro e no acompanhamento de comunidades, atores sociais, recursos, atividades, diários de campo, fichas de primeiro contato e avaliações pessoais.

> **Contexto acadêmico:** projeto criado por estudantes da Fatec para o Instituto Empodera no segundo semestre de 2025, com orientação de Paula Rocha e Antonio Munari. Esta documentação foi revisada como entrega da Tarefa 09 — Documentação de Software, da disciplina de Integração e Entrega Contínua (DevOps).

> **Branch documentada:** o código descrito aqui corresponde à branch `Online`, que concentra o desenvolvimento ativo. A branch `main` está desatualizada em relação a ela.

## 📑 Sumário

- [Visão geral](#-visão-geral)
- [Funcionalidades](#-funcionalidades)
- [Escopo e limitações](#-escopo-e-limitações)
- [Requisitos](#-requisitos)
- [Tecnologias](#-tecnologias)
- [Arquitetura](#-arquitetura)
- [Estrutura do repositório](#-estrutura-do-repositório)
- [Como executar](#-como-executar)
- [Configuração](#-configuração)
- [Banco de dados](#-banco-de-dados)
- [Rotas, APIs e integrações](#-rotas-apis-e-integrações)
- [Testes](#-testes)
- [Integração contínua e deploy](#-integração-contínua-e-deploy)
- [Documentação complementar](#-documentação-complementar)
- [Equipe](#-equipe)
- [Projetos relacionados](#-projetos-relacionados)

## 🎯 Visão geral

### Problema

O trabalho de campo do Instituto Empodera envolve diversas comunidades, as pessoas e organizações que atuam nelas e os atendimentos realizados ao longo do tempo. Sem um sistema único, essas informações ficam dispersas em planilhas e anotações, o que dificulta consolidar o histórico de cada território, relacionar pessoas e recursos e produzir relatórios.

### Objetivo

Centralizar, em uma aplicação web com controle de acesso, o cadastro das comunidades e de seus vínculos, o registro das atividades de campo e o acompanhamento individual dos atores, permitindo consultar relatórios e exportar os dados em planilhas.

### Público-alvo

| Perfil | Uso principal |
| --- | --- |
| Equipe técnica e de campo do Instituto Empodera | Cadastra comunidades, atores, atividades, diários e fichas |
| Coordenação | Consulta relatórios, acompanha processos e exporta dados |
| Administração do sistema | Gerencia usuários, perfis de acesso, importações e backups |

O acesso é controlado por **perfis**. Os perfis `Admin`, `Editor`, `Colaborador`, `Visualizador` e `Supervisor` são criados inicialmente, e cada um possui permissões de listar, detalhar, criar, atualizar e excluir em cada módulo.

## ✨ Funcionalidades

| Módulo | O que permite fazer |
| --- | --- |
| Autenticação | Login por e-mail e senha, logout, bloqueio após tentativas inválidas e recuperação de senha por e-mail |
| Comunidades | Cadastro com endereço principal e secundário, mapa, situação (`Em diagnóstico` / `Em processo`) e desativação |
| Atores | Cadastro de pessoas vinculadas às comunidades, com recursos, vulnerabilidades e rede primária de relacionamentos |
| Recursos | Rede de recursos de cada comunidade, classificada por eixos temáticos |
| Atividades | Atividades por comunidade, com eixos e ações associadas |
| Diários de Campo | Registro de visitas com eixos, ações institucionais e atores envolvidos |
| Ficha de 1º Contato | Formulário em três etapas, com conclusão ou abandono do atendimento |
| Avaliação Pessoal | Avaliação de um ator por meio de indicadores pontuados |
| Diário Pessoal | Registro do processo pessoal de um ator, com eixos e menções a outros atores |
| Relatórios | RSC, Fichas de 1º Contato, Avaliações Pessoais, Ações e Rede Primária |
| Dados e Backup | Exportação de planilhas `.xlsx`, importação de planilhas de comunidade e backup geral |
| Usuários e perfis | Gestão de usuários, ativação/desativação e configuração de permissões por módulo |
| Idiomas | Interface em português, inglês e espanhol |

A descrição detalhada, com atores e situação de implementação de cada item, está em [docs/requisitos.md](docs/requisitos.md).

## 📐 Escopo e limitações

**Dentro do escopo:** aplicação web renderizada no servidor, para uso interno, com autenticação própria, persistência relacional, relatórios e importação/exportação em Excel.

**Fora do escopo atual:**

- API REST pública para integração com outros sistemas;
- aplicativo móvel ou funcionamento offline;
- migrations do Entity Framework — o esquema é criado pelo EF Core a partir do modelo (ver [Banco de dados](#-banco-de-dados));
- implantação automatizada — a publicação em servidor é manual.

**Limitações conhecidas:** não há migrations; o script [`Banco.txt`](Banco.txt) da versão atual não é compatível com o modelo da aplicação; e parte das verificações de permissão ainda depende da sessão HTTP. A lista completa está em [docs/requisitos.md](docs/requisitos.md#limitações-conhecidas).

## 📋 Requisitos

Resumo dos requisitos identificados no código. A especificação completa, com critérios verificáveis, está em [docs/requisitos.md](docs/requisitos.md).

| Código | Requisito funcional | Situação |
| --- | --- | --- |
| RF-001 | Autenticar usuários por e-mail e senha | Implementado |
| RF-002 | Recuperar a senha por link enviado por e-mail | Parcial — depende da configuração do Gmail |
| RF-003 | Gerenciar usuários e perfis de acesso por módulo | Implementado |
| RF-004 | Gerenciar comunidades e sua situação | Implementado |
| RF-005 | Gerenciar atores e seus vínculos | Implementado |
| RF-006 | Gerenciar recursos e atividades das comunidades | Implementado |
| RF-007 | Registrar diários de campo | Implementado |
| RF-008 | Registrar fichas de primeiro contato | Implementado |
| RF-009 | Registrar avaliações pessoais e diários pessoais | Implementado |
| RF-010 | Emitir relatórios | Implementado |
| RF-011 | Importar e exportar dados em Excel | Implementado |
| RF-012 | Alternar o idioma da interface | Implementado |

| Código | Requisito não funcional (principais) | Situação |
| --- | --- | --- |
| RNF-001 | Exigir autenticação em todas as rotas não públicas | Implementado |
| RNF-002 | Proteger formulários contra CSRF | Implementado |
| RNF-003 | Limitar tentativas de login e de recuperação de senha | Implementado |
| RNF-004 | Aplicar política de senha forte e armazenar senhas somente como hash | Implementado |
| RNF-007 | Não versionar credenciais nem dados pessoais reais | Parcial |
| RNF-010 | Executar onde houver o .NET 9 (Windows, Linux e macOS) | Atendido pela plataforma |
| RNF-012 | Executar testes automatizados a cada push | Implementado |
| RNF-014 | Desempenho adequado ao uso interno | Sem meta definida nem medição |

## 🧰 Tecnologias

| Camada | Tecnologia | Versão |
| --- | --- | --- |
| Plataforma | .NET / ASP.NET Core MVC | `net9.0` |
| Linguagem | C# | — |
| Acesso a dados | Entity Framework Core | 9.0.19 |
| Banco principal | MySQL / MariaDB via `Pomelo.EntityFrameworkCore.MySql` | 9.0.0 (MariaDB 10.4.32 na CI) |
| Banco de demonstração | SQLite via `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.19 |
| Autenticação | ASP.NET Core Identity com cookies | 9.0.19 |
| Planilhas | ExcelDataReader (leitura) e ClosedXML (escrita) | 3.8.0 / 0.105.0 |
| Interface | Razor Views, Bootstrap, jQuery, jQuery Validation | 5.3.3 / 3.7.1 / 1.21.0 |
| Mapas e gráficos | Leaflet, Leaflet Control Geocoder, Chart.js | 1.9.4 / — / 4.5.1 |
| Testes | xUnit, Microsoft.AspNetCore.Mvc.Testing, Playwright, Vitest | 2.9.3 / 9.0.19 / 1.62.0 / 3.2.7 |
| Qualidade | Stryker.NET (mutação) e CSharpier (formatação) | 4.16.0 / 1.3.0 |
| CI | GitHub Actions | — |

As versões foram conferidas em [`InsEmpodera.csproj`](InsEmpodera.csproj), nos projetos de teste, em [`Frontend.Tests/package.json`](Frontend.Tests/package.json), em [`.config/dotnet-tools.json`](.config/dotnet-tools.json), em [`dotnet-tools.json`](dotnet-tools.json) e nos arquivos de `wwwroot/lib`.

## 🧱 Arquitetura

O InsEmpodera é um **monólito ASP.NET Core MVC**. O servidor recebe a requisição, consulta o banco pelo Entity Framework Core e devolve páginas HTML renderizadas com Razor. Não há uma camada de API separada nem um frontend SPA.

```mermaid
flowchart LR
    U["Navegador"] -->|HTTP/HTTPS| P["Pipeline ASP.NET Core<br/>idioma, cabeçalhos de segurança,<br/>sessão, autenticação e autorização"]
    P --> C["Controllers MVC"]
    C --> S["Services<br/>Excel, backup, e-mail e idioma"]
    C --> D["ApplicationDbContext<br/>Entity Framework Core"]
    S --> D
    D --> DB[("MySQL / MariaDB<br/>ou SQLite")]
    C --> V["Razor Views"]
    V -->|HTML| U
    S -->|SMTP 587| G["Gmail"]
    U -.->|mapas e endereços| E["OpenStreetMap, ArcGIS e ViaCEP"]
```

| Componente | Responsabilidade |
| --- | --- |
| [`Program.cs`](Program.cs) | Registra serviços, banco, Identity, sessão, idiomas e limites de requisição; monta o pipeline HTTP |
| [`Controllers/`](Controllers/) | Recebem as requisições, verificam permissões, aplicam as regras e escolhem a view |
| [`Models/`](Models/) | Entidades persistidas, view models e extensões de permissão |
| [`Views/`](Views/) | Páginas Razor organizadas por controller |
| [`Data/`](Data/) | `ApplicationDbContext`, relacionamentos, dados iniciais e catálogo de eixos |
| [`Services/`](Services/) | Importação/exportação Excel, backup, Identity, e-mail e localização |
| [`Resources/`](Resources/) | Traduções `.resx` em pt-BR, en e es |
| [`wwwroot/`](wwwroot/) | CSS, JavaScript, imagens e bibliotecas de terceiros |

O fluxo completo de uma requisição, o fluxo de autenticação e as decisões técnicas estão em [docs/arquitetura.md](docs/arquitetura.md).

## 📁 Estrutura do repositório

```text
InsEmpodera/
├── .github/workflows/        # Pipelines do GitHub Actions
├── Controllers/              # Controllers MVC
├── Data/                     # DbContext, dados iniciais e catálogo de eixos
├── Database/                 # Scripts SQL auxiliares
├── Frontend.Tests/           # Testes de JavaScript e DOM (Vitest)
├── InsEmpodera.E2ETests/     # Jornadas no navegador (Playwright)
├── InsEmpodera.Tests/        # Testes de backend, integração e segurança (xUnit)
├── Models/                   # Entidades e view models
├── Properties/               # launchSettings.json
├── Resources/                # Traduções .resx
├── Services/                 # Serviços de aplicação
├── Views/                    # Páginas Razor
├── deploy/                   # Pacote publicado em 31/08/2026
├── docs/                     # Documentação técnica complementar
├── scripts/                  # Relatórios e diagnósticos dos testes (PowerShell)
├── wwwroot/                  # Arquivos estáticos e bibliotecas
├── Banco.txt                 # Script SQL do banco MySQL/MariaDB (ver docs/banco-de-dados.md)
├── InsEmpodera.csproj        # Projeto web
├── InsEmpodera.sln           # Solução com a aplicação e os testes
├── Program.cs                # Inicialização da aplicação
├── appsettings.json          # Configuração sem segredos
└── testar.ps1                # Executor de todas as suítes de teste
```

## 🚀 Como executar

### Pré-requisitos

| Ferramenta | Versão | Necessária para |
| --- | --- | --- |
| Git | recente | Clonar o repositório |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | 9.0 | Compilar e executar |
| MySQL ou MariaDB | MariaDB 10.4.32 na CI; validado também com MySQL 9.7.1 | Modo de desenvolvimento, produção e testes de backend/E2E |
| Node.js e npm | 22.x na CI | Testes de frontend |
| PowerShell | 7 ou superior | Script `testar.ps1` |

### 1. Clonar e compilar

```bash
git clone -b Online https://github.com/Oroboni/InsEmpodera.git
cd InsEmpodera
dotnet restore InsEmpodera.sln
dotnet build InsEmpodera.sln
```

### 2. Executar em modo de demonstração (SQLite, sem MySQL)

É o caminho mais curto para conhecer o sistema. No ambiente `Testing`, a aplicação cria um banco SQLite local (`Empodera.testing.db`, ignorado pelo Git) a partir do modelo, já com perfis, permissões e usuários de exemplo. A variável `Testing__AdminPassword` define a senha do administrador de exemplo: escolha qualquer valor.

macOS e Linux:

```bash
ASPNETCORE_ENVIRONMENT=Testing \
Testing__AdminPassword='EscolhaUmaSenha@123' \
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

Windows (PowerShell):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:Testing__AdminPassword = "EscolhaUmaSenha@123"
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

Acesse `http://localhost:5000` e entre com o e-mail `joao@email.com` e a senha escolhida. Esse usuário pertence ao perfil `Admin`.

Nesse modo, os eixos temáticos não são cadastrados automaticamente; o passo opcional para cadastrá-los está em [docs/instalacao-e-configuracao.md](docs/instalacao-e-configuracao.md#opção-a--demonstração-com-sqlite).

> **Atenção:** o modo `Testing` afrouxa proteções (limite de tentativas de login, redirecionamento HTTPS e registro de dados sensíveis em log) e serve **apenas para demonstração local**. Não o utilize com dados reais. Para recomeçar do zero, pare a aplicação e apague `Empodera.testing.db`.

### 3. Executar em modo de desenvolvimento (MySQL/MariaDB)

É o modo equivalente ao de produção. Em resumo:

1. Criar o esquema e os dados iniciais **uma única vez**, executando a aplicação no ambiente `Testing` com um banco MySQL novo (o EF Core cria as tabelas a partir do modelo):

   ```bash
   ASPNETCORE_ENVIRONMENT=Testing \
   DatabaseProvider=MySql \
   ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=empodera;User=SEU_USUARIO;Password=SUA_SENHA;" \
   Testing__AdminPassword='EscolhaUmaSenha@123' \
   dotnet run --no-launch-profile --urls "http://localhost:5000"
   ```

   Quando aparecer `Now listening on`, encerre com `Ctrl+C`.

2. Registrar a connection string fora do repositório:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=empodera;User=SEU_USUARIO;Password=SUA_SENHA;"
   ```

3. Iniciar com o perfil definido em `Properties/launchSettings.json`:

   ```bash
   dotnet run --launch-profile https
   ```

4. Acessar `https://localhost:7121` e entrar com `joao@email.com` e a senha definida no passo 1.

> **Atenção:** nunca execute o passo 1 contra um banco com dados reais — ele redefine a senha do usuário de ID 1.

O passo a passo completo (inclusive para Windows), o motivo de não usar o `Banco.txt` e a solução de problemas estão em [docs/instalacao-e-configuracao.md](docs/instalacao-e-configuracao.md).

### Exemplo de uso

1. Entre no sistema e abra **Comunidades** para cadastrar uma comunidade fictícia.
2. Em **Atores**, cadastre uma pessoa vinculada a essa comunidade.
3. Registre uma visita em **Diários de Campo** e um atendimento em **Ficha de 1º Contato**.
4. Consulte o resultado em **Relatórios** ou exporte a lista de comunidades em planilha pelo botão de exportação da tela **Comunidades**.

## 🔧 Configuração

A configuração segue o padrão do ASP.NET Core: `appsettings.json`, `appsettings.{Ambiente}.json`, *user secrets* em desenvolvimento e variáveis de ambiente no formato `Secao__Chave`. Nenhuma senha deve ser gravada em arquivos versionados.

| Chave / variável | Obrigatória | Finalidade | Exemplo fictício |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Sim | Ambiente: `Development`, `Testing` ou `Production` | `Development` |
| `DatabaseProvider` | Não | `MySql` (padrão) ou `Sqlite` (padrão apenas em `Testing`) | `MySql` |
| `ConnectionStrings__DefaultConnection` | Com MySQL | Conexão com o MySQL/MariaDB | `Server=127.0.0.1;Port=3306;Database=empodera;User=app;Password=...;` |
| `ConnectionStrings__TestConnection` | Não | Conexão SQLite; padrão `Data Source=Empodera.testing.db` | `Data Source=demo.db` |
| `Testing__AdminPassword` | Não | Senha aplicada ao usuário de ID 1, somente em `Testing` | `EscolhaUmaSenha@123` |
| `Email__User` | Sim | Remetente; o código aceita apenas `empodera.ajuda@gmail.com` | já definido em `appsettings.json` |
| `Email__Password` | Em produção | Senha de aplicativo do Gmail | definida no servidor |
| `Email__PublicBaseUrl` | Em produção | URL HTTPS pública usada nos links de recuperação | `https://exemplo.org` |
| `TEST_MYSQL_CONNECTION` | Para testes | Servidor MySQL usado pelos testes de backend e E2E | `Server=127.0.0.1;Port=3306;User=root;Password=...;` |

Detalhes e valores padrão: [docs/instalacao-e-configuracao.md](docs/instalacao-e-configuracao.md#variáveis-de-configuração). A configuração do Gmail está em [CONFIGURAR-GMAIL.md](CONFIGURAR-GMAIL.md).

## 💾 Banco de dados

- **Mapeamento:** Entity Framework Core com 34 entidades registradas em [`Data/ApplicationDbContext.cs`](Data/ApplicationDbContext.cs).
- **Provedores:** MySQL/MariaDB em desenvolvimento e produção; SQLite no modo de demonstração.
- **Criação do esquema:** não há migrations no repositório. O esquema é criado pelo próprio EF Core (`EnsureCreated`) quando a aplicação roda no ambiente `Testing`, tanto em SQLite quanto em MySQL. O script [`Banco.txt`](Banco.txt) existe, mas sua versão atual é incompatível com o modelo.
- **Dados iniciais:** perfis, permissões e cinco usuários de exemplo, criados junto com o esquema; os dez eixos temáticos são cadastrados na inicialização em `Development` e `Production`, caso estejam ausentes.
- **Chaves primárias:** inteiros aleatórios entre 100000 e 999999, gerados pela aplicação.

O modelo de dados, os relacionamentos e os cuidados com os scripts estão em [docs/banco-de-dados.md](docs/banco-de-dados.md).

## 🔌 Rotas, APIs e integrações

O InsEmpodera **não expõe uma API REST pública**. As rotas seguem o padrão MVC `{controller=Home}/{action=Index}/{id?}` e retornam páginas HTML, redirecionamentos ou planilhas `.xlsx`. O Swagger está registrado nos ambientes `Development` e `Testing`, mas o documento gerado não contém operações.

| Serviço externo | Uso |
| --- | --- |
| Gmail SMTP (porta 587) | Envio dos links de recuperação de senha |
| OpenStreetMap (mapas e Nominatim) e ArcGIS | Exibição de mapas e busca de endereços, feitas pelo navegador |
| ViaCEP | Consulta de CEP no formulário de diário de campo |

As rotas por módulo e os endpoints que devolvem arquivos ou JSON estão em [docs/rotas-e-integracoes.md](docs/rotas-e-integracoes.md).

## 🧪 Testes

| Suíte | Ferramenta | Requer MySQL | Comando |
| --- | --- | :---: | --- |
| Backend, integração HTTP e segurança | xUnit | Sim | `dotnet test InsEmpodera.Tests/InsEmpodera.Tests.csproj` |
| Arquitetura e unidade (subconjunto) | xUnit | Não | ver [docs/testes.md](docs/testes.md#subconjunto-sem-banco) |
| Interface (JavaScript e DOM) | Vitest + jsdom | Não | `npm ci --prefix Frontend.Tests` e `npm test --prefix Frontend.Tests` |
| Jornadas no navegador | Playwright (Chromium) | Sim | `dotnet test InsEmpodera.E2ETests/InsEmpodera.E2ETests.csproj` |
| Todas as suítes, com relatório | PowerShell | Sim | `./testar.ps1` |

As suítes que usam banco criam e removem bancos temporários no servidor indicado por `TEST_MYSQL_CONNECTION`; o banco de desenvolvimento não é alterado. Instruções, cobertura e roteiro de validação manual: [docs/testes.md](docs/testes.md).

## 🔄 Integração contínua e deploy

| Workflow | Gatilho | O que faz |
| --- | --- | --- |
| [`testes.yml`](.github/workflows/testes.yml) | Push e pull request em todas as branches, exceto `import_exel` | Executa `testar.ps1` com MariaDB 10.4.32 e publica os resultados como artefato |
| [`main.yml`](.github/workflows/main.yml) | Push na branch `import_exel` | Compila, testa e guarda o resultado de `dotnet publish` como artefato |
| [`qualidade-periodica.yml`](.github/workflows/qualidade-periodica.yml) | Domingos às 03h (UTC) ou execução manual | Executa testes de mutação com Stryker.NET |

Nenhum workflow publica a aplicação em servidor. A implantação é **manual**: o pacote gerado por `dotnet publish` é enviado a uma instância AWS EC2 com Nginx, systemd e MariaDB. O resumo está em [docs/deploy.md](docs/deploy.md), e o roteiro operacional completo, em [HOSPEDAGEM-AWS.md](HOSPEDAGEM-AWS.md).

## 📚 Documentação complementar

| Documento | Conteúdo |
| --- | --- |
| [docs/requisitos.md](docs/requisitos.md) | Requisitos funcionais e não funcionais, regras de negócio e limitações |
| [docs/arquitetura.md](docs/arquitetura.md) | Componentes, fluxo de requisição, autenticação, autorização e idiomas |
| [docs/instalacao-e-configuracao.md](docs/instalacao-e-configuracao.md) | Instalação detalhada, variáveis e solução de problemas |
| [docs/banco-de-dados.md](docs/banco-de-dados.md) | Entidades, relacionamentos, scripts e dados iniciais |
| [docs/rotas-e-integracoes.md](docs/rotas-e-integracoes.md) | Rotas MVC, downloads, endpoints JSON e serviços externos |
| [docs/testes.md](docs/testes.md) | Suítes automatizadas, execução e validação manual |
| [docs/deploy.md](docs/deploy.md) | Pipelines, publicação e implantação |
| [TESTES.md](TESTES.md) | Matriz de cobertura CRUD |
| [InsEmpodera.E2ETests/README.md](InsEmpodera.E2ETests/README.md) | Detalhes da suíte Playwright |
| [CONFIGURAR-GMAIL.md](CONFIGURAR-GMAIL.md) | Configuração do envio de e-mails |
| [HOSPEDAGEM-AWS.md](HOSPEDAGEM-AWS.md) | Implantação na AWS |

## 👥 Equipe

Composição atual da equipe responsável pelo projeto:

- Isadora Correa
- Matheus Cuero
- Maria Julia Loureiro
- Sophia Marcelino

O projeto foi iniciado no segundo semestre de 2025, com orientação de Paula Rocha e Antonio Munari. O histórico completo de contribuições permanece registrado nos commits do repositório.

## 🔗 Projetos relacionados

- [Integração e Entrega Contínua — DevOps](https://github.com/Isadora-Correa/Integracao-Entrega-Continua-DevOps) — repositório da disciplina que reúne as atividades do semestre, incluindo a [Tarefa 09](https://github.com/Isadora-Correa/Integracao-Entrega-Continua-DevOps/blob/main/Aula09/Tarefa09/Atividade09.md), que motivou esta documentação.
- [Aula — Documentação de Software](https://github.com/deivisontakatu/aula-documentacao-software) — material de referência utilizado na revisão.
