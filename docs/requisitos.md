# Requisitos do InsEmpodera

[← Voltar ao README principal](../README.md)

Este documento descreve os requisitos identificados no código da branch `Online`. A coluna **Situação** indica o que foi comprovado no código e nos testes:

| Situação | Significado |
| --- | --- |
| Implementado | Existe código que realiza o comportamento descrito |
| Parcial | O comportamento existe, mas com restrições ou lacunas documentadas |
| Planejado | Desejado, porém sem implementação no repositório |

## Sumário

- [Atores do sistema](#atores-do-sistema)
- [Requisitos funcionais](#requisitos-funcionais)
- [Requisitos não funcionais](#requisitos-não-funcionais)
- [Regras de negócio](#regras-de-negócio)
- [Limitações conhecidas](#limitações-conhecidas)

## Atores do sistema

| Ator | Descrição |
| --- | --- |
| Visitante | Pessoa não autenticada; acessa somente login, recuperação de senha e troca de idioma |
| Usuário autenticado | Pessoa com conta ativa; o que pode fazer depende das permissões do seu perfil |
| Administrador | Usuário do perfil `Admin`; além das permissões completas, é o único que gera e importa o backup geral |

As permissões são registradas por perfil e por módulo, com cinco ações: **listar**, **detalhar**, **criar**, **atualizar** e **excluir**. Os módulos existentes são `Usuarios`, `Perfis`, `Comunidades`, `Atores`, `Recursos`, `Vulnerabilidades`, `Atividades`, `DiariosCampo`, `Ficha1Contato`, `AvaliacoesPessoais`, `DiariosProcessoPessoal` e `SER` (relatórios, importação e exportação).

## Requisitos funcionais

### RF-001 — Autenticar usuários

| Item | Descrição |
| --- | --- |
| Ator | Visitante |
| Comportamento esperado | O usuário informa e-mail e senha em `/Account`. Com credenciais válidas e conta ativa, é redirecionado à página inicial; caso contrário, recebe a mensagem genérica "E-mail ou senha inválidos." |
| Critério verificável | Após cinco senhas incorretas, a conta fica bloqueada por 15 minutos. Contas com `Ativo = "N"` não conseguem entrar. |
| Situação | Implementado |
| Evidência | `Controllers/AccountController.cs`, `Services/Identity/IdentityServiceCollectionExtensions.cs` |

### RF-002 — Recuperar a senha

| Item | Descrição |
| --- | --- |
| Ator | Visitante |
| Comportamento esperado | O usuário informa o e-mail em "Esqueci minha senha". Se existir uma conta ativa, o sistema envia um link de redefinição válido por 30 minutos. A resposta é a mesma para e-mails existentes e inexistentes. |
| Critério verificável | O link leva a `/Account/ResetPassword`, onde a nova senha precisa atender à política de senhas (RNF-004). |
| Situação | Parcial — o envio depende da senha de aplicativo do Gmail e de `Email__PublicBaseUrl`; sem essa configuração, nenhum e-mail é enviado. |
| Evidência | `Controllers/AccountController.cs`, `Services/Email/`, [CONFIGURAR-GMAIL.md](../CONFIGURAR-GMAIL.md) |

### RF-003 — Gerenciar usuários e perfis de acesso

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão nos módulos `Usuarios` e `Perfis` |
| Comportamento esperado | Criar, listar, detalhar, editar, desativar e reativar usuários; criar, editar e excluir perfis, definindo as cinco permissões de cada módulo. |
| Critério verificável | O e-mail de cada usuário é único. O usuário conectado não pode desativar a própria conta. O último administrador ativo não pode ser desativado. Um perfil com usuários vinculados, inclusive inativos, não pode ser excluído. |
| Situação | Implementado |
| Evidência | `Controllers/UsersController.cs`, `Controllers/AccessProfileController.cs` |

### RF-004 — Gerenciar comunidades

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo `Comunidades` |
| Comportamento esperado | Cadastrar comunidades com nome, endereço principal, endereço secundário opcional, descrição, descrição de acessibilidade e situação; visualizar a localização no mapa; avançar a situação de `Em diagnóstico` para `Em processo`. |
| Critério verificável | A exclusão é lógica: a comunidade e os atores vinculados recebem `Ativo = "N"` e deixam de aparecer nas listagens. |
| Situação | Implementado |
| Evidência | `Controllers/ComunidadeController.cs` |

### RF-005 — Gerenciar atores

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo `Atores` |
| Comportamento esperado | Cadastrar pessoas vinculadas a uma comunidade, com papéis sociais, recursos, vulnerabilidades e relações da rede primária com outros atores. |
| Critério verificável | A exclusão é lógica (`Ativo = "N"`). A página de resumo de um ator retorna 404 para atores inativos ou inexistentes. |
| Situação | Implementado |
| Evidência | `Controllers/AtoresController.cs`, `Controllers/ComunidadeController.cs` |

### RF-006 — Gerenciar recursos e atividades

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão nos módulos `Recursos` e `Atividades` |
| Comportamento esperado | Registrar a rede de recursos de uma comunidade e as atividades realizadas, associando ambos a um ou mais dos dez eixos temáticos; atividades podem conter ações. |
| Critério verificável | A exclusão de uma atividade é física e remove em cascata suas ações e associações a eixos. |
| Situação | Implementado |
| Evidência | `Controllers/AtividadesController.cs`, `Controllers/ComunidadeController.cs`, `Data/EixoCatalogo.cs` |

### RF-007 — Registrar diários de campo

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo `DiariosCampo` |
| Comportamento esperado | Registrar visitas a uma comunidade com data, descrição, localização, eixos, ações institucionais e atores envolvidos; consultar, filtrar, editar e excluir. |
| Critério verificável | A foto é opcional. A exclusão remove o diário e seus registros dependentes. |
| Situação | Implementado |
| Evidência | `Controllers/DiarioCampoController.cs` |

### RF-008 — Registrar fichas de primeiro contato

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo `Ficha1Contato` |
| Comportamento esperado | Preencher a ficha de um ator em um formulário de três etapas, com dados de contato, situação familiar, escolaridade, condições, petições, respostas e resultados; filtrar por texto, situação e comunidade. |
| Critério verificável | Uma ficha pode ser marcada como `Concluida` ou `Abandonada`. A exclusão remove a ficha e seus registros filhos. |
| Situação | Implementado |
| Evidência | `Controllers/FichaPrimeiroContatoController.cs` |

### RF-009 — Registrar avaliações e diários pessoais

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão nos módulos `AvaliacoesPessoais` e `DiariosProcessoPessoal` |
| Comportamento esperado | Avaliar um ator em nove indicadores; manter um diário do processo pessoal do ator, com eixos e menções a outros atores. |
| Critério verificável | Cada indicador da avaliação aceita somente inteiros de 1 a 5. Uma avaliação não pode ser registrada para um ator inativo ou inexistente. |
| Situação | Implementado |
| Evidência | `Controllers/PersonalAssessmentController.cs`, `Controllers/PersonalProcessController.cs`, `Models/AvaliacaoPessoal.cs` |

### RF-010 — Emitir relatórios

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo `SER` |
| Comportamento esperado | Consultar os relatórios RSC, Fichas de 1º Contato, Avaliações Pessoais, Ações (com filtros por comunidade, ator e período) e Rede Primária. |
| Critério verificável | Usuários sem permissão de detalhar `SER` não acessam os relatórios. |
| Situação | Implementado |
| Evidência | `Controllers/RelatoriosController.cs`, `Views/Relatorios/` |

### RF-011 — Importar e exportar dados em Excel

| Item | Descrição |
| --- | --- |
| Ator | Usuário com permissão no módulo correspondente; administrador para o backup geral |
| Comportamento esperado | Exportar comunidades, atores, atividades, recursos, diários de campo e fichas em `.xlsx`; importar planilhas de comunidade; gerar e importar um backup geral. |
| Critério verificável | A importação de comunidade aceita até 5 arquivos `.xls`/`.xlsx`, com 20 MB no total, em uma única transação. O backup geral aceita somente `.xlsx` de até 100 MB e insere apenas registros ausentes, preservando os existentes. |
| Situação | Implementado |
| Evidência | `Controllers/ReportController.cs`, `Controllers/ServicesController.cs`, `Services/ExcelBackupService.cs` |

### RF-012 — Alternar o idioma da interface

| Item | Descrição |
| --- | --- |
| Ator | Visitante e usuário autenticado |
| Comportamento esperado | Exibir a interface em português (padrão), inglês ou espanhol, seguindo o idioma do navegador ou a escolha explícita do usuário. |
| Critério verificável | A escolha é gravada em cookie e respeitada nas requisições seguintes. |
| Situação | Implementado |
| Evidência | `Controllers/LanguageController.cs`, `Services/UserCultureService.cs`, `Resources/` |

## Requisitos não funcionais

| Código | Categoria | Requisito | Como verificar | Situação |
| --- | --- | --- | --- | --- |
| RNF-001 | Segurança | Toda rota exige usuário autenticado, exceto as marcadas como públicas | Acessar `/` sem login redireciona para `/Account` | Implementado (política de autorização padrão) |
| RNF-002 | Segurança | Requisições que alteram dados exigem token antifalsificação | POST sem token é rejeitado; verificado pelos testes de arquitetura | Implementado |
| RNF-003 | Segurança | Limitar tentativas: 10 logins por minuto e 5 pedidos de recuperação a cada 15 minutos, por IP | Exceder o limite retorna HTTP 429 | Implementado (limites ampliados no ambiente `Testing`) |
| RNF-004 | Segurança | Senhas com no mínimo 8 caracteres, 4 caracteres distintos, letra maiúscula, minúscula, dígito e símbolo, armazenadas como hash | Criar usuário com senha fraca é rejeitado | Implementado (ASP.NET Core Identity) |
| RNF-005 | Segurança | Enviar cabeçalhos `Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy` e `Permissions-Policy` | Inspecionar os cabeçalhos de qualquer resposta | Implementado |
| RNF-006 | Segurança | Cookies de sessão e autenticação `HttpOnly`, `SameSite=Lax` e `Secure` fora de desenvolvimento; sessão de autenticação de 30 minutos com renovação | Inspecionar os cookies no navegador | Implementado |
| RNF-007 | Segurança e privacidade | Não versionar credenciais nem dados pessoais reais | Revisão do repositório | Parcial — ver [limitações](#limitações-conhecidas) |
| RNF-008 | Integridade | Uploads de planilhas validam extensão e tamanho e são processados em transação | Enviar arquivo com extensão inválida retorna HTTP 400 | Implementado |
| RNF-009 | Usabilidade | Interface em pt-BR, en e es, com layout adaptado a telas de computador e celular | Testes E2E de responsividade e idioma | Implementado |
| RNF-010 | Portabilidade | Executar onde houver o .NET 9 (Windows, Linux e macOS) | Seguir o guia de instalação | Atendido pela plataforma; validado pela CI em Linux |
| RNF-011 | Compatibilidade | Funcionar com MariaDB 10.4.32 (versão da CI) e com a versão instalada no servidor | Workflow `testes.yml`, job de compatibilidade | Implementado |
| RNF-012 | Manutenibilidade | Executar testes automatizados de backend, interface e navegador a cada push | Aba Actions do GitHub | Implementado |
| RNF-013 | Confiabilidade | Medir periodicamente a capacidade dos testes de detectar defeitos | Workflow `qualidade-periodica.yml` (Stryker.NET) | Implementado |
| RNF-014 | Desempenho | Tempos de resposta adequados ao uso interno | — | Sem meta definida nem medição no repositório |

## Regras de negócio

1. A situação de uma comunidade é `Em diagnóstico` ou `Em processo`; a ação **Processo** só avança de `Em diagnóstico` para `Em processo`.
2. Comunidades, atores e usuários são desativados logicamente (`Ativo = "N"`); atividades, diários, fichas, avaliações e diários pessoais são excluídos fisicamente.
3. Desativar uma comunidade desativa também os atores vinculados a ela.
4. Uma ficha de primeiro contato é criada com a situação `EmProgresso` e pode ser concluída (`Concluida`) ou abandonada (`Abandonada`).
5. Os eixos temáticos padrão são: Redes; Seguridade Social; Substâncias; Moradia; Prevenção; Assistência Básica e Redução de Danos; Educação; Saúde Física e Psicológica; Ocupação/Trabalho; Lazer/Cultura.
6. Somente o perfil `Admin` gera e importa o backup geral; a importação nunca sobrescreve registros existentes.
7. Um usuário sem permissão em um módulo é redirecionado para a listagem ou recebe HTTP 403, conforme a ação.

## Limitações conhecidas

| Limitação | Impacto |
| --- | --- |
| Não há migrations do Entity Framework no repositório | Em `Development` e produção, as tabelas não são criadas na inicialização; o esquema precisa ser criado antes (ver [instalacao-e-configuracao.md](instalacao-e-configuracao.md#opção-b--desenvolvimento-com-mysql-ou-mariadb)), e mudanças futuras no modelo exigem scripts manuais |
| `Banco.txt` começa com `DROP DATABASE IF EXISTS empodera` | Executá-lo em um servidor com dados apaga o banco existente |
| `Banco.txt` e `wwwroot/uploads/` contêm registros e planilhas de comunidades e pessoas | Possível exposição de dados pessoais em repositório público; recomenda-se revisão pela equipe |
| As verificações de permissão nos controllers leem o ID do usuário da sessão HTTP | A sessão é derivada da identidade autenticada, mas a lógica de permissão está repetida em cada controller |
| `Email__User` aceita somente `empodera.ajuda@gmail.com` | Trocar o remetente exige alteração de código |
| A recuperação de senha depende do Gmail | Sem credenciais configuradas, o fluxo é exibido, mas nenhum e-mail é enviado |
| Não há metas de desempenho nem testes de carga | RNF-014 não é verificável no estado atual |
