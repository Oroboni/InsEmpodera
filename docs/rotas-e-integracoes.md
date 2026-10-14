# Rotas, APIs e integrações

[← Voltar ao README principal](../README.md)

## Sumário

- [Existe uma API?](#existe-uma-api)
- [Convenções das rotas](#convenções-das-rotas)
- [Rotas por módulo](#rotas-por-módulo)
- [Endpoints que retornam arquivos ou JSON](#endpoints-que-retornam-arquivos-ou-json)
- [Respostas e erros](#respostas-e-erros)
- [Rotas antigas](#rotas-antigas)
- [Integrações externas](#integrações-externas)

## Existe uma API?

**Não há API REST pública.** Nenhum controller usa `[ApiController]` ou rotas `/api/...`. Todos herdam de `Controller` e devolvem páginas HTML, redirecionamentos ou arquivos.

O Swashbuckle (Swagger) está registrado e fica acessível em `/swagger` nos ambientes `Development` e `Testing`, mas o documento `/swagger/v1/swagger.json` é gerado sem operações (`"paths": {}`), porque as rotas MVC convencionais não são descritas por ele.

## Convenções das rotas

- Rota padrão: `{controller=Home}/{action=Index}/{id?}`. Exemplo: `GET /Comunidade/ComunidadesDetalhes/123`.
- Autenticação: cookie `.Empodera.Identity`, obtido em `POST /Account`. Não há autenticação por token.
- Todo `POST` exige o campo `__RequestVerificationToken`, gerado automaticamente nos formulários Razor.
- Os dados são enviados como formulário (`application/x-www-form-urlencoded` ou `multipart/form-data` em uploads), não como JSON.
- O acesso a cada ação depende das permissões do perfil no módulo indicado (ver [requisitos.md](requisitos.md#atores-do-sistema)).

## Rotas por módulo

As tabelas listam as rotas usadas pela interface. `{id}` representa o identificador numérico do registro.

### Acesso e configurações

| Método | Rota | Descrição | Acesso |
| --- | --- | --- | --- |
| GET / POST | `/Account` | Formulário e envio do login | Público; POST limitado a 10 por minuto por IP |
| GET | `/Account/Forgot` | Formulário de recuperação de senha | Público |
| POST | `/Account/ForgotPassword` | Solicita o e-mail de redefinição | Público; limitado a 5 a cada 15 minutos por IP |
| GET / POST | `/Account/ResetPassword` | Define nova senha a partir do link recebido | Público |
| POST | `/Account/Logout` | Encerra a sessão | Autenticado |
| POST | `/Language/Set` | Altera o idioma (`pt-BR`, `en`, `es` ou `auto`) | Público |
| GET | `/` | Página inicial com totais de comunidades, atores e usuários | Autenticado |
| GET | `/Ajuda`, `/Configuracoes` | Ajuda e configurações de idioma | Autenticado |

### Cadastros

| Módulo | Rotas principais (GET para telas, POST para gravações) | Permissão |
| --- | --- | --- |
| Comunidades | `/Comunidade`, `/Comunidade/ComunidadesDetalhes/{id}` (o ID `0` abre o cadastro), `POST /Comunidade/Processo/{id}`, `POST /Comunidade/Delete/{id}` | `Comunidades` |
| Recursos | `/Comunidade/ComunidadeRecursos?comunidadeId={id}`, `/Comunidade/Create_Recursos`, `POST /Comunidade/Edit_Recursos/{id}`, `POST /Comunidade/Delete_Recursos/{id}` | `Recursos` |
| Atores | `/Atores`, `/Atores/Create`, `/Atores/Edit/{id}`, `/Atores/Resumo/{id}`, `POST /Atores/Delete/{id}`; vínculos em `/Comunidade/AtoresVinculados/{id}` e `/Comunidade/Create_Atores/{id}` | `Atores` |
| Atividades | `/Atividades`, `/Atividades/Create`, `/Atividades/Edit/{id}`, `POST /Atividades/Delete/{id}`; por comunidade em `/Comunidade/AtividadesVinculadas?comunidadeId={id}` | `Atividades` |
| Diários de Campo | `/DiarioCampo`, `/DiarioCampo/Create`, `/DiarioCampo/Edit/{id}`, `/DiarioCampo/Details/{id}`, `/DiarioCampo/Delete/{id}` | `DiariosCampo` |
| Ficha de 1º Contato | `/FichaPrimeiroContato?search=&status=&comunidade=`, `/FichaPrimeiroContato/Create`, `/FichaPrimeiroContato/Edit/{id}`, `POST /FichaPrimeiroContato/Concluir/{id}`, `POST /FichaPrimeiroContato/Abandonar/{id}`, `/FichaPrimeiroContato/Delete/{id}` | `Ficha1Contato` |
| Avaliação Pessoal | `/PersonalAssessment?atorId={id}`, `/PersonalAssessment/Create`, `/PersonalAssessment/Edit/{id}`, `POST /PersonalAssessment/Delete/{id}` | `AvaliacoesPessoais` |
| Diário Pessoal | `/PersonalProcess?atorId={id}&searchQuery=`, `/PersonalProcess/Create`, `/PersonalProcess/Edit/{id}`, `POST /PersonalProcess/Delete/{id}` | `DiariosProcessoPessoal` |
| Usuários | `/Users`, `/Users/Details/{id}`, `/Users/Create`, `/Users/Edit/{id}`, `/Users/Delete/{id}`, `POST /Users/Reactivate/{id}` | `Usuarios` |
| Perfis de acesso | `/AccessProfile`, `/AccessProfile/Create`, `/AccessProfile/Edit/{id}`, `POST /AccessProfile/Delete/{id}` | `Perfis` |

### Relatórios e dados

| Método | Rota | Descrição | Permissão |
| --- | --- | --- | --- |
| GET | `/Relatorios` | Menu de relatórios | Listar `SER` |
| GET | `/Relatorios/Rsc` | Relatório RSC | Detalhar `SER` |
| GET | `/Relatorios/FirstContact` | Relatório de fichas de 1º contato | Detalhar `SER` |
| GET | `/Relatorios/PersonalAssessment?atorId=&avaliacaoId=` | Relatório de avaliações pessoais | Detalhar `SER` |
| GET | `/Relatorios/Actions?comunidadeId=&atorId=&dataDe=&dataAte=` | Relatório de ações | Detalhar `SER` |
| GET | `/Relatorios/PrimaryNetwork?atorId=` | Relatório de rede primária | Detalhar `SER` |
| GET | `/Report` | Tela **Dados e Backup** | Criar `SER` |
| POST | `/Report/RelatorioComunidade` | Importa até 5 planilhas `.xls`/`.xlsx` (20 MB no total) | Criar `SER` |
| POST | `/Report/ImportarBackup` | Importa o backup geral `.xlsx` (até 100 MB) | Perfil `Admin` |

## Endpoints que retornam arquivos ou JSON

Todos exigem sessão autenticada; sem ela, respondem com redirecionamento para `/Account`.

| Método | Rota | Resposta | Permissão (detalhar) |
| --- | --- | --- | --- |
| GET | `/Services/ExportComunidades` | `comunidades-empodera.xlsx` | `Comunidades` |
| GET | `/Services/ExportComunidadeCompleta/{id}` | Planilha completa da comunidade | `Comunidades` |
| GET | `/Services/ExportComunidade/{id}` | Planilha da comunidade | `Comunidades` |
| GET | `/Services/ExportAtoresComunidade/{id}` | Atores da comunidade | `Atores` |
| GET | `/Services/ExportAtividadesComunidade/{id}` | Atividades da comunidade | `Atividades` |
| GET | `/Services/ExportRecursosComunidade/{id}` | Recursos da comunidade | `Recursos` |
| GET | `/Services/ExportAtores` | `atores-empodera.xlsx` | `Atores` |
| GET | `/Services/ExportDiariosCampo` | `diarios-de-campo.xlsx` | `DiariosCampo` |
| GET | `/Services/ExportFichasPrimeiroContato` | `fichas-primeiro-contato.xlsx` | `Ficha1Contato` |
| GET | `/Services/ExportBackupGeral` | `insempodera-backup-geral-AAAAMMDD-HHMMSS.xlsx` | `SER` e perfil `Admin` |
| GET | `/Language/Catalog?page={caminho}` | JSON com as traduções da página, usado pelos scripts do navegador | Público |

As planilhas usam o tipo `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`.

Exemplo de chamada ao catálogo de idiomas:

```http
GET /Language/Catalog?page=Account HTTP/1.1
Host: localhost:5000
Cookie: .AspNetCore.Culture=c=en|uic=en
```

A resposta é um objeto JSON cujas chaves são os textos em português e cujos valores são as traduções no idioma ativo.

## Respostas e erros

| Situação | Resposta |
| --- | --- |
| Usuário não autenticado em rota protegida | `302` para `/Account` |
| Sessão invalidada (usuário desativado ou senha redefinida) | `403` |
| Usuário sem permissão no módulo | Redirecionamento para a listagem ou `403`, conforme a ação |
| `POST` sem token antifalsificação | `400` |
| Upload com extensão ou tamanho inválidos na importação de comunidade | `400` com mensagem em texto |
| Registro inexistente ou inativo | `404` |
| Limite de tentativas excedido | `429` |
| Erro inesperado fora de `Development` | Página `/Home/Error` |

## Rotas antigas

As rotas abaixo não aparecem no menu e não têm página própria. Elas foram mantidas por compatibilidade e redirecionam para a tela equivalente, mantendo os parâmetros e as verificações de login e permissão:

| Rota antiga | Redireciona para |
| --- | --- |
| `/Report/Rsc`, `/Report/FirstContact`, `/Report/PersonalAssessment`, `/Report/Actions`, `/Report/PrimaryNetwork` | Relatório de mesmo nome em `/Relatorios` |
| `/Dashboard` | `/` (página inicial) |
| `/DiariosDeCampo` | `/DiarioCampo` |
| `/DiarioProcessoPessoal` | `/PersonalProcess` |

## Integrações externas

| Serviço | Quem chama | Finalidade | Configuração |
| --- | --- | --- | --- |
| Gmail SMTP (`smtp.gmail.com:587`, STARTTLS) | Servidor | Enviar os links de recuperação de senha | `Email:*` — ver [CONFIGURAR-GMAIL.md](../CONFIGURAR-GMAIL.md) |
| OpenStreetMap — mapas (`tile.openstreetmap.org`) | Navegador | Exibir mapas com Leaflet | Nenhuma |
| OpenStreetMap — Nominatim | Navegador | Buscar coordenadas de endereços | Nenhuma |
| ArcGIS World Geocoding | Navegador | Busca alternativa de endereços | Nenhuma |
| ViaCEP | Navegador | Preencher endereço a partir do CEP | Nenhuma |

Os domínios externos permitidos ao navegador estão declarados no cabeçalho `Content-Security-Policy` montado em [`Program.cs`](../Program.cs). Sem acesso à internet, a aplicação funciona, mas mapas e buscas de endereço ficam indisponíveis.
