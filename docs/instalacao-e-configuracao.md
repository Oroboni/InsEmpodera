# Instalação e configuração

[← Voltar ao README principal](../README.md)

Guia para instalar, configurar e executar o InsEmpodera em uma máquina de desenvolvimento, sem depender da equipe.

## Sumário

- [Pré-requisitos](#pré-requisitos)
- [Ambientes de execução](#ambientes-de-execução)
- [Obter e compilar o código](#obter-e-compilar-o-código)
- [Opção A — demonstração com SQLite](#opção-a--demonstração-com-sqlite)
- [Opção B — desenvolvimento com MySQL](#opção-b--desenvolvimento-com-mysql)
- [Variáveis de configuração](#variáveis-de-configuração)
- [Solução de problemas](#solução-de-problemas)

## Pré-requisitos

| Ferramenta | Versão | Como conferir |
| --- | --- | --- |
| Git | recente | `git --version` |
| .NET SDK | 9.0 | `dotnet --list-sdks` deve listar uma versão `9.0.x` |
| MySQL | 8.4 LTS (versão adotada; também validado com 9.7.1). Para usar a mesma versão do servidor: `docker run -d --name mysql84 -p 3306:3306 -e MYSQL_ROOT_PASSWORD=<senha_local> mysql:8.4`. O MariaDB não é compatível com o esquema atual | `mysql --version` — necessário apenas na opção B |
| Node.js e npm | 22.x (versão da CI) | `node --version` — necessário apenas para os testes de frontend |
| PowerShell | 7+ | `pwsh --version` — necessário apenas para `testar.ps1` |

O projeto tem como alvo `net9.0`. Um SDK mais novo (por exemplo, o 10) também compila o projeto, mas, para executar, é preciso ter o runtime **ASP.NET Core 9** instalado ou permitir a execução em uma versão superior (ver [Solução de problemas](#solução-de-problemas)).

## Ambientes de execução

O comportamento da aplicação muda conforme a variável `ASPNETCORE_ENVIRONMENT`:

| Ambiente | Banco padrão | Criação do esquema | Uso indicado |
| --- | --- | --- | --- |
| `Testing` | SQLite (`Empodera.testing.db`) | Automática, pelo modelo do EF Core, com perfis, permissões e usuários de exemplo (sem os eixos) | Demonstração local e testes automatizados |
| `Development` | MySQL | Não cria tabelas; apenas garante os eixos padrão | Desenvolvimento diário |
| `Production` | MySQL | Não cria tabelas; apenas garante os eixos padrão | Servidor publicado |

Diferenças importantes do ambiente `Testing`: limites de login e de recuperação de senha ampliados para 1000, sem redirecionamento para HTTPS e com registro de dados sensíveis nos logs do EF Core. Por isso, ele **não deve ser usado com dados reais**.

## Obter e compilar o código

```bash
git clone -b Online https://github.com/Oroboni/InsEmpodera.git
cd InsEmpodera
dotnet restore InsEmpodera.sln
dotnet build InsEmpodera.sln
```

Resultado esperado: `Build succeeded` (ou `Compilação com êxito`) com 0 erros.

## Opção A — demonstração com SQLite

Não exige servidor de banco. A aplicação cria `Empodera.testing.db` na pasta do projeto com perfis, permissões e cinco usuários de exemplo.

**1. Iniciar a aplicação**

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

O parâmetro `--no-launch-profile` é necessário: sem ele, o `dotnet run` usaria o perfil de `Properties/launchSettings.json`, que força o ambiente `Development`.

**2. Confirmar a inicialização**

O terminal deve exibir:

```text
Now listening on: http://localhost:5000
Hosting environment: Testing
```

**3. Acessar**

Abra `http://localhost:5000`. Entre com:

| Campo | Valor |
| --- | --- |
| E-mail | `joao@email.com` (usuário de exemplo do perfil `Admin`) |
| Senha | o valor definido em `Testing__AdminPassword` |

A senha é reaplicada a cada inicialização no ambiente `Testing`. Os outros usuários de exemplo não têm senha conhecida.

**4. Cadastrar os eixos temáticos (opcional)**

O ambiente `Testing` não cadastra os dez eixos temáticos, usados nos formulários de recursos, atividades e diários. Para cadastrá-los, pare a aplicação e inicie-a uma vez em `Development` sobre o mesmo arquivo SQLite:

```bash
ASPNETCORE_ENVIRONMENT=Development \
DatabaseProvider=Sqlite \
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DatabaseProvider = "Sqlite"
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

Esse passo só funciona depois que o banco foi criado no passo 1. A senha definida anteriormente continua válida, e a aplicação pode continuar sendo usada nesse modo.

**5. Recomeçar do zero**

Pare a aplicação (`Ctrl+C`) e apague o banco local:

```bash
rm -f Empodera.testing.db Empodera.testing.db-shm Empodera.testing.db-wal
```

## Opção B — desenvolvimento com MySQL

Este é o modo equivalente ao de produção. Use um servidor local e um banco **novo**.

**1. Preparar o servidor**

Instale o MySQL (8.4 LTS ou posterior) e anote o endereço, a porta, o usuário e a senha. O usuário precisa de permissão para criar o banco e as tabelas. Exemplo para criar um usuário dedicado no MySQL:

```sql
CREATE USER 'empodera_dev'@'localhost' IDENTIFIED BY 'SUA_SENHA_LOCAL';
GRANT ALL PRIVILEGES ON empodera.* TO 'empodera_dev'@'localhost';
FLUSH PRIVILEGES;
```

**2. Criar o esquema e os dados iniciais (uma única vez)**

Como o repositório não possui migrations, o esquema é criado pelo próprio modelo do EF Core — o mesmo mecanismo usado pelos testes automatizados na CI. Execute a aplicação **uma vez** no ambiente `Testing` apontando para o MySQL:

macOS e Linux:

```bash
ASPNETCORE_ENVIRONMENT=Testing \
DatabaseProvider=MySql \
ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=empodera;User=empodera_dev;Password=SUA_SENHA_LOCAL;" \
Testing__AdminPassword='EscolhaUmaSenha@123' \
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

Windows (PowerShell):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:DatabaseProvider = "MySql"
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=empodera;User=empodera_dev;Password=SUA_SENHA_LOCAL;"
$env:Testing__AdminPassword = "EscolhaUmaSenha@123"
dotnet run --no-launch-profile --urls "http://localhost:5000"
```

Quando aparecer `Now listening on`, pare a aplicação com `Ctrl+C`. O banco `empodera` terá 34 tabelas, os perfis, as permissões e os usuários de exemplo. No PowerShell, feche o terminal ou remova as variáveis antes do próximo passo.

> **Atenção:** se o banco já tiver tabelas, nada é recriado, mas a senha do usuário de ID 1 é **substituída** pelo valor de `Testing__AdminPassword`. Nunca execute este passo contra um banco com dados reais.

**3. Registrar a connection string fora do repositório**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=empodera;User=empodera_dev;Password=SUA_SENHA_LOCAL;"
```

O valor fica no cofre de *user secrets* do usuário do sistema operacional (identificador `InsEmpodera-Local-Development`), fora da pasta do projeto.

**4. Confiar no certificado HTTPS de desenvolvimento (uma única vez)**

```bash
dotnet dev-certs https --trust
```

**5. Executar**

```bash
dotnet run --launch-profile https
```

O terminal deve exibir `Now listening on: https://localhost:7121` e `Hosting environment: Development`. Na primeira execução, a aplicação cadastra os dez eixos temáticos.

**6. Acessar**

Abra `https://localhost:7121` e entre com `joao@email.com` e a senha definida no passo 2.

### Recomendado: criar o banco pelo script oficial

O DDL oficial, no formato da [revisão técnica do DDL](revisao-ddl-mysql.md), é o script do repositório. Ele cria
as tabelas com todas as restrições nomeadas (o passo 2 cria a partir do modelo do EF, sem o `ON UPDATE` das datas):

| Script | Conteúdo | Comando |
| --- | --- | --- |
| [`Database/Empodera.schema.sql`](../Database/Empodera.schema.sql) | Estrutura e dados de exemplo fictícios | `mysql -u empodera_dev -p empodera < Database/Empodera.schema.sql` (o banco `empodera` precisa existir e estar vazio) |
| [`Database/migracao-uuid-para-int.sql`](../Database/migracao-uuid-para-int.sql) | Converte uma cópia do banco antigo (chaves UUID) | ver [passo a passo](revisao-ddl-mysql.md#migração-dos-dados) |

As senhas dos usuários de exemplo não são publicadas; para definir a senha do usuário de ID 1 numa cópia local,
execute o passo 2 uma vez sobre o banco criado.

A situação de cada script está detalhada em [banco-de-dados.md](banco-de-dados.md#scripts-sql-do-repositório).

## Variáveis de configuração

Ordem de prioridade (da menor para a maior): `appsettings.json` → `appsettings.{Ambiente}.json` → *user secrets* (somente em `Development`) → variáveis de ambiente → argumentos de linha de comando. Em variáveis de ambiente, o separador de seção é `__` (dois sublinhados).

| Chave | Padrão | Quando é usada | Descrição |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Sempre | `Development`, `Testing` ou `Production` |
| `ASPNETCORE_URLS` | definido pelo perfil | Sempre | Endereços em que o Kestrel escuta; equivale a `--urls` |
| `DatabaseProvider` | `Sqlite` em `Testing`; `MySql` nos demais | Sempre | Provedor do banco; outro valor interrompe a inicialização |
| `ConnectionStrings:DefaultConnection` | — | Com `MySql` | Obrigatória com MySQL; sem ela, a aplicação não inicia |
| `ConnectionStrings:TestConnection` | `Data Source=Empodera.testing.db` | Com `Sqlite` | Caminho do arquivo SQLite |
| `Testing:AdminPassword` | — | Somente em `Testing` | Define a senha do usuário de ID 1 e o reativa |
| `Email:User` | `empodera.ajuda@gmail.com` | Sempre | Remetente; qualquer outro valor é rejeitado na inicialização |
| `Email:FromName` | `Instituto Empodera` | Sempre | Nome exibido como remetente |
| `Email:Password` | — | Obrigatória em `Production` | Senha de aplicativo do Gmail (ao menos 16 caracteres, desconsiderando espaços) |
| `Email:PublicBaseUrl` | vazio | Obrigatória em `Production` | URL HTTPS pública usada nos links de recuperação |
| `AllowedHosts` | `localhost;127.0.0.1` | Sempre | Nomes de host aceitos; acessos por outro nome ou IP retornam HTTP 400 |
| `TEST_MYSQL_CONNECTION` | — | Testes | Conexão (sem `Database`) usada pelos testes de backend e E2E |
| `E2E_HEADED` / `E2E_SLOWMO` | — | Testes E2E | Exibem o navegador e desaceleram as ações |

Exemplos com valores fictícios:

```text
ConnectionStrings__DefaultConnection=Server=127.0.0.1;Port=3306;Database=empodera;User=app;Password=senha_local;
Email__PublicBaseUrl=https://empodera.exemplo.org
AllowedHosts=localhost;127.0.0.1;empodera.exemplo.org
```

A configuração completa do envio de e-mails está em [CONFIGURAR-GMAIL.md](../CONFIGURAR-GMAIL.md).

## Solução de problemas

| Sintoma | Causa | Solução |
| --- | --- | --- |
| `You must install or update .NET to run this application` citando `Microsoft.AspNetCore.App 9.0.0` | Runtime do ASP.NET Core 9 ausente | Instale o .NET 9 ou execute com `DOTNET_ROLL_FORWARD=Major` para usar um runtime mais novo (o perfil `https` já define essa variável) |
| `Defina ConnectionStrings__DefaultConnection por variável de ambiente ou cofre de segredos.` | Ambiente `Development`/`Production` sem conexão MySQL | Siga a opção B ou use a opção A |
| `SQLite Error 1: 'no such table: Eixos'` | `DatabaseProvider=Sqlite` fora do ambiente `Testing` com um arquivo SQLite ainda vazio | Crie o banco primeiro no ambiente `Testing` (opção A, passo 1); sem migrations, os outros ambientes não criam tabelas |
| `Table 'empodera.Usuarios' doesn't exist` | Esquema MySQL não criado | Execute o passo 2 da opção B |
| Ambiente `Testing` abre em `Development` | `dotnet run` sem `--no-launch-profile` | Inclua `--no-launch-profile` |
| Aviso de certificado no navegador em `https://localhost:7121` | Certificado de desenvolvimento não confiável | `dotnet dev-certs https --trust` |
| HTTP 400 `Bad Request - Invalid Hostname` | Acesso por um host fora de `AllowedHosts` | Acesse por `localhost` ou acrescente o host em `AllowedHosts` |
| HTTP 429 no login | Limite de 10 tentativas por minuto | Aguarde um minuto |
| "Acesso temporariamente bloqueado" | Cinco senhas incorretas seguidas | Aguarde 15 minutos |
| Em `Production`, a aplicação não inicia citando `Email__Password` ou `Email__PublicBaseUrl` | Configuração de e-mail obrigatória ausente | Configure conforme [CONFIGURAR-GMAIL.md](../CONFIGURAR-GMAIL.md) |
| Recuperação de senha não envia e-mail | Senha de aplicativo do Gmail não configurada | Configure `Email:Password` e `Email:PublicBaseUrl` |
