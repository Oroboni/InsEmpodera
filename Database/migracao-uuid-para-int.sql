-- =====================================================================
-- Migração dos dados: chaves UUID (BINARY(16)) -> INT UNSIGNED AUTO_INCREMENT
-- Gerado por scripts/GerarSchema. Não edite à mão.
--
-- Antes de rodar (MySQL 8.4 LTS ou posterior):
--   1. Faça backup do banco atual.
--   2. Importe o banco atual num schema chamado `empodera_antigo`.
--   3. Crie o banco novo `empodera` com Database/Empodera.schema.sql e NÃO
--      inicie a aplicação antes desta migração.
--   4. Rode:  mysql -u <usuario> -p < Database/migracao-uuid-para-int.sql
--
-- O script para sem gravar nada se encontrar problemas nos dados antigos
-- (registros órfãos, textos maiores que o novo tamanho, valores fora do
-- domínio, campos obrigatórios vazios ou duplicidades). A lista aparece
-- na tela; corrija no banco antigo e rode de novo.
--
-- Os novos Ids seguem a ordem de criação (DtCriacao) de cada tabela.
-- Detalhes: docs/revisao-ddl-mysql.md, seção "Migração dos dados".
-- =====================================================================

USE empodera;

-- ---------------------------------------------------------------------
-- 1. Verificações (nada é gravado se houver problema)
-- ---------------------------------------------------------------------

DROP TEMPORARY TABLE IF EXISTS migracao_problemas;
CREATE TEMPORARY TABLE migracao_problemas (
    problema   VARCHAR(255) NOT NULL,
    quantidade INT UNSIGNED NOT NULL
);

-- O banco novo só pode ter os dados iniciais do script de criação.
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em acoes', n FROM (SELECT COUNT(*) AS n FROM empodera.acoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em acoesatores', n FROM (SELECT COUNT(*) AS n FROM empodera.acoesatores) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em anexosdiario', n FROM (SELECT COUNT(*) AS n FROM empodera.anexosdiario) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em atividades', n FROM (SELECT COUNT(*) AS n FROM empodera.atividades) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em atividadeseixo', n FROM (SELECT COUNT(*) AS n FROM empodera.atividadeseixo) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em atorcomunidades', n FROM (SELECT COUNT(*) AS n FROM empodera.atorcomunidades) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em atores', n FROM (SELECT COUNT(*) AS n FROM empodera.atores) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em avaliacaopessoal', n FROM (SELECT COUNT(*) AS n FROM empodera.avaliacaopessoal) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em comunidades', n FROM (SELECT COUNT(*) AS n FROM empodera.comunidades) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em daatores', n FROM (SELECT COUNT(*) AS n FROM empodera.daatores) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em detalhesdacoes', n FROM (SELECT COUNT(*) AS n FROM empodera.detalhesdacoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em detalheseixos', n FROM (SELECT COUNT(*) AS n FROM empodera.detalheseixos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diarioacoes', n FROM (SELECT COUNT(*) AS n FROM empodera.diarioacoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diariodacoes', n FROM (SELECT COUNT(*) AS n FROM empodera.diariodacoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diarioeixos', n FROM (SELECT COUNT(*) AS n FROM empodera.diarioeixos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diarioscampo', n FROM (SELECT COUNT(*) AS n FROM empodera.diarioscampo) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diariosprocessoeixos', n FROM (SELECT COUNT(*) AS n FROM empodera.diariosprocessoeixos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em diariosprocessopessoal', n FROM (SELECT COUNT(*) AS n FROM empodera.diariosprocessopessoal) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em eixos', n FROM (SELECT COUNT(*) AS n FROM empodera.eixos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em fichacondicoes', n FROM (SELECT COUNT(*) AS n FROM empodera.fichacondicoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em fichapeticoes', n FROM (SELECT COUNT(*) AS n FROM empodera.fichapeticoes) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em ficharespostas', n FROM (SELECT COUNT(*) AS n FROM empodera.ficharespostas) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em ficharesultados', n FROM (SELECT COUNT(*) AS n FROM empodera.ficharesultados) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em fichasprimeirocontato', n FROM (SELECT COUNT(*) AS n FROM empodera.fichasprimeirocontato) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em fontesinfo', n FROM (SELECT COUNT(*) AS n FROM empodera.fontesinfo) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em recursosatores', n FROM (SELECT COUNT(*) AS n FROM empodera.recursosatores) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em redeeixos', n FROM (SELECT COUNT(*) AS n FROM empodera.redeeixos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em rederecursos', n FROM (SELECT COUNT(*) AS n FROM empodera.rederecursos) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em redesprimarias', n FROM (SELECT COUNT(*) AS n FROM empodera.redesprimarias) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em vulnerabilidades', n FROM (SELECT COUNT(*) AS n FROM empodera.vulnerabilidades) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'banco novo já tem dados em vulnerabilidadeseixo', n FROM (SELECT COUNT(*) AS n FROM empodera.vulnerabilidadeseixo) x WHERE n > 0;
-- acoes
INSERT INTO migracao_problemas SELECT 'acoes.FkIdAtividade aponta para atividades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes c LEFT JOIN empodera_antigo.atividades p ON p.IdAtividade = c.FkIdAtividade WHERE c.FkIdAtividade IS NOT NULL AND p.IdAtividade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.FkIdAtividade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE FkIdAtividade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Provedor vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE Provedor IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Provedor com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE CHAR_LENGTH(Provedor) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Quantidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE Quantidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoes.Quantidade fora da regra CK_Acoes_Quantidade', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoes WHERE Quantidade IS NOT NULL AND NOT (Quantidade >= 0)) x WHERE n > 0;
-- acoesatores
INSERT INTO migracao_problemas SELECT 'acoesatores.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoesatores c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoesatores.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoesatores WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoesatores.FkIdAcoes aponta para acoes inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoesatores c LEFT JOIN empodera_antigo.acoes p ON p.IdAcoes = c.FkIdAcoes WHERE c.FkIdAcoes IS NOT NULL AND p.IdAcoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'acoesatores.FkIdAcoes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.acoesatores WHERE FkIdAcoes IS NULL) x WHERE n > 0;
-- anexosdiario
INSERT INTO migracao_problemas SELECT 'anexosdiario.FkIdDiario aponta para diarioscampo inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.anexosdiario c LEFT JOIN empodera_antigo.diarioscampo p ON p.IdDCampo = c.FkIdDiario WHERE c.FkIdDiario IS NOT NULL AND p.IdDCampo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'anexosdiario.FkIdDiario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.anexosdiario WHERE FkIdDiario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'anexosdiario.Caminho vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.anexosdiario WHERE Caminho IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'anexosdiario.Caminho com mais de 500 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.anexosdiario WHERE CHAR_LENGTH(Caminho) > 500) x WHERE n > 0;
-- atividades
INSERT INTO migracao_problemas SELECT 'atividades.FkIdComunidade aponta para comunidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades c LEFT JOIN empodera_antigo.comunidades p ON p.Id_Comunidade = c.FkIdComunidade WHERE c.FkIdComunidade IS NOT NULL AND p.Id_Comunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.FkIdComunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE FkIdComunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.Descricao com mais de 2000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE CHAR_LENGTH(Descricao) > 2000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.Foto com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE CHAR_LENGTH(Foto) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividades.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividades WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
-- atividadeseixo
INSERT INTO migracao_problemas SELECT 'atividadeseixo.FkIdAtividade aponta para atividades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividadeseixo c LEFT JOIN empodera_antigo.atividades p ON p.IdAtividade = c.FkIdAtividade WHERE c.FkIdAtividade IS NOT NULL AND p.IdAtividade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividadeseixo.FkIdAtividade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividadeseixo WHERE FkIdAtividade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividadeseixo.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividadeseixo c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atividadeseixo.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atividadeseixo WHERE FkIdEixo IS NULL) x WHERE n > 0;
-- atorcomunidades
INSERT INTO migracao_problemas SELECT 'atorcomunidades.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atorcomunidades c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atorcomunidades.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atorcomunidades WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atorcomunidades.FkIdComunidade aponta para comunidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atorcomunidades c LEFT JOIN empodera_antigo.comunidades p ON p.Id_Comunidade = c.FkIdComunidade WHERE c.FkIdComunidade IS NOT NULL AND p.Id_Comunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atorcomunidades.FkIdComunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atorcomunidades WHERE FkIdComunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atorcomunidades.TipoRelacionamento fora da regra CK_AtorComunidades_TipoRelacionamento', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atorcomunidades WHERE TipoRelacionamento IS NOT NULL AND NOT (TipoRelacionamento BETWEEN 1 AND 5)) x WHERE n > 0;
-- atores
INSERT INTO migracao_problemas SELECT 'atores.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Ativo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Ativo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Ativo com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(Ativo) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Ativo fora da regra CK_Atores_Ativo', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Ativo IS NOT NULL AND NOT (Ativo COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.DaEquipe vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE DaEquipe IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.DaEquipe fora da regra CK_Atores_DaEquipe', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE DaEquipe IS NOT NULL AND NOT (DaEquipe IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Genero fora da regra CK_Atores_Genero', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Genero IS NOT NULL AND NOT (Genero BETWEEN 0 AND 3)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Idade fora da regra CK_Atores_Idade', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Idade IS NOT NULL AND NOT (Idade <= 130)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Interesse com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(Interesse) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Lopiniao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Lopiniao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Lopiniao fora da regra CK_Atores_Lopiniao', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Lopiniao IS NOT NULL AND NOT (Lopiniao IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Mcomunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Mcomunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Mcomunidade fora da regra CK_Atores_Mcomunidade', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Mcomunidade IS NOT NULL AND NOT (Mcomunidade IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.PapelSocial1 com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(PapelSocial1) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.PapelSocial2 com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(PapelSocial2) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Rope vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Rope IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Rope fora da regra CK_Atores_Rope', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE Rope IS NOT NULL AND NOT (Rope IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'atores.Telefone com mais de 60 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.atores WHERE CHAR_LENGTH(Telefone) > 60) x WHERE n > 0;
-- avaliacaopessoal
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.AssBasica vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE AssBasica IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.AssBasica fora da regra CK_AvaliacaoPessoal_AssBasica', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE AssBasica IS NOT NULL AND NOT (AssBasica BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.CCrimes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE CCrimes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.CCrimes fora da regra CK_AvaliacaoPessoal_CCrimes', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE CCrimes IS NOT NULL AND NOT (CCrimes BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Educacao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Educacao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Educacao fora da regra CK_AvaliacaoPessoal_Educacao', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Educacao IS NOT NULL AND NOT (Educacao BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Lazer vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Lazer IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Lazer fora da regra CK_AvaliacaoPessoal_Lazer', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Lazer IS NOT NULL AND NOT (Lazer BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Moradia vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Moradia IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Moradia fora da regra CK_AvaliacaoPessoal_Moradia', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Moradia IS NOT NULL AND NOT (Moradia BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Ocupacao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Ocupacao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Ocupacao fora da regra CK_AvaliacaoPessoal_Ocupacao', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Ocupacao IS NOT NULL AND NOT (Ocupacao BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Prevencao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Prevencao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Prevencao fora da regra CK_AvaliacaoPessoal_Prevencao', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Prevencao IS NOT NULL AND NOT (Prevencao BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Saude vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Saude IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Saude fora da regra CK_AvaliacaoPessoal_Saude', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Saude IS NOT NULL AND NOT (Saude BETWEEN 1 AND 5)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Substancias vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Substancias IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'avaliacaopessoal.Substancias fora da regra CK_AvaliacaoPessoal_Substancias', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.avaliacaopessoal WHERE Substancias IS NOT NULL AND NOT (Substancias BETWEEN 1 AND 5)) x WHERE n > 0;
-- comunidades
INSERT INTO migracao_problemas SELECT 'comunidades.FK_Id_Usuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FK_Id_Usuario WHERE c.FK_Id_Usuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.FK_Id_Usuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE FK_Id_Usuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Ativo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE Ativo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Ativo com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Ativo) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Ativo fora da regra CK_Comunidades_Ativo', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE Ativo IS NOT NULL AND NOT (Ativo COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Complemento com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Complemento) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Descricao com mais de 5000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Descricao) > 5000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Descricao_Acessibilidade com mais de 2000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Descricao_Acessibilidade) > 2000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Local com mais de 500 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Local) > 500) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.LocalMapa com mais de 500 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(LocalMapa) > 500) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.LocalMapaSecundario com mais de 500 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(LocalMapaSecundario) > 500) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.LocalSecundario com mais de 500 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(LocalSecundario) > 500) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Status vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE Status IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'comunidades.Status com mais de 30 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.comunidades WHERE CHAR_LENGTH(Status) > 30) x WHERE n > 0;
-- daatores
INSERT INTO migracao_problemas SELECT 'daatores.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.daatores c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'daatores.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.daatores WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'daatores.FkIdDDacoes aponta para diariodacoes inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.daatores c LEFT JOIN empodera_antigo.diariodacoes p ON p.IdDAcoes = c.FkIdDDacoes WHERE c.FkIdDDacoes IS NOT NULL AND p.IdDAcoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'daatores.FkIdDDacoes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.daatores WHERE FkIdDDacoes IS NULL) x WHERE n > 0;
-- detalhesdacoes
INSERT INTO migracao_problemas SELECT 'detalhesdacoes.FkIdDDacoes aponta para diariodacoes inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalhesdacoes c LEFT JOIN empodera_antigo.diariodacoes p ON p.IdDAcoes = c.FkIdDDacoes WHERE c.FkIdDDacoes IS NOT NULL AND p.IdDAcoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalhesdacoes.FkIdDDacoes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalhesdacoes WHERE FkIdDDacoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalhesdacoes.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalhesdacoes WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalhesdacoes.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalhesdacoes WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
-- detalheseixos
INSERT INTO migracao_problemas SELECT 'detalheseixos.FkIdDetalhes aponta para detalhesdacoes inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalheseixos c LEFT JOIN empodera_antigo.detalhesdacoes p ON p.Id = c.FkIdDetalhes WHERE c.FkIdDetalhes IS NOT NULL AND p.Id IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalheseixos.FkIdDetalhes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalheseixos WHERE FkIdDetalhes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalheseixos.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalheseixos c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'detalheseixos.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.detalheseixos WHERE FkIdEixo IS NULL) x WHERE n > 0;
-- diarioacoes
INSERT INTO migracao_problemas SELECT 'diarioacoes.FkIdAcoes aponta para acoes inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioacoes c LEFT JOIN empodera_antigo.acoes p ON p.IdAcoes = c.FkIdAcoes WHERE c.FkIdAcoes IS NOT NULL AND p.IdAcoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioacoes.FkIdAcoes vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioacoes WHERE FkIdAcoes IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioacoes.FkIdDiario aponta para diarioscampo inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioacoes c LEFT JOIN empodera_antigo.diarioscampo p ON p.IdDCampo = c.FkIdDiario WHERE c.FkIdDiario IS NOT NULL AND p.IdDCampo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioacoes.FkIdDiario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioacoes WHERE FkIdDiario IS NULL) x WHERE n > 0;
-- diariodacoes
INSERT INTO migracao_problemas SELECT 'diariodacoes.FkIdDiario aponta para diarioscampo inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes c LEFT JOIN empodera_antigo.diarioscampo p ON p.IdDCampo = c.FkIdDiario WHERE c.FkIdDiario IS NOT NULL AND p.IdDCampo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.FkIdDiario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE FkIdDiario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.PeovedorEx vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE PeovedorEx IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.PeovedorEx com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE CHAR_LENGTH(PeovedorEx) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.Quantidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE Quantidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariodacoes.Quantidade fora da regra CK_DiarioDAcoes_Quantidade', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariodacoes WHERE Quantidade IS NOT NULL AND NOT (Quantidade >= 0)) x WHERE n > 0;
-- diarioeixos
INSERT INTO migracao_problemas SELECT 'diarioeixos.FkIdDiario aponta para diarioscampo inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioeixos c LEFT JOIN empodera_antigo.diarioscampo p ON p.IdDCampo = c.FkIdDiario WHERE c.FkIdDiario IS NOT NULL AND p.IdDCampo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioeixos.FkIdDiario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioeixos WHERE FkIdDiario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioeixos.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioeixos c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioeixos.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioeixos WHERE FkIdEixo IS NULL) x WHERE n > 0;
-- diarioscampo
INSERT INTO migracao_problemas SELECT 'diarioscampo.FkIdComunidade aponta para comunidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo c LEFT JOIN empodera_antigo.comunidades p ON p.Id_Comunidade = c.FkIdComunidade WHERE c.FkIdComunidade IS NOT NULL AND p.Id_Comunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.FkIdComunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE FkIdComunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.Data vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE DATE(Data) IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.Descricao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE Descricao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.Descricao com mais de 8000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE CHAR_LENGTH(Descricao) > 8000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.Foto com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE CHAR_LENGTH(Foto) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diarioscampo.Localizacao com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diarioscampo WHERE CHAR_LENGTH(Localizacao) > 255) x WHERE n > 0;
-- diariosprocessoeixos
INSERT INTO migracao_problemas SELECT 'diariosprocessoeixos.FkIdDiarioProcesso aponta para diariosprocessopessoal inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessoeixos c LEFT JOIN empodera_antigo.diariosprocessopessoal p ON p.IdDiarioProcesso = c.FkIdDiarioProcesso WHERE c.FkIdDiarioProcesso IS NOT NULL AND p.IdDiarioProcesso IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessoeixos.FkIdDiarioProcesso vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessoeixos WHERE FkIdDiarioProcesso IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessoeixos.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessoeixos c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessoeixos.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessoeixos WHERE FkIdEixo IS NULL) x WHERE n > 0;
-- diariosprocessopessoal
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.Data vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal WHERE DATE(Data) IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.Descricao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal WHERE Descricao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'diariosprocessopessoal.Descricao com mais de 4000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.diariosprocessopessoal WHERE CHAR_LENGTH(Descricao) > 4000) x WHERE n > 0;
-- eixos
INSERT INTO migracao_problemas SELECT 'eixos.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.eixos WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'eixos.Nome com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.eixos WHERE CHAR_LENGTH(Nome) > 100) x WHERE n > 0;
-- fichacondicoes
INSERT INTO migracao_problemas SELECT 'fichacondicoes.FkIdFicha aponta para fichasprimeirocontato inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichacondicoes c LEFT JOIN empodera_antigo.fichasprimeirocontato p ON p.IdFicha = c.FkIdFicha WHERE c.FkIdFicha IS NOT NULL AND p.IdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichacondicoes.FkIdFicha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichacondicoes WHERE FkIdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichacondicoes.Cond vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichacondicoes WHERE Cond IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichacondicoes.Cond com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichacondicoes WHERE CHAR_LENGTH(Cond) > 150) x WHERE n > 0;
-- fichapeticoes
INSERT INTO migracao_problemas SELECT 'fichapeticoes.FkIdFicha aponta para fichasprimeirocontato inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichapeticoes c LEFT JOIN empodera_antigo.fichasprimeirocontato p ON p.IdFicha = c.FkIdFicha WHERE c.FkIdFicha IS NOT NULL AND p.IdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichapeticoes.FkIdFicha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichapeticoes WHERE FkIdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichapeticoes.Pet vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichapeticoes WHERE Pet IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichapeticoes.Pet com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichapeticoes WHERE CHAR_LENGTH(Pet) > 150) x WHERE n > 0;
-- ficharespostas
INSERT INTO migracao_problemas SELECT 'ficharespostas.FkIdFicha aponta para fichasprimeirocontato inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharespostas c LEFT JOIN empodera_antigo.fichasprimeirocontato p ON p.IdFicha = c.FkIdFicha WHERE c.FkIdFicha IS NOT NULL AND p.IdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharespostas.FkIdFicha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharespostas WHERE FkIdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharespostas.Resp vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharespostas WHERE Resp IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharespostas.Resp com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharespostas WHERE CHAR_LENGTH(Resp) > 150) x WHERE n > 0;
-- ficharesultados
INSERT INTO migracao_problemas SELECT 'ficharesultados.FkIdFicha aponta para fichasprimeirocontato inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharesultados c LEFT JOIN empodera_antigo.fichasprimeirocontato p ON p.IdFicha = c.FkIdFicha WHERE c.FkIdFicha IS NOT NULL AND p.IdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharesultados.FkIdFicha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharesultados WHERE FkIdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharesultados.Result vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharesultados WHERE Result IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'ficharesultados.Result com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.ficharesultados WHERE CHAR_LENGTH(Result) > 150) x WHERE n > 0;
-- fichasprimeirocontato
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.AEscolar fora da regra CK_FichasPrimeiroContato_AEscolar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE AEscolar IS NOT NULL AND NOT (AEscolar <= 30)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.CEstabeleceu com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(CEstabeleceu) > 100) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Coment com mais de 2000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Coment) > 2000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Complemento com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Complemento) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.DtContato vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE DATE(DtContato) IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Emprego com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Emprego) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Endereco com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Endereco) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.EstaFamiliar com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(EstaFamiliar) > 100) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.EstruFamiliar com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(EstruFamiliar) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FonteDados com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(FonteDados) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FornecidoParceiro com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(FornecidoParceiro) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.FornecidoParceiro fora da regra CK_FichasPrimeiroContato_FornecidoParceiro', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CASE FornecidoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE FornecidoParceiro END IS NOT NULL AND NOT (CASE FornecidoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE FornecidoParceiro END COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.HoraContato vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE HoraContato IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.LContato com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(LContato) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.LTrat com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(LTrat) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.NFIlhos fora da regra CK_FichasPrimeiroContato_NFilhos', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE NFIlhos IS NOT NULL AND NOT (NFIlhos <= 30)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.NFilhas fora da regra CK_FichasPrimeiroContato_NFilhas', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE NFilhas IS NOT NULL AND NOT (NFilhas <= 30)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.NovoParceiro com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(NovoParceiro) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.NovoParceiro fora da regra CK_FichasPrimeiroContato_NovoParceiro', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CASE NovoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE NovoParceiro END IS NOT NULL AND NOT (CASE NovoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE NovoParceiro END COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.QReabili fora da regra CK_FichasPrimeiroContato_QReabili', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE QReabili IS NOT NULL AND NOT (QReabili <= 100)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SCalc com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(SCalc) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SCalc fora da regra CK_FichasPrimeiroContato_SCalc', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CASE SCalc WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SCalc END IS NOT NULL AND NOT (CASE SCalc WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SCalc END COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SComp com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(SComp) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SComp fora da regra CK_FichasPrimeiroContato_SComp', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CASE SComp WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SComp END IS NOT NULL AND NOT (CASE SComp WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SComp END COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SLer com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(SLer) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.SLer fora da regra CK_FichasPrimeiroContato_SLer', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CASE SLer WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SLer END IS NOT NULL AND NOT (CASE SLer WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE SLer END COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Status com mais de 20 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Status) > 20) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Status fora da regra CK_FichasPrimeiroContato_Status', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE Status IS NOT NULL AND NOT (Status IN ('EmProgresso', 'Concluida', 'Abandonada'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fichasprimeirocontato.Telefone com mais de 60 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fichasprimeirocontato WHERE CHAR_LENGTH(Telefone) > 60) x WHERE n > 0;
-- fontesinfo
INSERT INTO migracao_problemas SELECT 'fontesinfo.FkIdFicha aponta para fichasprimeirocontato inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo c LEFT JOIN empodera_antigo.fichasprimeirocontato p ON p.IdFicha = c.FkIdFicha WHERE c.FkIdFicha IS NOT NULL AND p.IdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.FkIdFicha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE FkIdFicha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Extra vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Extra IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Extra com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(Extra) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Genero fora da regra CK_FontesInfo_Genero', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Genero IS NOT NULL AND NOT (Genero BETWEEN 0 AND 3)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Idade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Idade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Idade fora da regra CK_FontesInfo_Idade', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Idade IS NOT NULL AND NOT (Idade <= 130)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Interesse vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Interesse IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Interesse com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(Interesse) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.PapelSocial1 vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE PapelSocial1 IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.PapelSocial1 com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(PapelSocial1) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.PapelSocial2 vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE PapelSocial2 IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.PapelSocial2 com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(PapelSocial2) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Telefone vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE Telefone IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'fontesinfo.Telefone com mais de 60 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.fontesinfo WHERE CHAR_LENGTH(Telefone) > 60) x WHERE n > 0;
-- perfis
INSERT INTO migracao_problemas SELECT 'perfis.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.perfis WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'perfis.Nome com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.perfis WHERE CHAR_LENGTH(Nome) > 100) x WHERE n > 0;
-- permissoes
INSERT INTO migracao_problemas SELECT 'permissoes.FkIdPerfil aponta para perfis inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes c LEFT JOIN empodera_antigo.perfis p ON p.IdPerfil = c.FkIdPerfil WHERE c.FkIdPerfil IS NOT NULL AND p.IdPerfil IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.FkIdPerfil vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE FkIdPerfil IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.Modulo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE Modulo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.Modulo com mais de 50 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(Modulo) > 50) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeAtualizar vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeAtualizar IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeAtualizar com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(PodeAtualizar) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeAtualizar fora da regra CK_Permissoes_PodeAtualizar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeAtualizar IS NOT NULL AND NOT (PodeAtualizar COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeCriar vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeCriar IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeCriar com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(PodeCriar) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeCriar fora da regra CK_Permissoes_PodeCriar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeCriar IS NOT NULL AND NOT (PodeCriar COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDeletar vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeDeletar IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDeletar com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(PodeDeletar) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDeletar fora da regra CK_Permissoes_PodeDeletar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeDeletar IS NOT NULL AND NOT (PodeDeletar COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDetalhar vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeDetalhar IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDetalhar com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(PodeDetalhar) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeDetalhar fora da regra CK_Permissoes_PodeDetalhar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeDetalhar IS NOT NULL AND NOT (PodeDetalhar COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeListar vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeListar IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeListar com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE CHAR_LENGTH(PodeListar) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'permissoes.PodeListar fora da regra CK_Permissoes_PodeListar', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.permissoes WHERE PodeListar IS NOT NULL AND NOT (PodeListar COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
-- recursosatores
INSERT INTO migracao_problemas SELECT 'recursosatores.FK_id_Atores aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FK_id_Atores WHERE c.FK_id_Atores IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.FK_id_Atores vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE FK_id_Atores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Pode com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE CHAR_LENGTH(Pode) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Pode fora da regra CK_RecursosAtores_Pode', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE Pode IS NOT NULL AND NOT (Pode COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Tipo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE Tipo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'recursosatores.Tipo com mais de 50 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.recursosatores WHERE CHAR_LENGTH(Tipo) > 50) x WHERE n > 0;
-- redeeixos
INSERT INTO migracao_problemas SELECT 'redeeixos.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redeeixos c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redeeixos.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redeeixos WHERE FkIdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redeeixos.FkIdRede aponta para rederecursos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redeeixos c LEFT JOIN empodera_antigo.rederecursos p ON p.Id_Rede = c.FkIdRede WHERE c.FkIdRede IS NOT NULL AND p.Id_Rede IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redeeixos.FkIdRede vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redeeixos WHERE FkIdRede IS NULL) x WHERE n > 0;
-- rederecursos
INSERT INTO migracao_problemas SELECT 'rederecursos.FkIdComunidade aponta para comunidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos c LEFT JOIN empodera_antigo.comunidades p ON p.Id_Comunidade = c.FkIdComunidade WHERE c.FkIdComunidade IS NOT NULL AND p.Id_Comunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.FkIdComunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE FkIdComunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.FkIdUsuario aponta para usuarios inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos c LEFT JOIN empodera_antigo.usuarios p ON p.IdUsuario = c.FkIdUsuario WHERE c.FkIdUsuario IS NOT NULL AND p.IdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.FkIdUsuario vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE FkIdUsuario IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Dispositivo com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE CHAR_LENGTH(Dispositivo) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Localizacao com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE CHAR_LENGTH(Localizacao) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Servicos com mais de 2000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE CHAR_LENGTH(Servicos) > 2000) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Tipo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE Tipo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'rederecursos.Tipo com mais de 50 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.rederecursos WHERE CHAR_LENGTH(Tipo) > 50) x WHERE n > 0;
-- redesprimarias
INSERT INTO migracao_problemas SELECT 'redesprimarias.FkIdAtorPrincipal aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FkIdAtorPrincipal WHERE c.FkIdAtorPrincipal IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.FkIdAtorPrincipal vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE FkIdAtorPrincipal IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.FkIdAtorRelacionados aponta para atores inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias c LEFT JOIN empodera_antigo.atores p ON p.IdAtores = c.FkIdAtorRelacionados WHERE c.FkIdAtorRelacionados IS NOT NULL AND p.IdAtores IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.FkIdAtorRelacionados vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE FkIdAtorRelacionados IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.TipoRede vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE TipoRede IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.TipoRede com mais de 20 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE CHAR_LENGTH(TipoRede) > 20) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.TipoRede fora da regra CK_RedesPrimarias_TipoRede', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE TipoRede IS NOT NULL AND NOT (TipoRede IN ('Primaria', 'Subjetiva'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.TipoRelacao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE TipoRelacao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'redesprimarias.TipoRelacao com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.redesprimarias WHERE CHAR_LENGTH(TipoRelacao) > 100) x WHERE n > 0;
-- usuarios
INSERT INTO migracao_problemas SELECT 'usuarios.FkIdPerfil aponta para perfis inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios c LEFT JOIN empodera_antigo.perfis p ON p.IdPerfil = c.FkIdPerfil WHERE c.FkIdPerfil IS NOT NULL AND p.IdPerfil IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.FkIdPerfil vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE FkIdPerfil IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.AccessFailedCount vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE AccessFailedCount IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Ativo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Ativo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Ativo com mais de 1 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Ativo) > 1) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Ativo fora da regra CK_Usuarios_Ativo', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Ativo IS NOT NULL AND NOT (Ativo COLLATE utf8mb4_bin IN ('S', 'N'))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.ConcurrencyStamp vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE ConcurrencyStamp IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.ConcurrencyStamp com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(ConcurrencyStamp) > 100) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.DtNascimento vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE DATE(DtNascimento) IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Email vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Email IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Email com mais de 254 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Email) > 254) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.EmailConfirmed vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE EmailConfirmed IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.EmailConfirmed fora da regra CK_Usuarios_EmailConfirmed', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE EmailConfirmed IS NOT NULL AND NOT (EmailConfirmed IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Foto com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Foto) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Genero fora da regra CK_Usuarios_Genero', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Genero IS NOT NULL AND NOT (Genero BETWEEN 0 AND 3)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.IdiomaPreferido vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE IdiomaPreferido IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.IdiomaPreferido fora da regra CK_Usuarios_IdiomaPreferido', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE IdiomaPreferido IS NOT NULL AND NOT (IdiomaPreferido BETWEEN 0 AND 3)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.LockoutEnabled vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE LockoutEnabled IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.LockoutEnabled fora da regra CK_Usuarios_LockoutEnabled', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE LockoutEnabled IS NOT NULL AND NOT (LockoutEnabled IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.NormalizedEmail vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE NormalizedEmail IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.NormalizedEmail com mais de 254 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(NormalizedEmail) > 254) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.NormalizedUserName vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE NormalizedUserName IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.NormalizedUserName com mais de 254 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(NormalizedUserName) > 254) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Ocupacao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Ocupacao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Ocupacao com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Ocupacao) > 100) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.PhoneNumber com mais de 60 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(PhoneNumber) > 60) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.PhoneNumberConfirmed vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE PhoneNumberConfirmed IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.PhoneNumberConfirmed fora da regra CK_Usuarios_PhoneNumberConfirmed', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE PhoneNumberConfirmed IS NOT NULL AND NOT (PhoneNumberConfirmed IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.SecurityStamp vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE SecurityStamp IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.SecurityStamp com mais de 100 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(SecurityStamp) > 100) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Senha vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE Senha IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Senha com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(Senha) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.Senha com caracteres fora do ASCII', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CAST(Senha AS BINARY) <> CAST(CONVERT(Senha USING ascii) AS BINARY)) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.TwoFactorEnabled vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE TwoFactorEnabled IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.TwoFactorEnabled fora da regra CK_Usuarios_TwoFactorEnabled', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE TwoFactorEnabled IS NOT NULL AND NOT (TwoFactorEnabled IN (0, 1))) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.UserName vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE UserName IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios.UserName com mais de 254 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.usuarios WHERE CHAR_LENGTH(UserName) > 254) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios com NormalizedEmail repetido (UQ_Usuarios_NormalizedEmail)', n FROM (SELECT COUNT(*) AS n FROM (SELECT NormalizedEmail FROM empodera_antigo.usuarios GROUP BY NormalizedEmail HAVING COUNT(*) > 1) d) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'usuarios com NormalizedUserName repetido (UQ_Usuarios_NormalizedUserName)', n FROM (SELECT COUNT(*) AS n FROM (SELECT NormalizedUserName FROM empodera_antigo.usuarios GROUP BY NormalizedUserName HAVING COUNT(*) > 1) d) x WHERE n > 0;
-- vulnerabilidades
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.FkIdComunidade aponta para comunidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades c LEFT JOIN empodera_antigo.comunidades p ON p.Id_Comunidade = c.FkIdComunidade WHERE c.FkIdComunidade IS NOT NULL AND p.Id_Comunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.FkIdComunidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE FkIdComunidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Localizacao vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE Localizacao IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Localizacao com mais de 255 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE CHAR_LENGTH(Localizacao) > 255) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Nome vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE Nome IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Nome com mais de 150 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE CHAR_LENGTH(Nome) > 150) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Servicos vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE Servicos IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidades.Servicos com mais de 2000 caracteres', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidades WHERE CHAR_LENGTH(Servicos) > 2000) x WHERE n > 0;
-- vulnerabilidadeseixo
INSERT INTO migracao_problemas SELECT 'vulnerabilidadeseixo.FkIdEixo aponta para eixos inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidadeseixo c LEFT JOIN empodera_antigo.eixos p ON p.IdEixo = c.FkIdEixo WHERE c.FkIdEixo IS NOT NULL AND p.IdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidadeseixo.FkIdEixo vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidadeseixo WHERE FkIdEixo IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidadeseixo.FkIdVulnerabilidade aponta para vulnerabilidades inexistente', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidadeseixo c LEFT JOIN empodera_antigo.vulnerabilidades p ON p.IdVulnerabilidade = c.FkIdVulnerabilidade WHERE c.FkIdVulnerabilidade IS NOT NULL AND p.IdVulnerabilidade IS NULL) x WHERE n > 0;
INSERT INTO migracao_problemas SELECT 'vulnerabilidadeseixo.FkIdVulnerabilidade vazio (obrigatório)', n FROM (SELECT COUNT(*) AS n FROM empodera_antigo.vulnerabilidadeseixo WHERE FkIdVulnerabilidade IS NULL) x WHERE n > 0;

SELECT problema AS `Problema encontrado`, quantidade AS `Registros` FROM migracao_problemas;

DROP PROCEDURE IF EXISTS migracao_parar_se_houver_problemas;
DELIMITER //
CREATE PROCEDURE migracao_parar_se_houver_problemas()
BEGIN
    IF (SELECT COUNT(*) FROM migracao_problemas) > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Migração interrompida: corrija os problemas listados acima no banco antigo.';
    END IF;
END //
DELIMITER ;
CALL migracao_parar_se_houver_problemas();
DROP PROCEDURE migracao_parar_se_houver_problemas;


-- ---------------------------------------------------------------------
-- 2. Mapas de Ids (UUID antigo -> novo Id inteiro, na ordem de criação)
-- ---------------------------------------------------------------------
DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_eixos;
CREATE TABLE empodera_antigo.migracao_mapa_eixos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_eixos (antigo, novo, primeiro)
SELECT e.IdEixo, DENSE_RANK() OVER (ORDER BY p.primeiro), p.primeiro
FROM empodera_antigo.eixos e
JOIN (SELECT TRIM(Nome) AS nome, MIN(IdEixo) AS primeiro FROM empodera_antigo.eixos GROUP BY TRIM(Nome)) p
  ON p.nome = TRIM(e.Nome);
DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_perfis;
CREATE TABLE empodera_antigo.migracao_mapa_perfis (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_perfis (antigo, novo, primeiro) SELECT IdPerfil, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdPerfil), IdPerfil FROM empodera_antigo.perfis;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_permissoes;
CREATE TABLE empodera_antigo.migracao_mapa_permissoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_permissoes (antigo, novo, primeiro) SELECT IdPermissoes, ROW_NUMBER() OVER (ORDER BY IdPermissoes), IdPermissoes FROM empodera_antigo.permissoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_usuarios;
CREATE TABLE empodera_antigo.migracao_mapa_usuarios (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_usuarios (antigo, novo, primeiro) SELECT IdUsuario, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdUsuario), IdUsuario FROM empodera_antigo.usuarios;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_atores;
CREATE TABLE empodera_antigo.migracao_mapa_atores (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_atores (antigo, novo, primeiro) SELECT IdAtores, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdAtores), IdAtores FROM empodera_antigo.atores;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_avaliacaopessoal;
CREATE TABLE empodera_antigo.migracao_mapa_avaliacaopessoal (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_avaliacaopessoal (antigo, novo, primeiro) SELECT IdAvaliacao, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdAvaliacao), IdAvaliacao FROM empodera_antigo.avaliacaopessoal;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_comunidades;
CREATE TABLE empodera_antigo.migracao_mapa_comunidades (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_comunidades (antigo, novo, primeiro) SELECT Id_Comunidade, ROW_NUMBER() OVER (ORDER BY Dt_Criacao, Id_Comunidade), Id_Comunidade FROM empodera_antigo.comunidades;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_atividades;
CREATE TABLE empodera_antigo.migracao_mapa_atividades (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_atividades (antigo, novo, primeiro) SELECT IdAtividade, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdAtividade), IdAtividade FROM empodera_antigo.atividades;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_acoes;
CREATE TABLE empodera_antigo.migracao_mapa_acoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_acoes (antigo, novo, primeiro) SELECT IdAcoes, ROW_NUMBER() OVER (ORDER BY IdAcoes), IdAcoes FROM empodera_antigo.acoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_acoesatores;
CREATE TABLE empodera_antigo.migracao_mapa_acoesatores (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_acoesatores (antigo, novo, primeiro) SELECT IdAAtores, ROW_NUMBER() OVER (ORDER BY IdAAtores), IdAAtores FROM empodera_antigo.acoesatores;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_atividadeseixo;
CREATE TABLE empodera_antigo.migracao_mapa_atividadeseixo (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_atividadeseixo (antigo, novo, primeiro) SELECT IdAEixo, ROW_NUMBER() OVER (ORDER BY IdAEixo), IdAEixo FROM empodera_antigo.atividadeseixo;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_atorcomunidades;
CREATE TABLE empodera_antigo.migracao_mapa_atorcomunidades (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_atorcomunidades (antigo, novo, primeiro) SELECT IdAtorComunidade, ROW_NUMBER() OVER (ORDER BY IdAtorComunidade), IdAtorComunidade FROM empodera_antigo.atorcomunidades;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diarioscampo;
CREATE TABLE empodera_antigo.migracao_mapa_diarioscampo (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diarioscampo (antigo, novo, primeiro) SELECT IdDCampo, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdDCampo), IdDCampo FROM empodera_antigo.diarioscampo;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_anexosdiario;
CREATE TABLE empodera_antigo.migracao_mapa_anexosdiario (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_anexosdiario (antigo, novo, primeiro) SELECT IdAnexos, ROW_NUMBER() OVER (ORDER BY IdAnexos), IdAnexos FROM empodera_antigo.anexosdiario;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diarioacoes;
CREATE TABLE empodera_antigo.migracao_mapa_diarioacoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diarioacoes (antigo, novo, primeiro) SELECT IdDAcoes, ROW_NUMBER() OVER (ORDER BY IdDAcoes), IdDAcoes FROM empodera_antigo.diarioacoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diariodacoes;
CREATE TABLE empodera_antigo.migracao_mapa_diariodacoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diariodacoes (antigo, novo, primeiro) SELECT IdDAcoes, ROW_NUMBER() OVER (ORDER BY IdDAcoes), IdDAcoes FROM empodera_antigo.diariodacoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_daatores;
CREATE TABLE empodera_antigo.migracao_mapa_daatores (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_daatores (antigo, novo, primeiro) SELECT Id, ROW_NUMBER() OVER (ORDER BY Id), Id FROM empodera_antigo.daatores;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_detalhesdacoes;
CREATE TABLE empodera_antigo.migracao_mapa_detalhesdacoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_detalhesdacoes (antigo, novo, primeiro) SELECT Id, ROW_NUMBER() OVER (ORDER BY Id), Id FROM empodera_antigo.detalhesdacoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_detalheseixos;
CREATE TABLE empodera_antigo.migracao_mapa_detalheseixos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_detalheseixos (antigo, novo, primeiro) SELECT IdDiarioEixo, ROW_NUMBER() OVER (ORDER BY IdDiarioEixo), IdDiarioEixo FROM empodera_antigo.detalheseixos;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diarioeixos;
CREATE TABLE empodera_antigo.migracao_mapa_diarioeixos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diarioeixos (antigo, novo, primeiro) SELECT IdDiarioEixo, ROW_NUMBER() OVER (ORDER BY IdDiarioEixo), IdDiarioEixo FROM empodera_antigo.diarioeixos;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diariosprocessopessoal;
CREATE TABLE empodera_antigo.migracao_mapa_diariosprocessopessoal (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diariosprocessopessoal (antigo, novo, primeiro) SELECT IdDiarioProcesso, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdDiarioProcesso), IdDiarioProcesso FROM empodera_antigo.diariosprocessopessoal;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_diariosprocessoeixos;
CREATE TABLE empodera_antigo.migracao_mapa_diariosprocessoeixos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_diariosprocessoeixos (antigo, novo, primeiro) SELECT IdDiarioProcessoEixo, ROW_NUMBER() OVER (ORDER BY IdDiarioProcessoEixo), IdDiarioProcessoEixo FROM empodera_antigo.diariosprocessoeixos;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_fichasprimeirocontato;
CREATE TABLE empodera_antigo.migracao_mapa_fichasprimeirocontato (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_fichasprimeirocontato (antigo, novo, primeiro) SELECT IdFicha, ROW_NUMBER() OVER (ORDER BY DtCriacao, IdFicha), IdFicha FROM empodera_antigo.fichasprimeirocontato;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_fichacondicoes;
CREATE TABLE empodera_antigo.migracao_mapa_fichacondicoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_fichacondicoes (antigo, novo, primeiro) SELECT IdCondicoes, ROW_NUMBER() OVER (ORDER BY IdCondicoes), IdCondicoes FROM empodera_antigo.fichacondicoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_fichapeticoes;
CREATE TABLE empodera_antigo.migracao_mapa_fichapeticoes (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_fichapeticoes (antigo, novo, primeiro) SELECT IdPeticoes, ROW_NUMBER() OVER (ORDER BY IdPeticoes), IdPeticoes FROM empodera_antigo.fichapeticoes;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_ficharespostas;
CREATE TABLE empodera_antigo.migracao_mapa_ficharespostas (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_ficharespostas (antigo, novo, primeiro) SELECT IdCondicoes, ROW_NUMBER() OVER (ORDER BY IdCondicoes), IdCondicoes FROM empodera_antigo.ficharespostas;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_ficharesultados;
CREATE TABLE empodera_antigo.migracao_mapa_ficharesultados (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_ficharesultados (antigo, novo, primeiro) SELECT IdCondicoes, ROW_NUMBER() OVER (ORDER BY IdCondicoes), IdCondicoes FROM empodera_antigo.ficharesultados;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_fontesinfo;
CREATE TABLE empodera_antigo.migracao_mapa_fontesinfo (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_fontesinfo (antigo, novo, primeiro) SELECT IdFonte, ROW_NUMBER() OVER (ORDER BY IdFonte), IdFonte FROM empodera_antigo.fontesinfo;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_recursosatores;
CREATE TABLE empodera_antigo.migracao_mapa_recursosatores (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_recursosatores (antigo, novo, primeiro) SELECT Id_Recursos_Atores, ROW_NUMBER() OVER (ORDER BY Id_Recursos_Atores), Id_Recursos_Atores FROM empodera_antigo.recursosatores;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_rederecursos;
CREATE TABLE empodera_antigo.migracao_mapa_rederecursos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_rederecursos (antigo, novo, primeiro) SELECT Id_Rede, ROW_NUMBER() OVER (ORDER BY DtCriacao, Id_Rede), Id_Rede FROM empodera_antigo.rederecursos;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_redeeixos;
CREATE TABLE empodera_antigo.migracao_mapa_redeeixos (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_redeeixos (antigo, novo, primeiro) SELECT IdRedeEixo, ROW_NUMBER() OVER (ORDER BY IdRedeEixo), IdRedeEixo FROM empodera_antigo.redeeixos;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_redesprimarias;
CREATE TABLE empodera_antigo.migracao_mapa_redesprimarias (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_redesprimarias (antigo, novo, primeiro) SELECT IdRedePrimaria, ROW_NUMBER() OVER (ORDER BY IdRedePrimaria), IdRedePrimaria FROM empodera_antigo.redesprimarias;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_vulnerabilidades;
CREATE TABLE empodera_antigo.migracao_mapa_vulnerabilidades (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_vulnerabilidades (antigo, novo, primeiro) SELECT IdVulnerabilidade, ROW_NUMBER() OVER (ORDER BY IdVulnerabilidade), IdVulnerabilidade FROM empodera_antigo.vulnerabilidades;

DROP TABLE IF EXISTS empodera_antigo.migracao_mapa_vulnerabilidadeseixo;
CREATE TABLE empodera_antigo.migracao_mapa_vulnerabilidadeseixo (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));
INSERT INTO empodera_antigo.migracao_mapa_vulnerabilidadeseixo (antigo, novo, primeiro) SELECT IdVEixo, ROW_NUMBER() OVER (ORDER BY IdVEixo), IdVEixo FROM empodera_antigo.vulnerabilidadeseixo;


-- ---------------------------------------------------------------------
-- 3. Cópia dos dados (numa transação: ou copia tudo, ou nada)
-- ---------------------------------------------------------------------

-- "@[Nome](ator:<uuid>)" -> "@[Nome](ator:<novo id>)" no Diário de Processo Pessoal.
-- DtModificacao = DtModificacao mantém a data de modificação original.
-- (CREATE/DROP PROCEDURE encerram transações no MySQL, por isso ficam fora dela.)
DROP PROCEDURE IF EXISTS migracao_mencoes;
DELIMITER //
CREATE PROCEDURE migracao_mencoes()
BEGIN
    REPEAT
        UPDATE empodera.diariosprocessopessoal d
        JOIN empodera_antigo.migracao_mapa_atores m
          ON LOCATE(CONCAT('(ator:', BIN_TO_UUID(m.antigo), ')'), d.Descricao) > 0
        SET d.Descricao = REPLACE(d.Descricao, CONCAT('(ator:', BIN_TO_UUID(m.antigo), ')'), CONCAT('(ator:', m.novo, ')')),
            d.DtModificacao = d.DtModificacao;
    UNTIL ROW_COUNT() = 0 END REPEAT;
END //
DELIMITER ;

START TRANSACTION;

-- Dados iniciais do script de criação dão lugar aos dados reais.
DELETE FROM empodera.permissoes;
DELETE FROM empodera.usuarios;
DELETE FROM empodera.perfis;

INSERT INTO empodera.eixos (IdEixo, Nome)
SELECT c0, c1 FROM (
SELECT m.novo AS c0, TRIM(o.Nome) AS c1
FROM empodera_antigo.eixos o
JOIN empodera_antigo.migracao_mapa_eixos m ON m.antigo = o.IdEixo
WHERE o.IdEixo = m.primeiro
) dados;

INSERT INTO empodera.perfis (IdPerfil, DtCriacao, DtModificacao, IdUsuario, Nome)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT m.novo AS c0, o.DtCriacao AS c1, o.DtModificacao AS c2, NULL AS c3, o.Nome AS c4
FROM empodera_antigo.perfis o
JOIN empodera_antigo.migracao_mapa_perfis m ON m.antigo = o.IdPerfil
) dados;

INSERT INTO empodera.permissoes (IdPermissao, IdPerfil, Modulo, PodeAtualizar, PodeCriar, PodeDeletar, PodeDetalhar, PodeListar)
SELECT c0, c1, c2, c3, c4, c5, c6, c7 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Modulo AS c2, o.PodeAtualizar AS c3, o.PodeCriar AS c4, o.PodeDeletar AS c5, o.PodeDetalhar AS c6, o.PodeListar AS c7
FROM empodera_antigo.permissoes o
JOIN empodera_antigo.migracao_mapa_permissoes m ON m.antigo = o.IdPermissoes
LEFT JOIN empodera_antigo.migracao_mapa_perfis f1 ON f1.antigo = o.FkIdPerfil
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.usuarios (IdUsuario, AccessFailedCount, Ativo, ConcurrencyStamp, DtModificacao, DtCriacao, DtNascimento, Email, EmailConfirmed, IdPerfil, Foto, Genero, IdiomaPreferido, LockoutEnabled, LockoutEnd, Nome, NormalizedEmail, NormalizedUserName, Ocupacao, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, Senha, TwoFactorEnabled, UserName)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18, c19, c20, c21, c22, c23, c24 FROM (
SELECT m.novo AS c0, o.AccessFailedCount AS c1, o.Ativo AS c2, o.ConcurrencyStamp AS c3, o.DtAtualizacao AS c4, o.DtCriacao AS c5, DATE(o.DtNascimento) AS c6, o.Email AS c7, o.EmailConfirmed AS c8, f1.novo AS c9, o.Foto AS c10, o.Genero AS c11, o.IdiomaPreferido AS c12, o.LockoutEnabled AS c13, o.LockoutEnd AS c14, o.Nome AS c15, o.NormalizedEmail AS c16, o.NormalizedUserName AS c17, o.Ocupacao AS c18, o.PhoneNumber AS c19, o.PhoneNumberConfirmed AS c20, o.SecurityStamp AS c21, o.Senha AS c22, o.TwoFactorEnabled AS c23, o.UserName AS c24
FROM empodera_antigo.usuarios o
JOIN empodera_antigo.migracao_mapa_usuarios m ON m.antigo = o.IdUsuario
LEFT JOIN empodera_antigo.migracao_mapa_perfis f1 ON f1.antigo = o.FkIdPerfil
) dados;

INSERT INTO empodera.atores (IdAtor, Ativo, DaEquipe, DtCriacao, DtModificacao, IdUsuario, IdUsuarioModificacao, Genero, Idade, Interesse, Lopiniao, Mcomunidade, Nome, PapelSocial1, PapelSocial2, Rope, Telefone)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16 FROM (
SELECT m.novo AS c0, o.Ativo AS c1, o.DaEquipe AS c2, o.DtCriacao AS c3, o.DtModificacao AS c4, f1.novo AS c5, f2.novo AS c6, o.Genero AS c7, o.Idade AS c8, o.Interesse AS c9, o.Lopiniao AS c10, o.Mcomunidade AS c11, o.Nome AS c12, o.PapelSocial1 AS c13, o.PapelSocial2 AS c14, o.Rope AS c15, o.Telefone AS c16
FROM empodera_antigo.atores o
JOIN empodera_antigo.migracao_mapa_atores m ON m.antigo = o.IdAtores
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f1 ON f1.antigo = o.FkIdUsuario
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FkIdUsuarioM
) dados;

INSERT INTO empodera.avaliacaopessoal (IdAvaliacao, AssBasica, CCrimes, DtCriacao, DtModificacao, Educacao, IdAtor, IdUsuario, Lazer, Moradia, Ocupacao, Prevencao, Saude, Substancias)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13 FROM (
SELECT m.novo AS c0, o.AssBasica AS c1, o.CCrimes AS c2, o.DtCriacao AS c3, o.DtModificacao AS c4, o.Educacao AS c5, f1.novo AS c6, f2.novo AS c7, o.Lazer AS c8, o.Moradia AS c9, o.Ocupacao AS c10, o.Prevencao AS c11, o.Saude AS c12, o.Substancias AS c13
FROM empodera_antigo.avaliacaopessoal o
JOIN empodera_antigo.migracao_mapa_avaliacaopessoal m ON m.antigo = o.IdAvaliacao
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FkIdUsuario
) dados;

INSERT INTO empodera.comunidades (IdComunidade, Ativo, Complemento, Descricao, DescricaoAcessibilidade, DtCriacao, DtModificacao, IdUsuario, IdUsuarioModificacao, Local, LocalMapa, LocalMapaSecundario, LocalSecundario, Nome, Status)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14 FROM (
SELECT m.novo AS c0, o.Ativo AS c1, o.Complemento AS c2, o.Descricao AS c3, o.Descricao_Acessibilidade AS c4, o.Dt_Criacao AS c5, o.Dt_Modificacao AS c6, f1.novo AS c7, f2.novo AS c8, o.Local AS c9, o.LocalMapa AS c10, o.LocalMapaSecundario AS c11, o.LocalSecundario AS c12, o.Nome AS c13, o.Status AS c14
FROM empodera_antigo.comunidades o
JOIN empodera_antigo.migracao_mapa_comunidades m ON m.antigo = o.Id_Comunidade
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f1 ON f1.antigo = o.FK_Id_Usuario
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FK_Id_UsuarioM
) dados;

INSERT INTO empodera.atividades (IdAtividade, Descricao, DtCriacao, DtModificacao, IdComunidade, IdUsuario, IdUsuarioModificacao, Foto, Nome)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8 FROM (
SELECT m.novo AS c0, o.Descricao AS c1, o.DtCriacao AS c2, o.DtModificacao AS c3, f1.novo AS c4, f2.novo AS c5, f3.novo AS c6, o.Foto AS c7, o.Nome AS c8
FROM empodera_antigo.atividades o
JOIN empodera_antigo.migracao_mapa_atividades m ON m.antigo = o.IdAtividade
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f1 ON f1.antigo = o.FkIdComunidade
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FkIdUsuario
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f3 ON f3.antigo = o.FkIdUsuarioM
) dados;

INSERT INTO empodera.acoes (IdAcao, IdAtividade, Nome, Provedor, Quantidade)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Nome AS c2, o.Provedor AS c3, o.Quantidade AS c4
FROM empodera_antigo.acoes o
JOIN empodera_antigo.migracao_mapa_acoes m ON m.antigo = o.IdAcoes
LEFT JOIN empodera_antigo.migracao_mapa_atividades f1 ON f1.antigo = o.FkIdAtividade
) dados;

INSERT INTO empodera.acoesatores (IdAcaoAtor, IdAtor, IdAcao)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c2, c1 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.acoesatores o
JOIN empodera_antigo.migracao_mapa_acoesatores m ON m.antigo = o.IdAAtores
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_acoes f2 ON f2.antigo = o.FkIdAcoes
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.atividadeseixo (IdAtividadeEixo, IdAtividade, IdEixo)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.atividadeseixo o
JOIN empodera_antigo.migracao_mapa_atividadeseixo m ON m.antigo = o.IdAEixo
LEFT JOIN empodera_antigo.migracao_mapa_atividades f1 ON f1.antigo = o.FkIdAtividade
LEFT JOIN empodera_antigo.migracao_mapa_eixos f2 ON f2.antigo = o.FkIdEixo
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.atorcomunidades (IdAtorComunidade, IdAtor, IdComunidade, TipoRelacionamento)
SELECT c0, c1, c2, c3 FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2, o.TipoRelacionamento AS c3
FROM empodera_antigo.atorcomunidades o
JOIN empodera_antigo.migracao_mapa_atorcomunidades m ON m.antigo = o.IdAtorComunidade
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f2 ON f2.antigo = o.FkIdComunidade
) dados;

INSERT INTO empodera.diarioscampo (IdDiarioCampo, Data, Descricao, DtCriacao, DtModificacao, IdComunidade, IdUsuario, Foto, Localizacao)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8 FROM (
SELECT m.novo AS c0, DATE(o.Data) AS c1, o.Descricao AS c2, o.DtCriacao AS c3, o.DtModificacao AS c4, f1.novo AS c5, f2.novo AS c6, o.Foto AS c7, o.Localizacao AS c8
FROM empodera_antigo.diarioscampo o
JOIN empodera_antigo.migracao_mapa_diarioscampo m ON m.antigo = o.IdDCampo
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f1 ON f1.antigo = o.FkIdComunidade
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FkIdUsuario
) dados;

INSERT INTO empodera.anexosdiario (IdAnexo, Caminho, IdDiarioCampo)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, o.Caminho AS c1, f1.novo AS c2
FROM empodera_antigo.anexosdiario o
JOIN empodera_antigo.migracao_mapa_anexosdiario m ON m.antigo = o.IdAnexos
LEFT JOIN empodera_antigo.migracao_mapa_diarioscampo f1 ON f1.antigo = o.FkIdDiario
) dados;

INSERT INTO empodera.diarioacoes (IdDiarioAcao, IdAcao, IdDiarioCampo)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c2, c1 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.diarioacoes o
JOIN empodera_antigo.migracao_mapa_diarioacoes m ON m.antigo = o.IdDAcoes
LEFT JOIN empodera_antigo.migracao_mapa_acoes f1 ON f1.antigo = o.FkIdAcoes
LEFT JOIN empodera_antigo.migracao_mapa_diarioscampo f2 ON f2.antigo = o.FkIdDiario
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.diariodacoes (IdDiarioDAcao, IdDiarioCampo, Nome, ProvedorExterno, Quantidade)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Nome AS c2, o.PeovedorEx AS c3, o.Quantidade AS c4
FROM empodera_antigo.diariodacoes o
JOIN empodera_antigo.migracao_mapa_diariodacoes m ON m.antigo = o.IdDAcoes
LEFT JOIN empodera_antigo.migracao_mapa_diarioscampo f1 ON f1.antigo = o.FkIdDiario
) dados;

INSERT INTO empodera.daatores (IdDAAtor, IdAtor, IdDiarioDAcao)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c2, c1 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.daatores o
JOIN empodera_antigo.migracao_mapa_daatores m ON m.antigo = o.Id
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_diariodacoes f2 ON f2.antigo = o.FkIdDDacoes
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.detalhesdacoes (IdDetalheDAcao, IdDiarioDAcao, Nome)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Nome AS c2
FROM empodera_antigo.detalhesdacoes o
JOIN empodera_antigo.migracao_mapa_detalhesdacoes m ON m.antigo = o.Id
LEFT JOIN empodera_antigo.migracao_mapa_diariodacoes f1 ON f1.antigo = o.FkIdDDacoes
) dados;

INSERT INTO empodera.detalheseixos (IdDetalheEixo, IdDetalheDAcao, IdEixo)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.detalheseixos o
JOIN empodera_antigo.migracao_mapa_detalheseixos m ON m.antigo = o.IdDiarioEixo
LEFT JOIN empodera_antigo.migracao_mapa_detalhesdacoes f1 ON f1.antigo = o.FkIdDetalhes
LEFT JOIN empodera_antigo.migracao_mapa_eixos f2 ON f2.antigo = o.FkIdEixo
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.diarioeixos (IdDiarioEixo, IdDiarioCampo, IdEixo)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.diarioeixos o
JOIN empodera_antigo.migracao_mapa_diarioeixos m ON m.antigo = o.IdDiarioEixo
LEFT JOIN empodera_antigo.migracao_mapa_diarioscampo f1 ON f1.antigo = o.FkIdDiario
LEFT JOIN empodera_antigo.migracao_mapa_eixos f2 ON f2.antigo = o.FkIdEixo
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.diariosprocessopessoal (IdDiarioProcesso, Data, Descricao, DtCriacao, DtModificacao, IdAtor, IdUsuario, IdUsuarioModificacao)
SELECT c0, c1, c2, c3, c4, c5, c6, c7 FROM (
SELECT m.novo AS c0, DATE(o.Data) AS c1, o.Descricao AS c2, o.DtCriacao AS c3, o.DtModificacao AS c4, f1.novo AS c5, f2.novo AS c6, f3.novo AS c7
FROM empodera_antigo.diariosprocessopessoal o
JOIN empodera_antigo.migracao_mapa_diariosprocessopessoal m ON m.antigo = o.IdDiarioProcesso
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f2 ON f2.antigo = o.FkIdUsuario
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f3 ON f3.antigo = o.FkIdUsuarioM
) dados;

INSERT INTO empodera.diariosprocessoeixos (IdDiarioProcessoEixo, IdDiarioProcesso, IdEixo)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.diariosprocessoeixos o
JOIN empodera_antigo.migracao_mapa_diariosprocessoeixos m ON m.antigo = o.IdDiarioProcessoEixo
LEFT JOIN empodera_antigo.migracao_mapa_diariosprocessopessoal f1 ON f1.antigo = o.FkIdDiarioProcesso
LEFT JOIN empodera_antigo.migracao_mapa_eixos f2 ON f2.antigo = o.FkIdEixo
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.fichasprimeirocontato (IdFicha, AEscolar, CEstabeleceu, Coment, Complemento, DtContato, DtCriacao, DtModificacao, Emprego, Endereco, EstaFamiliar, EstruFamiliar, IdAtor, IdComunidade, IdUsuario, FonteDados, FornecidoParceiro, HoraContato, LContato, LTrat, NFilhos, NFilhas, NovoParceiro, QReabili, SCalc, SComp, SLer, Status, Telefone)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18, c19, c20, c21, c22, c23, c24, c25, c26, c27, c28 FROM (
SELECT m.novo AS c0, o.AEscolar AS c1, o.CEstabeleceu AS c2, o.Coment AS c3, o.Complemento AS c4, DATE(o.DtContato) AS c5, o.DtCriacao AS c6, o.DtModificacao AS c7, o.Emprego AS c8, o.Endereco AS c9, o.EstaFamiliar AS c10, o.EstruFamiliar AS c11, f1.novo AS c12, f2.novo AS c13, f3.novo AS c14, o.FonteDados AS c15, CASE o.FornecidoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE o.FornecidoParceiro END AS c16, o.HoraContato AS c17, o.LContato AS c18, o.LTrat AS c19, o.NFIlhos AS c20, o.NFilhas AS c21, CASE o.NovoParceiro WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE o.NovoParceiro END AS c22, o.QReabili AS c23, CASE o.SCalc WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE o.SCalc END AS c24, CASE o.SComp WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE o.SComp END AS c25, CASE o.SLer WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE o.SLer END AS c26, o.Status AS c27, o.Telefone AS c28
FROM empodera_antigo.fichasprimeirocontato o
JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato m ON m.antigo = o.IdFicha
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f2 ON f2.antigo = o.FkIdComunidade
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f3 ON f3.antigo = o.FkIdUsuario
) dados;

INSERT INTO empodera.fichacondicoes (IdFichaCondicao, Cond, IdFicha)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, o.Cond AS c1, f1.novo AS c2
FROM empodera_antigo.fichacondicoes o
JOIN empodera_antigo.migracao_mapa_fichacondicoes m ON m.antigo = o.IdCondicoes
LEFT JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato f1 ON f1.antigo = o.FkIdFicha
) dados;

INSERT INTO empodera.fichapeticoes (IdFichaPeticao, IdFicha, Pet)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Pet AS c2
FROM empodera_antigo.fichapeticoes o
JOIN empodera_antigo.migracao_mapa_fichapeticoes m ON m.antigo = o.IdPeticoes
LEFT JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato f1 ON f1.antigo = o.FkIdFicha
) dados;

INSERT INTO empodera.ficharespostas (IdFichaResposta, IdFicha, Resp)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Resp AS c2
FROM empodera_antigo.ficharespostas o
JOIN empodera_antigo.migracao_mapa_ficharespostas m ON m.antigo = o.IdCondicoes
LEFT JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato f1 ON f1.antigo = o.FkIdFicha
) dados;

INSERT INTO empodera.ficharesultados (IdFichaResultado, IdFicha, Result)
SELECT c0, c1, c2 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Result AS c2
FROM empodera_antigo.ficharesultados o
JOIN empodera_antigo.migracao_mapa_ficharesultados m ON m.antigo = o.IdCondicoes
LEFT JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato f1 ON f1.antigo = o.FkIdFicha
) dados;

INSERT INTO empodera.fontesinfo (IdFonte, Extra, IdFicha, IdAtor, Genero, Idade, Interesse, Nome, PapelSocial1, PapelSocial2, Telefone)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10 FROM (
SELECT m.novo AS c0, o.Extra AS c1, f1.novo AS c2, COALESCE(f2.novo, f2b.novo) AS c3, o.Genero AS c4, o.Idade AS c5, o.Interesse AS c6, o.Nome AS c7, o.PapelSocial1 AS c8, o.PapelSocial2 AS c9, o.Telefone AS c10
FROM empodera_antigo.fontesinfo o
JOIN empodera_antigo.migracao_mapa_fontesinfo m ON m.antigo = o.IdFonte
LEFT JOIN empodera_antigo.migracao_mapa_fichasprimeirocontato f1 ON f1.antigo = o.FkIdFicha
LEFT JOIN empodera_antigo.migracao_mapa_atores f2 ON f2.antigo = o.AtorIdAtores
LEFT JOIN empodera_antigo.migracao_mapa_atores f2b ON f2b.antigo = o.Fk_Id_Ator
) dados;

INSERT INTO empodera.recursosatores (IdRecursoAtor, IdAtor, Nome, Pode, Tipo)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Nome AS c2, o.Pode AS c3, o.Tipo AS c4
FROM empodera_antigo.recursosatores o
JOIN empodera_antigo.migracao_mapa_recursosatores m ON m.antigo = o.Id_Recursos_Atores
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
) dados;

INSERT INTO empodera.rederecursos (IdRede, Dispositivo, DtCriacao, DtModificacao, IdAtor, IdComunidade, IdUsuario, Localizacao, Nome, Servicos, Tipo)
SELECT c0, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10 FROM (
SELECT m.novo AS c0, o.Dispositivo AS c1, o.DtCriacao AS c2, o.DtModificacao AS c3, f1.novo AS c4, f2.novo AS c5, f3.novo AS c6, o.Localizacao AS c7, o.Nome AS c8, o.Servicos AS c9, o.Tipo AS c10
FROM empodera_antigo.rederecursos o
JOIN empodera_antigo.migracao_mapa_rederecursos m ON m.antigo = o.Id_Rede
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FK_id_Atores
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f2 ON f2.antigo = o.FkIdComunidade
LEFT JOIN empodera_antigo.migracao_mapa_usuarios f3 ON f3.antigo = o.FkIdUsuario
) dados;

INSERT INTO empodera.redeeixos (IdRedeEixo, IdEixo, IdRede)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c2, c1 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.redeeixos o
JOIN empodera_antigo.migracao_mapa_redeeixos m ON m.antigo = o.IdRedeEixo
LEFT JOIN empodera_antigo.migracao_mapa_eixos f1 ON f1.antigo = o.FkIdEixo
LEFT JOIN empodera_antigo.migracao_mapa_rederecursos f2 ON f2.antigo = o.FkIdRede
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.redesprimarias (IdRedePrimaria, IdAtorPrincipal, IdAtorRelacionado, TipoRede, TipoRelacao)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c1, c2, c3 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2, o.TipoRede AS c3, o.TipoRelacao AS c4
FROM empodera_antigo.redesprimarias o
JOIN empodera_antigo.migracao_mapa_redesprimarias m ON m.antigo = o.IdRedePrimaria
LEFT JOIN empodera_antigo.migracao_mapa_atores f1 ON f1.antigo = o.FkIdAtorPrincipal
LEFT JOIN empodera_antigo.migracao_mapa_atores f2 ON f2.antigo = o.FkIdAtorRelacionados
) dados
) unicos WHERE repeticao = 1;

INSERT INTO empodera.vulnerabilidades (IdVulnerabilidade, IdComunidade, Localizacao, Nome, Servicos)
SELECT c0, c1, c2, c3, c4 FROM (
SELECT m.novo AS c0, f1.novo AS c1, o.Localizacao AS c2, o.Nome AS c3, o.Servicos AS c4
FROM empodera_antigo.vulnerabilidades o
JOIN empodera_antigo.migracao_mapa_vulnerabilidades m ON m.antigo = o.IdVulnerabilidade
LEFT JOIN empodera_antigo.migracao_mapa_comunidades f1 ON f1.antigo = o.FkIdComunidade
) dados;

INSERT INTO empodera.vulnerabilidadeseixo (IdVulnerabilidadeEixo, IdEixo, IdVulnerabilidade)
SELECT c0, c1, c2 FROM (
SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY c2, c1 ORDER BY c0) AS repeticao FROM (
SELECT m.novo AS c0, f1.novo AS c1, f2.novo AS c2
FROM empodera_antigo.vulnerabilidadeseixo o
JOIN empodera_antigo.migracao_mapa_vulnerabilidadeseixo m ON m.antigo = o.IdVEixo
LEFT JOIN empodera_antigo.migracao_mapa_eixos f1 ON f1.antigo = o.FkIdEixo
LEFT JOIN empodera_antigo.migracao_mapa_vulnerabilidades f2 ON f2.antigo = o.FkIdVulnerabilidade
) dados
) unicos WHERE repeticao = 1;

UPDATE empodera.perfis n
JOIN empodera_antigo.migracao_mapa_perfis m ON m.novo = n.IdPerfil
JOIN empodera_antigo.perfis o ON o.IdPerfil = m.antigo
JOIN empodera_antigo.migracao_mapa_usuarios p ON p.antigo = o.FkIdUsuario
SET n.IdUsuario = p.novo, n.DtModificacao = n.DtModificacao;

-- Menções no Diário de Processo Pessoal (procedimento criado antes da transação).
CALL migracao_mencoes();

COMMIT;

DROP PROCEDURE migracao_mencoes;


-- ---------------------------------------------------------------------
-- 4. Fotos e cores dos usuários (App_Data)
-- Os arquivos usavam o UUID sem hífens no nome; agora usam o novo Id.
-- Renomeie conforme esta lista (docs/revisao-ddl-mysql.md traz o comando):
-- ---------------------------------------------------------------------

SELECT REPLACE(BIN_TO_UUID(antigo), '-', '') AS `Nome antigo`, novo AS `Nome novo`
FROM empodera_antigo.migracao_mapa_usuarios
ORDER BY novo;
