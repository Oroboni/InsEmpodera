# EcoRedes na AWS — prontidão para produção

Como o EcoRedes atende aos requisitos do **PI de Computação em Nuvem** no **AWS Academy (Learner Lab)**.
Onde o Academy limita algo, a seção mostra **como está no Academy** e **como seria numa conta AWS real**.

## Arquitetura

```text
 Navegador ──HTTPS (Let's Encrypt)──▶ IP elástico 44.215.85.75
                                         │
              EC2 ecoredes-prod-web (Amazon Linux 2023, t3.small) ─ LabInstanceProfile (LabRole)
              ├─ Nginx :443 (certificado renovado pelo certbot) ──▶ contêiner "app" ASP.NET Core 10 (127.0.0.1:8080)
              │                                                     └─▶ contêiner "mysql" MySQL 8.4 (rede interna, sem porta pública)
              │                                                           └─ volume Docker ecoredes_mysql-dados
              └─ Docker awslogs ──▶ Amazon CloudWatch Logs (/ecoredes/app, /ecoredes/mysql)

 Amazon RDS for MySQL 8.4 (ecoredes-prod-db): banco da versão anterior, mantido como histórico; só aceita conexões da EC2.

 GitHub (main) ──push──▶ GitHub Actions: CI (build + testes) ──passou──▶ CD: a EC2 detecta (timer de 5 min) → git pull → docker compose up
```

| Peça | Serviço AWS | Arquivo no repositório |
|---|---|---|
| Aplicação e banco | Amazon EC2 + Docker Compose | `compose.yaml`, `compose.aws.yaml`, `Dockerfile` |
| HTTPS público | Nginx + Let's Encrypt no IP elástico (CloudFront bloqueado no Academy) | `/etc/nginx/conf.d/empodera.conf` na EC2 |
| Logs | Amazon CloudWatch Logs | `compose.aws.yaml`, `Services/RegistroDeOperacoes.cs` |
| Permissões | IAM (LabRole via LabInstanceProfile) | — |
| CI / CD | GitHub Actions | `.github/workflows/testes.yml`, `.github/workflows/deploy-aws.yml` |
| Atualização do servidor (CD) | — | `deploy/aws/atualizar.sh`, `deploy/aws/deploy-automatico.sh`, `deploy/aws/ecoredes-deploy.timer` |
| Servidor novo (Ubuntu + CloudFront), se a conta permitir | — | `deploy/aws/preparar-ec2.sh`, `deploy/aws/nginx-ecoredes.conf` |

## 1. Aplicação implantada na AWS

Frontend (páginas MVC), backend, banco MySQL e armazenamento (volumes) rodam na EC2 `ecoredes-prod-web`. Nada depende
do computador de um integrante: os contêineres têm `restart: unless-stopped` e o Docker sobe com a instância.

- **Academy:** a sessão do laboratório termina e a EC2 para; ao iniciar o lab de novo, a instância e os contêineres voltam
  sozinhos, e o **IP elástico** mantém o endereço `44.215.85.75`.
- **Conta real:** banco no **Amazon RDS** (já existe `ecoredes-prod-db`, MySQL 8.4, acessível só pelo Security Group da EC2;
  em produção, Multi-AZ e backups automáticos), aplicação em **ECS Fargate** ou **EC2 com Auto Scaling** atrás de um
  **Application Load Balancer**, imagens no **Amazon ECR**.

## 2. HTTPS

Todo acesso público é por **`https://44.215.85.75`**: o Nginx redireciona HTTP → HTTPS (301) e usa um certificado
**Let's Encrypt emitido para o IP elástico**, renovado automaticamente pelo `certbot-renew.timer`. A aplicação envia
**HSTS** (1 ano) e cookies `Secure`; o Nginx não informa a versão (`server_tokens off`).

- **Limitação do Academy:** o **Amazon CloudFront** está bloqueado para a role do laboratório
  (`AccessDenied` em `cloudfront:ListDistributions`/`CreateDistribution`), e sem domínio próprio não há certificado do ACM.
- **Conta real:** domínio no **Route 53**, certificado do **ACM** no **CloudFront** ou num **ALB**, e a EC2 aceitando a porta 80
  só da lista `com.amazonaws.global.cloudfront.origin-facing` (configuração pronta em `deploy/aws/nginx-ecoredes.conf`).

## 3. Código no GitHub

Repositório: `https://github.com/Oroboni/InsEmpodera` (branch `main`). Contém código, documentação (`docs/`),
configuração (`compose*.yaml`, `Dockerfile`, `deploy/aws/`), CI/CD (`.github/workflows/`), `README.md` e esta arquitetura.
**Não versiona** senhas, tokens, Access Keys nem credenciais de banco: o `.env` está no `.gitignore` e o repositório só traz
o `.env.example` com valores de exemplo.

## 4. Segredos e configuração

| Segredo | Onde fica |
|---|---|
| Senhas do MySQL, senha inicial do admin, senha do e-mail | `/opt/ecoredes/.env` na EC2 (`chmod 600`, fora do Git) → variáveis de ambiente dos contêineres |
| Chave SSH do deploy | GitHub → *Settings › Secrets and variables › Actions* (`AWS_EC2_CHAVE_SSH`, com `AWS_EC2_HOST` e `AWS_EC2_USUARIO`) |
| Credenciais AWS para os logs | Nenhuma: o Docker usa o **perfil da instância** (LabInstanceProfile) |
| Chaves de proteção de dados (login) | Volume Docker `chaves` |

- **Conta real:** senhas no **AWS Secrets Manager** (rotação automática) ou **SSM Parameter Store** (SecureString),
  lidas pela aplicação com a role da instância/tarefa.

## 5. IAM e limites do AWS Academy

**Academy:** não é possível criar usuários, roles nem políticas IAM. A EC2 usa o perfil **LabInstanceProfile**
(role **LabRole**), que tem permissões amplas.

Fluxo no Academy: `EC2 → LabRole → CloudWatch Logs`

Fluxo em produção: `EC2 → role própria "ecoredes-ec2" → só as permissões abaixo → CloudWatch Logs`

**Quais permissões essa aplicação realmente precisaria?** Apenas gravar os próprios logs (menor privilégio):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "LogsDoEcoRedes",
      "Effect": "Allow",
      "Action": ["logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents", "logs:DescribeLogStreams"],
      "Resource": "arn:aws:logs:us-east-1:<conta>:log-group:/ecoredes/*"
    }
  ]
}
```

Com RDS/Secrets Manager seriam somadas só `secretsmanager:GetSecretValue` no segredo do banco
(`arn:aws:secretsmanager:us-east-1:<conta>:secret:ecoredes/*`). Para o deploy, o GitHub Actions usaria **OIDC**
(sem chave fixa) com uma role que só pode executar `ssm:SendCommand` nesta instância.

## 6. CI — GitHub Actions

`.github/workflows/testes.yml` roda em todo push e pull request: dependências → build → testes (244 testes de backend
contra um MySQL 8.4 real, além dos testes de banco e chave estrangeira). Se algo falha, a execução fica vermelha e o
deploy **não** acontece.

## 7. CD — deploy automático

**Ativo na AWS (modelo "pull", sem segredos):** a EC2 roda o timer do systemd `ecoredes-deploy.timer` a cada 5 minutos.
O `deploy/aws/deploy-automatico.sh` busca a `main`; se há commit novo **e** o CI *Testes de backend e banco* daquele
commit terminou com sucesso (consulta pública à API do GitHub Actions), executa `deploy/aws/atualizar.sh`
(git pull → `docker compose up -d --build` → confere se a aplicação responde). Commit com teste falhando não é publicado.

```text
Merge na main → GitHub Actions (CI: build + testes) → sucesso → EC2 detecta em até 5 min → build → deploy
```

Histórico: `journalctl -u ecoredes-deploy` na EC2 (linhas `INFO deploy:` / `WARN deploy:`).

- **Limitação:** o modelo "push" (`.github/workflows/deploy-aws.yml`, GitHub entrando na EC2 por SSH) exige cadastrar a chave
  nos *Secrets* do repositório, o que só o dono do repositório pode fazer; o workflow fica pronto e pula o deploy enquanto
  os segredos não existem. Com o lab desligado, a EC2 publica a versão pendente assim que volta (timer com `Persistent=true`).
- **Conta real:** GitHub OIDC → role IAM com `ssm:SendCommand` (sem porta 22 aberta), ou imagem no ECR + deploy no ECS.

## 8. CloudWatch Logs

O Docker envia a saída dos contêineres ao CloudWatch (driver `awslogs`): grupos **/ecoredes/app** e **/ecoredes/mysql**,
uma linha por registro, com data e nível:

```text
2026-11-13 18:02:11 info: Empodera.Services.RegistroDeOperacoes[0] Operação concluída DiarioCampo/Create usuário=1
2026-11-13 18:03:40 warn: Empodera.Services.RegistroDeOperacoes[0] Formulário recusado FichaPrimeiroContato/Create usuário=1 campos=DtContato
2026-11-13 18:05:02 fail: Empodera.Controllers.ErroController[0] Erro não tratado em /Atividades/Create
```

- **INFO:** operação concluída (cadastro, edição, exclusão).
- **WARN:** situação inesperada (formulário recusado, acesso negado, login inválido).
- **ERROR:** falha ao processar (exceções, com o código mostrado ao usuário na página de erro).

**Nunca registramos:** senhas, tokens, chaves, cookies, conteúdo dos formulários, e-mails, telefones nem dados pessoais
das comunidades. O log leva só a tela, a ação, o id do usuário, os *nomes* dos campos recusados e o resultado.

## Como verificar (roteiro de apresentação)

1. Abrir `https://44.215.85.75` → cadeado HTTPS → login; `http://44.215.85.75` redireciona para HTTPS.
2. EC2 › Instâncias → `ecoredes-prod-web`, IP elástico, perfil **LabInstanceProfile**; RDS › `ecoredes-prod-db` (acesso só da EC2).
3. CloudWatch › Log groups › `/ecoredes/app` → criar um diário e ver a linha `Operação concluída`.
4. GitHub › Actions → execução verde de *Testes de backend e banco*; na EC2, `journalctl -u ecoredes-deploy` mostra a publicação.
5. GitHub › Settings › Secrets → segredos do deploy existem, mas o valor não aparece; `.env` não está no repositório.
