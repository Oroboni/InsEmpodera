# Revisão técnica do DDL MySQL — o que foi aplicado

[← Voltar ao banco de dados](banco-de-dados.md)

Este documento responde, item por item, à **Revisão técnica do DDL MySQL** (17/10/2026, @Cesar).
Ele registra o que mudou no esquema, as decisões tomadas, o que ficou para discutir com o orientador
e como verificar cada ponto no próprio banco.

| Artefato | Papel |
| --- | --- |
| [`Database/Empodera.schema.sql`](../Database/Empodera.schema.sql) | DDL oficial (34 tabelas), no formato pedido na revisão |
| [`Database/migracao-uuid-para-int.sql`](../Database/migracao-uuid-para-int.sql) | Migração dos dados atuais (UUID) para o novo esquema |
| [`Data/ApplicationDbContext.Esquema.cs`](../Data/ApplicationDbContext.Esquema.cs) | Modelo do EF com tipos, nomes e restrições da revisão |
| [`scripts/GerarSchema`](../scripts/GerarSchema) | Gera os dois scripts a partir do modelo: `dotnet run --project scripts/GerarSchema` |
| [`InsEmpodera.Tests/Architecture/RevisaoDdlTests.cs`](../InsEmpodera.Tests/Architecture/RevisaoDdlTests.cs) | Checklist da seção 9 verificado automaticamente |

Os dois scripts são **gerados** a partir do modelo do EF, que é a fonte única. Assim o banco que a aplicação
espera e o DDL entregue não divergem; um teste falha se alguém alterar o modelo sem regenerar os scripts.
Todos os testes de backend (228) criam o banco executando `Empodera.schema.sql`, não pelo EF.

**Restrição combinada com a equipe:** a coluna `usuarios.Senha` não foi alterada (continua `LONGTEXT NOT NULL`).

---

## 1. Verificação prévia: SGBD e versão

| Ambiente | `SELECT VERSION();` |
| --- | --- |
| Versão adotada em todos os ambientes | **MySQL 8.4 LTS** |
| Validação desta revisão | **8.4.11** (Linux, Docker) e **9.7.1** (macOS): os 228 testes passam nos dois |
| CI (GitHub Actions) | `mysql:8.4` (antes era MariaDB 10.4.32) |
| Servidor de produção | Hoje MariaDB 10.5 — **vai para MySQL 8.4 LTS** na VPS nova ([servidor-hostinger.md](servidor-hostinger.md)) |

O DDL usa apenas recursos do MySQL 8.0.16+ (`CHECK` aplicado) e o collation `utf8mb4_0900_ai_ci`
(8.0+); foi validado em 8.4 LTS e 9.7. **Ele não roda no MariaDB**, que não tem esse collation —
coerente com a regra da revisão de usar o mesmo SGBD em todos os ambientes.

## 2. Problemas críticos

### 2.1 Chaves estrangeiras

Todas as referências agora têm `FOREIGN KEY` declarada **em nível de tabela, via `ALTER TABLE`**, com nome e
regra de exclusão explícita: **57 chaves estrangeiras** nas 34 tabelas.

Duas referências não tinham FK nenhuma e passaram a ter:

| Coluna | Antes | Agora |
| --- | --- | --- |
| `perfis.IdUsuario` (quem criou o perfil) | sem FK; a aplicação gravava um UUID zerado | FK para `usuarios`, `ON DELETE SET NULL`; a aplicação grava o usuário logado; perfis antigos ficam `NULL` |
| `fontesinfo.IdAtor` | duas colunas (`Fk_Id_Ator` sem FK e `AtorIdAtores` criada pelo EF) | uma coluna só, FK para `atores`, `ON DELETE SET NULL` |

### 2.2 Armadilhas do MySQL com chaves estrangeiras

- Nenhuma coluna usa `REFERENCES` na própria definição.
- O script final **não** usa `SET FOREIGN_KEY_CHECKS = 0`: as tabelas são criadas, depois as restrições,
  e os `INSERT` dos dados iniciais seguem a ordem das FKs.
- Não há índice separado para coluna de FK: o MySQL cria o índice da FK com o nome da restrição, e um
  `UNIQUE` que começa pela coluna da FK já serve de índice (ex.: `UQ_Permissoes_Perfil_Modulo`).
- Órfãos: a migração verifica e **para sem gravar nada** se houver registro apontando para um pai inexistente
  em referência obrigatória (seção [Migração dos dados](#migração-dos-dados)).

#### Regra ON DELETE de cada FK

As regras são as que a aplicação já praticava (o comportamento do sistema não mudou):

- **CASCADE** — o registro filho só existe por causa do pai (itens de uma ficha, eixos de um diário,
  vínculos de um ator). Excluir o pai remove os filhos.
- **RESTRICT** — impede excluir quem ainda é referenciado: perfil com usuários, usuário que modificou
  registros (`IdUsuarioModificacao`), autor de diário pessoal e atores ligados em rede primária/subjetiva.
- **SET NULL** — referência opcional que pode deixar de existir: criador do perfil e ator da fonte de informação.

Na prática, comunidades, atores e usuários não são apagados pela interface: recebem `Ativo = 'N'`.

| Restrição | Coluna → tabela referenciada | ON DELETE |
| --- | --- | --- |
| `FK_Atividades_Usuarios_Modificacao` | `atividades.IdUsuarioModificacao` → `usuarios` | RESTRICT |
| `FK_Atores_Usuarios_Modificacao` | `atores.IdUsuarioModificacao` → `usuarios` | RESTRICT |
| `FK_Comunidades_Usuarios_Modificacao` | `comunidades.IdUsuarioModificacao` → `usuarios` | RESTRICT |
| `FK_DiariosProcessoPessoal_Usuarios` | `diariosprocessopessoal.IdUsuario` → `usuarios` | RESTRICT |
| `FK_DiariosProcessoPessoal_Usuarios_Modificacao` | `diariosprocessopessoal.IdUsuarioModificacao` → `usuarios` | RESTRICT |
| `FK_RedesPrimarias_Atores_Principal` | `redesprimarias.IdAtorPrincipal` → `atores` | RESTRICT |
| `FK_RedesPrimarias_Atores_Relacionado` | `redesprimarias.IdAtorRelacionado` → `atores` | RESTRICT |
| `FK_Usuarios_Perfis` | `usuarios.IdPerfil` → `perfis` | RESTRICT |
| `FK_Acoes_Atividades` | `acoes.IdAtividade` → `atividades` | CASCADE |
| `FK_AcoesAtores_Acoes` | `acoesatores.IdAcao` → `acoes` | CASCADE |
| `FK_AcoesAtores_Atores` | `acoesatores.IdAtor` → `atores` | CASCADE |
| `FK_AnexosDiario_DiariosCampo` | `anexosdiario.IdDiarioCampo` → `diarioscampo` | CASCADE |
| `FK_Atividades_Comunidades` | `atividades.IdComunidade` → `comunidades` | CASCADE |
| `FK_Atividades_Usuarios` | `atividades.IdUsuario` → `usuarios` | CASCADE |
| `FK_AtividadesEixo_Atividades` | `atividadeseixo.IdAtividade` → `atividades` | CASCADE |
| `FK_AtividadesEixo_Eixos` | `atividadeseixo.IdEixo` → `eixos` | CASCADE |
| `FK_AtorComunidades_Atores` | `atorcomunidades.IdAtor` → `atores` | CASCADE |
| `FK_AtorComunidades_Comunidades` | `atorcomunidades.IdComunidade` → `comunidades` | CASCADE |
| `FK_Atores_Usuarios` | `atores.IdUsuario` → `usuarios` | CASCADE |
| `FK_AvaliacaoPessoal_Atores` | `avaliacaopessoal.IdAtor` → `atores` | CASCADE |
| `FK_AvaliacaoPessoal_Usuarios` | `avaliacaopessoal.IdUsuario` → `usuarios` | CASCADE |
| `FK_Comunidades_Usuarios` | `comunidades.IdUsuario` → `usuarios` | CASCADE |
| `FK_DAAtores_Atores` | `daatores.IdAtor` → `atores` | CASCADE |
| `FK_DAAtores_DiarioDAcoes` | `daatores.IdDiarioDAcao` → `diariodacoes` | CASCADE |
| `FK_DetalhesDAcoes_DiarioDAcoes` | `detalhesdacoes.IdDiarioDAcao` → `diariodacoes` | CASCADE |
| `FK_DetalhesEixos_DetalhesDAcoes` | `detalheseixos.IdDetalheDAcao` → `detalhesdacoes` | CASCADE |
| `FK_DetalhesEixos_Eixos` | `detalheseixos.IdEixo` → `eixos` | CASCADE |
| `FK_DiarioAcoes_Acoes` | `diarioacoes.IdAcao` → `acoes` | CASCADE |
| `FK_DiarioAcoes_DiariosCampo` | `diarioacoes.IdDiarioCampo` → `diarioscampo` | CASCADE |
| `FK_DiarioDAcoes_DiariosCampo` | `diariodacoes.IdDiarioCampo` → `diarioscampo` | CASCADE |
| `FK_DiarioEixos_DiariosCampo` | `diarioeixos.IdDiarioCampo` → `diarioscampo` | CASCADE |
| `FK_DiarioEixos_Eixos` | `diarioeixos.IdEixo` → `eixos` | CASCADE |
| `FK_DiariosCampo_Comunidades` | `diarioscampo.IdComunidade` → `comunidades` | CASCADE |
| `FK_DiariosCampo_Usuarios` | `diarioscampo.IdUsuario` → `usuarios` | CASCADE |
| `FK_DiariosProcessoEixos_DiariosProcessoPessoal` | `diariosprocessoeixos.IdDiarioProcesso` → `diariosprocessopessoal` | CASCADE |
| `FK_DiariosProcessoEixos_Eixos` | `diariosprocessoeixos.IdEixo` → `eixos` | CASCADE |
| `FK_DiariosProcessoPessoal_Atores` | `diariosprocessopessoal.IdAtor` → `atores` | CASCADE |
| `FK_FichaCondicoes_FichasPrimeiroContato` | `fichacondicoes.IdFicha` → `fichasprimeirocontato` | CASCADE |
| `FK_FichaPeticoes_FichasPrimeiroContato` | `fichapeticoes.IdFicha` → `fichasprimeirocontato` | CASCADE |
| `FK_FichaRespostas_FichasPrimeiroContato` | `ficharespostas.IdFicha` → `fichasprimeirocontato` | CASCADE |
| `FK_FichaResultados_FichasPrimeiroContato` | `ficharesultados.IdFicha` → `fichasprimeirocontato` | CASCADE |
| `FK_FichasPrimeiroContato_Atores` | `fichasprimeirocontato.IdAtor` → `atores` | CASCADE |
| `FK_FichasPrimeiroContato_Comunidades` | `fichasprimeirocontato.IdComunidade` → `comunidades` | CASCADE |
| `FK_FichasPrimeiroContato_Usuarios` | `fichasprimeirocontato.IdUsuario` → `usuarios` | CASCADE |
| `FK_FontesInfo_FichasPrimeiroContato` | `fontesinfo.IdFicha` → `fichasprimeirocontato` | CASCADE |
| `FK_Permissoes_Perfis` | `permissoes.IdPerfil` → `perfis` | CASCADE |
| `FK_RecursosAtores_Atores` | `recursosatores.IdAtor` → `atores` | CASCADE |
| `FK_RedeEixos_Eixos` | `redeeixos.IdEixo` → `eixos` | CASCADE |
| `FK_RedeEixos_RedeRecursos` | `redeeixos.IdRede` → `rederecursos` | CASCADE |
| `FK_RedeRecursos_Atores` | `rederecursos.IdAtor` → `atores` | CASCADE |
| `FK_RedeRecursos_Comunidades` | `rederecursos.IdComunidade` → `comunidades` | CASCADE |
| `FK_RedeRecursos_Usuarios` | `rederecursos.IdUsuario` → `usuarios` | CASCADE |
| `FK_Vulnerabilidades_Comunidades` | `vulnerabilidades.IdComunidade` → `comunidades` | CASCADE |
| `FK_VulnerabilidadesEixo_Eixos` | `vulnerabilidadeseixo.IdEixo` → `eixos` | CASCADE |
| `FK_VulnerabilidadesEixo_Vulnerabilidades` | `vulnerabilidadeseixo.IdVulnerabilidade` → `vulnerabilidades` | CASCADE |
| `FK_FontesInfo_Atores` | `fontesinfo.IdAtor` → `atores` | SET NULL |
| `FK_Perfis_Usuarios` | `perfis.IdUsuario` → `usuarios` | SET NULL |

**Decisão pendente com o orientador:** manter `CASCADE` ao excluir uma atividade (as ações vão junto),
como a aplicação já faz, ou mudar para `RESTRICT`.

## 3. Chaves primárias: UUID → `INT UNSIGNED AUTO_INCREMENT`

As 34 tabelas usam `INT UNSIGNED AUTO_INCREMENT`, aplicado no `ALTER TABLE` junto com a `PRIMARY KEY`
(o MySQL exige a coluna indexada antes do atributo):

```sql
CREATE TABLE atividades (
    IdAtividade INT UNSIGNED NOT NULL,
    ...
);
ALTER TABLE atividades
    ADD PRIMARY KEY (IdAtividade),
    MODIFY IdAtividade INT UNSIGNED NOT NULL AUTO_INCREMENT;
```

- Toda coluna de FK tem exatamente o tipo da PK referenciada (`INT UNSIGNED`).
- A aplicação deixou de gerar Ids (`Guid.NewGuid()`); o banco gera. Onde um registro novo era usado logo
  em seguida (ator criado dentro da Rede Primária, por exemplo), a gravação passou a ligar as entidades e o
  EF preenche a chave depois do `INSERT` — o problema de `LAST_INSERT_ID()` citado na revisão.
- Nas URLs os Ids voltaram a ser números (`/Atores/Edit/12`).

## 4. Nulabilidade e regras de domínio

As regras de negócio que só existiam na aplicação agora também estão no banco: `NOT NULL` onde o campo é
obrigatório, **47 `CHECK` nomeados** e **14 `UNIQUE` nomeados**.

| Regra | Onde |
| --- | --- |
| `'S'` ou `'N'` | `Ativo` (usuários, atores, comunidades), `Pode*` (permissões), `RecursosAtores.Pode` e as respostas Sim/Não da ficha |
| `0` ou `1` | colunas booleanas (`DaEquipe`, `Rope`, `EmailConfirmed`...) |
| 1 a 5 | as nove notas da avaliação pessoal (um `CHECK` por coluna) |
| `Quantidade >= 0` | `acoes`, `diariodacoes` |
| Faixas | gênero 0–3, idade até 130, tipo de relacionamento 1–5, idioma 0–3, filhos/anos escolares até 30 |
| Valores fixos | `fichasprimeirocontato.Status` (`EmProgresso`, `Concluida`, `Abandonada`), `redesprimarias.TipoRede` (`Primaria`, `Subjetiva`) |
| Coerência | rede primária não liga um ator a ele mesmo |
| Unicidade | e-mail/usuário normalizados, nome do eixo, módulo por perfil, pares das tabelas associativas, rede (principal, relacionado, tipo) |

Detalhe do collation: com `utf8mb4_0900_ai_ci`, `'s' IN ('S', 'N')` é verdadeiro. Por isso os `CHECK` de
Sim/Não comparam em binário (`Ativo COLLATE utf8mb4_bin IN ('S', 'N')`).

**Decisões tomadas sobre `UNIQUE` em nomes** (revisão, seção 4):

- `atividades.Nome`, `acoes (IdAtividade, Nome)`, nomes de pessoas e de comunidades: **sem `UNIQUE`**.
  Homônimos são esperados e a aplicação permite repetir.
- `perfis.Nome`: **sem `UNIQUE` por enquanto** — a aplicação não impede nomes repetidos. Pendente com o orientador.
- `eixos.Nome`: **com `UNIQUE`** — é um catálogo fixo; a migração unifica eixos com o mesmo nome.

## 5. Tipos de dados

| Item da revisão | Situação |
| --- | --- |
| 5.1 `INT(11)` | Nenhum `INT(n)`. Números pequenos usam `TINYINT UNSIGNED` (notas, gênero, idade, contagens) |
| 5.2 `DATETIME(6)` | `DATETIME` sem precisão em todas as colunas; datas sem horário viraram `DATE` (data do diário, nascimento, data do contato) e `HoraContato` é `TIME` |
| 5.3 `VARCHAR(255)` em tudo | Cada coluna tem tamanho justificado (tabela abaixo); texto livre longo usa `TEXT` |
| 5.4 Senhas | Já são **hash** do ASP.NET Core Identity (PBKDF2). A coluna `Senha` **não foi alterada** por decisão da equipe (continua `LONGTEXT`); a recomendação `VARCHAR(255) ascii_bin` fica registrada para depois |
| 5.5 Sim/Não | `CHAR(1)` + `CHECK ('S','N')` para os campos que já eram S/N; booleanos em `TINYINT UNSIGNED` + `CHECK (0,1)`. As respostas da ficha, que eram texto livre ("Sim"/"Não"/"S"), passaram a `CHAR(1)`; a aplicação continua mostrando "Sim"/"Não" |

### Tamanhos

Os tamanhos consideram o conteúdo real: a migração mede os dados atuais e para se algum valor não couber.

| Tamanho | Uso | Justificativa |
| --- | --- | --- |
| `CHAR(1)` | Sim/Não | um caractere fixo |
| `VARCHAR(20)`–`VARCHAR(50)` | status, tipos, módulos | valores de lista fixa (o maior tem 18 caracteres) |
| `VARCHAR(60)` | telefones | DDI + número + formatação; um registro real tem 44 caracteres |
| `VARCHAR(100)` | ocupação, nome de perfil e de eixo, relação na rede | rótulos curtos |
| `VARCHAR(150)` | nomes de pessoas, comunidades, atividades, recursos, ações | nome completo; maior nome real: 53 caracteres |
| `VARCHAR(254)` | e-mail e campos do Identity | limite de um endereço de e-mail (RFC 5321) |
| `VARCHAR(255)` | papel social, interesse, complemento, endereço da ficha, estrutura familiar, localização, informação extra, caminho de foto | textos descritivos curtos ou caminhos de arquivo; papel social real chega a 152 caracteres |
| `VARCHAR(500)` | endereço da comunidade e link do mapa | endereços reais trazem referências e horários (353 caracteres) |
| `VARCHAR(4000)` | descrição do diário pessoal | limite já imposto pela tela |
| `TEXT` | descrições, comentários, serviços | texto livre sem limite na aplicação |

O provedor (`acoes.Provedor`, `diariodacoes.ProvedorExterno`) continua **texto livre**: hoje é digitado em
campo aberto. Uma tabela própria exigiria trocar a tela por uma lista; fica como **decisão pendente**.

## 6. Nomenclatura e nomeação de restrições

Todas as restrições têm nome explícito no padrão da revisão; nenhum nome automático (`*_ibfk_*`).

| Tipo | Padrão | Exemplo |
| --- | --- | --- |
| Chave estrangeira | `FK_<Filha>_<Pai>` (+ papel quando a filha aponta duas vezes para o mesmo pai) | `FK_Acoes_Atividades`, `FK_Atores_Usuarios_Modificacao` |
| Unicidade | `UQ_<Tabela>_<Colunas>` | `UQ_Permissoes_Perfil_Modulo` |
| Verificação | `CK_<Tabela>_<Coluna>` | `CK_Acoes_Quantidade` |

Convenções:

- Tabelas em **minúsculas** (o MySQL no Linux diferencia maiúsculas de minúsculas).
- Chave primária no **singular** (`IdAcao`, `IdAtor`, `IdComunidade`).
- Coluna de FK com o **nome da PK referenciada, sem o prefixo `Fk`** (`acoes.IdAtividade`); quando a tabela
  aponta duas vezes para o mesmo pai, o papel completa o nome (`IdUsuarioModificacao`, `IdAtorPrincipal`, `IdAtorRelacionado`).
- Auditoria com os mesmos nomes em todas as tabelas (`DtCriacao`, `DtModificacao`; antes havia `Dt_Criacao` e `DtAtualizacao`).
- Sempre espaço entre tipo e modificador (`VARCHAR(100) NULL`).

As classes C# mantiveram os nomes das propriedades; o mapeamento para os nomes novos está no modelo do EF.
Colunas renomeadas:

| Tabela | Antes → agora |
| --- | --- |
| `acoes` | `IdAcoes` → `IdAcao`<br>`FkIdAtividade` → `IdAtividade` |
| `acoesatores` | `IdAAtores` → `IdAcaoAtor`<br>`FK_id_Atores` → `IdAtor`<br>`FkIdAcoes` → `IdAcao` |
| `anexosdiario` | `IdAnexos` → `IdAnexo`<br>`FkIdDiario` → `IdDiarioCampo` |
| `atividades` | `FkIdComunidade` → `IdComunidade`<br>`FkIdUsuario` → `IdUsuario`<br>`FkIdUsuarioM` → `IdUsuarioModificacao` |
| `atividadeseixo` | `IdAEixo` → `IdAtividadeEixo`<br>`FkIdAtividade` → `IdAtividade`<br>`FkIdEixo` → `IdEixo` |
| `atorcomunidades` | `FK_id_Atores` → `IdAtor`<br>`FkIdComunidade` → `IdComunidade` |
| `atores` | `IdAtores` → `IdAtor`<br>`FkIdUsuario` → `IdUsuario`<br>`FkIdUsuarioM` → `IdUsuarioModificacao` |
| `avaliacaopessoal` | `FK_id_Atores` → `IdAtor`<br>`FkIdUsuario` → `IdUsuario` |
| `comunidades` | `Id_Comunidade` → `IdComunidade`<br>`Descricao_Acessibilidade` → `DescricaoAcessibilidade`<br>`Dt_Criacao` → `DtCriacao`<br>`Dt_Modificacao` → `DtModificacao`<br>`FK_Id_Usuario` → `IdUsuario`<br>`FK_Id_UsuarioM` → `IdUsuarioModificacao` |
| `daatores` | `Id` → `IdDAAtor`<br>`FK_id_Atores` → `IdAtor`<br>`FkIdDDacoes` → `IdDiarioDAcao` |
| `detalhesdacoes` | `Id` → `IdDetalheDAcao`<br>`FkIdDDacoes` → `IdDiarioDAcao` |
| `detalheseixos` | `IdDiarioEixo` → `IdDetalheEixo`<br>`FkIdDetalhes` → `IdDetalheDAcao`<br>`FkIdEixo` → `IdEixo` |
| `diarioacoes` | `IdDAcoes` → `IdDiarioAcao`<br>`FkIdAcoes` → `IdAcao`<br>`FkIdDiario` → `IdDiarioCampo` |
| `diariodacoes` | `IdDAcoes` → `IdDiarioDAcao`<br>`FkIdDiario` → `IdDiarioCampo`<br>`PeovedorEx` → `ProvedorExterno` |
| `diarioeixos` | `FkIdDiario` → `IdDiarioCampo`<br>`FkIdEixo` → `IdEixo` |
| `diarioscampo` | `IdDCampo` → `IdDiarioCampo`<br>`FkIdComunidade` → `IdComunidade`<br>`FkIdUsuario` → `IdUsuario` |
| `diariosprocessoeixos` | `FkIdDiarioProcesso` → `IdDiarioProcesso`<br>`FkIdEixo` → `IdEixo` |
| `diariosprocessopessoal` | `FK_id_Atores` → `IdAtor`<br>`FkIdUsuario` → `IdUsuario`<br>`FkIdUsuarioM` → `IdUsuarioModificacao` |
| `fichacondicoes` | `IdCondicoes` → `IdFichaCondicao`<br>`FkIdFicha` → `IdFicha` |
| `fichapeticoes` | `IdPeticoes` → `IdFichaPeticao`<br>`FkIdFicha` → `IdFicha` |
| `ficharespostas` | `IdCondicoes` → `IdFichaResposta`<br>`FkIdFicha` → `IdFicha` |
| `ficharesultados` | `IdCondicoes` → `IdFichaResultado`<br>`FkIdFicha` → `IdFicha` |
| `fichasprimeirocontato` | `FK_id_Atores` → `IdAtor`<br>`FkIdComunidade` → `IdComunidade`<br>`FkIdUsuario` → `IdUsuario`<br>`NFIlhos` → `NFilhos` |
| `fontesinfo` | `FkIdFicha` → `IdFicha`<br>`Fk_Id_Ator` → `IdAtor` |
| `perfis` | `FkIdUsuario` → `IdUsuario` |
| `permissoes` | `IdPermissoes` → `IdPermissao`<br>`FkIdPerfil` → `IdPerfil` |
| `recursosatores` | `Id_Recursos_Atores` → `IdRecursoAtor`<br>`FK_id_Atores` → `IdAtor` |
| `redeeixos` | `FkIdEixo` → `IdEixo`<br>`FkIdRede` → `IdRede` |
| `rederecursos` | `Id_Rede` → `IdRede`<br>`FK_id_Atores` → `IdAtor`<br>`FkIdComunidade` → `IdComunidade`<br>`FkIdUsuario` → `IdUsuario` |
| `redesprimarias` | `FkIdAtorPrincipal` → `IdAtorPrincipal`<br>`FkIdAtorRelacionados` → `IdAtorRelacionado` |
| `usuarios` | `DtAtualizacao` → `DtModificacao`<br>`FkIdPerfil` → `IdPerfil` |
| `vulnerabilidades` | `FkIdComunidade` → `IdComunidade` |
| `vulnerabilidadeseixo` | `IdVEixo` → `IdVulnerabilidadeEixo`<br>`FkIdEixo` → `IdEixo`<br>`FkIdVulnerabilidade` → `IdVulnerabilidade` |

## 7. Conjunto de caracteres e collation

- O script começa com `ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;` e as tabelas não
  repetem `CHARSET`/`COLLATE`: herdam do banco.
- Nenhuma tabela ou coluna usa `utf8` (utf8mb3).
- A insensibilidade a acentos (`ai`) foi mantida: "Jose" encontra "José".
- Espaços no fim deixaram de ser ignorados em comparações (`0900`): a migração remove espaços nas pontas dos
  nomes de eixo antes de unificá-los.

## 8. DDL de referência

O DDL completo está em [`Database/Empodera.schema.sql`](../Database/Empodera.schema.sql), em quatro partes:
tabelas (só colunas) → PK/`AUTO_INCREMENT`, `UNIQUE` e `CHECK` → FKs → dados iniciais.

## 9. Checklist para as 34 tabelas

| Item | Situação | Como é verificado |
| --- | --- | --- |
| Executar `SELECT VERSION();` e registrar | ✅ 8.4.11 e 9.7.1 (validação), 8.4 (CI e servidor) | seção 1 |
| Executar o script completo num banco vazio, sem erros | ✅ | todos os testes de backend criam o banco com ele |
| PKs `INT UNSIGNED AUTO_INCREMENT`, via `ALTER TABLE` | ✅ 34/34 | `RevisaoDdlTests` |
| Toda coluna que referencia outra tabela tem FK nomeada | ✅ 57 FKs | `RevisaoDdlTests` |
| Regra `ON DELETE` de cada FK decidida e documentada | ✅ (1 ponto pendente) | seção 2.2 |
| Colunas de FK com tipo idêntico ao da PK | ✅ | `RevisaoDdlTests` |
| Nenhum dado órfão antes da criação das FKs | ✅ | a migração verifica e para se houver |
| Colunas obrigatórias como `NOT NULL` | ✅ | modelo + migração verifica nulos |
| Faixas de valores com `CHECK` nomeado | ✅ 47 | seção 4 |
| Unicidade com `UNIQUE` nomeado, onde o negócio exigir | ✅ 14 (pendente: nome de perfil) | seção 4 |
| Tamanho de cada `VARCHAR` justificado | ✅ | seção 5 |
| Nenhum `INT(n)`; `DATETIME` sem precisão | ✅ | `RevisaoDdlTests` |
| Sim/Não como `TINYINT` ou `CHAR(1)` com `CHECK` | ✅ | seção 5 |
| Senhas armazenadas apenas como hash | ✅ (já eram; coluna não alterada) | seção 5 |
| Colunas de auditoria com o mesmo critério | ✅ `DtCriacao NOT NULL DEFAULT CURRENT_TIMESTAMP` e `DtModificacao NULL ON UPDATE CURRENT_TIMESTAMP` nas 34 tabelas | script |
| Nomes no padrão da seção 6 | ✅ | `RevisaoDdlTests` |
| Nenhuma tabela/coluna `utf8`; collation herdado | ✅ | `RevisaoDdlTests` |
| Nenhum `SET FOREIGN_KEY_CHECKS = 0` no script final | ✅ | script |

Resultado das consultas de verificação da revisão, no banco criado pelo script:

| Consulta | Resultado |
| --- | --- |
| Restrições por tipo (`TABLE_CONSTRAINTS`) | 34 `PRIMARY KEY`, 57 `FOREIGN KEY`, 47 `CHECK`, 14 `UNIQUE` |
| FKs existentes (`REFERENTIAL_CONSTRAINTS`) | 57, todas nomeadas |
| Colunas com collation fora do padrão | nenhuma |
| Colunas `VARCHAR(255)` | 17 (papel social, interesse, complemento, endereço da ficha, estrutura familiar, localização, informação extra e caminho de foto), todas justificadas na seção 5 |

---

## Migração dos dados

O banco em produção usa as chaves antigas (UUID em `BINARY(16)`) e os nomes antigos de colunas. A migração
copia os dados para o esquema novo **sem perder registros nem vínculos**.

**Passo a passo** (com a aplicação parada):

1. Faça backup do banco atual (`mariadb-dump`/`mysqldump`).
2. Num servidor **MySQL 8.4 ou posterior**, importe o banco atual num schema chamado `empodera_antigo`.
   Se o servidor atual ainda não tiver as colunas `LocalSecundario`, `LocalMapaSecundario` e `TipoRede`,
   aplique antes os scripts incrementais de `Database/2026*.sql`.
3. Crie o banco novo: `CREATE DATABASE empodera;` e execute `Database/Empodera.schema.sql` nele.
4. Execute `mysql -u <usuario> -p < Database/migracao-uuid-para-int.sql`.
5. Renomeie as fotos e as cores dos usuários em `App_Data` com a lista que o script mostra no fim:

   ```bash
   # cole a lista "Nome antigo / Nome novo" num arquivo mapa.txt (sem o cabeçalho)
   while read antigo novo; do
     for f in App_Data/fotos-usuarios/$antigo.*; do [ -e "$f" ] && mv "$f" "App_Data/fotos-usuarios/$novo.${f##*.}"; done
     [ -e "App_Data/temas-usuarios/$antigo.json" ] && mv "App_Data/temas-usuarios/$antigo.json" "App_Data/temas-usuarios/$novo.json"
   done < mapa.txt
   ```
6. Aponte a aplicação para o banco novo e inicie. Quem estava logado precisa entrar de novo.

**O que o script faz:**

- **Verifica antes de gravar.** Procura registros órfãos em referências obrigatórias, campos obrigatórios
  vazios, textos maiores que o novo tamanho, valores fora dos `CHECK` e duplicidades. Se encontrar qualquer
  problema, mostra a lista e **para sem gravar nada**.
- **Numera os registros** de cada tabela na ordem de criação (`DtCriacao`) e traduz todas as FKs pelo mapa
  de Ids. Uma referência opcional quebrada (inclusive o UUID zerado) vira `NULL`.
- **Converte valores:** "Sim"/"Não" → `'S'`/`'N'`; datas sem horário → `DATE`.
- **Atualiza as menções** do Diário de Processo Pessoal (`@[Nome](ator:<uuid>)` → `@[Nome](ator:<novo id>)`),
  preservando a data de modificação.
- **Unifica eixos** com o mesmo nome e remove vínculos repetidos (que os novos `UNIQUE` não aceitariam).
- **Grava tudo numa transação:** se algo falhar, nada é gravado.

**Validação feita:** com o último dump da equipe convertido para UUID (283 atores, 24 comunidades,
44 fichas, 31 diários de campo, 111 recursos...), a migração copiou as 34 tabelas com **o mesmo número de
registros**. A aplicação rodou sobre o banco migrado sem erros: listagens, detalhes, menções viraram links
para os atores certos, respostas "Sim" das fichas aparecem marcadas, e criar ator, perfil, foto e cores
funcionou. Casos especiais também foram testados com dados sintéticos: eixo repetido com espaço no fim,
rede duplicada, perfil com criador zerado, duas menções no mesmo texto e fonte de informação ligada pela coluna antiga.

Na primeira execução, a verificação encontrou 9 valores reais maiores que os tamanhos inicialmente
escolhidos (papéis sociais longos, um endereço com horários e um telefone com e-mail). Os tamanhos foram
revistos com base nesses dados, sem cortar informação.

---

## Pontos para discutir com o orientador

1. `ON DELETE` ao excluir uma atividade: manter `CASCADE` (atual) ou usar `RESTRICT`.
2. `UNIQUE` em `perfis.Nome`: hoje a aplicação permite nomes repetidos.
3. Provedor das ações (`Provedor`, `ProvedorExterno`): manter texto livre ou criar tabela própria (exige mudança de tela).
4. Coluna `Senha`: manter `LONGTEXT` (decisão atual da equipe) ou adotar `VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin`.
5. Servidor de produção: a equipe vai para uma VPS (Hostinger) com MySQL 8.4 LTS; roteiro em [servidor-hostinger.md](servidor-hostinger.md).
