ALTER DATABASE CHARACTER SET utf8mb4;


CREATE TABLE `Eixos` (
    `IdEixo` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Eixos` PRIMARY KEY (`IdEixo`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `Perfis` (
    `IdPerfil` int NOT NULL AUTO_INCREMENT,
    `FkIdUsuario` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    CONSTRAINT `PK_Perfis` PRIMARY KEY (`IdPerfil`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `Permissoes` (
    `IdPermissoes` int NOT NULL AUTO_INCREMENT,
    `FkIdPerfil` int NOT NULL,
    `Modulo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PodeListar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PodeDetalhar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PodeCriar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PodeAtualizar` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PodeDeletar` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Permissoes` PRIMARY KEY (`IdPermissoes`),
    CONSTRAINT `FK_Permissoes_Perfis_FkIdPerfil` FOREIGN KEY (`FkIdPerfil`) REFERENCES `Perfis` (`IdPerfil`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `Usuarios` (
    `IdUsuario` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Senha` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Foto` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Ocupacao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Genero` int NULL,
    `DtNascimento` datetime(6) NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtAtualizacao` datetime(6) NULL,
    `Ativo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IdiomaPreferido` int NOT NULL,
    `FkIdPerfil` int NOT NULL,
    CONSTRAINT `PK_Usuarios` PRIMARY KEY (`IdUsuario`),
    CONSTRAINT `FK_Usuarios_Perfis_FkIdPerfil` FOREIGN KEY (`FkIdPerfil`) REFERENCES `Perfis` (`IdPerfil`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `Atores` (
    `IdAtores` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Genero` int NULL,
    `Idade` int NULL,
    `PapelSocial1` longtext CHARACTER SET utf8mb4 NULL,
    `PapelSocial2` longtext CHARACTER SET utf8mb4 NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NULL,
    `DaEquipe` tinyint(1) NOT NULL,
    `Rope` tinyint(1) NOT NULL,
    `Lopiniao` tinyint(1) NOT NULL,
    `Mcomunidade` tinyint(1) NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `Ativo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FkIdUsuario` int NOT NULL,
    `FkIdUsuarioM` int NULL,
    CONSTRAINT `PK_Atores` PRIMARY KEY (`IdAtores`),
    CONSTRAINT `FK_Atores_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE,
    CONSTRAINT `FK_Atores_Usuarios_FkIdUsuarioM` FOREIGN KEY (`FkIdUsuarioM`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `Comunidades` (
    `Id_Comunidade` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Local` longtext CHARACTER SET utf8mb4 NULL,
    `LocalMapa` longtext CHARACTER SET utf8mb4 NULL,
    `LocalSecundario` longtext CHARACTER SET utf8mb4 NULL,
    `LocalMapaSecundario` longtext CHARACTER SET utf8mb4 NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Complemento` longtext CHARACTER SET utf8mb4 NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NULL,
    `Descricao_Acessibilidade` longtext CHARACTER SET utf8mb4 NULL,
    `Dt_Criacao` datetime(6) NOT NULL,
    `Dt_Modificacao` datetime(6) NOT NULL,
    `FK_Id_Usuario` int NOT NULL,
    `FK_Id_UsuarioM` int NULL,
    `Ativo` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Comunidades` PRIMARY KEY (`Id_Comunidade`),
    CONSTRAINT `FK_Comunidades_Usuarios_FK_Id_Usuario` FOREIGN KEY (`FK_Id_Usuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE,
    CONSTRAINT `FK_Comunidades_Usuarios_FK_Id_UsuarioM` FOREIGN KEY (`FK_Id_UsuarioM`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `AvaliacaoPessoal` (
    `IdAvaliacao` int NOT NULL AUTO_INCREMENT,
    `FK_id_Atores` int NOT NULL,
    `CCrimes` int NOT NULL,
    `Substancias` int NOT NULL,
    `Moradia` int NOT NULL,
    `Prevencao` int NOT NULL,
    `AssBasica` int NOT NULL,
    `Educacao` int NOT NULL,
    `Saude` int NOT NULL,
    `Ocupacao` int NOT NULL,
    `Lazer` int NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `FkIdUsuario` int NOT NULL,
    CONSTRAINT `PK_AvaliacaoPessoal` PRIMARY KEY (`IdAvaliacao`),
    CONSTRAINT `FK_AvaliacaoPessoal_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE,
    CONSTRAINT `FK_AvaliacaoPessoal_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RecursosAtores` (
    `Id_Recursos_Atores` int NOT NULL AUTO_INCREMENT,
    `FK_id_Atores` int NOT NULL,
    `Tipo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Pode` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_RecursosAtores` PRIMARY KEY (`Id_Recursos_Atores`),
    CONSTRAINT `FK_RecursosAtores_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RedesPrimarias` (
    `IdRedePrimaria` int NOT NULL AUTO_INCREMENT,
    `FkIdAtorPrincipal` int NOT NULL,
    `FkIdAtorRelacionados` int NOT NULL,
    `TipoRelacao` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_RedesPrimarias` PRIMARY KEY (`IdRedePrimaria`),
    CONSTRAINT `FK_RedesPrimarias_Atores_FkIdAtorPrincipal` FOREIGN KEY (`FkIdAtorPrincipal`) REFERENCES `Atores` (`IdAtores`) ON DELETE RESTRICT,
    CONSTRAINT `FK_RedesPrimarias_Atores_FkIdAtorRelacionados` FOREIGN KEY (`FkIdAtorRelacionados`) REFERENCES `Atores` (`IdAtores`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `Atividades` (
    `IdAtividade` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Foto` longtext CHARACTER SET utf8mb4 NULL,
    `FkIdComunidade` int NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `FkIdUsuario` int NOT NULL,
    `FkIdUsuarioM` int NULL,
    CONSTRAINT `PK_Atividades` PRIMARY KEY (`IdAtividade`),
    CONSTRAINT `FK_Atividades_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE,
    CONSTRAINT `FK_Atividades_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE,
    CONSTRAINT `FK_Atividades_Usuarios_FkIdUsuarioM` FOREIGN KEY (`FkIdUsuarioM`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;


CREATE TABLE `AtorComunidades` (
    `IdAtorComunidade` int NOT NULL AUTO_INCREMENT,
    `FkIdComunidade` int NOT NULL,
    `FK_id_Atores` int NOT NULL,
    CONSTRAINT `PK_AtorComunidades` PRIMARY KEY (`IdAtorComunidade`),
    CONSTRAINT `FK_AtorComunidades_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE,
    CONSTRAINT `FK_AtorComunidades_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DiariosCampo` (
    `IdDCampo` int NOT NULL AUTO_INCREMENT,
    `FkIdComunidade` int NOT NULL,
    `Data` datetime(6) NOT NULL,
    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Localizacao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `Foto` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FkIdUsuario` int NOT NULL,
    CONSTRAINT `PK_DiariosCampo` PRIMARY KEY (`IdDCampo`),
    CONSTRAINT `FK_DiariosCampo_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE,
    CONSTRAINT `FK_DiariosCampo_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FichasPrimeiroContato` (
    `IdFicha` int NOT NULL AUTO_INCREMENT,
    `FK_id_Atores` int NOT NULL,
    `Endereco` longtext CHARACTER SET utf8mb4 NULL,
    `Complemento` longtext CHARACTER SET utf8mb4 NULL,
    `Emprego` longtext CHARACTER SET utf8mb4 NULL,
    `CEstabeleceu` longtext CHARACTER SET utf8mb4 NULL,
    `NovoParceiro` longtext CHARACTER SET utf8mb4 NULL,
    `FornecidoParceiro` longtext CHARACTER SET utf8mb4 NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NULL,
    `LContato` longtext CHARACTER SET utf8mb4 NULL,
    `FonteDados` longtext CHARACTER SET utf8mb4 NULL,
    `EstaFamiliar` longtext CHARACTER SET utf8mb4 NULL,
    `EstruFamiliar` longtext CHARACTER SET utf8mb4 NULL,
    `NFIlhos` int NULL,
    `NFilhas` int NULL,
    `AEscolar` int NULL,
    `Status` longtext CHARACTER SET utf8mb4 NULL,
    `SLer` longtext CHARACTER SET utf8mb4 NULL,
    `SCalc` longtext CHARACTER SET utf8mb4 NULL,
    `SComp` longtext CHARACTER SET utf8mb4 NULL,
    `QReabili` int NULL,
    `LTrat` longtext CHARACTER SET utf8mb4 NULL,
    `Coment` longtext CHARACTER SET utf8mb4 NULL,
    `DtContato` datetime(6) NOT NULL,
    `HoraContato` time(6) NOT NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `FkIdComunidade` int NULL,
    `FkIdUsuario` int NOT NULL,
    CONSTRAINT `PK_FichasPrimeiroContato` PRIMARY KEY (`IdFicha`),
    CONSTRAINT `FK_FichasPrimeiroContato_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE,
    CONSTRAINT `FK_FichasPrimeiroContato_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE,
    CONSTRAINT `FK_FichasPrimeiroContato_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RedeRecursos` (
    `Id_Rede` int NOT NULL AUTO_INCREMENT,
    `FK_id_Atores` int NULL,
    `FkIdComunidade` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NULL,
    `Tipo` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Dispositivo` longtext CHARACTER SET utf8mb4 NULL,
    `Localizacao` longtext CHARACTER SET utf8mb4 NULL,
    `Servicos` longtext CHARACTER SET utf8mb4 NULL,
    `DtCriacao` datetime(6) NOT NULL,
    `DtModificacao` datetime(6) NOT NULL,
    `FkIdUsuario` int NOT NULL,
    CONSTRAINT `PK_RedeRecursos` PRIMARY KEY (`Id_Rede`),
    CONSTRAINT `FK_RedeRecursos_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE,
    CONSTRAINT `FK_RedeRecursos_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE,
    CONSTRAINT `FK_RedeRecursos_Usuarios_FkIdUsuario` FOREIGN KEY (`FkIdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `Vulnerabilidades` (
    `IdVulnerabilidade` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Localizacao` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Servicos` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FkIdComunidade` int NOT NULL,
    CONSTRAINT `PK_Vulnerabilidades` PRIMARY KEY (`IdVulnerabilidade`),
    CONSTRAINT `FK_Vulnerabilidades_Comunidades_FkIdComunidade` FOREIGN KEY (`FkIdComunidade`) REFERENCES `Comunidades` (`Id_Comunidade`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `Acoes` (
    `IdAcoes` int NOT NULL AUTO_INCREMENT,
    `Quantidade` int NOT NULL,
    `FkIdAtividade` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Provedor` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Acoes` PRIMARY KEY (`IdAcoes`),
    CONSTRAINT `FK_Acoes_Atividades_FkIdAtividade` FOREIGN KEY (`FkIdAtividade`) REFERENCES `Atividades` (`IdAtividade`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `AtividadesEixo` (
    `IdAEixo` int NOT NULL AUTO_INCREMENT,
    `FkIdEixo` int NOT NULL,
    `FkIdAtividade` int NOT NULL,
    CONSTRAINT `PK_AtividadesEixo` PRIMARY KEY (`IdAEixo`),
    CONSTRAINT `FK_AtividadesEixo_Atividades_FkIdAtividade` FOREIGN KEY (`FkIdAtividade`) REFERENCES `Atividades` (`IdAtividade`) ON DELETE CASCADE,
    CONSTRAINT `FK_AtividadesEixo_Eixos_FkIdEixo` FOREIGN KEY (`FkIdEixo`) REFERENCES `Eixos` (`IdEixo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `AnexosDiario` (
    `IdAnexos` int NOT NULL AUTO_INCREMENT,
    `FkIdDiario` int NOT NULL,
    `Caminho` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AnexosDiario` PRIMARY KEY (`IdAnexos`),
    CONSTRAINT `FK_AnexosDiario_DiariosCampo_FkIdDiario` FOREIGN KEY (`FkIdDiario`) REFERENCES `DiariosCampo` (`IdDCampo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DiarioDAcoes` (
    `IdDAcoes` int NOT NULL AUTO_INCREMENT,
    `FkIdDiario` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PeovedorEx` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Quantidade` int NOT NULL,
    CONSTRAINT `PK_DiarioDAcoes` PRIMARY KEY (`IdDAcoes`),
    CONSTRAINT `FK_DiarioDAcoes_DiariosCampo_FkIdDiario` FOREIGN KEY (`FkIdDiario`) REFERENCES `DiariosCampo` (`IdDCampo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DiarioEixos` (
    `IdDiarioEixo` int NOT NULL AUTO_INCREMENT,
    `FkIdDiario` int NOT NULL,
    `FkIdEixo` int NOT NULL,
    CONSTRAINT `PK_DiarioEixos` PRIMARY KEY (`IdDiarioEixo`),
    CONSTRAINT `FK_DiarioEixos_DiariosCampo_FkIdDiario` FOREIGN KEY (`FkIdDiario`) REFERENCES `DiariosCampo` (`IdDCampo`) ON DELETE CASCADE,
    CONSTRAINT `FK_DiarioEixos_Eixos_FkIdEixo` FOREIGN KEY (`FkIdEixo`) REFERENCES `Eixos` (`IdEixo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FichaCondicoes` (
    `IdCondicoes` int NOT NULL AUTO_INCREMENT,
    `FkIdFicha` int NOT NULL,
    `Cond` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_FichaCondicoes` PRIMARY KEY (`IdCondicoes`),
    CONSTRAINT `FK_FichaCondicoes_FichasPrimeiroContato_FkIdFicha` FOREIGN KEY (`FkIdFicha`) REFERENCES `FichasPrimeiroContato` (`IdFicha`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FichaPeticoes` (
    `IdPeticoes` int NOT NULL AUTO_INCREMENT,
    `FkIdFicha` int NOT NULL,
    `Pet` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_FichaPeticoes` PRIMARY KEY (`IdPeticoes`),
    CONSTRAINT `FK_FichaPeticoes_FichasPrimeiroContato_FkIdFicha` FOREIGN KEY (`FkIdFicha`) REFERENCES `FichasPrimeiroContato` (`IdFicha`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FichaRespostas` (
    `IdCondicoes` int NOT NULL AUTO_INCREMENT,
    `FkIdFicha` int NOT NULL,
    `Resp` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_FichaRespostas` PRIMARY KEY (`IdCondicoes`),
    CONSTRAINT `FK_FichaRespostas_FichasPrimeiroContato_FkIdFicha` FOREIGN KEY (`FkIdFicha`) REFERENCES `FichasPrimeiroContato` (`IdFicha`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FichaResultados` (
    `IdCondicoes` int NOT NULL AUTO_INCREMENT,
    `FkIdFicha` int NOT NULL,
    `Result` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_FichaResultados` PRIMARY KEY (`IdCondicoes`),
    CONSTRAINT `FK_FichaResultados_FichasPrimeiroContato_FkIdFicha` FOREIGN KEY (`FkIdFicha`) REFERENCES `FichasPrimeiroContato` (`IdFicha`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `FontesInfo` (
    `IdFonte` int NOT NULL AUTO_INCREMENT,
    `FkIdFicha` int NOT NULL,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Genero` int NULL,
    `Idade` int NOT NULL,
    `PapelSocial1` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PapelSocial2` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Extra` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Fk_Id_Ator` int NOT NULL,
    `AtorIdAtores` int NULL,
    CONSTRAINT `PK_FontesInfo` PRIMARY KEY (`IdFonte`),
    CONSTRAINT `FK_FontesInfo_Atores_AtorIdAtores` FOREIGN KEY (`AtorIdAtores`) REFERENCES `Atores` (`IdAtores`),
    CONSTRAINT `FK_FontesInfo_FichasPrimeiroContato_FkIdFicha` FOREIGN KEY (`FkIdFicha`) REFERENCES `FichasPrimeiroContato` (`IdFicha`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `RedeEixos` (
    `IdRedeEixo` int NOT NULL AUTO_INCREMENT,
    `FkIdRede` int NOT NULL,
    `FkIdEixo` int NOT NULL,
    CONSTRAINT `PK_RedeEixos` PRIMARY KEY (`IdRedeEixo`),
    CONSTRAINT `FK_RedeEixos_Eixos_FkIdEixo` FOREIGN KEY (`FkIdEixo`) REFERENCES `Eixos` (`IdEixo`) ON DELETE CASCADE,
    CONSTRAINT `FK_RedeEixos_RedeRecursos_FkIdRede` FOREIGN KEY (`FkIdRede`) REFERENCES `RedeRecursos` (`Id_Rede`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `VulnerabilidadesEixo` (
    `IdVEixo` int NOT NULL AUTO_INCREMENT,
    `FkIdEixo` int NOT NULL,
    `FkIdVulnerabilidade` int NOT NULL,
    CONSTRAINT `PK_VulnerabilidadesEixo` PRIMARY KEY (`IdVEixo`),
    CONSTRAINT `FK_VulnerabilidadesEixo_Eixos_FkIdEixo` FOREIGN KEY (`FkIdEixo`) REFERENCES `Eixos` (`IdEixo`) ON DELETE CASCADE,
    CONSTRAINT `FK_VulnerabilidadesEixo_Vulnerabilidades_FkIdVulnerabilidade` FOREIGN KEY (`FkIdVulnerabilidade`) REFERENCES `Vulnerabilidades` (`IdVulnerabilidade`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `AcoesAtores` (
    `IdAAtores` int NOT NULL AUTO_INCREMENT,
    `FK_id_Atores` int NOT NULL,
    `FkIdAcoes` int NOT NULL,
    CONSTRAINT `PK_AcoesAtores` PRIMARY KEY (`IdAAtores`),
    CONSTRAINT `FK_AcoesAtores_Acoes_FkIdAcoes` FOREIGN KEY (`FkIdAcoes`) REFERENCES `Acoes` (`IdAcoes`) ON DELETE CASCADE,
    CONSTRAINT `FK_AcoesAtores_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DiarioAcoes` (
    `IdDAcoes` int NOT NULL AUTO_INCREMENT,
    `FkIdAcoes` int NOT NULL,
    `FkIdDiario` int NOT NULL,
    CONSTRAINT `PK_DiarioAcoes` PRIMARY KEY (`IdDAcoes`),
    CONSTRAINT `FK_DiarioAcoes_Acoes_FkIdAcoes` FOREIGN KEY (`FkIdAcoes`) REFERENCES `Acoes` (`IdAcoes`) ON DELETE CASCADE,
    CONSTRAINT `FK_DiarioAcoes_DiariosCampo_FkIdDiario` FOREIGN KEY (`FkIdDiario`) REFERENCES `DiariosCampo` (`IdDCampo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DAAtores` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FkIdDDacoes` int NOT NULL,
    `FK_id_Atores` int NOT NULL,
    CONSTRAINT `PK_DAAtores` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DAAtores_Atores_FK_id_Atores` FOREIGN KEY (`FK_id_Atores`) REFERENCES `Atores` (`IdAtores`) ON DELETE CASCADE,
    CONSTRAINT `FK_DAAtores_DiarioDAcoes_FkIdDDacoes` FOREIGN KEY (`FkIdDDacoes`) REFERENCES `DiarioDAcoes` (`IdDAcoes`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DetalhesDAcoes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FkIdDDacoes` int NOT NULL,
    CONSTRAINT `PK_DetalhesDAcoes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DetalhesDAcoes_DiarioDAcoes_FkIdDDacoes` FOREIGN KEY (`FkIdDDacoes`) REFERENCES `DiarioDAcoes` (`IdDAcoes`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `DetalhesEixos` (
    `IdDiarioEixo` int NOT NULL AUTO_INCREMENT,
    `FkIdDetalhes` int NOT NULL,
    `FkIdEixo` int NOT NULL,
    CONSTRAINT `PK_DetalhesEixos` PRIMARY KEY (`IdDiarioEixo`),
    CONSTRAINT `FK_DetalhesEixos_DetalhesDAcoes_FkIdDetalhes` FOREIGN KEY (`FkIdDetalhes`) REFERENCES `DetalhesDAcoes` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DetalhesEixos_Eixos_FkIdEixo` FOREIGN KEY (`FkIdEixo`) REFERENCES `Eixos` (`IdEixo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


INSERT INTO `Perfis` (`IdPerfil`, `DtCriacao`, `DtModificacao`, `FkIdUsuario`, `Nome`)
VALUES (1, TIMESTAMP '0001-01-01 00:00:00', TIMESTAMP '0001-01-01 00:00:00', 1, 'Admin'),
(2, TIMESTAMP '0001-01-01 00:00:00', TIMESTAMP '0001-01-01 00:00:00', 2, 'Editor'),
(3, TIMESTAMP '0001-01-01 00:00:00', TIMESTAMP '0001-01-01 00:00:00', 3, 'Colaborador'),
(4, TIMESTAMP '0001-01-01 00:00:00', TIMESTAMP '0001-01-01 00:00:00', 4, 'Visualizador'),
(5, TIMESTAMP '0001-01-01 00:00:00', TIMESTAMP '0001-01-01 00:00:00', 5, 'Supervisor');


INSERT INTO `Permissoes` (`IdPermissoes`, `FkIdPerfil`, `Modulo`, `PodeAtualizar`, `PodeCriar`, `PodeDeletar`, `PodeDetalhar`, `PodeListar`)
VALUES (1, 1, 'Usuarios', 'S', 'S', 'S', 'S', 'S'),
(2, 1, 'Perfis', 'S', 'S', 'S', 'S', 'S'),
(3, 1, 'Atividades', 'S', 'S', 'S', 'S', 'S'),
(4, 1, 'Comunidades', 'S', 'S', 'S', 'S', 'S'),
(5, 1, 'Vulnerabilidades', 'S', 'S', 'S', 'S', 'S'),
(6, 1, 'Recursos', 'S', 'S', 'S', 'S', 'S'),
(7, 1, 'DiariosCampo', 'S', 'S', 'S', 'S', 'S'),
(8, 1, 'Atores', 'S', 'S', 'S', 'S', 'S'),
(9, 1, 'Ficha1Contato', 'S', 'S', 'S', 'S', 'S'),
(10, 1, 'DiariosProcessoPessoal', 'S', 'S', 'S', 'S', 'S'),
(11, 1, 'AvaliacoesPessoais', 'S', 'S', 'S', 'S', 'S'),
(12, 1, 'SER', 'S', 'S', 'S', 'S', 'S'),
(13, 2, 'Usuarios', 'S', 'S', 'N', 'S', 'S'),
(14, 2, 'Perfis', 'S', 'S', 'N', 'S', 'S'),
(15, 2, 'Atividades', 'S', 'S', 'N', 'S', 'S'),
(16, 2, 'Comunidades', 'S', 'S', 'N', 'S', 'S'),
(17, 2, 'Vulnerabilidades', 'S', 'S', 'N', 'S', 'S'),
(18, 2, 'Recursos', 'S', 'S', 'N', 'S', 'S'),
(19, 2, 'DiariosCampo', 'S', 'S', 'N', 'S', 'S'),
(20, 2, 'Atores', 'S', 'S', 'N', 'S', 'S'),
(21, 2, 'Ficha1Contato', 'S', 'S', 'N', 'S', 'S'),
(22, 2, 'DiariosProcessoPessoal', 'S', 'S', 'N', 'S', 'S'),
(23, 2, 'AvaliacoesPessoais', 'S', 'S', 'N', 'S', 'S'),
(24, 2, 'SER', 'S', 'S', 'N', 'S', 'S'),
(25, 3, 'Usuarios', 'N', 'N', 'N', 'S', 'S'),
(26, 3, 'Perfis', 'N', 'N', 'N', 'S', 'S'),
(27, 3, 'Atividades', 'N', 'N', 'N', 'S', 'S'),
(28, 3, 'Comunidades', 'N', 'N', 'N', 'S', 'S'),
(29, 3, 'Vulnerabilidades', 'N', 'N', 'N', 'S', 'S'),
(30, 3, 'Recursos', 'N', 'N', 'N', 'S', 'S'),
(31, 3, 'DiariosCampo', 'N', 'N', 'N', 'S', 'S'),
(32, 3, 'Atores', 'N', 'N', 'N', 'S', 'S'),
(33, 3, 'Ficha1Contato', 'N', 'N', 'N', 'S', 'S'),
(34, 3, 'DiariosProcessoPessoal', 'N', 'N', 'N', 'S', 'S'),
(35, 3, 'AvaliacoesPessoais', 'N', 'N', 'N', 'S', 'S'),
(36, 3, 'SER', 'N', 'N', 'N', 'S', 'S'),
(37, 4, 'Usuarios', 'N', 'N', 'N', 'N', 'S'),
(38, 4, 'Perfis', 'N', 'N', 'N', 'N', 'S'),
(39, 4, 'Atividades', 'N', 'N', 'N', 'N', 'S'),
(40, 4, 'Comunidades', 'N', 'N', 'N', 'N', 'S'),
(41, 4, 'Vulnerabilidades', 'N', 'N', 'N', 'N', 'S'),
(42, 4, 'Recursos', 'N', 'N', 'N', 'N', 'S');
INSERT INTO `Permissoes` (`IdPermissoes`, `FkIdPerfil`, `Modulo`, `PodeAtualizar`, `PodeCriar`, `PodeDeletar`, `PodeDetalhar`, `PodeListar`)
VALUES (43, 4, 'DiariosCampo', 'N', 'N', 'N', 'N', 'S'),
(44, 4, 'Atores', 'N', 'N', 'N', 'N', 'S'),
(45, 4, 'Ficha1Contato', 'N', 'N', 'N', 'N', 'S'),
(46, 4, 'DiariosProcessoPessoal', 'N', 'N', 'N', 'N', 'S'),
(47, 4, 'AvaliacoesPessoais', 'N', 'N', 'N', 'N', 'S'),
(48, 4, 'SER', 'N', 'N', 'N', 'N', 'S'),
(49, 5, 'Usuarios', 'S', 'S', 'N', 'S', 'S'),
(50, 5, 'Perfis', 'S', 'S', 'N', 'S', 'S'),
(51, 5, 'Atividades', 'S', 'S', 'N', 'S', 'S'),
(52, 5, 'Comunidades', 'S', 'S', 'N', 'S', 'S'),
(53, 5, 'Vulnerabilidades', 'S', 'S', 'N', 'S', 'S'),
(54, 5, 'Recursos', 'S', 'S', 'N', 'S', 'S'),
(55, 5, 'DiariosCampo', 'S', 'S', 'N', 'S', 'S'),
(56, 5, 'Atores', 'S', 'S', 'N', 'S', 'S'),
(57, 5, 'Ficha1Contato', 'S', 'S', 'N', 'S', 'S'),
(58, 5, 'DiariosProcessoPessoal', 'S', 'S', 'N', 'S', 'S'),
(59, 5, 'AvaliacoesPessoais', 'S', 'S', 'N', 'S', 'S'),
(60, 5, 'SER', 'S', 'S', 'N', 'S', 'S');


INSERT INTO `Usuarios` (`IdUsuario`, `Ativo`, `DtAtualizacao`, `DtCriacao`, `DtNascimento`, `Email`, `FkIdPerfil`, `Foto`, `Genero`, `IdiomaPreferido`, `Nome`, `Ocupacao`, `Senha`)
VALUES (1, 'S', TIMESTAMP '2025-01-01 00:00:00', TIMESTAMP '2024-01-01 00:00:00', TIMESTAMP '1990-01-01 00:00:00', 'joao@email.com', 1, 'foto1.jpg', 1, 0, 'joao', 'Coordenador', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA=='),
(2, 'S', TIMESTAMP '2025-02-01 00:00:00', TIMESTAMP '2024-02-01 00:00:00', TIMESTAMP '1985-02-02 00:00:00', 'u2@example.com', 2, 'foto2.jpg', 2, 0, 'Usuario Dois', 'Pesquisador', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA=='),
(3, 'S', TIMESTAMP '2025-03-01 00:00:00', TIMESTAMP '2024-03-01 00:00:00', TIMESTAMP '1995-03-03 00:00:00', 'u3@example.com', 3, 'foto3.jpg', 1, 0, 'Usuario Tres', 'Voluntario', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA=='),
(4, 'N', TIMESTAMP '2025-04-01 00:00:00', TIMESTAMP '2024-04-01 00:00:00', TIMESTAMP '1992-04-04 00:00:00', 'u4@example.com', 4, 'foto4.jpg', 2, 0, 'Usuario Quatro', 'Analista', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA=='),
(5, 'N', TIMESTAMP '2025-05-01 00:00:00', TIMESTAMP '2024-05-01 00:00:00', TIMESTAMP '1988-05-05 00:00:00', 'u5@example.com', 5, 'foto5.jpg', 1, 0, 'Usuario Cinco', 'Gerente', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==');


CREATE INDEX `IX_Acoes_FkIdAtividade` ON `Acoes` (`FkIdAtividade`);


CREATE INDEX `IX_AcoesAtores_FK_id_Atores` ON `AcoesAtores` (`FK_id_Atores`);


CREATE INDEX `IX_AcoesAtores_FkIdAcoes` ON `AcoesAtores` (`FkIdAcoes`);


CREATE INDEX `IX_AnexosDiario_FkIdDiario` ON `AnexosDiario` (`FkIdDiario`);


CREATE INDEX `IX_Atividades_FkIdComunidade` ON `Atividades` (`FkIdComunidade`);


CREATE INDEX `IX_Atividades_FkIdUsuario` ON `Atividades` (`FkIdUsuario`);


CREATE INDEX `IX_Atividades_FkIdUsuarioM` ON `Atividades` (`FkIdUsuarioM`);


CREATE INDEX `IX_AtividadesEixo_FkIdAtividade` ON `AtividadesEixo` (`FkIdAtividade`);


CREATE INDEX `IX_AtividadesEixo_FkIdEixo` ON `AtividadesEixo` (`FkIdEixo`);


CREATE INDEX `IX_AtorComunidades_FK_id_Atores` ON `AtorComunidades` (`FK_id_Atores`);


CREATE INDEX `IX_AtorComunidades_FkIdComunidade` ON `AtorComunidades` (`FkIdComunidade`);


CREATE INDEX `IX_Atores_FkIdUsuario` ON `Atores` (`FkIdUsuario`);


CREATE INDEX `IX_Atores_FkIdUsuarioM` ON `Atores` (`FkIdUsuarioM`);


CREATE INDEX `IX_AvaliacaoPessoal_FK_id_Atores` ON `AvaliacaoPessoal` (`FK_id_Atores`);


CREATE INDEX `IX_AvaliacaoPessoal_FkIdUsuario` ON `AvaliacaoPessoal` (`FkIdUsuario`);


CREATE INDEX `IX_Comunidades_FK_Id_Usuario` ON `Comunidades` (`FK_Id_Usuario`);


CREATE INDEX `IX_Comunidades_FK_Id_UsuarioM` ON `Comunidades` (`FK_Id_UsuarioM`);


CREATE INDEX `IX_DAAtores_FK_id_Atores` ON `DAAtores` (`FK_id_Atores`);


CREATE INDEX `IX_DAAtores_FkIdDDacoes` ON `DAAtores` (`FkIdDDacoes`);


CREATE INDEX `IX_DetalhesDAcoes_FkIdDDacoes` ON `DetalhesDAcoes` (`FkIdDDacoes`);


CREATE INDEX `IX_DetalhesEixos_FkIdDetalhes` ON `DetalhesEixos` (`FkIdDetalhes`);


CREATE INDEX `IX_DetalhesEixos_FkIdEixo` ON `DetalhesEixos` (`FkIdEixo`);


CREATE INDEX `IX_DiarioAcoes_FkIdAcoes` ON `DiarioAcoes` (`FkIdAcoes`);


CREATE INDEX `IX_DiarioAcoes_FkIdDiario` ON `DiarioAcoes` (`FkIdDiario`);


CREATE INDEX `IX_DiarioDAcoes_FkIdDiario` ON `DiarioDAcoes` (`FkIdDiario`);


CREATE INDEX `IX_DiarioEixos_FkIdDiario` ON `DiarioEixos` (`FkIdDiario`);


CREATE INDEX `IX_DiarioEixos_FkIdEixo` ON `DiarioEixos` (`FkIdEixo`);


CREATE INDEX `IX_DiariosCampo_FkIdComunidade` ON `DiariosCampo` (`FkIdComunidade`);


CREATE INDEX `IX_DiariosCampo_FkIdUsuario` ON `DiariosCampo` (`FkIdUsuario`);


CREATE INDEX `IX_FichaCondicoes_FkIdFicha` ON `FichaCondicoes` (`FkIdFicha`);


CREATE INDEX `IX_FichaPeticoes_FkIdFicha` ON `FichaPeticoes` (`FkIdFicha`);


CREATE INDEX `IX_FichaRespostas_FkIdFicha` ON `FichaRespostas` (`FkIdFicha`);


CREATE INDEX `IX_FichaResultados_FkIdFicha` ON `FichaResultados` (`FkIdFicha`);


CREATE INDEX `IX_FichasPrimeiroContato_FK_id_Atores` ON `FichasPrimeiroContato` (`FK_id_Atores`);


CREATE INDEX `IX_FichasPrimeiroContato_FkIdComunidade` ON `FichasPrimeiroContato` (`FkIdComunidade`);


CREATE INDEX `IX_FichasPrimeiroContato_FkIdUsuario` ON `FichasPrimeiroContato` (`FkIdUsuario`);


CREATE INDEX `IX_FontesInfo_AtorIdAtores` ON `FontesInfo` (`AtorIdAtores`);


CREATE INDEX `IX_FontesInfo_FkIdFicha` ON `FontesInfo` (`FkIdFicha`);


CREATE INDEX `IX_Permissoes_FkIdPerfil` ON `Permissoes` (`FkIdPerfil`);


CREATE INDEX `IX_RecursosAtores_FK_id_Atores` ON `RecursosAtores` (`FK_id_Atores`);


CREATE INDEX `IX_RedeEixos_FkIdEixo` ON `RedeEixos` (`FkIdEixo`);


CREATE INDEX `IX_RedeEixos_FkIdRede` ON `RedeEixos` (`FkIdRede`);


CREATE INDEX `IX_RedeRecursos_FK_id_Atores` ON `RedeRecursos` (`FK_id_Atores`);


CREATE INDEX `IX_RedeRecursos_FkIdComunidade` ON `RedeRecursos` (`FkIdComunidade`);


CREATE INDEX `IX_RedeRecursos_FkIdUsuario` ON `RedeRecursos` (`FkIdUsuario`);


CREATE INDEX `IX_RedesPrimarias_FkIdAtorPrincipal` ON `RedesPrimarias` (`FkIdAtorPrincipal`);


CREATE INDEX `IX_RedesPrimarias_FkIdAtorRelacionados` ON `RedesPrimarias` (`FkIdAtorRelacionados`);


CREATE INDEX `IX_Usuarios_FkIdPerfil` ON `Usuarios` (`FkIdPerfil`);


CREATE INDEX `IX_Vulnerabilidades_FkIdComunidade` ON `Vulnerabilidades` (`FkIdComunidade`);


CREATE INDEX `IX_VulnerabilidadesEixo_FkIdEixo` ON `VulnerabilidadesEixo` (`FkIdEixo`);


CREATE INDEX `IX_VulnerabilidadesEixo_FkIdVulnerabilidade` ON `VulnerabilidadesEixo` (`FkIdVulnerabilidade`);

