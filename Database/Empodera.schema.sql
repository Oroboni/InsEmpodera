-- =====================================================================
-- EcoRedes / Empodera — esquema do banco (MySQL 8.4 LTS ou posterior)
-- Gerado por scripts/GerarSchema a partir de Data/ApplicationDbContext*.cs.
-- Não edite à mão: altere o modelo e rode
--   dotnet run --project scripts/GerarSchema
-- Decisões e checklist da revisão: docs/revisao-ddl-mysql.md
--
-- Execute num banco vazio. O script não apaga bancos nem desliga a
-- verificação de chaves estrangeiras.
-- =====================================================================

-- Padrão do banco: as tabelas herdam o conjunto de caracteres e o collation.
ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;


-- ---------------------------------------------------------------------
-- 1. Tabelas (somente colunas)
-- ---------------------------------------------------------------------
CREATE TABLE acoes (
    IdAcao        INT UNSIGNED NOT NULL,
    Quantidade    INT NOT NULL,
    IdAtividade   INT UNSIGNED NOT NULL,
    Nome          VARCHAR(150) NOT NULL,
    Provedor      VARCHAR(150) NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE acoesatores (
    IdAcaoAtor    INT UNSIGNED NOT NULL,
    IdAtor        INT UNSIGNED NOT NULL,
    IdAcao        INT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE anexosdiario (
    IdAnexo       INT UNSIGNED NOT NULL,
    IdDiarioCampo INT UNSIGNED NOT NULL,
    Caminho       VARCHAR(500) NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE atividades (
    IdAtividade          INT UNSIGNED NOT NULL,
    Nome                 VARCHAR(150) NOT NULL,
    Descricao            VARCHAR(2000) NULL,
    Foto                 VARCHAR(255) NULL,
    IdComunidade         INT UNSIGNED NOT NULL,
    DtCriacao            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao        DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdUsuario            INT UNSIGNED NOT NULL,
    IdUsuarioModificacao INT UNSIGNED NULL
);

CREATE TABLE atividadeseixo (
    IdAtividadeEixo INT UNSIGNED NOT NULL,
    IdEixo          INT UNSIGNED NOT NULL,
    IdAtividade     INT UNSIGNED NOT NULL,
    DtCriacao       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao   DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE atorcomunidades (
    IdAtorComunidade   INT UNSIGNED NOT NULL,
    IdComunidade       INT UNSIGNED NOT NULL,
    IdAtor             INT UNSIGNED NOT NULL,
    TipoRelacionamento TINYINT UNSIGNED NULL,
    DtCriacao          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao      DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE atores (
    IdAtor               INT UNSIGNED NOT NULL,
    Nome                 VARCHAR(150) NOT NULL,
    Genero               TINYINT UNSIGNED NULL,
    Idade                TINYINT UNSIGNED NULL,
    PapelSocial1         VARCHAR(255) NULL,
    PapelSocial2         VARCHAR(255) NULL,
    Interesse            VARCHAR(255) NULL,
    Telefone             VARCHAR(60) NULL,
    DaEquipe             TINYINT UNSIGNED NOT NULL,
    Rope                 TINYINT UNSIGNED NOT NULL,
    Lopiniao             TINYINT UNSIGNED NOT NULL,
    Mcomunidade          TINYINT UNSIGNED NOT NULL,
    DtCriacao            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao        DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    Ativo                CHAR(1) NOT NULL,
    IdUsuario            INT UNSIGNED NOT NULL,
    IdUsuarioModificacao INT UNSIGNED NULL
);

CREATE TABLE avaliacaopessoal (
    IdAvaliacao   INT UNSIGNED NOT NULL,
    IdAtor        INT UNSIGNED NOT NULL,
    CCrimes       TINYINT UNSIGNED NOT NULL,
    Substancias   TINYINT UNSIGNED NOT NULL,
    Moradia       TINYINT UNSIGNED NOT NULL,
    Prevencao     TINYINT UNSIGNED NOT NULL,
    AssBasica     TINYINT UNSIGNED NOT NULL,
    Educacao      TINYINT UNSIGNED NOT NULL,
    Saude         TINYINT UNSIGNED NOT NULL,
    Ocupacao      TINYINT UNSIGNED NOT NULL,
    Lazer         TINYINT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdUsuario     INT UNSIGNED NOT NULL
);

CREATE TABLE comunidades (
    IdComunidade            INT UNSIGNED NOT NULL,
    Nome                    VARCHAR(150) NOT NULL,
    Local                   VARCHAR(500) NULL,
    LocalMapa               VARCHAR(500) NULL,
    LocalSecundario         VARCHAR(500) NULL,
    LocalMapaSecundario     VARCHAR(500) NULL,
    Status                  VARCHAR(30) NOT NULL,
    Complemento             VARCHAR(255) NULL,
    Descricao               VARCHAR(5000) NULL,
    DescricaoAcessibilidade VARCHAR(2000) NULL,
    DtCriacao               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao           DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdUsuario               INT UNSIGNED NOT NULL,
    IdUsuarioModificacao    INT UNSIGNED NULL,
    Ativo                   CHAR(1) NOT NULL
);

CREATE TABLE daatores (
    IdDAAtor      INT UNSIGNED NOT NULL,
    IdDiarioDAcao INT UNSIGNED NOT NULL,
    IdAtor        INT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE detalhesdacoes (
    IdDetalheDAcao INT UNSIGNED NOT NULL,
    Nome           VARCHAR(150) NOT NULL,
    IdDiarioDAcao  INT UNSIGNED NOT NULL,
    DtCriacao      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao  DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE detalheseixos (
    IdDetalheEixo  INT UNSIGNED NOT NULL,
    IdDetalheDAcao INT UNSIGNED NOT NULL,
    IdEixo         INT UNSIGNED NOT NULL,
    DtCriacao      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao  DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE diarioacoes (
    IdDiarioAcao  INT UNSIGNED NOT NULL,
    IdAcao        INT UNSIGNED NOT NULL,
    IdDiarioCampo INT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE diariodacoes (
    IdDiarioDAcao   INT UNSIGNED NOT NULL,
    IdDiarioCampo   INT UNSIGNED NOT NULL,
    Nome            VARCHAR(150) NOT NULL,
    ProvedorExterno VARCHAR(150) NOT NULL,
    Quantidade      INT NOT NULL,
    DtCriacao       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao   DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE diarioeixos (
    IdDiarioEixo  INT UNSIGNED NOT NULL,
    IdDiarioCampo INT UNSIGNED NOT NULL,
    IdEixo        INT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE diarioscampo (
    IdDiarioCampo INT UNSIGNED NOT NULL,
    IdComunidade  INT UNSIGNED NOT NULL,
    Data          DATE NOT NULL,
    Descricao     VARCHAR(8000) NOT NULL,
    Localizacao   VARCHAR(255) NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    Foto          VARCHAR(255) NULL,
    IdUsuario     INT UNSIGNED NOT NULL
);

CREATE TABLE diariosprocessoeixos (
    IdDiarioProcessoEixo INT UNSIGNED NOT NULL,
    IdDiarioProcesso     INT UNSIGNED NOT NULL,
    IdEixo               INT UNSIGNED NOT NULL,
    DtCriacao            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao        DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE diariosprocessopessoal (
    IdDiarioProcesso     INT UNSIGNED NOT NULL,
    IdAtor               INT UNSIGNED NOT NULL,
    Data                 DATE NOT NULL,
    Descricao            VARCHAR(4000) NOT NULL,
    DtCriacao            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao        DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdUsuario            INT UNSIGNED NOT NULL,
    IdUsuarioModificacao INT UNSIGNED NULL
);

CREATE TABLE eixos (
    IdEixo        INT UNSIGNED NOT NULL,
    Nome          VARCHAR(100) NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE fichacondicoes (
    IdFichaCondicao INT UNSIGNED NOT NULL,
    IdFicha         INT UNSIGNED NOT NULL,
    Cond            VARCHAR(150) NOT NULL,
    DtCriacao       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao   DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE fichapeticoes (
    IdFichaPeticao INT UNSIGNED NOT NULL,
    IdFicha        INT UNSIGNED NOT NULL,
    Pet            VARCHAR(150) NOT NULL,
    DtCriacao      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao  DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE ficharespostas (
    IdFichaResposta INT UNSIGNED NOT NULL,
    IdFicha         INT UNSIGNED NOT NULL,
    Resp            VARCHAR(150) NOT NULL,
    DtCriacao       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao   DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE ficharesultados (
    IdFichaResultado INT UNSIGNED NOT NULL,
    IdFicha          INT UNSIGNED NOT NULL,
    Result           VARCHAR(150) NOT NULL,
    DtCriacao        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao    DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE fichasprimeirocontato (
    IdFicha           INT UNSIGNED NOT NULL,
    IdAtor            INT UNSIGNED NOT NULL,
    Endereco          VARCHAR(255) NULL,
    Complemento       VARCHAR(255) NULL,
    Emprego           VARCHAR(150) NULL,
    CEstabeleceu      VARCHAR(100) NULL,
    NovoParceiro      CHAR(1) NULL,
    FornecidoParceiro CHAR(1) NULL,
    Telefone          VARCHAR(60) NULL,
    LContato          VARCHAR(150) NULL,
    FonteDados        VARCHAR(150) NULL,
    EstaFamiliar      VARCHAR(100) NULL,
    EstruFamiliar     VARCHAR(255) NULL,
    NFilhos           TINYINT UNSIGNED NULL,
    NFilhas           TINYINT UNSIGNED NULL,
    AEscolar          TINYINT UNSIGNED NULL,
    Status            VARCHAR(20) NULL,
    SLer              CHAR(1) NULL,
    SCalc             CHAR(1) NULL,
    SComp             CHAR(1) NULL,
    QReabili          TINYINT UNSIGNED NULL,
    LTrat             VARCHAR(150) NULL,
    Coment            VARCHAR(2000) NULL,
    DtContato         DATE NOT NULL,
    HoraContato       TIME NOT NULL,
    DtCriacao         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao     DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdComunidade      INT UNSIGNED NULL,
    IdUsuario         INT UNSIGNED NOT NULL
);

CREATE TABLE fontesinfo (
    IdFonte       INT UNSIGNED NOT NULL,
    IdFicha       INT UNSIGNED NOT NULL,
    Nome          VARCHAR(150) NOT NULL,
    Genero        TINYINT UNSIGNED NULL,
    Idade         TINYINT UNSIGNED NOT NULL,
    PapelSocial1  VARCHAR(255) NOT NULL,
    PapelSocial2  VARCHAR(255) NOT NULL,
    Interesse     VARCHAR(255) NOT NULL,
    Telefone      VARCHAR(60) NOT NULL,
    Extra         VARCHAR(255) NOT NULL,
    IdAtor        INT UNSIGNED NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE perfis (
    IdPerfil      INT UNSIGNED NOT NULL,
    IdUsuario     INT UNSIGNED NULL,
    Nome          VARCHAR(100) NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE permissoes (
    IdPermissao   INT UNSIGNED NOT NULL,
    IdPerfil      INT UNSIGNED NOT NULL,
    Modulo        VARCHAR(50) NOT NULL,
    PodeListar    CHAR(1) NOT NULL,
    PodeDetalhar  CHAR(1) NOT NULL,
    PodeCriar     CHAR(1) NOT NULL,
    PodeAtualizar CHAR(1) NOT NULL,
    PodeDeletar   CHAR(1) NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE recursosatores (
    IdRecursoAtor INT UNSIGNED NOT NULL,
    IdAtor        INT UNSIGNED NOT NULL,
    Tipo          VARCHAR(50) NOT NULL,
    Nome          VARCHAR(150) NOT NULL,
    Pode          CHAR(1) NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE redeeixos (
    IdRedeEixo    INT UNSIGNED NOT NULL,
    IdRede        INT UNSIGNED NOT NULL,
    IdEixo        INT UNSIGNED NOT NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE rederecursos (
    IdRede        INT UNSIGNED NOT NULL,
    IdAtor        INT UNSIGNED NULL,
    IdComunidade  INT UNSIGNED NOT NULL,
    Nome          VARCHAR(150) NULL,
    Tipo          VARCHAR(50) NOT NULL,
    Dispositivo   VARCHAR(150) NULL,
    Localizacao   VARCHAR(255) NULL,
    Servicos      VARCHAR(2000) NULL,
    DtCriacao     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IdUsuario     INT UNSIGNED NOT NULL
);

CREATE TABLE redesprimarias (
    IdRedePrimaria    INT UNSIGNED NOT NULL,
    IdAtorPrincipal   INT UNSIGNED NOT NULL,
    IdAtorRelacionado INT UNSIGNED NOT NULL,
    TipoRelacao       VARCHAR(100) NOT NULL,
    TipoRede          VARCHAR(20) NOT NULL,
    DtCriacao         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao     DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE usuarios (
    IdUsuario            INT UNSIGNED NOT NULL,
    Nome                 VARCHAR(150) NOT NULL,
    Senha                VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    Foto                 VARCHAR(255) NULL,
    Email                VARCHAR(254) NOT NULL,
    Ocupacao             VARCHAR(100) NOT NULL,
    Genero               TINYINT UNSIGNED NULL,
    DtNascimento         DATE NOT NULL,
    DtCriacao            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao        DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    Ativo                CHAR(1) NOT NULL,
    IdiomaPreferido      TINYINT UNSIGNED NOT NULL DEFAULT 0,
    IdPerfil             INT UNSIGNED NOT NULL,
    UserName             VARCHAR(254) NOT NULL,
    NormalizedUserName   VARCHAR(254) NOT NULL,
    NormalizedEmail      VARCHAR(254) NOT NULL,
    EmailConfirmed       TINYINT UNSIGNED NOT NULL,
    SecurityStamp        VARCHAR(100) NOT NULL,
    ConcurrencyStamp     VARCHAR(100) NOT NULL,
    PhoneNumber          VARCHAR(60) NULL,
    PhoneNumberConfirmed TINYINT UNSIGNED NOT NULL,
    TwoFactorEnabled     TINYINT UNSIGNED NOT NULL,
    LockoutEnd           DATETIME NULL,
    LockoutEnabled       TINYINT UNSIGNED NOT NULL,
    AccessFailedCount    INT UNSIGNED NOT NULL
);

CREATE TABLE vulnerabilidades (
    IdVulnerabilidade INT UNSIGNED NOT NULL,
    Nome              VARCHAR(150) NOT NULL,
    Localizacao       VARCHAR(255) NOT NULL,
    Servicos          VARCHAR(2000) NOT NULL,
    IdComunidade      INT UNSIGNED NOT NULL,
    DtCriacao         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao     DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE vulnerabilidadeseixo (
    IdVulnerabilidadeEixo INT UNSIGNED NOT NULL,
    IdEixo                INT UNSIGNED NOT NULL,
    IdVulnerabilidade     INT UNSIGNED NOT NULL,
    DtCriacao             DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DtModificacao         DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);


-- ---------------------------------------------------------------------
-- 2. Chaves primárias (INT UNSIGNED AUTO_INCREMENT), unicidade e verificações
-- ---------------------------------------------------------------------
ALTER TABLE acoes
    ADD PRIMARY KEY (IdAcao),
    MODIFY IdAcao INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_Acoes_Quantidade CHECK (Quantidade >= 0);

ALTER TABLE acoesatores
    ADD PRIMARY KEY (IdAcaoAtor),
    MODIFY IdAcaoAtor INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_AcoesAtores_Acao_Ator UNIQUE (IdAcao, IdAtor);

ALTER TABLE anexosdiario
    ADD PRIMARY KEY (IdAnexo),
    MODIFY IdAnexo INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE atividades
    ADD PRIMARY KEY (IdAtividade),
    MODIFY IdAtividade INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE atividadeseixo
    ADD PRIMARY KEY (IdAtividadeEixo),
    MODIFY IdAtividadeEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_AtividadesEixo_Atividade_Eixo UNIQUE (IdAtividade, IdEixo);

ALTER TABLE atorcomunidades
    ADD PRIMARY KEY (IdAtorComunidade),
    MODIFY IdAtorComunidade INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_AtorComunidades_TipoRelacionamento CHECK (TipoRelacionamento BETWEEN 1 AND 5);

ALTER TABLE atores
    ADD PRIMARY KEY (IdAtor),
    MODIFY IdAtor INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_Atores_Ativo CHECK (Ativo COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Atores_DaEquipe CHECK (DaEquipe IN (0, 1)),
    ADD CONSTRAINT CK_Atores_Genero CHECK (Genero BETWEEN 0 AND 3),
    ADD CONSTRAINT CK_Atores_Idade CHECK (Idade <= 130),
    ADD CONSTRAINT CK_Atores_Lopiniao CHECK (Lopiniao IN (0, 1)),
    ADD CONSTRAINT CK_Atores_Mcomunidade CHECK (Mcomunidade IN (0, 1)),
    ADD CONSTRAINT CK_Atores_Rope CHECK (Rope IN (0, 1));

ALTER TABLE avaliacaopessoal
    ADD PRIMARY KEY (IdAvaliacao),
    MODIFY IdAvaliacao INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_AvaliacaoPessoal_AssBasica CHECK (AssBasica BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_CCrimes CHECK (CCrimes BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Educacao CHECK (Educacao BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Lazer CHECK (Lazer BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Moradia CHECK (Moradia BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Ocupacao CHECK (Ocupacao BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Prevencao CHECK (Prevencao BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Saude CHECK (Saude BETWEEN 1 AND 5),
    ADD CONSTRAINT CK_AvaliacaoPessoal_Substancias CHECK (Substancias BETWEEN 1 AND 5);

ALTER TABLE comunidades
    ADD PRIMARY KEY (IdComunidade),
    MODIFY IdComunidade INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_Comunidades_Ativo CHECK (Ativo COLLATE utf8mb4_bin IN ('S', 'N'));

ALTER TABLE daatores
    ADD PRIMARY KEY (IdDAAtor),
    MODIFY IdDAAtor INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_DAAtores_DiarioDAcao_Ator UNIQUE (IdDiarioDAcao, IdAtor);

ALTER TABLE detalhesdacoes
    ADD PRIMARY KEY (IdDetalheDAcao),
    MODIFY IdDetalheDAcao INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE detalheseixos
    ADD PRIMARY KEY (IdDetalheEixo),
    MODIFY IdDetalheEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_DetalhesEixos_Detalhe_Eixo UNIQUE (IdDetalheDAcao, IdEixo);

ALTER TABLE diarioacoes
    ADD PRIMARY KEY (IdDiarioAcao),
    MODIFY IdDiarioAcao INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_DiarioAcoes_Diario_Acao UNIQUE (IdDiarioCampo, IdAcao);

ALTER TABLE diariodacoes
    ADD PRIMARY KEY (IdDiarioDAcao),
    MODIFY IdDiarioDAcao INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_DiarioDAcoes_Quantidade CHECK (Quantidade >= 0);

ALTER TABLE diarioeixos
    ADD PRIMARY KEY (IdDiarioEixo),
    MODIFY IdDiarioEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_DiarioEixos_Diario_Eixo UNIQUE (IdDiarioCampo, IdEixo);

ALTER TABLE diarioscampo
    ADD PRIMARY KEY (IdDiarioCampo),
    MODIFY IdDiarioCampo INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE diariosprocessoeixos
    ADD PRIMARY KEY (IdDiarioProcessoEixo),
    MODIFY IdDiarioProcessoEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_DiariosProcessoEixos_Diario_Eixo UNIQUE (IdDiarioProcesso, IdEixo);

ALTER TABLE diariosprocessopessoal
    ADD PRIMARY KEY (IdDiarioProcesso),
    MODIFY IdDiarioProcesso INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE eixos
    ADD PRIMARY KEY (IdEixo),
    MODIFY IdEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_Eixos_Nome UNIQUE (Nome);

ALTER TABLE fichacondicoes
    ADD PRIMARY KEY (IdFichaCondicao),
    MODIFY IdFichaCondicao INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE fichapeticoes
    ADD PRIMARY KEY (IdFichaPeticao),
    MODIFY IdFichaPeticao INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE ficharespostas
    ADD PRIMARY KEY (IdFichaResposta),
    MODIFY IdFichaResposta INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE ficharesultados
    ADD PRIMARY KEY (IdFichaResultado),
    MODIFY IdFichaResultado INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE fichasprimeirocontato
    ADD PRIMARY KEY (IdFicha),
    MODIFY IdFicha INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_FichasPrimeiroContato_AEscolar CHECK (AEscolar <= 30),
    ADD CONSTRAINT CK_FichasPrimeiroContato_FornecidoParceiro CHECK (FornecidoParceiro COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_FichasPrimeiroContato_NFilhas CHECK (NFilhas <= 30),
    ADD CONSTRAINT CK_FichasPrimeiroContato_NFilhos CHECK (NFilhos <= 30),
    ADD CONSTRAINT CK_FichasPrimeiroContato_NovoParceiro CHECK (NovoParceiro COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_FichasPrimeiroContato_QReabili CHECK (QReabili <= 100),
    ADD CONSTRAINT CK_FichasPrimeiroContato_SCalc CHECK (SCalc COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_FichasPrimeiroContato_SComp CHECK (SComp COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_FichasPrimeiroContato_SLer CHECK (SLer COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_FichasPrimeiroContato_Status CHECK (Status IN ('EmProgresso', 'Concluida', 'Abandonada'));

ALTER TABLE fontesinfo
    ADD PRIMARY KEY (IdFonte),
    MODIFY IdFonte INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_FontesInfo_Genero CHECK (Genero BETWEEN 0 AND 3),
    ADD CONSTRAINT CK_FontesInfo_Idade CHECK (Idade <= 130);

ALTER TABLE perfis
    ADD PRIMARY KEY (IdPerfil),
    MODIFY IdPerfil INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE permissoes
    ADD PRIMARY KEY (IdPermissao),
    MODIFY IdPermissao INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_Permissoes_Perfil_Modulo UNIQUE (IdPerfil, Modulo),
    ADD CONSTRAINT CK_Permissoes_PodeAtualizar CHECK (PodeAtualizar COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Permissoes_PodeCriar CHECK (PodeCriar COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Permissoes_PodeDeletar CHECK (PodeDeletar COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Permissoes_PodeDetalhar CHECK (PodeDetalhar COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Permissoes_PodeListar CHECK (PodeListar COLLATE utf8mb4_bin IN ('S', 'N'));

ALTER TABLE recursosatores
    ADD PRIMARY KEY (IdRecursoAtor),
    MODIFY IdRecursoAtor INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT CK_RecursosAtores_Pode CHECK (Pode COLLATE utf8mb4_bin IN ('S', 'N'));

ALTER TABLE redeeixos
    ADD PRIMARY KEY (IdRedeEixo),
    MODIFY IdRedeEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_RedeEixos_Rede_Eixo UNIQUE (IdRede, IdEixo);

ALTER TABLE rederecursos
    ADD PRIMARY KEY (IdRede),
    MODIFY IdRede INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE redesprimarias
    ADD PRIMARY KEY (IdRedePrimaria),
    MODIFY IdRedePrimaria INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_RedesPrimarias_Principal_Relacionado_Tipo UNIQUE (IdAtorPrincipal, IdAtorRelacionado, TipoRede),
    ADD CONSTRAINT CK_RedesPrimarias_AtoresDistintos CHECK (IdAtorPrincipal <> IdAtorRelacionado),
    ADD CONSTRAINT CK_RedesPrimarias_TipoRede CHECK (TipoRede IN ('Primaria', 'Subjetiva'));

ALTER TABLE usuarios
    ADD PRIMARY KEY (IdUsuario),
    MODIFY IdUsuario INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_Usuarios_NormalizedEmail UNIQUE (NormalizedEmail),
    ADD CONSTRAINT UQ_Usuarios_NormalizedUserName UNIQUE (NormalizedUserName),
    ADD CONSTRAINT CK_Usuarios_Ativo CHECK (Ativo COLLATE utf8mb4_bin IN ('S', 'N')),
    ADD CONSTRAINT CK_Usuarios_EmailConfirmed CHECK (EmailConfirmed IN (0, 1)),
    ADD CONSTRAINT CK_Usuarios_Genero CHECK (Genero BETWEEN 0 AND 3),
    ADD CONSTRAINT CK_Usuarios_IdiomaPreferido CHECK (IdiomaPreferido BETWEEN 0 AND 3),
    ADD CONSTRAINT CK_Usuarios_LockoutEnabled CHECK (LockoutEnabled IN (0, 1)),
    ADD CONSTRAINT CK_Usuarios_PhoneNumberConfirmed CHECK (PhoneNumberConfirmed IN (0, 1)),
    ADD CONSTRAINT CK_Usuarios_TwoFactorEnabled CHECK (TwoFactorEnabled IN (0, 1));

ALTER TABLE vulnerabilidades
    ADD PRIMARY KEY (IdVulnerabilidade),
    MODIFY IdVulnerabilidade INT UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE vulnerabilidadeseixo
    ADD PRIMARY KEY (IdVulnerabilidadeEixo),
    MODIFY IdVulnerabilidadeEixo INT UNSIGNED NOT NULL AUTO_INCREMENT,
    ADD CONSTRAINT UQ_VulnerabilidadesEixo_Vulnerabilidade_Eixo UNIQUE (IdVulnerabilidade, IdEixo);


-- ---------------------------------------------------------------------
-- 3. Chaves estrangeiras (nome FK_<Filha>_<Pai>, regra de exclusão explícita)
-- ---------------------------------------------------------------------
ALTER TABLE acoes
    ADD CONSTRAINT FK_Acoes_Atividades
        FOREIGN KEY (IdAtividade) REFERENCES atividades (IdAtividade)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE acoesatores
    ADD CONSTRAINT FK_AcoesAtores_Acoes
        FOREIGN KEY (IdAcao) REFERENCES acoes (IdAcao)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_AcoesAtores_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE anexosdiario
    ADD CONSTRAINT FK_AnexosDiario_DiariosCampo
        FOREIGN KEY (IdDiarioCampo) REFERENCES diarioscampo (IdDiarioCampo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE atividades
    ADD CONSTRAINT FK_Atividades_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_Atividades_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_Atividades_Usuarios_Modificacao
        FOREIGN KEY (IdUsuarioModificacao) REFERENCES usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE atividadeseixo
    ADD CONSTRAINT FK_AtividadesEixo_Atividades
        FOREIGN KEY (IdAtividade) REFERENCES atividades (IdAtividade)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_AtividadesEixo_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE atorcomunidades
    ADD CONSTRAINT FK_AtorComunidades_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_AtorComunidades_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE atores
    ADD CONSTRAINT FK_Atores_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_Atores_Usuarios_Modificacao
        FOREIGN KEY (IdUsuarioModificacao) REFERENCES usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE avaliacaopessoal
    ADD CONSTRAINT FK_AvaliacaoPessoal_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_AvaliacaoPessoal_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE comunidades
    ADD CONSTRAINT FK_Comunidades_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_Comunidades_Usuarios_Modificacao
        FOREIGN KEY (IdUsuarioModificacao) REFERENCES usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE daatores
    ADD CONSTRAINT FK_DAAtores_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DAAtores_DiarioDAcoes
        FOREIGN KEY (IdDiarioDAcao) REFERENCES diariodacoes (IdDiarioDAcao)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE detalhesdacoes
    ADD CONSTRAINT FK_DetalhesDAcoes_DiarioDAcoes
        FOREIGN KEY (IdDiarioDAcao) REFERENCES diariodacoes (IdDiarioDAcao)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE detalheseixos
    ADD CONSTRAINT FK_DetalhesEixos_DetalhesDAcoes
        FOREIGN KEY (IdDetalheDAcao) REFERENCES detalhesdacoes (IdDetalheDAcao)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DetalhesEixos_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diarioacoes
    ADD CONSTRAINT FK_DiarioAcoes_Acoes
        FOREIGN KEY (IdAcao) REFERENCES acoes (IdAcao)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiarioAcoes_DiariosCampo
        FOREIGN KEY (IdDiarioCampo) REFERENCES diarioscampo (IdDiarioCampo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diariodacoes
    ADD CONSTRAINT FK_DiarioDAcoes_DiariosCampo
        FOREIGN KEY (IdDiarioCampo) REFERENCES diarioscampo (IdDiarioCampo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diarioeixos
    ADD CONSTRAINT FK_DiarioEixos_DiariosCampo
        FOREIGN KEY (IdDiarioCampo) REFERENCES diarioscampo (IdDiarioCampo)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiarioEixos_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diarioscampo
    ADD CONSTRAINT FK_DiariosCampo_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiariosCampo_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diariosprocessoeixos
    ADD CONSTRAINT FK_DiariosProcessoEixos_DiariosProcessoPessoal
        FOREIGN KEY (IdDiarioProcesso) REFERENCES diariosprocessopessoal (IdDiarioProcesso)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiariosProcessoEixos_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE diariosprocessopessoal
    ADD CONSTRAINT FK_DiariosProcessoPessoal_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiariosProcessoPessoal_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_DiariosProcessoPessoal_Usuarios_Modificacao
        FOREIGN KEY (IdUsuarioModificacao) REFERENCES usuarios (IdUsuario)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE fichacondicoes
    ADD CONSTRAINT FK_FichaCondicoes_FichasPrimeiroContato
        FOREIGN KEY (IdFicha) REFERENCES fichasprimeirocontato (IdFicha)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE fichapeticoes
    ADD CONSTRAINT FK_FichaPeticoes_FichasPrimeiroContato
        FOREIGN KEY (IdFicha) REFERENCES fichasprimeirocontato (IdFicha)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE ficharespostas
    ADD CONSTRAINT FK_FichaRespostas_FichasPrimeiroContato
        FOREIGN KEY (IdFicha) REFERENCES fichasprimeirocontato (IdFicha)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE ficharesultados
    ADD CONSTRAINT FK_FichaResultados_FichasPrimeiroContato
        FOREIGN KEY (IdFicha) REFERENCES fichasprimeirocontato (IdFicha)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE fichasprimeirocontato
    ADD CONSTRAINT FK_FichasPrimeiroContato_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_FichasPrimeiroContato_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_FichasPrimeiroContato_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE fontesinfo
    ADD CONSTRAINT FK_FontesInfo_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE SET NULL ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_FontesInfo_FichasPrimeiroContato
        FOREIGN KEY (IdFicha) REFERENCES fichasprimeirocontato (IdFicha)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE perfis
    ADD CONSTRAINT FK_Perfis_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE SET NULL ON UPDATE RESTRICT;

ALTER TABLE permissoes
    ADD CONSTRAINT FK_Permissoes_Perfis
        FOREIGN KEY (IdPerfil) REFERENCES perfis (IdPerfil)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE recursosatores
    ADD CONSTRAINT FK_RecursosAtores_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE redeeixos
    ADD CONSTRAINT FK_RedeEixos_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_RedeEixos_RedeRecursos
        FOREIGN KEY (IdRede) REFERENCES rederecursos (IdRede)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE rederecursos
    ADD CONSTRAINT FK_RedeRecursos_Atores
        FOREIGN KEY (IdAtor) REFERENCES atores (IdAtor)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_RedeRecursos_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_RedeRecursos_Usuarios
        FOREIGN KEY (IdUsuario) REFERENCES usuarios (IdUsuario)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE redesprimarias
    ADD CONSTRAINT FK_RedesPrimarias_Atores_Principal
        FOREIGN KEY (IdAtorPrincipal) REFERENCES atores (IdAtor)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_RedesPrimarias_Atores_Relacionado
        FOREIGN KEY (IdAtorRelacionado) REFERENCES atores (IdAtor)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE usuarios
    ADD CONSTRAINT FK_Usuarios_Perfis
        FOREIGN KEY (IdPerfil) REFERENCES perfis (IdPerfil)
        ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE vulnerabilidades
    ADD CONSTRAINT FK_Vulnerabilidades_Comunidades
        FOREIGN KEY (IdComunidade) REFERENCES comunidades (IdComunidade)
        ON DELETE CASCADE ON UPDATE RESTRICT;

ALTER TABLE vulnerabilidadeseixo
    ADD CONSTRAINT FK_VulnerabilidadesEixo_Eixos
        FOREIGN KEY (IdEixo) REFERENCES eixos (IdEixo)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    ADD CONSTRAINT FK_VulnerabilidadesEixo_Vulnerabilidades
        FOREIGN KEY (IdVulnerabilidade) REFERENCES vulnerabilidades (IdVulnerabilidade)
        ON DELETE CASCADE ON UPDATE RESTRICT;


-- ---------------------------------------------------------------------
-- 4. Dados iniciais fictícios: perfis, usuários de exemplo e permissões
--    (os perfis padrão não têm usuário criador: IdUsuario fica NULL)
-- ---------------------------------------------------------------------
INSERT INTO perfis (IdPerfil, Nome, DtCriacao) VALUES
    (1, 'Admin', '0001-01-01 00:00:00'),
    (2, 'Editor', '0001-01-01 00:00:00'),
    (3, 'Colaborador', '0001-01-01 00:00:00'),
    (4, 'Visualizador', '0001-01-01 00:00:00'),
    (5, 'Supervisor', '0001-01-01 00:00:00');

INSERT INTO usuarios (IdUsuario, Nome, Senha, Foto, Email, Ocupacao, Genero, DtNascimento, DtCriacao, DtModificacao, Ativo, IdiomaPreferido, IdPerfil, UserName, NormalizedUserName, NormalizedEmail, EmailConfirmed, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount) VALUES
    (1, 'joao', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==', 'foto1.jpg', 'joao@email.com', 'Coordenador', 1, '1990-01-01', '2024-01-01 00:00:00', '2025-01-01 00:00:00', 'S', 0, 1, 'joao@email.com', 'JOAO@EMAIL.COM', 'JOAO@EMAIL.COM', 1, 'seed-security-stamp-1', 'seed-concurrency-stamp-1', 0, 0, 1, 0),
    (2, 'Usuario Dois', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==', 'foto2.jpg', 'u2@example.com', 'Pesquisador', 2, '1985-02-02', '2024-02-01 00:00:00', '2025-02-01 00:00:00', 'S', 0, 2, 'u2@example.com', 'U2@EXAMPLE.COM', 'U2@EXAMPLE.COM', 1, 'seed-security-stamp-2', 'seed-concurrency-stamp-2', 0, 0, 1, 0),
    (3, 'Usuario Tres', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==', 'foto3.jpg', 'u3@example.com', 'Voluntario', 1, '1995-03-03', '2024-03-01 00:00:00', '2025-03-01 00:00:00', 'S', 0, 3, 'u3@example.com', 'U3@EXAMPLE.COM', 'U3@EXAMPLE.COM', 1, 'seed-security-stamp-3', 'seed-concurrency-stamp-3', 0, 0, 1, 0),
    (4, 'Usuario Quatro', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==', 'foto4.jpg', 'u4@example.com', 'Analista', 2, '1992-04-04', '2024-04-01 00:00:00', '2025-04-01 00:00:00', 'N', 0, 4, 'u4@example.com', 'U4@EXAMPLE.COM', 'U4@EXAMPLE.COM', 1, 'seed-security-stamp-4', 'seed-concurrency-stamp-4', 0, 0, 1, 0),
    (5, 'Usuario Cinco', 'AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==', 'foto5.jpg', 'u5@example.com', 'Gerente', 1, '1988-05-05', '2024-05-01 00:00:00', '2025-05-01 00:00:00', 'N', 0, 5, 'u5@example.com', 'U5@EXAMPLE.COM', 'U5@EXAMPLE.COM', 1, 'seed-security-stamp-5', 'seed-concurrency-stamp-5', 0, 0, 1, 0);

INSERT INTO permissoes (IdPermissao, IdPerfil, Modulo, PodeListar, PodeDetalhar, PodeCriar, PodeAtualizar, PodeDeletar) VALUES
    (1, 1, 'Usuarios', 'S', 'S', 'S', 'S', 'S'),
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
    (13, 2, 'Usuarios', 'S', 'S', 'S', 'S', 'N'),
    (14, 2, 'Perfis', 'S', 'S', 'S', 'S', 'N'),
    (15, 2, 'Atividades', 'S', 'S', 'S', 'S', 'N'),
    (16, 2, 'Comunidades', 'S', 'S', 'S', 'S', 'N'),
    (17, 2, 'Vulnerabilidades', 'S', 'S', 'S', 'S', 'N'),
    (18, 2, 'Recursos', 'S', 'S', 'S', 'S', 'N'),
    (19, 2, 'DiariosCampo', 'S', 'S', 'S', 'S', 'N'),
    (20, 2, 'Atores', 'S', 'S', 'S', 'S', 'N'),
    (21, 2, 'Ficha1Contato', 'S', 'S', 'S', 'S', 'N'),
    (22, 2, 'DiariosProcessoPessoal', 'S', 'S', 'S', 'S', 'N'),
    (23, 2, 'AvaliacoesPessoais', 'S', 'S', 'S', 'S', 'N'),
    (24, 2, 'SER', 'S', 'S', 'S', 'S', 'N'),
    (25, 3, 'Usuarios', 'S', 'S', 'N', 'N', 'N'),
    (26, 3, 'Perfis', 'S', 'S', 'N', 'N', 'N'),
    (27, 3, 'Atividades', 'S', 'S', 'N', 'N', 'N'),
    (28, 3, 'Comunidades', 'S', 'S', 'N', 'N', 'N'),
    (29, 3, 'Vulnerabilidades', 'S', 'S', 'N', 'N', 'N'),
    (30, 3, 'Recursos', 'S', 'S', 'N', 'N', 'N'),
    (31, 3, 'DiariosCampo', 'S', 'S', 'N', 'N', 'N'),
    (32, 3, 'Atores', 'S', 'S', 'N', 'N', 'N'),
    (33, 3, 'Ficha1Contato', 'S', 'S', 'N', 'N', 'N'),
    (34, 3, 'DiariosProcessoPessoal', 'S', 'S', 'N', 'N', 'N'),
    (35, 3, 'AvaliacoesPessoais', 'S', 'S', 'N', 'N', 'N'),
    (36, 3, 'SER', 'S', 'S', 'N', 'N', 'N'),
    (37, 4, 'Usuarios', 'S', 'N', 'N', 'N', 'N'),
    (38, 4, 'Perfis', 'S', 'N', 'N', 'N', 'N'),
    (39, 4, 'Atividades', 'S', 'N', 'N', 'N', 'N'),
    (40, 4, 'Comunidades', 'S', 'N', 'N', 'N', 'N'),
    (41, 4, 'Vulnerabilidades', 'S', 'N', 'N', 'N', 'N'),
    (42, 4, 'Recursos', 'S', 'N', 'N', 'N', 'N'),
    (43, 4, 'DiariosCampo', 'S', 'N', 'N', 'N', 'N'),
    (44, 4, 'Atores', 'S', 'N', 'N', 'N', 'N'),
    (45, 4, 'Ficha1Contato', 'S', 'N', 'N', 'N', 'N'),
    (46, 4, 'DiariosProcessoPessoal', 'S', 'N', 'N', 'N', 'N'),
    (47, 4, 'AvaliacoesPessoais', 'S', 'N', 'N', 'N', 'N'),
    (48, 4, 'SER', 'S', 'N', 'N', 'N', 'N'),
    (49, 5, 'Usuarios', 'S', 'S', 'S', 'S', 'N'),
    (50, 5, 'Perfis', 'S', 'S', 'S', 'S', 'N'),
    (51, 5, 'Atividades', 'S', 'S', 'S', 'S', 'N'),
    (52, 5, 'Comunidades', 'S', 'S', 'S', 'S', 'N'),
    (53, 5, 'Vulnerabilidades', 'S', 'S', 'S', 'S', 'N'),
    (54, 5, 'Recursos', 'S', 'S', 'S', 'S', 'N'),
    (55, 5, 'DiariosCampo', 'S', 'S', 'S', 'S', 'N'),
    (56, 5, 'Atores', 'S', 'S', 'S', 'S', 'N'),
    (57, 5, 'Ficha1Contato', 'S', 'S', 'S', 'S', 'N'),
    (58, 5, 'DiariosProcessoPessoal', 'S', 'S', 'S', 'S', 'N'),
    (59, 5, 'AvaliacoesPessoais', 'S', 'S', 'S', 'S', 'N'),
    (60, 5, 'SER', 'S', 'S', 'S', 'S', 'N');

