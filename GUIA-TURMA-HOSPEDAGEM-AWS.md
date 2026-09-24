# Guia da turma — hospedagem de ASP.NET Core na AWS

> Ambiente padronizado: Windows no computador local, Amazon EC2 com Amazon Linux 2023, ASP.NET Core 9, MariaDB 10.5, Nginx, systemd, IP elástico e HTTPS do Let's Encrypt.

**Revisão:** 22 de setembro de 2026

---

## 1. Resultado esperado

```text
Navegador
   |
   | HTTP 80 / HTTPS 443
   v
IP elástico da AWS
   |
   v
Nginx
   |
   | proxy HTTP interno
   v
ASP.NET Core em 127.0.0.1:5000
   |
   v
MariaDB em 127.0.0.1:3306
```

Somente as portas públicas `80` e `443` chegam à aplicação. A porta `22` é usada para administração por SSH. As portas `5000` e `3306` não devem ser abertas no grupo de segurança da AWS.

---

## 2. Padrão obrigatório da turma

| Item | Padrão |
|---|---|
| Compilação e publicação | Windows com PowerShell |
| Nuvem | AWS EC2 |
| Sistema da instância | Amazon Linux 2023 |
| Usuário SSH | `ec2-user` |
| Aplicação | ASP.NET Core 9 |
| Servidor da aplicação | Kestrel |
| Endereço interno | `127.0.0.1:5000` |
| Proxy público | Nginx |
| Banco | MariaDB 10.5 |
| Inicialização automática | systemd |
| Endereço público | IP elástico |
| HTTPS | Let's Encrypt para o próprio IP |
| Perfil do certificado | `shortlived` |
| Pasta do desafio ACME | `/var/www/certbot` |

O guia é genérico somente nos nomes e endereços. Durante a atividade, não troque Amazon Linux por Ubuntu, MariaDB por SQLite, Nginx por IIS nem faça a compilação principal dentro da EC2.

---

## 3. Dados de cada aluno

Preencha antes de começar:

| Campo | Exemplo | Seu valor |
|---|---|---|
| Nome curto | `minhaapp` | |
| Projeto | `MinhaApp.csproj` | |
| DLL principal | `MinhaApp.dll` | |
| Banco | `minhaapp` | |
| Usuário do banco | `minhaapp_app` | |
| IP elástico | `203.0.113.10` | |
| Chave no Windows | `C:/Users/Aluno/Downloads/turma.pem` | |
| E-mail do certificado | `aluno@exemplo.com` | |

Substitua nos comandos:

- `<APP>`: nome curto, em minúsculas e sem espaços;
- `<PROJETO>`: arquivo `.csproj`;
- `<DLL>`: DLL principal;
- `<BANCO>`: banco da aplicação;
- `<USUARIO_BANCO>`: usuário do banco;
- `<SENHA_BANCO>`: senha escolhida;
- `<IP_ELASTICO>`: IP elástico associado à instância;
- `<CHAVE_PEM>`: caminho completo da chave;
- `<EMAIL>`: e-mail do Let's Encrypt.

Os sinais `<` e `>` são apenas marcadores. Não devem permanecer no comando final.

---

## 4. Criar a infraestrutura na AWS

### 4.1. Instância EC2

No console da AWS:

1. Abra **EC2 > Instâncias > Executar instâncias**.
2. Escolha **Amazon Linux 2023**.
3. Use o tipo de instância definido pelo professor, por exemplo `t3.micro`.
4. Crie ou selecione um par de chaves `.pem`.
5. Use pelo menos 15 GiB de armazenamento.
6. Associe um grupo de segurança.

### 4.2. Grupo de segurança

| Tipo | Porta | Origem |
|---|---:|---|
| SSH | 22 | Meu IP |
| HTTP | 80 | `0.0.0.0/0` |
| HTTPS | 443 | `0.0.0.0/0` |

Não crie regras públicas para `3306` ou `5000`.

### 4.3. IP elástico

1. Abra **EC2 > Rede e segurança > IPs elásticos**.
2. Aloque um IP.
3. Associe-o à instância.
4. Anote o endereço em `<IP_ELASTICO>`.

O certificado será emitido para esse endereço. Se o IP mudar, será necessário emitir outro certificado.

---

## 5. Conectar por SSH

No PowerShell do Windows:

```powershell
ssh -i "<CHAVE_PEM>" ec2-user@<IP_ELASTICO>
```

Na primeira conexão, confirme digitando `yes`.

```bash
whoami
cat /etc/os-release
```

O resultado deve indicar `ec2-user` e Amazon Linux 2023.

---

## 6. Instalar o ambiente

Na EC2:

```bash
sudo dnf upgrade -y
sudo dnf install -y aspnetcore-runtime-9.0 nginx mariadb105-server python3 python3-pip
sudo systemctl enable --now mariadb
sudo systemctl enable --now nginx
```

Valide:

```bash
dotnet --list-runtimes
nginx -v
mariadb --version
sudo systemctl is-active nginx
sudo systemctl is-active mariadb
```

O runtime `Microsoft.AspNetCore.App 9.x` deve aparecer. Nginx e MariaDB devem responder `active`.

---

## 7. Criar o banco

```bash
sudo mariadb
```

```sql
CREATE DATABASE <BANCO>
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE USER '<USUARIO_BANCO>'@'localhost'
  IDENTIFIED BY '<SENHA_BANCO>';

CREATE USER '<USUARIO_BANCO>'@'127.0.0.1'
  IDENTIFIED BY '<SENHA_BANCO>';

GRANT ALL PRIVILEGES ON <BANCO>.* TO '<USUARIO_BANCO>'@'localhost';
GRANT ALL PRIVILEGES ON <BANCO>.* TO '<USUARIO_BANCO>'@'127.0.0.1';

FLUSH PRIVILEGES;
EXIT;
```

Teste:

```bash
mariadb -h 127.0.0.1 -u <USUARIO_BANCO> -p <BANCO>
```

Após entrar:

```sql
SELECT DATABASE();
EXIT;
```

### 7.1. Criar as tabelas

Se o projeto usa um arquivo SQL, envie pelo Windows:

```powershell
scp -i "<CHAVE_PEM>" "C:/caminho/banco.sql" ec2-user@<IP_ELASTICO>:/home/ec2-user/banco.sql
```

Importe na EC2:

```bash
mariadb -h 127.0.0.1 -u <USUARIO_BANCO> -p <BANCO> < /home/ec2-user/banco.sql
```

Se o projeto usa migrations do Entity Framework, aplique-as conforme o método definido no projeto. A aplicação não deve exigir o SDK completo do .NET no servidor apenas para iniciar.

```bash
mariadb -h 127.0.0.1 -u <USUARIO_BANCO> -p -e "USE <BANCO>; SHOW TABLES;"
```

---

## 8. Preparar o ASP.NET Core para o proxy

O Nginx recebe HTTPS e encaminha a requisição por HTTP local. No `Program.cs`, configure os cabeçalhos encaminhados:

```csharp
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();
```

`app.UseForwardedHeaders()` deve ficar antes de autenticação, redirecionamentos e outros componentes que dependam do esquema da requisição.

Confirme também:

```csharp
app.UseStaticFiles();
```

Sem `UseStaticFiles`, o site pode abrir sem CSS, JavaScript ou imagens da pasta `wwwroot`.

---

## 9. Publicar no Windows

Abra o PowerShell na pasta do projeto:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish "./<PROJETO>" -c Release -o "./publish-linux" --no-self-contained
```

Confira:

```powershell
Test-Path "./publish-linux/<DLL>"
Test-Path "./publish-linux/wwwroot"
```

Os dois resultados devem ser `True`.

Compacte e envie:

```powershell
tar -czf "./<APP>-publish.tar.gz" -C "./publish-linux" .
scp -i "<CHAVE_PEM>" "./<APP>-publish.tar.gz" ec2-user@<IP_ELASTICO>:/home/ec2-user/
```

---

## 10. Instalar a primeira release

Na EC2:

```bash
APP=<APP>
VERSAO=$(date +%Y%m%d-%H%M%S)
RELEASE="/home/ec2-user/${APP}-releases/${VERSAO}"

mkdir -p "${RELEASE}"
tar -xzf "/home/ec2-user/${APP}-publish.tar.gz" -C "${RELEASE}"
test -f "${RELEASE}/<DLL>"
test -d "${RELEASE}/wwwroot"
ln -sfn "${RELEASE}" "/home/ec2-user/${APP}-current"
```

Estrutura final:

```text
/home/ec2-user/<APP>-releases/20260922-120000/
/home/ec2-user/<APP>-current -> release ativa
```

Se houver uploads, guarde-os fora da release e use um link simbólico. Assim, uma nova publicação não elimina arquivos enviados pelos usuários.

---

## 11. Criar o arquivo de ambiente

```bash
sudo mkdir -p /etc/<APP>
sudo tee /etc/<APP>/<APP>.env >/dev/null <<'EOF'
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:5000
ConnectionStrings__DefaultConnection=Server=127.0.0.1;Port=3306;Database=<BANCO>;User=<USUARIO_BANCO>;Password=<SENHA_BANCO>;ConvertZeroDateTime=True;
Email__PublicBaseUrl=https://<IP_ELASTICO>
EOF

sudo chown root:root /etc/<APP>/<APP>.env
sudo chmod 600 /etc/<APP>/<APP>.env
```

Acrescente ao arquivo outras variáveis exigidas pelo projeto, como credenciais de e-mail ou conta administrativa. Não coloque valores reais no repositório ou em capturas de tela.

---

## 12. Criar o serviço systemd

```bash
sudo tee /etc/systemd/system/<APP>.service >/dev/null <<'EOF'
[Unit]
Description=Aplicacao ASP.NET Core <APP>
After=network-online.target mariadb.service
Wants=network-online.target

[Service]
WorkingDirectory=/home/ec2-user/<APP>-current
ExecStart=/usr/bin/dotnet /home/ec2-user/<APP>-current/<DLL>
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=<APP>
User=ec2-user
EnvironmentFile=/etc/<APP>/<APP>.env

[Install]
WantedBy=multi-user.target
EOF

sudo systemd-analyze verify /etc/systemd/system/<APP>.service
sudo systemctl daemon-reload
sudo systemctl enable --now <APP>
sudo systemctl status <APP> --no-pager
```

Teste o Kestrel:

```bash
curl -I http://127.0.0.1:5000/
sudo ss -ltnp | grep 5000
```

Ele deve escutar somente em `127.0.0.1:5000`.

Se falhar:

```bash
sudo journalctl -u <APP> -n 150 --no-pager
```

---

## 13. Configurar o Nginx em HTTP

Crie a pasta do desafio do Let's Encrypt:

```bash
sudo mkdir -p /var/www/certbot/.well-known/acme-challenge
```

Crie a configuração:

```bash
sudo tee /etc/nginx/conf.d/<APP>.conf >/dev/null <<'EOF'
server {
    listen 80;
    listen [::]:80;
    server_name <IP_ELASTICO>;

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
    }
}
EOF

sudo nginx -t
sudo systemctl reload nginx
```

Teste:

```bash
curl -I http://<IP_ELASTICO>/
```

### 13.1. Provar o caminho do desafio

```bash
printf 'acme-ok' | sudo tee /var/www/certbot/.well-known/acme-challenge/teste >/dev/null
curl -fsS http://<IP_ELASTICO>/.well-known/acme-challenge/teste
echo
```

A resposta precisa ser exatamente:

```text
acme-ok
```

Se aparecer outro texto ou erro `404`, não prossiga. O Nginx e o Certbot precisam usar exatamente a mesma pasta: `/var/www/certbot`.

---

## 14. Instalar o Certbot

Certificados para IP exigem Certbot 5.4 ou superior. Instale-o em um ambiente próprio:

```bash
sudo python3 -m venv /opt/certbot
sudo /opt/certbot/bin/pip install --upgrade pip
sudo /opt/certbot/bin/pip install "certbot>=5.4"
sudo ln -sfn /opt/certbot/bin/certbot /usr/local/bin/certbot
```

Confira:

```bash
command -v certbot
certbot --version
```

O caminho esperado é `/usr/local/bin/certbot`.

---

## 15. Emitir o certificado para o IP

Primeiro, teste sem solicitar um certificado público:

```bash
sudo certbot certonly --staging --preferred-profile shortlived --webroot --webroot-path /var/www/certbot --ip-address <IP_ELASTICO> --cert-name <IP_ELASTICO> --email <EMAIL> --agree-tos --no-eff-email
```

Se o teste funcionar, emita o certificado público:

```bash
sudo certbot certonly --preferred-profile shortlived --webroot --webroot-path /var/www/certbot --ip-address <IP_ELASTICO> --cert-name <IP_ELASTICO> --force-renewal --email <EMAIL> --agree-tos --no-eff-email
```

Arquivos gerados:

```text
/etc/letsencrypt/live/<IP_ELASTICO>/fullchain.pem
/etc/letsencrypt/live/<IP_ELASTICO>/privkey.pem
```

Valide:

```bash
sudo openssl x509 -in /etc/letsencrypt/live/<IP_ELASTICO>/fullchain.pem -noout -issuer -dates -ext subjectAltName
```

O `subjectAltName` deve conter `IP Address:<IP_ELASTICO>`. O `--force-renewal` substitui o certificado de teste pelo certificado público.

O certificado de IP usa o perfil `shortlived` e vale pouco mais de seis dias. A renovação automática da seção 17 é obrigatória para manter o site disponível em HTTPS.

---

## 16. Ativar HTTPS no Nginx

Substitua a configuração:

```bash
sudo tee /etc/nginx/conf.d/<APP>.conf >/dev/null <<'EOF'
server {
    listen 80;
    listen [::]:80;
    server_name <IP_ELASTICO>;

    location ^~ /.well-known/acme-challenge/ {
        root /var/www/certbot;
        default_type text/plain;
        try_files $uri =404;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

server {
    listen 443 ssl;
    listen [::]:443 ssl;
    server_name <IP_ELASTICO>;

    ssl_certificate /etc/letsencrypt/live/<IP_ELASTICO>/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/<IP_ELASTICO>/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
EOF

sudo nginx -t
sudo systemctl reload nginx
```

Confira as portas e as respostas:

```bash
sudo ss -ltnp | grep -E ':80|:443|:5000'
curl -I http://<IP_ELASTICO>/
curl -I https://<IP_ELASTICO>/
```

O HTTP deve redirecionar para HTTPS. O HTTPS deve responder sem erro de certificado.

---

## 17. Criar a renovação automática

Crie o serviço:

```bash
sudo tee /etc/systemd/system/certbot-renew.service >/dev/null <<'EOF'
[Unit]
Description=Renovar certificados do Let's Encrypt
Wants=network-online.target
After=network-online.target nginx.service

[Service]
Type=oneshot
ExecStart=/usr/local/bin/certbot renew --quiet --deploy-hook "/usr/bin/systemctl reload nginx"
EOF
```

Crie o temporizador:

```bash
sudo tee /etc/systemd/system/certbot-renew.timer >/dev/null <<'EOF'
[Unit]
Description=Verificar renovacao do Let's Encrypt quatro vezes por dia

[Timer]
OnCalendar=*-*-* 00,06,12,18:17:00
RandomizedDelaySec=30m
Persistent=true

[Install]
WantedBy=timers.target
EOF
```

Valide e ative:

```bash
sudo systemd-analyze verify /etc/systemd/system/certbot-renew.service /etc/systemd/system/certbot-renew.timer
sudo systemctl daemon-reload
sudo systemctl enable --now certbot-renew.timer
sudo systemctl is-enabled certbot-renew.timer
sudo systemctl is-active certbot-renew.timer
sudo systemctl list-timers certbot-renew.timer --all
```

Resultados esperados:

```text
enabled
active
```

Teste o serviço:

```bash
sudo systemctl start certbot-renew.service
sudo systemctl show certbot-renew.service -p Result -p ExecMainStatus
sudo journalctl -u certbot-renew.service -n 100 --no-pager
```

Execução correta:

```text
Result=success
ExecMainStatus=0
```

O serviço é `oneshot` e pode aparecer como `inactive (dead)` depois de concluir. Isso é normal. Quem deve permanecer `active` é `certbot-renew.timer`.

Se aparecer `Unit certbot-renew.service not found` ou `Unit certbot-renew.timer not found`, repita esta seção inteira e execute `sudo systemctl daemon-reload`.

---

## 18. Validar toda a hospedagem

### 18.1. Serviços

```bash
sudo systemctl is-active mariadb
sudo systemctl is-active <APP>
sudo systemctl is-active nginx
sudo systemctl is-active certbot-renew.timer
```

Todos devem responder `active`.

### 18.2. Portas

```bash
sudo ss -ltnp | grep -E ':80|:443|:5000|:3306'
```

Confirme:

- Nginx em `0.0.0.0:80` e `0.0.0.0:443`;
- Kestrel em `127.0.0.1:5000`;
- MariaDB acessível localmente e sem regra pública na AWS.

### 18.3. Site e arquivos estáticos

```bash
curl -I http://<IP_ELASTICO>/
curl -I https://<IP_ELASTICO>/
curl -I https://<IP_ELASTICO>/css/site.css
```

Se o CSS possui outro nome, use a URL que aparece no HTML da aplicação.

### 18.4. Certificado servido

```bash
echo | openssl s_client -connect <IP_ELASTICO>:443 -servername <IP_ELASTICO> 2>/dev/null | openssl x509 -noout -issuer -dates -ext subjectAltName
```

O resultado deve mostrar o IP correto no SAN.

### 18.5. Teste no navegador

1. Abra `https://<IP_ELASTICO>`.
2. Confirme que o navegador aceita o certificado.
3. Confira CSS, JavaScript e imagens.
4. Teste cadastro ou login.
5. Faça uma operação que leia o banco.
6. Faça uma operação que grave no banco.
7. Se houver e-mail, confira se os links usam `https://<IP_ELASTICO>`.

---

## 19. Publicar uma nova versão

No Windows:

```powershell
Remove-Item "./publish-linux" -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish "./<PROJETO>" -c Release -o "./publish-linux" --no-self-contained
tar -czf "./<APP>-publish.tar.gz" -C "./publish-linux" .
scp -i "<CHAVE_PEM>" "./<APP>-publish.tar.gz" ec2-user@<IP_ELASTICO>:/home/ec2-user/
```

Na EC2:

```bash
APP=<APP>
VERSAO=$(date +%Y%m%d-%H%M%S)
RELEASE="/home/ec2-user/${APP}-releases/${VERSAO}"

mkdir -p "${RELEASE}"
tar -xzf "/home/ec2-user/${APP}-publish.tar.gz" -C "${RELEASE}"
test -f "${RELEASE}/<DLL>"
test -d "${RELEASE}/wwwroot"
ln -sfn "${RELEASE}" "/home/ec2-user/${APP}-current"
sudo systemctl restart "${APP}"
sudo systemctl is-active "${APP}"
curl -I http://127.0.0.1:5000/
```

Depois, teste o endereço HTTPS no navegador.

### 19.1. Rollback

```bash
ls -1dt /home/ec2-user/<APP>-releases/*
ln -sfn /home/ec2-user/<APP>-releases/<VERSAO_ANTERIOR> /home/ec2-user/<APP>-current
sudo systemctl restart <APP>
```

---

## 20. Diagnóstico rápido

### 502 Bad Gateway

```bash
sudo systemctl status <APP> --no-pager
sudo journalctl -u <APP> -n 150 --no-pager
curl -I http://127.0.0.1:5000/
```

### CSS ou JavaScript não carrega

```bash
test -d /home/ec2-user/<APP>-current/wwwroot
find /home/ec2-user/<APP>-current/wwwroot -maxdepth 2 -type f | head
curl -I https://<IP_ELASTICO>/css/site.css
```

Confira também `app.UseStaticFiles()`.

### Certbot responde unauthorized ou 404

```bash
printf 'desafio-ok' | sudo tee /var/www/certbot/.well-known/acme-challenge/teste >/dev/null
curl -fsS http://<IP_ELASTICO>/.well-known/acme-challenge/teste
echo
sudo nginx -T
```

A causa mais comum é o Nginx servir uma pasta diferente de `/var/www/certbot`.

### Porta 443 não conecta

```bash
sudo nginx -t
sudo systemctl status nginx --no-pager
sudo ss -ltnp | grep 443
```

Confira também a regra HTTPS na AWS.

### Aplicação não conecta ao banco

```bash
sudo systemctl status mariadb --no-pager
mariadb -h 127.0.0.1 -u <USUARIO_BANCO> -p <BANCO>
sudo journalctl -u <APP> -n 150 --no-pager
```

### Redirecionamento infinito ou links HTTP

Confira os cabeçalhos do Nginx e a posição de `app.UseForwardedHeaders()` no `Program.cs`.

### Timer não existe

```bash
sudo systemctl cat certbot-renew.service
sudo systemctl cat certbot-renew.timer
sudo systemctl daemon-reload
sudo systemctl enable --now certbot-renew.timer
```

Se as unidades não existirem, refaça a seção 17.

---

## 21. Checklist de entrega

- [ ] Amazon Linux 2023.
- [ ] IP elástico associado.
- [ ] SSH na porta 22 restrito ao IP do aluno.
- [ ] Portas 80 e 443 abertas.
- [ ] Portas 3306 e 5000 não públicas.
- [ ] ASP.NET Core Runtime 9 instalado.
- [ ] MariaDB ativo, com banco e tabelas.
- [ ] Aplicação instalada em uma pasta de release.
- [ ] Link `<APP>-current` apontando para a release ativa.
- [ ] Variáveis de produção fora do repositório.
- [ ] Serviço da aplicação `enabled` e `active`.
- [ ] Kestrel somente em `127.0.0.1:5000`.
- [ ] Nginx nas portas 80 e 443.
- [ ] HTTP redirecionando para HTTPS.
- [ ] Certificado com o IP correto no SAN.
- [ ] CSS e JavaScript carregando.
- [ ] Leitura e gravação no MariaDB testadas.
- [ ] Login ou cadastro testado.
- [ ] `Email__PublicBaseUrl` com o endereço HTTPS.
- [ ] `certbot-renew.timer` `enabled` e `active`.
- [ ] Renovação manual com `Result=success`.
- [ ] Rollback disponível.

---

## 22. Evidências para entregar

Entregue apenas saídas sem senhas:

1. IP elástico associado à instância.
2. Regras do grupo de segurança.
3. Estado de MariaDB, aplicação, Nginx e timer.
4. Portas 80, 443 e 5000.
5. Respostas HTTP e HTTPS.
6. SAN e validade do certificado.
7. Página no navegador com CSS.
8. Uma leitura e uma gravação no sistema.
9. Lista do timer de renovação.

Não entregue o conteúdo de `/etc/<APP>/<APP>.env`, senhas, tokens, chaves privadas ou o arquivo `.pem`.

---

## 23. Referências oficiais

- [Hospedar ASP.NET Core no Linux com Nginx — Microsoft](https://learn.microsoft.com/aspnet/core/host-and-deploy/linux-nginx)
- [Configurar ASP.NET Core para proxies — Microsoft](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer)
- [Grupos de segurança do EC2 — AWS](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/ec2-security-groups.html)
- [Regras de grupos de segurança — AWS](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/security-group-rules-reference.html)
- [Certificados de seis dias e para IP — Let's Encrypt](https://letsencrypt.org/2026/03/11/shorter-certs-certbot/)
- [Documentação do Certbot](https://eff-certbot.readthedocs.io/)
