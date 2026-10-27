# Segurança: dados pessoais e credenciais no repositório

[← Voltar ao README principal](../README.md)

O repositório `Oroboni/InsEmpodera` é **público**. Este documento registra o que estava exposto, o que já foi
corrigido e o plano para o que depende da equipe.

## Situação

| Item | Situação |
| --- | --- |
| Planilhas de comunidades em `wwwroot/uploads/` (23 arquivos, com nomes e telefones) | **Corrigido na versão atual:** removidas do controle do Git e ignoradas (`.gitignore`). A aplicação não usa essa pasta |
| Senha fixa do MySQL nos workflows do GitHub Actions | **Corrigido:** usa o *secret* `TEST_MYSQL_PASSWORD` ou uma senha gerada a cada execução |
| Senhas de produção (banco, e-mail) | Fora do repositório: variáveis de ambiente / arquivo `.env`, que está no `.gitignore` |
| `Banco.txt`, `Empodera.db`, pacotes `deploy/` e planilhas **no histórico** | **Ainda expostos** — exigem reescrever o histórico (abaixo) |
| Hashes de senha dos usuários reais (dentro do antigo `Banco.txt`) | Devem ser tratados como comprometidos: troca de senha (abaixo) |

Busca feita em 27/10/2026 em todos os arquivos versionados: nenhuma senha real; só valores de exemplo na
documentação e textos falsos usados nos testes de ocultação de dados.

## Plano para o histórico do Git

Remover um arquivo hoje não o apaga dos commits antigos: qualquer pessoa ainda abre, por exemplo, o `Banco.txt`
de um commit anterior. A correção reescreve o histórico e precisa ser feita **pelo dono do repositório**,
combinada com toda a equipe:

1. **Combinar uma data** em que ninguém tenha trabalho não enviado; enviar tudo antes.
2. **Limpar numa cópia:** `scripts/limpar-historico.sh` (requer `pip install git-filter-repo`). Ele cria uma cópia
   espelho, remove `Banco.txt`, `Empodera.db`, `Empodera.testing.db`, `deploy/`, `wwwroot/uploads/` e `strix_runs/` de
   todos os commits e mostra a conferência. Testado numa cópia local: nenhum desses arquivos sobra no histórico.
3. **Enviar:** `PASTA=<cópia> scripts/limpar-historico.sh --enviar` (força o envio de todas as branches).
4. **Cada pessoa clona de novo** (cópias antigas trazem os dados de volta se alguém enviar).
5. **Pedir ao suporte do GitHub** a remoção das visualizações em cache e das referências de pull requests antigos
   (não podem ser apagadas pelo usuário) e conferir se há *forks*.
6. **Considerar tornar o repositório privado.**

**Critério de aceite:** `git log --all -- Banco.txt wwwroot/uploads` vazio no repositório remoto e o link do arquivo
num commit antigo deixa de abrir no GitHub.

## Senhas dos usuários

Os hashes de senha dos usuários reais estiveram públicos. Mesmo sendo hashes (PBKDF2 do ASP.NET Core Identity),
devem ser tratados como comprometidos: depois da migração para o servidor novo, cada usuário real troca a senha
("Esqueci minha senha" ou pelo administrador em Usuários). Na instalação nova, a senha do administrador é definida
por `SENHA_INICIAL_ADMIN` e trocada no primeiro acesso.

## Regras daqui em diante

- Nunca versionar dumps, bancos (`*.db`), planilhas com dados reais, pacotes de publicação ou o arquivo `.env`.
- Dados reais só em ambiente controlado; para testes, usar dados fictícios.
- Credenciais só em variáveis de ambiente ou *secrets*.
