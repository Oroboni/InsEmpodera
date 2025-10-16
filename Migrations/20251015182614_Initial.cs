using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acoes",
                columns: table => new
                {
                    IdAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    AtividadeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acoes", x => x.IdAcoes);
                });

            migrationBuilder.CreateTable(
                name: "AcoesAtores",
                columns: table => new
                {
                    IdAAtores = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtorId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcoesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcoesAtores", x => x.IdAAtores);
                });

            migrationBuilder.CreateTable(
                name: "AnexosDiario",
                columns: table => new
                {
                    IdAnexos = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Caminho = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnexosDiario", x => x.IdAnexos);
                });

            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    IdAtividade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.IdAtividade);
                });

            migrationBuilder.CreateTable(
                name: "AtorComunidades",
                columns: table => new
                {
                    IdAComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComunidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    AtorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtorComunidades", x => x.IdAComunidade);
                });

            migrationBuilder.CreateTable(
                name: "Atores",
                columns: table => new
                {
                    IdAtores = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: false),
                    Idade = table.Column<int>(type: "INTEGER", nullable: false),
                    PapelSocial1 = table.Column<string>(type: "TEXT", nullable: false),
                    PapelSocial2 = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<int>(type: "INTEGER", nullable: false),
                    Extra = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atores", x => x.IdAtores);
                });

            migrationBuilder.CreateTable(
                name: "Comunidades",
                columns: table => new
                {
                    IdComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Local = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    DescricaoAcessibilidade = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunidades", x => x.IdComunidade);
                });

            migrationBuilder.CreateTable(
                name: "DiarioAcoes",
                columns: table => new
                {
                    IdDAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AcoesId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioAcoes", x => x.IdDAcoes);
                });

            migrationBuilder.CreateTable(
                name: "DiarioEixos",
                columns: table => new
                {
                    IdDiarioEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    EixoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioEixos", x => x.IdDiarioEixo);
                });

            migrationBuilder.CreateTable(
                name: "DiariosCampo",
                columns: table => new
                {
                    IdDCampo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComunidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiariosCampo", x => x.IdDCampo);
                });

            migrationBuilder.CreateTable(
                name: "Eixos",
                columns: table => new
                {
                    IdEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eixos", x => x.IdEixo);
                });

            migrationBuilder.CreateTable(
                name: "PerfisAcesso",
                columns: table => new
                {
                    IdPAcesso = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisAcesso", x => x.IdPAcesso);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    IdPermissoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PerfilAcessoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissaoNome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.IdPermissoes);
                });

            migrationBuilder.CreateTable(
                name: "RedeEixos",
                columns: table => new
                {
                    IdRedeEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RedeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EixoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedeEixos", x => x.IdRedeEixo);
                });

            migrationBuilder.CreateTable(
                name: "Redes",
                columns: table => new
                {
                    IdRede = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComunidadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Dispositivo = table.Column<string>(type: "TEXT", nullable: false),
                    Servicos = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redes", x => x.IdRede);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPerfis",
                columns: table => new
                {
                    IdPUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    PerfilAcessoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPerfis", x => x.IdPUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Senha = table.Column<string>(type: "TEXT", nullable: false),
                    Foto = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Ocupacao = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: false),
                    DtNascimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NivelPermissao = table.Column<int>(type: "INTEGER", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "VulnerabilidadeEixos",
                columns: table => new
                {
                    IdVEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EixoId = table.Column<int>(type: "INTEGER", nullable: false),
                    VulnerabilidadeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnerabilidadeEixos", x => x.IdVEixo);
                });

            migrationBuilder.CreateTable(
                name: "Vulnerabilidades",
                columns: table => new
                {
                    IdVulnerabilidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    Servicos = table.Column<string>(type: "TEXT", nullable: false),
                    ComunidadeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vulnerabilidades", x => x.IdVulnerabilidade);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacoesPessoais",
                columns: table => new
                {
                    IdAvaliacao = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CCrimes = table.Column<int>(type: "INTEGER", nullable: false),
                    Substancias = table.Column<int>(type: "INTEGER", nullable: false),
                    Moradia = table.Column<int>(type: "INTEGER", nullable: false),
                    Prevenção = table.Column<int>(type: "INTEGER", nullable: false),
                    AssBasica = table.Column<int>(type: "INTEGER", nullable: false),
                    Educacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Saude = table.Column<int>(type: "INTEGER", nullable: false),
                    Ocupacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Lazer = table.Column<int>(type: "INTEGER", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesPessoais", x => x.IdAvaliacao);
                    table.ForeignKey(
                        name: "FK_AvaliacoesPessoais_Atores_AtorId",
                        column: x => x.AtorId,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichasPrimeiroContato",
                columns: table => new
                {
                    IdFicha = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LContato = table.Column<string>(type: "TEXT", nullable: false),
                    FonteDados = table.Column<string>(type: "TEXT", nullable: false),
                    EstaFamiliar = table.Column<string>(type: "TEXT", nullable: false),
                    EstruFamiliar = table.Column<string>(type: "TEXT", nullable: false),
                    NFilhos = table.Column<int>(type: "INTEGER", nullable: false),
                    NFilhas = table.Column<int>(type: "INTEGER", nullable: false),
                    AEscola = table.Column<int>(type: "INTEGER", nullable: false),
                    SLer = table.Column<string>(type: "TEXT", nullable: false),
                    SCalc = table.Column<string>(type: "TEXT", nullable: false),
                    SComp = table.Column<string>(type: "TEXT", nullable: false),
                    QReabili = table.Column<int>(type: "INTEGER", nullable: false),
                    LTrat = table.Column<string>(type: "TEXT", nullable: false),
                    Coment = table.Column<string>(type: "TEXT", nullable: false),
                    CPrimeiroContato = table.Column<string>(type: "TEXT", nullable: false),
                    EParceiro = table.Column<string>(type: "TEXT", nullable: false),
                    FPeloParceirto = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasPrimeiroContato", x => x.IdFicha);
                    table.ForeignKey(
                        name: "FK_FichasPrimeiroContato_Atores_AtorId",
                        column: x => x.AtorId,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedesPrimarias",
                columns: table => new
                {
                    IdRedePrimaria = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtorPrincipalId = table.Column<int>(type: "INTEGER", nullable: false),
                    AtorRelacionadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoRelacao = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedesPrimarias", x => x.IdRedePrimaria);
                    table.ForeignKey(
                        name: "FK_RedesPrimarias_Atores_AtorRelacionadoId",
                        column: x => x.AtorRelacionadoId,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AtividadeEixos",
                columns: table => new
                {
                    IdAEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EixoId = table.Column<int>(type: "INTEGER", nullable: false),
                    AtividadeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadeEixos", x => x.IdAEixo);
                    table.ForeignKey(
                        name: "FK_AtividadeEixos_Atividades_AtividadeId",
                        column: x => x.AtividadeId,
                        principalTable: "Atividades",
                        principalColumn: "IdAtividade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtividadeEixos_Eixos_EixoId",
                        column: x => x.EixoId,
                        principalTable: "Eixos",
                        principalColumn: "IdEixo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaCondicoes",
                columns: table => new
                {
                    IdCondicoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FichaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cond = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaCondicoes", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaCondicoes_FichasPrimeiroContato_FichaId",
                        column: x => x.FichaId,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaPeticoes",
                columns: table => new
                {
                    IdPeticoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FichaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Pet = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaPeticoes", x => x.IdPeticoes);
                    table.ForeignKey(
                        name: "FK_FichaPeticoes_FichasPrimeiroContato_FichaId",
                        column: x => x.FichaId,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaRespostas",
                columns: table => new
                {
                    IdCondicoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FichaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Resp = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaRespostas", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaRespostas_FichasPrimeiroContato_FichaId",
                        column: x => x.FichaId,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaResultados",
                columns: table => new
                {
                    IdCondicoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FichaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaResultados", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaResultados_FichasPrimeiroContato_FichaId",
                        column: x => x.FichaId,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Acoes",
                columns: new[] { "IdAcoes", "AtividadeId", "Quantidade" },
                values: new object[,]
                {
                    { 1, 1, 10 },
                    { 2, 2, 15 },
                    { 3, 3, 20 },
                    { 4, 4, 12 },
                    { 5, 5, 25 }
                });

            migrationBuilder.InsertData(
                table: "AcoesAtores",
                columns: new[] { "IdAAtores", "AcoesId", "AtorId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "AnexosDiario",
                columns: new[] { "IdAnexos", "Caminho", "DiarioId" },
                values: new object[,]
                {
                    { 1, "foto1.png", 1 },
                    { 2, "foto2.png", 2 },
                    { 3, "documento1.pdf", 3 },
                    { 4, "audio1.mp3", 4 },
                    { 5, "video1.mp4", 5 }
                });

            migrationBuilder.InsertData(
                table: "Atividades",
                columns: new[] { "IdAtividade", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, "Atividade de pintura e desenho", "Oficina de Artes" },
                    { 2, "Instrumentos e canto", "Aula de Música" },
                    { 3, "Futebol, vôlei, basquete", "Esporte Comunitário" },
                    { 4, "Informática básica", "Curso de Tecnologia" },
                    { 5, "Troca de produtos", "Feira Solidária" }
                });

            migrationBuilder.InsertData(
                table: "AtorComunidades",
                columns: new[] { "IdAComunidade", "AtorId", "ComunidadeId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "Atores",
                columns: new[] { "IdAtores", "DtCriacao", "DtModificacao", "Extra", "Genero", "Idade", "Nome", "PapelSocial1", "PapelSocial2", "Telefone" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ativo", "Masculino", 30, "Lucas Andrade", "Líder", "Professor", 119111111 },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ativo", "Feminino", 28, "Fernanda Costa", "Médica", "Voluntária", 219222222 },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ativo", "Masculino", 35, "Roberto Alves", "Advogado", "Consultor", 319333333 },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ativo", "Feminino", 26, "Juliana Prado", "Estudante", "Estagiária", 419444444 },
                    { 5, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ativo", "Masculino", 40, "Marcelo Nunes", "Empresário", "Mentor", 519555555 }
                });

            migrationBuilder.InsertData(
                table: "Comunidades",
                columns: new[] { "IdComunidade", "Complemento", "Descricao", "DescricaoAcessibilidade", "DtCriacao", "DtModificacao", "Local", "Nome", "Status" },
                values: new object[,]
                {
                    { 1, "Zona Leste", "Comunidade em SP", "Acessível", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "São Paulo", "Comunidade A", "Ativa" },
                    { 2, "Zona Norte", "Comunidade no RJ", "Parcial", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rio de Janeiro", "Comunidade B", "Ativa" },
                    { 3, "Centro", "Comunidade em BH", "Baixa", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Belo Horizonte", "Comunidade C", "Inativa" },
                    { 4, "Sul", "Comunidade no PR", "Alta", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Curitiba", "Comunidade D", "Ativa" },
                    { 5, "Norte", "Comunidade na BA", "Média", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Salvador", "Comunidade E", "Ativa" }
                });

            migrationBuilder.InsertData(
                table: "DiarioAcoes",
                columns: new[] { "IdDAcoes", "AcoesId", "DiarioId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "DiarioEixos",
                columns: new[] { "IdDiarioEixo", "DiarioId", "EixoId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "DiariosCampo",
                columns: new[] { "IdDCampo", "ComunidadeId", "Data", "Descricao", "DtCriacao", "DtModificacao", "Localizacao" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reunião comunitária", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Praça central" },
                    { 2, 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Atividade esportiva", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quadra" },
                    { 3, 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Feira cultural", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Centro comunitário" },
                    { 4, 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ação social", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Escola local" },
                    { 5, 5, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Encontro de líderes", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Associação" }
                });

            migrationBuilder.InsertData(
                table: "Eixos",
                columns: new[] { "IdEixo", "Nome" },
                values: new object[,]
                {
                    { 1, "Educação" },
                    { 2, "Saúde" },
                    { 3, "Segurança" },
                    { 4, "Cultura" },
                    { 5, "Infraestrutura" }
                });

            migrationBuilder.InsertData(
                table: "PerfisAcesso",
                column: "IdPAcesso",
                values: new object[]
                {
                    1,
                    2,
                    3,
                    4,
                    5
                });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "IdPermissoes", "PerfilAcessoId", "PermissaoNome" },
                values: new object[,]
                {
                    { 1, 1, "Ler" },
                    { 2, 2, "Escrever" },
                    { 3, 3, "Editar" },
                    { 4, 4, "Excluir" },
                    { 5, 5, "Administrador" }
                });

            migrationBuilder.InsertData(
                table: "RedeEixos",
                columns: new[] { "IdRedeEixo", "EixoId", "RedeId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "Redes",
                columns: new[] { "IdRede", "AtorId", "ComunidadeId", "Dispositivo", "DtCriacao", "DtModificacao", "Servicos", "Tipo" },
                values: new object[,]
                {
                    { 1, 1, 1, "PC", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Acesso remoto", "Internet" },
                    { 2, 2, 2, "Celular", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chamadas", "Telefonia" },
                    { 3, 3, 3, "Gerador", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Suporte elétrico", "Energia" },
                    { 4, 4, 4, "Reservatório", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Abastecimento", "Água" },
                    { 5, 5, 5, "Clínica", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Atendimento médico", "Saúde" }
                });

            migrationBuilder.InsertData(
                table: "UsuarioPerfis",
                columns: new[] { "IdPUsuario", "PerfilAcessoId", "UsuarioId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "IdUsuario", "DtCriacao", "DtNascimento", "Email", "Foto", "Genero", "NivelPermissao", "Nome", "Ocupacao", "Senha" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "joao@email.com", "joao.png", "Masculino", 1, "João Silva", "Professor", "123456" },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "maria@email.com", "maria.png", "Feminino", 2, "Maria Souza", "Engenheira", "123456" },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2000, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "carlos@email.com", "carlos.png", "Masculino", 1, "Carlos Lima", "Estudante", "123456" },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "ana@email.com", "ana.png", "Feminino", 3, "Ana Pereira", "Médica", "123456" },
                    { 5, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "pedro@email.com", "pedro.png", "Masculino", 2, "Pedro Santos", "Advogado", "123456" }
                });

            migrationBuilder.InsertData(
                table: "VulnerabilidadeEixos",
                columns: new[] { "IdVEixo", "EixoId", "VulnerabilidadeId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "Vulnerabilidades",
                columns: new[] { "IdVulnerabilidade", "ComunidadeId", "Localizacao", "Nome", "Servicos" },
                values: new object[,]
                {
                    { 1, 1, "Zona Norte", "Falta de saneamento", "Nenhum" },
                    { 2, 2, "Zona Leste", "Violência urbana", "Polícia comunitária" },
                    { 3, 3, "Centro", "Desemprego", "Cursos profissionalizantes" },
                    { 4, 4, "Zona Oeste", "Drogas", "Clínica de reabilitação" },
                    { 5, 5, "Zona Sul", "Falta de escolas", "Projetos de ensino" }
                });

            migrationBuilder.InsertData(
                table: "AtividadeEixos",
                columns: new[] { "IdAEixo", "AtividadeId", "EixoId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "AvaliacoesPessoais",
                columns: new[] { "IdAvaliacao", "AssBasica", "AtorId", "CCrimes", "DtCriacao", "DtModificacao", "Educacao", "Lazer", "Moradia", "Ocupacao", "Prevenção", "Saude", "Substancias" },
                values: new object[,]
                {
                    { 1, 2, 1, 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2, 1, 1, 1, 2, 0 },
                    { 2, 2, 2, 0, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 3, 2, 2, 1, 3, 1 },
                    { 3, 3, 3, 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, 3, 3, 2, 2, 1 },
                    { 4, 4, 4, 0, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 4, 1, 2, 2, 3, 0 },
                    { 5, 2, 5, 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2, 2, 3, 3, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "FichasPrimeiroContato",
                columns: new[] { "IdFicha", "AEscola", "AtorId", "CPrimeiroContato", "Coment", "Data", "DtCriacao", "DtModificacao", "EParceiro", "EstaFamiliar", "EstruFamiliar", "FPeloParceirto", "FonteDados", "LContato", "LTrat", "Localizacao", "NFilhas", "NFilhos", "QReabili", "SCalc", "SComp", "SLer" },
                values: new object[,]
                {
                    { 1, 1, 1, "2023-1", "Boa integração", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sim", "Sim", "Completa", "Não", "Censo", "Vizinho", "Nenhum", "Zona Norte", 1, 2, 0, "Sim", "Não", "Sim" },
                    { 2, 0, 2, "2023-2", "Necessita apoio", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Não", "Não", "Parcial", "Não", "Empoderavista", "ONG", "Clínica", "Zona Sul", 0, 1, 1, "Não", "Não", "Sim" },
                    { 3, 1, 3, "2023-3", "Boa situação", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sim", "Sim", "Completa", "Sim", "Formulário", "Assistente social", "Nenhum", "Zona Leste", 2, 3, 0, "Sim", "Sim", "Sim" },
                    { 4, 1, 4, "2023-4", "Vulnerável", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Não", "Não", "Incompleta", "Não", "Empoderavista", "Igreja", "Hospital", "Zona Oeste", 1, 0, 2, "Não", "Não", "Não" },
                    { 5, 1, 5, "2023-5", "Estável", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sim", "Sim", "Completa", "Não", "Censo escolar", "Escola", "Nenhum", "Centro", 2, 2, 0, "Sim", "Sim", "Sim" }
                });

            migrationBuilder.InsertData(
                table: "RedesPrimarias",
                columns: new[] { "IdRedePrimaria", "AtorPrincipalId", "AtorRelacionadoId", "TipoRelacao" },
                values: new object[,]
                {
                    { 1, 1, 2, "Amizade" },
                    { 2, 2, 3, "Parceria" },
                    { 3, 3, 4, "Trabalho" },
                    { 4, 4, 5, "Estudo" },
                    { 5, 5, 1, "Mentoria" }
                });

            migrationBuilder.InsertData(
                table: "FichaCondicoes",
                columns: new[] { "IdCondicoes", "Cond", "FichaId" },
                values: new object[,]
                {
                    { 1, "Boa saúde", 1 },
                    { 2, "Necessita assistência", 2 },
                    { 3, "Situação estável", 3 },
                    { 4, "Risco social", 4 },
                    { 5, "Bem-estar adequado", 5 }
                });

            migrationBuilder.InsertData(
                table: "FichaPeticoes",
                columns: new[] { "IdPeticoes", "FichaId", "Pet" },
                values: new object[,]
                {
                    { 1, 1, "Solicitação de escola" },
                    { 2, 2, "Auxílio financeiro" },
                    { 3, 3, "Atendimento médico" },
                    { 4, 4, "Programa social" },
                    { 5, 5, "Cursos de capacitação" }
                });

            migrationBuilder.InsertData(
                table: "FichaRespostas",
                columns: new[] { "IdCondicoes", "FichaId", "Resp" },
                values: new object[,]
                {
                    { 1, 1, "Atendido" },
                    { 2, 2, "Pendente" },
                    { 3, 3, "Atendido" },
                    { 4, 4, "Em análise" },
                    { 5, 5, "Negado" }
                });

            migrationBuilder.InsertData(
                table: "FichaResultados",
                columns: new[] { "IdCondicoes", "FichaId", "Result" },
                values: new object[,]
                {
                    { 1, 1, "Positivo" },
                    { 2, 2, "Negativo" },
                    { 3, 3, "Positivo" },
                    { 4, 4, "Neutro" },
                    { 5, 5, "Positivo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtividadeEixos_AtividadeId",
                table: "AtividadeEixos",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadeEixos_EixoId",
                table: "AtividadeEixos",
                column: "EixoId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesPessoais_AtorId",
                table: "AvaliacoesPessoais",
                column: "AtorId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaCondicoes_FichaId",
                table: "FichaCondicoes",
                column: "FichaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaPeticoes_FichaId",
                table: "FichaPeticoes",
                column: "FichaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaRespostas_FichaId",
                table: "FichaRespostas",
                column: "FichaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaResultados_FichaId",
                table: "FichaResultados",
                column: "FichaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichasPrimeiroContato_AtorId",
                table: "FichasPrimeiroContato",
                column: "AtorId");

            migrationBuilder.CreateIndex(
                name: "IX_RedesPrimarias_AtorRelacionadoId",
                table: "RedesPrimarias",
                column: "AtorRelacionadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acoes");

            migrationBuilder.DropTable(
                name: "AcoesAtores");

            migrationBuilder.DropTable(
                name: "AnexosDiario");

            migrationBuilder.DropTable(
                name: "AtividadeEixos");

            migrationBuilder.DropTable(
                name: "AtorComunidades");

            migrationBuilder.DropTable(
                name: "AvaliacoesPessoais");

            migrationBuilder.DropTable(
                name: "Comunidades");

            migrationBuilder.DropTable(
                name: "DiarioAcoes");

            migrationBuilder.DropTable(
                name: "DiarioEixos");

            migrationBuilder.DropTable(
                name: "DiariosCampo");

            migrationBuilder.DropTable(
                name: "FichaCondicoes");

            migrationBuilder.DropTable(
                name: "FichaPeticoes");

            migrationBuilder.DropTable(
                name: "FichaRespostas");

            migrationBuilder.DropTable(
                name: "FichaResultados");

            migrationBuilder.DropTable(
                name: "PerfisAcesso");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "RedeEixos");

            migrationBuilder.DropTable(
                name: "Redes");

            migrationBuilder.DropTable(
                name: "RedesPrimarias");

            migrationBuilder.DropTable(
                name: "UsuarioPerfis");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "VulnerabilidadeEixos");

            migrationBuilder.DropTable(
                name: "Vulnerabilidades");

            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "Eixos");

            migrationBuilder.DropTable(
                name: "FichasPrimeiroContato");

            migrationBuilder.DropTable(
                name: "Atores");
        }
    }
}
