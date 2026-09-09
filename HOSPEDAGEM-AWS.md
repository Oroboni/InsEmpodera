# Hospedagem do Instituto Empodera na AWS

> Guia completo para publicar o **InsEmpodera** em uma instância EC2 com Amazon Linux 2023, MariaDB, ASP.NET Core 9, systemd, Nginx e HTTPS no IP elástico.

**Projeto:** `Oroboni/InsEmpodera`  
**Branch de implantação:** `Online`  
**URL pública atual:** `https://34.234.8.175`  
**Última revisão deste guia:** 9 de setembro de 2026

---

## Sumário

1. [Arquitetura final](#1-arquitetura-final)
2. [Dados atuais da infraestrutura](#2-dados-atuais-da-infraestrutura)
3. [Criar ou conferir a EC2](#3-criar-ou-conferir-a-ec2)
4. [Preparar o Amazon Linux](#4-preparar-o-amazon-linux)
5. [Configurar o MariaDB](#5-configurar-o-mariadb)
6. [Importar e validar o banco](#6-importar-e-validar-o-banco)
7. [Configurar as variáveis da aplicação](#7-configurar-as-variáveis-da-aplicação)
8. [Publicar no computador e enviar para a AWS](#8-publicar-no-computador-e-enviar-para-a-aws)
9. [Instalar uma versão na EC2](#9-instalar-uma-versão-na-ec2)
10. [Criar o serviço systemd](#10-criar-o-serviço-systemd)
11. [Configurar o Nginx inicialmente em HTTP](#11-configurar-o-nginx-inicialmente-em-http)
12. [Emitir HTTPS para o IP elástico](#12-emitir-https-para-o-ip-elástico)
13. [Ativar a configuração HTTPS definitiva](#13-ativar-a-configuração-https-definitiva)
14. [Validar a implantação](#14-validar-a-implantação)
15. [Atualizar o sistema futuramente](#15-atualizar-o-sistema-futuramente)
16. [Fazer rollback](#16-fazer-rollback)
17. [Backup e restauração do MariaDB](#17-backup-e-restauração-do-mariadb)
18. [Diagnóstico de problemas](#18-diagnóstico-de-problemas)
19. [Checklist final](#19-checklist-final)

---

## 1. Arquitetura final

```text
Internet
   │
   ├── HTTP  :80 ────────┐
   └── HTTPS :443 ───────┤
                         ▼
              Nginx — 34.234.8.175
                         │
                         │ proxy HTTP local
                         ▼
              Kestrel — 127.0.0.1:5000
                         │
                         │ conexão local
                         ▼
              MariaDB — 127.0.0.1:3306
```

Responsabilidades de cada parte:

| Componente | Função |
|---|---|
| EC2 | Executa o sistema operacional e os serviços. |
| IP elástico | Mantém o endereço público `34.234.8.175` associado à instância. |
| Nginx | Recebe as conexões públicas nas portas 80 e 443, termina o HTTPS e encaminha as requisições ao Kestrel. |
| Kestrel | Executa `InsEmpodera.dll` somente em `127.0.0.1:5000`. |
| systemd | Inicia, reinicia e registra os logs da aplicação. |
| MariaDB | Armazena os dados no banco `empodera`. |
| `/etc/empodera/empodera.env` | Guarda a conexão do banco, a configuração de e-mail e os demais valores de produção. |
| `~/empodera-releases` | Guarda versões publicadas da aplicação. |
| `~/empodera-current` | Link para a versão que está ativa. |

---

## 2. Dados atuais da infraestrutura

| Item | Valor atual |
|---|---|
| Região | `us-east-1` |
| Instância | `i-019821aac717e792f` |
| IP privado | `172.31.31.141` |
| IP elástico | `34.234.8.175` |
| Allocation ID | `eipalloc-0ab924868e551fccb` |
| Association ID | `eipassoc-0256fbcdfe0923969` |
| Usuário SSH | `ec2-user` |
| Banco | `empodera` |
| Usuário da aplicação no banco | `empodera_app` |
| Porta interna do Kestrel | `127.0.0.1:5000` |
| URL pública | `https://34.234.8.175` |

Os identificadores acima servem para localizar a infraestrutura atual. Em uma instalação nova, substitua-os pelos valores da nova instância.

---

## 3. Criar ou conferir a EC2

No AWS Academy/Vocareum, inicie o Learner Lab e abra o console da AWS. Para uma instalação nova, crie a instância com:

| Opção | Valor |
|---|---|
| Nome | `EmpoderaBE` |
| AMI | Amazon Linux 2023 |
| Tipo | `t3.micro` |
| Arquitetura | `x86_64` |
| Disco | 15 GiB ou mais |
| Key pair | `Empodera` ou outra chave `.pem` disponível |

### Regras de entrada do Security Group

| Tipo | Porta | Origem |
|---|---:|---|
| SSH | 22 | Seu IP público |
| HTTP | 80 | `0.0.0.0/0` |
| HTTPS | 443 | `0.0.0.0/0` |

Não crie regras públicas para as portas `3306` ou `5000`.

### Associar o IP elástico

Na AWS:

1. Abra **EC2 → Elastic IP addresses**.
2. Selecione `34.234.8.175`.
3. Use **Actions → Associate Elastic IP address**.
4. Selecione a instância `i-019821aac717e792f`.
5. Confirme a associação.

### Conectar pelo Windows

No PowerShell, substitua somente o caminho da chave:

```powershell
ssh -i "C:\caminho\Empodera.pem" ec2-user@34.234.8.175
```

Se o SSH rejeitar a chave porque as permissões do arquivo estão abertas demais, ajuste as permissões da `.pem` ou conecte pelo terminal oferecido pelo console da EC2.

---

## 4. Preparar o Amazon Linux

Esta seção é executada na EC2 apenas na configuração inicial.

### 4.1 Atualizar o sistema e instalar os componentes

```bash
sudo dnf upgrade -y
sudo dnf install -y aspnetcore-runtime-9.0 nginx mariadb105-server python3 python3-pip tar gzip
```

Confira as versões:

```bash
dotnet --info
nginx -v
mariadb --version
python3 --version
```

O projeto tem como alvo `net9.0`; portanto, a EC2 precisa ter o runtime ASP.NET Core 9 instalado.

### 4.2 Iniciar os serviços básicos

```bash
sudo systemctl enable --now mariadb
sudo systemctl enable --now nginx
```

Confira:

```bash
sudo systemctl status mariadb --no-pager
sudo systemctl status nginx --no-pager
```

Os dois serviços devem aparecer como `active (running)`.

### 4.3 Criar a estrutura de diretórios

```bash
mkdir -p /home/ec2-user/empodera-releases
mkdir -p /home/ec2-user/empodera-data/uploads
mkdir -p /home/ec2-user/backups
sudo install -d -m 755 /etc/empodera
sudo install -d -m 755 /var/www/certbot/.well-known/acme-challenge
```

---

## 5. Configurar o MariaDB

### 5.1 Criar o banco e o usuário da aplicação

Entre no MariaDB:

```bash
sudo mariadb
```

Execute o SQL abaixo. Troque `SENHA_FORTE_DO_BANCO` pela senha que será usada também no arquivo de ambiente:

```sql
CREATE DATABASE IF NOT EXISTS empodera
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'empodera_app'@'localhost'
    IDENTIFIED BY 'SENHA_FORTE_DO_BANCO';

CREATE USER IF NOT EXISTS 'empodera_app'@'127.0.0.1'
    IDENTIFIED BY 'SENHA_FORTE_DO_BANCO';

GRANT ALL PRIVILEGES ON empodera.*
    TO 'empodera_app'@'localhost';

GRANT ALL PRIVILEGES ON empodera.*
    TO 'empodera_app'@'127.0.0.1';

FLUSH PRIVILEGES;
EXIT;
```

Teste a conexão usando exatamente o usuário da aplicação:

```bash
mariadb -h 127.0.0.1 -u empodera_app -p empodera -e "SELECT 1 AS ConexaoOK;"
```

### 5.2 Conferir se o MariaDB está local

```bash
sudo ss -ltnp | grep ':3306'
```

O banco deve ser acessado localmente pela aplicação. A porta `3306` não precisa estar liberada no Security Group.

---

## 6. Importar e validar o banco

O projeto atual não possui migrations suficientes para reconstruir toda a base histórica sozinho. Em uma instalação vazia, importe um dump SQL antes de iniciar `InsEmpodera.dll`.

### 6.1 Gerar o dump no computador Windows

No PowerShell, dentro do projeto:

```powershell
Set-Location C:\Users\melio\Downloads\InsEmpodera
New-Item -ItemType Directory -Force .\deploy | Out-Null

mysqldump `
  --host=127.0.0.1 `
  --port=3306 `
  --user=root `
  --password `
  --single-transaction `
  --routines `
  --triggers `
  --default-character-set=utf8mb4 `
  --result-file=.\deploy\empodera-inicial.sql `
  Empodera
```

O parâmetro `--password` solicitará a senha. Se o usuário local não tiver senha, pressione Enter.

Envie o dump:

```powershell
scp -i "C:\caminho\Empodera.pem" `
  .\deploy\empodera-inicial.sql `
  ec2-user@34.234.8.175:/home/ec2-user/
```

### 6.2 Importar na EC2

```bash
sudo mariadb empodera < /home/ec2-user/empodera-inicial.sql
```

### 6.3 Corrigir nomes de tabelas vindos do Windows

O MariaDB no Linux diferencia maiúsculas e minúsculas nos nomes físicos das tabelas. O modelo do Empodera espera, por exemplo, `Usuarios`, não `usuarios`.

Em um banco recém-importado e ainda sem a aplicação iniciada, abra:

```bash
sudo mariadb empodera
```

Se `SHOW TABLES;` exibir os nomes todos em minúsculas, execute uma vez:

```sql
RENAME TABLE
    `__efmigrationshistory` TO `__EFMigrationsHistory`,
    `acoes` TO `Acoes`,
    `acoesatores` TO `AcoesAtores`,
    `anexosdiario` TO `AnexosDiario`,
    `atividades` TO `Atividades`,
    `atividadeseixo` TO `AtividadesEixo`,
    `atorcomunidades` TO `AtorComunidades`,
    `atores` TO `Atores`,
    `avaliacaopessoal` TO `AvaliacaoPessoal`,
    `comunidades` TO `Comunidades`,
    `daatores` TO `DAAtores`,
    `detalhesdacoes` TO `DetalhesDAcoes`,
    `detalheseixos` TO `DetalhesEixos`,
    `diarioacoes` TO `DiarioAcoes`,
    `diariodacoes` TO `DiarioDAcoes`,
    `diarioeixos` TO `DiarioEixos`,
    `diarioscampo` TO `DiariosCampo`,
    `eixos` TO `Eixos`,
    `fichacondicoes` TO `FichaCondicoes`,
    `fichapeticoes` TO `FichaPeticoes`,
    `ficharespostas` TO `FichaRespostas`,
    `ficharesultados` TO `FichaResultados`,
    `fichasprimeirocontato` TO `FichasPrimeiroContato`,
    `fontesinfo` TO `FontesInfo`,
    `perfis` TO `Perfis`,
    `permissoes` TO `Permissoes`,
    `recursosatores` TO `RecursosAtores`,
    `redeeixos` TO `RedeEixos`,
    `rederecursos` TO `RedeRecursos`,
    `redesprimarias` TO `RedesPrimarias`,
    `usuarios` TO `Usuarios`,
    `vulnerabilidades` TO `Vulnerabilidades`,
    `vulnerabilidadeseixo` TO `VulnerabilidadesEixo`;
```

As tabelas `diariosprocessopessoal` e `diariosprocessoeixos` devem continuar em minúsculas porque possuem mapeamento explícito com esses nomes no código.

Confira os nomes essenciais:

```sql
SHOW TABLES LIKE 'Usuarios';
SHOW TABLES LIKE 'Perfis';
SHOW TABLES LIKE 'Permissoes';
SHOW TABLES LIKE 'Comunidades';
SHOW TABLES LIKE 'diariosprocessopessoal';
```

Confira também as colunas de autenticação:

```sql
SELECT COLUMN_NAME
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'empodera'
  AND TABLE_NAME = 'Usuarios'
  AND COLUMN_NAME IN (
      'Senha',
      'NormalizedEmail',
      'NormalizedUserName',
      'SecurityStamp',
      'ConcurrencyStamp',
      'AccessFailedCount',
      'LockoutEnd'
  )
ORDER BY COLUMN_NAME;
```

Finalize:

```sql
SELECT COUNT(*) AS Usuarios FROM Usuarios;
SELECT COUNT(*) AS Comunidades FROM Comunidades;
EXIT;
```

---

## 7. Configurar as variáveis da aplicação

O `appsettings.json` do projeto é adequado para desenvolvimento local. Na EC2, os valores de produção devem vir do arquivo `/etc/empodera/empodera.env`.

Crie o arquivo:

```bash
sudo nano /etc/empodera/empodera.env
```

Use o conteúdo abaixo e substitua somente as duas senhas:

```ini
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:5000
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
DatabaseProvider=MySql

ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=empodera;User=empodera_app;Password=SENHA_FORTE_DO_BANCO;ConvertZeroDateTime=True;"

Email__User=empodera.ajuda@gmail.com
Email__Password="SENHA_DE_APLICATIVO_DO_GMAIL"
Email__FromName="Instituto Empodera"
Email__PublicBaseUrl=https://34.234.8.175
```

Observações funcionais:

- `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` faz a aplicação reconhecer que a requisição original chegou por HTTPS ao Nginx.
- `Email__PublicBaseUrl` precisa ser uma URL HTTPS e não deve terminar com um caminho como `/Account`.
- `Testing__AdminPassword` não deve estar nesse arquivo.
- O valor de `Email__Password` é apenas a senha de aplicativo do Gmail, sem comandos como `dotnet user-secrets set` dentro do valor.

Restrinja o arquivo:

```bash
sudo chown root:root /etc/empodera/empodera.env
sudo chmod 600 /etc/empodera/empodera.env
```

---

## 8. Publicar no computador e enviar para a AWS

Esta é a forma principal de publicar. O build não precisa ocorrer na `t3.micro`.

### 8.1 Atualizar e validar o projeto

No PowerShell:

```powershell
Set-Location C:\Users\melio\Downloads\InsEmpodera
git switch Online
git pull --ff-only origin Online
```

Execute a validação completa do repositório:

```powershell
.\testar.ps1
```

### 8.2 Gerar o pacote de publicação

```powershell
$Release = Get-Date -Format 'yyyyMMdd-HHmmss'
$PublishDir = Join-Path $PWD "deploy\empodera-$Release"
$Archive = Join-Path $PWD "deploy\empodera-$Release.tar.gz"

dotnet publish .\InsEmpodera.csproj `
  --configuration Release `
  --output $PublishDir `
  --no-self-contained
```

Verifique os arquivos fundamentais:

```powershell
Test-Path "$PublishDir\InsEmpodera.dll"
Test-Path "$PublishDir\InsEmpodera.runtimeconfig.json"
Test-Path "$PublishDir\wwwroot\css\styles.css"
Test-Path "$PublishDir\wwwroot\js\login.js"
```

Todos devem retornar `True`.

Empacote preservando toda a estrutura, inclusive `wwwroot`:

```powershell
tar -czf $Archive -C $PublishDir .
Get-Item $Archive | Select-Object FullName, Length
```

### 8.3 Enviar o pacote

```powershell
scp -i "C:\caminho\Empodera.pem" `
  $Archive `
  ec2-user@34.234.8.175:/home/ec2-user/
```

Anote o valor exibido em `$Release`:

```powershell
$Release
```

Esse valor será usado como nome da versão na EC2.

---

## 9. Instalar uma versão na EC2

Conecte-se por SSH e defina o número exato da versão enviada:

```bash
RELEASE=20260831-230000
RELEASE_DIR="/home/ec2-user/empodera-releases/$RELEASE"
PACKAGE="/home/ec2-user/empodera-$RELEASE.tar.gz"
```

Substitua `20260831-230000` pelo valor real gerado no seu computador.

Crie o diretório e extraia:

```bash
mkdir -p "$RELEASE_DIR"
tar -xzf "$PACKAGE" -C "$RELEASE_DIR"
```

Valide antes de ativar:

```bash
test -f "$RELEASE_DIR/InsEmpodera.dll" && echo "DLL OK"
test -f "$RELEASE_DIR/InsEmpodera.runtimeconfig.json" && echo "Runtime OK"
test -f "$RELEASE_DIR/wwwroot/css/styles.css" && echo "CSS OK"
test -f "$RELEASE_DIR/wwwroot/js/login.js" && echo "JavaScript OK"
```

### 9.1 Manter os uploads fora das versões

Na primeira publicação, copie os arquivos entregues pelo projeto para a pasta persistente:

```bash
if [ -d "$RELEASE_DIR/wwwroot/uploads" ]; then
    cp -an "$RELEASE_DIR/wwwroot/uploads/." /home/ec2-user/empodera-data/uploads/
    mv "$RELEASE_DIR/wwwroot/uploads" "$RELEASE_DIR/wwwroot/uploads-from-package"
fi

ln -s /home/ec2-user/empodera-data/uploads "$RELEASE_DIR/wwwroot/uploads"
```

Assim, os uploads não desaparecem quando `empodera-current` passa a apontar para outra versão.

### 9.2 Ativar a versão

```bash
ln -sfn "$RELEASE_DIR" /home/ec2-user/empodera-current
readlink -f /home/ec2-user/empodera-current
```

O último comando deve mostrar o diretório da versão recém-extraída.

---

## 10. Criar o serviço systemd

Crie:

```bash
sudo nano /etc/systemd/system/empodera.service
```

Conteúdo:

```ini
[Unit]
Description=Instituto Empodera ASP.NET Core
Wants=network-online.target
After=network-online.target mariadb.service

[Service]
WorkingDirectory=/home/ec2-user/empodera-current
ExecStart=/usr/bin/dotnet /home/ec2-user/empodera-current/InsEmpodera.dll
User=ec2-user
Group=ec2-user
EnvironmentFile=/etc/empodera/empodera.env
Restart=always
RestartSec=5
KillSignal=SIGINT
TimeoutStopSec=30
SyslogIdentifier=empodera

[Install]
WantedBy=multi-user.target
```

Ative e inicie:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now empodera
sudo systemctl status empodera --no-pager
```

Confirme a porta interna:

```bash
sudo ss -ltnp | grep ':5000'
```

O endereço esperado é:

```text
127.0.0.1:5000
```

Teste o Kestrel com o Host público:

```bash
curl -I -H 'Host: 34.234.8.175' http://127.0.0.1:5000/Account
```

Uma resposta `200`, `301` ou `302` comprova que o processo está respondendo. Se houver redirecionamento para HTTPS, isso é esperado em `Production`.

---

## 11. Configurar o Nginx inicialmente em HTTP

Antes de emitir o certificado, o Nginx precisa servir o desafio HTTP do Let's Encrypt.

Crie:

```bash
sudo nano /etc/nginx/conf.d/empodera.conf
```

Configuração inicial:

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name 34.234.8.175;

    location ^~ /.well-known/acme-challenge/ {
        root /var/www/certbot;
        default_type text/plain;
        try_files $uri =404;
    }

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;

        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;

        proxy_connect_timeout 30s;
        proxy_send_timeout 120s;
        proxy_read_timeout 120s;
        client_max_body_size 25m;
    }
}
```

Valide e recarregue:

```bash
sudo nginx -t
sudo systemctl reload nginx
```

Teste especificamente o diretório do certificado:

```bash
printf 'acme-novo' | sudo tee /var/www/certbot/.well-known/acme-challenge/teste >/dev/null
curl -fsS http://34.234.8.175/.well-known/acme-challenge/teste
echo
```

O resultado deve ser:

```text
acme-novo
```

O texto devolvido pelo `curl` precisa ser exatamente o texto gravado no arquivo. Se aparecer um valor antigo ou diferente, o Nginx está servindo outro webroot ou outro bloco `server`. Confira a configuração realmente carregada antes de executar o Certbot:

```bash
sudo nginx -T 2>/dev/null | grep -n -A 10 -B 5 'acme-challenge'
```

---

## 12. Emitir HTTPS para o IP elástico

Desde 2026, o Let's Encrypt emite certificados para endereços IP. Esses certificados usam o perfil `shortlived`, duram aproximadamente seis dias e precisam de renovação automática. Para o modo `webroot` com IP, use Certbot 5.4 ou mais recente.

### 12.1 Instalar uma versão atual do Certbot

```bash
sudo python3 -m venv /opt/certbot
sudo /opt/certbot/bin/pip install --upgrade pip
sudo /opt/certbot/bin/pip install 'certbot>=5.4'
sudo ln -sfn /opt/certbot/bin/certbot /usr/local/bin/certbot
certbot --version
```

### 12.2 Testar contra o ambiente de homologação

Troque `SEU_EMAIL` por um endereço válido para os avisos do certificado:

```bash
sudo certbot certonly \
  --staging \
  --preferred-profile shortlived \
  --webroot \
  --webroot-path /var/www/certbot \
  --ip-address 34.234.8.175 \
  --cert-name empodera-ip-staging \
  --email SEU_EMAIL \
  --agree-tos
```

Se o teste terminar com sucesso, emita o certificado confiável:

```bash
sudo certbot certonly \
  --preferred-profile shortlived \
  --webroot \
  --webroot-path /var/www/certbot \
  --ip-address 34.234.8.175 \
  --cert-name 34.234.8.175 \
  --email SEU_EMAIL \
  --agree-tos
```

Confira os arquivos:

```bash
sudo ls -l /etc/letsencrypt/live/34.234.8.175/fullchain.pem
sudo ls -l /etc/letsencrypt/live/34.234.8.175/privkey.pem
```

O certificado de staging pode ser removido depois do teste:

```bash
sudo certbot delete --cert-name empodera-ip-staging --non-interactive
```

### 12.3 Automatizar a renovação

Crie o serviço:

```bash
sudo nano /etc/systemd/system/certbot-renew.service
```

Conteúdo:

```ini
[Unit]
Description=Renovar certificados do Let's Encrypt
After=network-online.target nginx.service

[Service]
Type=oneshot
ExecStart=/usr/local/bin/certbot renew --quiet
ExecStartPost=/usr/bin/systemctl reload nginx
```

Crie o temporizador:

```bash
sudo nano /etc/systemd/system/certbot-renew.timer
```

Conteúdo:

```ini
[Unit]
Description=Verificar renovação do Let's Encrypt quatro vezes por dia

[Timer]
OnCalendar=*-*-* 00,06,12,18:17:00
RandomizedDelaySec=30m
Persistent=true

[Install]
WantedBy=timers.target
```

Ative:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now certbot-renew.timer
sudo systemctl list-timers certbot-renew.timer --all
sudo systemctl start certbot-renew.service
sudo systemctl status certbot-renew.service --no-pager
```

Teste o processo de renovação:

```bash
sudo certbot renew --dry-run --deploy-hook "/usr/bin/systemctl reload nginx"
```

O certificado de IP dura aproximadamente seis dias. O temporizador acima verifica a renovação quatro vezes por dia e recarrega o Nginx depois de uma execução bem-sucedida do Certbot.

---

## 13. Ativar a configuração HTTPS definitiva

Substitua o conteúdo de `/etc/nginx/conf.d/empodera.conf`:

```bash
sudo nano /etc/nginx/conf.d/empodera.conf
```

Use:

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name 34.234.8.175;

    location ^~ /.well-known/acme-challenge/ {
        root /var/www/certbot;
        default_type text/plain;
        try_files $uri =404;
    }

    location / {
        return 301 https://34.234.8.175$request_uri;
    }
}

server {
    listen 443 ssl;
    listen [::]:443 ssl;
    server_name 34.234.8.175;

    ssl_certificate /etc/letsencrypt/live/34.234.8.175/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/34.234.8.175/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 1d;

    client_max_body_size 25m;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;

        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Port 443;

        proxy_connect_timeout 30s;
        proxy_send_timeout 120s;
        proxy_read_timeout 120s;
    }
}
```

Valide antes de recarregar:

```bash
sudo nginx -t
sudo systemctl reload nginx
sudo systemctl status nginx --no-pager
```

---

## 14. Validar a implantação

### 14.1 Serviços

```bash
sudo systemctl is-active mariadb
sudo systemctl is-active empodera
sudo systemctl is-active nginx
sudo systemctl is-active certbot-renew.timer
```

Todos devem retornar `active`.

### 14.2 Portas

```bash
sudo ss -ltnp | grep -E ':80|:443|:5000|:3306'
```

Resultado lógico esperado:

| Porta | Processo | Exposição |
|---:|---|---|
| 80 | Nginx | Pública |
| 443 | Nginx | Pública |
| 5000 | dotnet/Kestrel | Somente `127.0.0.1` |
| 3306 | MariaDB | Uso local; sem regra pública no Security Group |

### 14.3 HTTP, HTTPS e arquivos estáticos

```bash
curl -I http://34.234.8.175
curl -I https://34.234.8.175/Account
curl -I https://34.234.8.175/css/styles.css
curl -I https://34.234.8.175/js/login.js
```

Resultados esperados:

- HTTP retorna `301` para `https://34.234.8.175/...`.
- `/Account` retorna `200`.
- CSS e JavaScript retornam `200`.
- Não deve ser necessário usar `curl -k`; o certificado deve ser confiável.

### 14.4 Banco e aplicação

```bash
mariadb -h 127.0.0.1 -u empodera_app -p empodera \
  -e "SELECT COUNT(*) AS Usuarios FROM Usuarios;"

sudo journalctl -u empodera -n 100 --no-pager
```

Abra no navegador:

```text
https://34.234.8.175
```

Confirme:

1. A página de login abre com CSS.
2. Um usuário ativo consegue entrar com a senha salva no campo `Usuarios.Senha`.
3. A página inicial carrega dados do MariaDB.
4. A troca de idioma funciona.
5. A recuperação de senha gera um link começando com `https://34.234.8.175`.

---

## 15. Atualizar o sistema futuramente

O fluxo de atualização não recompila nada na EC2.

### 15.1 No computador

```powershell
Set-Location C:\Users\melio\Downloads\InsEmpodera
git switch Online
git pull --ff-only origin Online
.\testar.ps1

$Release = Get-Date -Format 'yyyyMMdd-HHmmss'
$PublishDir = Join-Path $PWD "deploy\empodera-$Release"
$Archive = Join-Path $PWD "deploy\empodera-$Release.tar.gz"

dotnet publish .\InsEmpodera.csproj `
  --configuration Release `
  --output $PublishDir `
  --no-self-contained

if (-not (Test-Path "$PublishDir\InsEmpodera.dll")) {
    throw 'InsEmpodera.dll não foi publicado.'
}

if (-not (Test-Path "$PublishDir\wwwroot\css\styles.css")) {
    throw 'O CSS não foi publicado.'
}

tar -czf $Archive -C $PublishDir .

scp -i "C:\caminho\Empodera.pem" `
  $Archive `
  ec2-user@34.234.8.175:/home/ec2-user/

$Release
```

### 15.2 Na EC2

```bash
RELEASE=20260831-230000
RELEASE_DIR="/home/ec2-user/empodera-releases/$RELEASE"
PACKAGE="/home/ec2-user/empodera-$RELEASE.tar.gz"

mkdir -p "$RELEASE_DIR"
tar -xzf "$PACKAGE" -C "$RELEASE_DIR"

test -f "$RELEASE_DIR/InsEmpodera.dll" || exit 1
test -f "$RELEASE_DIR/wwwroot/css/styles.css" || exit 1
test -f "$RELEASE_DIR/wwwroot/js/login.js" || exit 1

if [ -d "$RELEASE_DIR/wwwroot/uploads" ]; then
    cp -an "$RELEASE_DIR/wwwroot/uploads/." /home/ec2-user/empodera-data/uploads/
    mv "$RELEASE_DIR/wwwroot/uploads" "$RELEASE_DIR/wwwroot/uploads-from-package"
fi

ln -s /home/ec2-user/empodera-data/uploads "$RELEASE_DIR/wwwroot/uploads"

PREVIOUS_RELEASE="$(readlink -f /home/ec2-user/empodera-current)"
echo "Versão anterior: $PREVIOUS_RELEASE"

sudo systemctl stop empodera
ln -sfn "$RELEASE_DIR" /home/ec2-user/empodera-current
sudo systemctl start empodera

sudo systemctl status empodera --no-pager
curl -I -H 'Host: 34.234.8.175' http://127.0.0.1:5000/Account
curl -I https://34.234.8.175/css/styles.css
```

Se o serviço e os testes responderem corretamente, a nova versão está ativa. O MariaDB e `/home/ec2-user/empodera-data/uploads` permanecem fora da troca de versão.

---

## 16. Fazer rollback

Liste as versões disponíveis:

```bash
ls -1dt /home/ec2-user/empodera-releases/*
readlink -f /home/ec2-user/empodera-current
```

Escolha a versão anterior e ative-a:

```bash
ROLLBACK_DIR=/home/ec2-user/empodera-releases/VERSAO_ANTERIOR

test -f "$ROLLBACK_DIR/InsEmpodera.dll" || exit 1

sudo systemctl stop empodera
ln -sfn "$ROLLBACK_DIR" /home/ec2-user/empodera-current
sudo systemctl start empodera

sudo systemctl status empodera --no-pager
readlink -f /home/ec2-user/empodera-current
```

O rollback troca apenas os arquivos da aplicação. Ele não desfaz alterações realizadas no banco depois da publicação.

---

## 17. Backup e restauração do MariaDB

### 17.1 Criar um backup

```bash
BACKUP_FILE="/home/ec2-user/backups/empodera-$(date +%Y%m%d-%H%M%S).sql.gz"

sudo mariadb-dump \
  --single-transaction \
  --routines \
  --triggers \
  --default-character-set=utf8mb4 \
  empodera | gzip > "$BACKUP_FILE"

ls -lh "$BACKUP_FILE"
```

Copie para o Windows quando desejar:

```powershell
scp -i "C:\caminho\Empodera.pem" `
  ec2-user@34.234.8.175:/home/ec2-user/backups/NOME_DO_BACKUP.sql.gz `
  .\deploy\
```

### 17.2 Restaurar um backup

Pare a aplicação, importe e inicie novamente:

```bash
sudo systemctl stop empodera
gunzip -c /home/ec2-user/backups/NOME_DO_BACKUP.sql.gz | sudo mariadb empodera
sudo systemctl start empodera
sudo systemctl status empodera --no-pager
```

---

## 18. Diagnóstico de problemas

### 18.1 Comandos principais

```bash
sudo systemctl status empodera --no-pager
sudo journalctl -u empodera -n 200 --no-pager
sudo journalctl -fu empodera

sudo nginx -t
sudo systemctl status nginx --no-pager
sudo tail -n 100 /var/log/nginx/access.log
sudo tail -n 100 /var/log/nginx/error.log

sudo systemctl status mariadb --no-pager
sudo ss -ltnp | grep -E ':80|:443|:5000|:3306'
```

### 18.2 `502 Bad Gateway`

Confira o serviço e a porta:

```bash
sudo systemctl status empodera --no-pager
sudo ss -ltnp | grep ':5000'
curl -I -H 'Host: 34.234.8.175' http://127.0.0.1:5000/Account
```

Se o Kestrel responde localmente, mas o Nginx retorna `502`, confira `/var/log/nginx/error.log`.

Se o SELinux estiver em modo `Enforcing` e o log indicar conexão negada:

```bash
getenforce
sudo setsebool -P httpd_can_network_connect 1
sudo systemctl restart nginx
```

### 18.3 Redirecionamento HTTPS infinito

Confirme estas duas partes:

No `/etc/empodera/empodera.env`:

```ini
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

No bloco HTTPS do Nginx:

```nginx
proxy_set_header X-Forwarded-Proto https;
```

Depois:

```bash
sudo systemctl restart empodera
sudo nginx -t
sudo systemctl reload nginx
```

### 18.4 O site abre, mas o CSS não carrega

Valide em cada camada:

```bash
readlink -f /home/ec2-user/empodera-current
ls -lh /home/ec2-user/empodera-current/wwwroot/css/styles.css

curl -I \
  -H 'Host: 34.234.8.175' \
  http://127.0.0.1:5000/css/styles.css

curl -I https://34.234.8.175/css/styles.css
```

Interpretação:

| Resultado | Causa provável |
|---|---|
| Arquivo não existe | Pacote incompleto ou extraído no diretório errado. |
| Kestrel retorna 404 | `wwwroot` não foi publicado junto da DLL. |
| Kestrel retorna 200, mas Nginx retorna erro | Configuração ou cache do Nginx. |
| Ambos retornam 200, mas o navegador não aplica | Cache do navegador ou caminho incorreto no HTML. |

Após uma atualização dos arquivos estáticos, teste também em janela anônima.

### 18.5 A aplicação não inicia por causa do e-mail

Erro típico:

```text
Defina Email__PublicBaseUrl com a URL HTTPS pública do Empodera.
```

Confira:

```bash
sudo grep -E '^(Email__User|Email__FromName|Email__PublicBaseUrl)=' \
  /etc/empodera/empodera.env
```

O valor correto da URL atual é:

```ini
Email__PublicBaseUrl=https://34.234.8.175
```

Depois de editar:

```bash
sudo systemctl restart empodera
sudo journalctl -u empodera -n 100 --no-pager
```

### 18.6 `Table 'empodera.Usuarios' doesn't exist`

```bash
sudo mariadb empodera -e "SHOW TABLES LIKE '%suarios';"
```

Se aparecer somente `usuarios`, o dump preservou o nome minúsculo do Windows. Em um banco recém-importado, aplique a normalização da seção 6.3 antes de iniciar a aplicação.

As tabelas `diariosprocessopessoal` e `diariosprocessoeixos` são exceções intencionais e permanecem minúsculas.

### 18.7 Login não aceita a senha do usuário

O fluxo normal usa ASP.NET Core Identity:

1. Localiza o usuário pelo e-mail normalizado.
2. Exige `Ativo = 'S'`.
3. Verifica o hash armazenado em `Usuarios.Senha`.
4. Considera bloqueio e contagem de tentativas.

Confira um usuário:

```sql
SELECT
    IdUsuario,
    Email,
    NormalizedEmail,
    Ativo,
    AccessFailedCount,
    LockoutEnd,
    CHAR_LENGTH(Senha) AS TamanhoHash
FROM Usuarios
WHERE LOWER(Email) = LOWER('usuario@email.com');
```

`Testing__AdminPassword` não é uma senha universal. O código só lê essa variável quando `ASPNETCORE_ENVIRONMENT=Testing` e, nesse ambiente, substitui o hash do usuário de ID 1 durante a inicialização. O serviço de produção deve continuar com:

```ini
ASPNETCORE_ENVIRONMENT=Production
```

### 18.8 O certificado não renova

Confira o temporizador e os logs:

```bash
sudo systemctl status certbot-renew.timer --no-pager
sudo systemctl list-timers certbot-renew.timer --all
sudo journalctl -u certbot-renew.service -n 100 --no-pager
sudo certbot certificates
```

Se o certificado já estiver vencido, reemita imediatamente usando o mesmo nome e o mesmo webroot:

```bash
sudo certbot certonly \
  --preferred-profile shortlived \
  --webroot \
  --webroot-path /var/www/certbot \
  --ip-address 34.234.8.175 \
  --cert-name 34.234.8.175 \
  --force-renewal

sudo nginx -t
sudo systemctl reload nginx
```

Confira tanto o arquivo local quanto o certificado que o Nginx está entregando:

```bash
sudo openssl x509 \
  -in /etc/letsencrypt/live/34.234.8.175/fullchain.pem \
  -noout -issuer -dates -ext subjectAltName

echo | openssl s_client \
  -connect 34.234.8.175:443 \
  -servername 34.234.8.175 2>/dev/null | \
  openssl x509 -noout -issuer -dates -ext subjectAltName
```

Em certificados emitidos diretamente para IP, o campo CN pode estar ausente. A identidade esperada aparece em `Subject Alternative Name` como `IP Address:34.234.8.175`.

Confirme que o desafio continua público:

```bash
printf 'renew-novo' | sudo tee /var/www/certbot/.well-known/acme-challenge/renew-test >/dev/null
curl -fsS http://34.234.8.175/.well-known/acme-challenge/renew-test
echo
```

O último `curl` deve devolver exatamente `renew-novo`. Uma resposta antiga ou diferente confirma que o webroot configurado no Nginx não é `/var/www/certbot`.

### 18.9 A aplicação demora ou trava durante publicação

Não publique na EC2. Execute `dotnet publish` no computador Windows, compacte o diretório e envie o `.tar.gz` pronto. Na EC2, faça apenas extração, ativação do link e reinício do serviço.

---

## 19. Checklist final

### AWS

- [ ] Instância EC2 em execução.
- [ ] IP elástico `34.234.8.175` associado à instância correta.
- [ ] Porta 22 limitada ao IP de administração.
- [ ] Portas 80 e 443 liberadas.
- [ ] Portas 3306 e 5000 não estão públicas.

### Banco

- [ ] MariaDB está `active`.
- [ ] Banco `empodera` existe.
- [ ] Usuário `empodera_app` conecta por `127.0.0.1`.
- [ ] Tabela `Usuarios` existe com a capitalização correta.
- [ ] Tabelas de processo explicitamente mapeadas continuam em minúsculas.
- [ ] Dados esperados foram importados.

### Aplicação

- [ ] Branch `Online` foi publicada.
- [ ] `InsEmpodera.dll` existe na versão ativa.
- [ ] `wwwroot/css/styles.css` existe na versão ativa.
- [ ] `wwwroot/js/login.js` existe na versão ativa.
- [ ] `empodera-current` aponta para a versão correta.
- [ ] `empodera.service` está `active`.
- [ ] Kestrel escuta somente em `127.0.0.1:5000`.

### Configuração

- [ ] `/etc/empodera/empodera.env` existe com permissão `600`.
- [ ] `ASPNETCORE_ENVIRONMENT=Production`.
- [ ] `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`.
- [ ] `DatabaseProvider=MySql`.
- [ ] `Email__PublicBaseUrl=https://34.234.8.175`.
- [ ] Não existe `Testing__AdminPassword` na configuração de produção.

### Nginx e HTTPS

- [ ] `nginx -t` retorna sucesso.
- [ ] Nginx escuta nas portas 80 e 443.
- [ ] HTTP redireciona para HTTPS.
- [ ] O certificado de `34.234.8.175` é confiável.
- [ ] `certbot-renew.timer` está ativo.
- [ ] `/Account`, CSS e JavaScript retornam HTTP 200 por HTTPS.

### Teste funcional

- [ ] Tela de login aparece formatada.
- [ ] Login com usuário ativo funciona.
- [ ] Home apresenta dados do MariaDB.
- [ ] Troca de idioma funciona.
- [ ] Recuperação de senha usa a URL pública correta.
- [ ] A versão anterior continua disponível para rollback.

---

## Referências oficiais

- [AWS — .NET no Amazon Linux 2023](https://docs.aws.amazon.com/linux/al2023/ug/dotnet.html)
- [AWS — Associar um IP elástico a uma instância](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/working-with-eips.html)
- [AWS — Criar e configurar Security Groups](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/creating-security-group.html)
- [Microsoft — Hospedar ASP.NET Core no Linux com Nginx](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-9.0)
- [Microsoft — ASP.NET Core atrás de proxies e balanceadores](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-9.0)
- [Let's Encrypt — Certificados de IP e curta duração no Certbot](https://letsencrypt.org/2026/03/11/shorter-certs-certbot)
- [Let's Encrypt — Disponibilidade geral de certificados de IP](https://letsencrypt.org/2026/01/15/6day-and-ip-general-availability.html)

---

**Resultado esperado:** o Empodera fica disponível em `https://34.234.8.175`, com o Nginx exposto nas portas 80/443, o ASP.NET Core executado pelo systemd em `127.0.0.1:5000`, o MariaDB acessado localmente e as atualizações publicadas no Windows antes de serem enviadas à EC2.
