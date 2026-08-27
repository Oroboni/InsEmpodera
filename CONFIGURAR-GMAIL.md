# Configurar a recuperação de senha pelo Gmail

O Empodera envia os links de recuperação por `empodera.ajuda@gmail.com`, usando o SMTP oficial do Gmail com STARTTLS na porta 587.

## 1. Criar a senha de aplicativo

1. Entre na conta `empodera.ajuda@gmail.com`.
2. Ative a verificação em duas etapas da Conta Google.
3. Abra `https://myaccount.google.com/apppasswords`.
4. Crie uma senha de aplicativo chamada `Empodera`.
5. Copie a senha gerada. Não coloque essa senha em nenhum arquivo do projeto.

## 2. Configurar no computador de desenvolvimento

Execute na pasta do projeto, substituindo o valor indicado pela senha de aplicativo:

```powershell
dotnet user-secrets set "Email:Password" "COLE_A_SENHA_DE_APLICATIVO"
dotnet user-secrets set "Email:PublicBaseUrl" "https://localhost:7121"
```

Os segredos ficam fora do repositório e não são enviados ao GitHub.

## 3. Configurar no servidor

Cadastre estas variáveis no painel da hospedagem ou no cofre de segredos:

```text
Email__User=empodera.ajuda@gmail.com
Email__Password=SENHA_DE_APLICATIVO
Email__FromName=Instituto Empodera
Email__PublicBaseUrl=https://ENDERECO_PUBLICO_DO_SISTEMA
```

`Email__PublicBaseUrl` deve ser o endereço HTTPS real do Empodera, sem caminhos adicionais. Em produção, a aplicação não inicia se a senha ou a URL HTTPS estiverem ausentes.

## 4. Revogar uma credencial

Se houver suspeita de exposição, remova imediatamente a senha de aplicativo na Conta Google e crie outra. Não é necessário alterar a senha principal do Gmail quando somente a senha de aplicativo foi comprometida e revogada.
