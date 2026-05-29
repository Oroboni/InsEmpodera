using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Perfis",
                columns: table => new
                {
                    IdPerfil = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.IdPerfil);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    IdPermissoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdPerfil = table.Column<int>(type: "INTEGER", nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", nullable: false),
                    PodeListar = table.Column<string>(type: "TEXT", nullable: false),
                    PodeDetalhar = table.Column<string>(type: "TEXT", nullable: false),
                    PodeCriar = table.Column<string>(type: "TEXT", nullable: false),
                    PodeAtualizar = table.Column<string>(type: "TEXT", nullable: false),
                    PodeDeletar = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.IdPermissoes);
                    table.ForeignKey(
                        name: "FK_Permissoes_Perfis_FkIdPerfil",
                        column: x => x.FkIdPerfil,
                        principalTable: "Perfis",
                        principalColumn: "IdPerfil",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Senha = table.Column<string>(type: "TEXT", nullable: false),
                    Foto = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Ocupacao = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: true),
                    DtNascimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<string>(type: "TEXT", nullable: false),
                    FkIdPerfil = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_FkIdPerfil",
                        column: x => x.FkIdPerfil,
                        principalTable: "Perfis",
                        principalColumn: "IdPerfil",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Atores",
                columns: table => new
                {
                    IdAtores = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: true),
                    Idade = table.Column<int>(type: "INTEGER", nullable: true),
                    PapelSocial1 = table.Column<string>(type: "TEXT", nullable: true),
                    PapelSocial2 = table.Column<string>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", nullable: true),
                    DaEquipe = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rope = table.Column<bool>(type: "INTEGER", nullable: false),
                    Lopiniao = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mcomunidade = table.Column<bool>(type: "INTEGER", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<string>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdUsuarioM = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atores", x => x.IdAtores);
                    table.ForeignKey(
                        name: "FK_Atores_Usuarios_FkIdUsuarioM",
                        column: x => x.FkIdUsuarioM,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Comunidades",
                columns: table => new
                {
                    Id_Comunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Local = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true),
                    Descricao_Acessibilidade = table.Column<string>(type: "TEXT", nullable: true),
                    Dt_Criacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Dt_Modificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FK_Id_Usuario = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Id_UsuarioM = table.Column<int>(type: "INTEGER", nullable: true),
                    Ativo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunidades", x => x.Id_Comunidade);
                    table.ForeignKey(
                        name: "FK_Comunidades_Usuarios_FK_Id_UsuarioM",
                        column: x => x.FK_Id_UsuarioM,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacaoPessoal",
                columns: table => new
                {
                    IdAvaliacao = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false),
                    CCrimes = table.Column<int>(type: "INTEGER", nullable: false),
                    Substancias = table.Column<int>(type: "INTEGER", nullable: false),
                    Moradia = table.Column<int>(type: "INTEGER", nullable: false),
                    Prevencao = table.Column<int>(type: "INTEGER", nullable: false),
                    AssBasica = table.Column<int>(type: "INTEGER", nullable: false),
                    Educacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Saude = table.Column<int>(type: "INTEGER", nullable: false),
                    Ocupacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Lazer = table.Column<int>(type: "INTEGER", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacaoPessoal", x => x.IdAvaliacao);
                    table.ForeignKey(
                        name: "FK_AvaliacaoPessoal_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacaoPessoal_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedesPrimarias",
                columns: table => new
                {
                    IdRedePrimaria = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdAtorPrincipal = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdAtorRelacionados = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoRelacao = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedesPrimarias", x => x.IdRedePrimaria);
                    table.ForeignKey(
                        name: "FK_RedesPrimarias_Atores_FkIdAtorPrincipal",
                        column: x => x.FkIdAtorPrincipal,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RedesPrimarias_Atores_FkIdAtorRelacionados",
                        column: x => x.FkIdAtorRelacionados,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    IdAtividade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Foto = table.Column<string>(type: "TEXT", nullable: true),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdUsuarioM = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.IdAtividade);
                    table.ForeignKey(
                        name: "FK_Atividades_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Atividades_Usuarios_FkIdUsuarioM",
                        column: x => x.FkIdUsuarioM,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AtorComunidades",
                columns: table => new
                {
                    IdAtorComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtorComunidades", x => x.IdAtorComunidade);
                    table.ForeignKey(
                        name: "FK_AtorComunidades_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtorComunidades_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiariosCampo",
                columns: table => new
                {
                    IdDCampo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Foto = table.Column<string>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiariosCampo", x => x.IdDCampo);
                    table.ForeignKey(
                        name: "FK_DiariosCampo_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiariosCampo_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FichasPrimeiroContato",
                columns: table => new
                {
                    IdFicha = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: true),
                    Complemento = table.Column<string>(type: "TEXT", nullable: true),
                    Emprego = table.Column<string>(type: "TEXT", nullable: true),
                    CEstabeleceu = table.Column<string>(type: "TEXT", nullable: true),
                    NovoParceiro = table.Column<string>(type: "TEXT", nullable: true),
                    FornecidoParceiro = table.Column<string>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", nullable: true),
                    LContato = table.Column<string>(type: "TEXT", nullable: true),
                    FonteDados = table.Column<string>(type: "TEXT", nullable: true),
                    EstaFamiliar = table.Column<string>(type: "TEXT", nullable: true),
                    EstruFamiliar = table.Column<string>(type: "TEXT", nullable: true),
                    NFIlhos = table.Column<int>(type: "INTEGER", nullable: true),
                    NFilhas = table.Column<int>(type: "INTEGER", nullable: true),
                    AEscolar = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    SLer = table.Column<string>(type: "TEXT", nullable: true),
                    SCalc = table.Column<string>(type: "TEXT", nullable: true),
                    SComp = table.Column<string>(type: "TEXT", nullable: true),
                    QReabili = table.Column<int>(type: "INTEGER", nullable: true),
                    LTrat = table.Column<string>(type: "TEXT", nullable: true),
                    Coment = table.Column<string>(type: "TEXT", nullable: true),
                    DtContato = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraContato = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: true),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasPrimeiroContato", x => x.IdFicha);
                    table.ForeignKey(
                        name: "FK_FichasPrimeiroContato_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FichasPrimeiroContato_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FichasPrimeiroContato_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RedeRecursos",
                columns: table => new
                {
                    IdRede = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: true),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Dispositivo = table.Column<string>(type: "TEXT", nullable: true),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: true),
                    Servicos = table.Column<string>(type: "TEXT", nullable: true),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedeRecursos", x => x.IdRede);
                    table.ForeignKey(
                        name: "FK_RedeRecursos_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RedeRecursos_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RedeRecursos_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
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
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vulnerabilidades", x => x.IdVulnerabilidade);
                    table.ForeignKey(
                        name: "FK_Vulnerabilidades_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "Id_Comunidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Acoes",
                columns: table => new
                {
                    IdAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdAtividade = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Provedor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acoes", x => x.IdAcoes);
                    table.ForeignKey(
                        name: "FK_Acoes_Atividades_FkIdAtividade",
                        column: x => x.FkIdAtividade,
                        principalTable: "Atividades",
                        principalColumn: "IdAtividade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtividadesEixo",
                columns: table => new
                {
                    IdAEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdEixo = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdAtividade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadesEixo", x => x.IdAEixo);
                    table.ForeignKey(
                        name: "FK_AtividadesEixo_Atividades_FkIdAtividade",
                        column: x => x.FkIdAtividade,
                        principalTable: "Atividades",
                        principalColumn: "IdAtividade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtividadesEixo_Eixos_FkIdEixo",
                        column: x => x.FkIdEixo,
                        principalTable: "Eixos",
                        principalColumn: "IdEixo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnexosDiario",
                columns: table => new
                {
                    IdAnexos = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdDiario = table.Column<int>(type: "INTEGER", nullable: false),
                    Caminho = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnexosDiario", x => x.IdAnexos);
                    table.ForeignKey(
                        name: "FK_AnexosDiario_DiariosCampo_FkIdDiario",
                        column: x => x.FkIdDiario,
                        principalTable: "DiariosCampo",
                        principalColumn: "IdDCampo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiarioDAcoes",
                columns: table => new
                {
                    IdDAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdDiario = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    PeovedorEx = table.Column<string>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioDAcoes", x => x.IdDAcoes);
                    table.ForeignKey(
                        name: "FK_DiarioDAcoes_DiariosCampo_FkIdDiario",
                        column: x => x.FkIdDiario,
                        principalTable: "DiariosCampo",
                        principalColumn: "IdDCampo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiarioEixos",
                columns: table => new
                {
                    IdDiarioEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdDiario = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdEixo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioEixos", x => x.IdDiarioEixo);
                    table.ForeignKey(
                        name: "FK_DiarioEixos_DiariosCampo_FkIdDiario",
                        column: x => x.FkIdDiario,
                        principalTable: "DiariosCampo",
                        principalColumn: "IdDCampo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiarioEixos_Eixos_FkIdEixo",
                        column: x => x.FkIdEixo,
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
                    FkIdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    Cond = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaCondicoes", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaCondicoes_FichasPrimeiroContato_FkIdFicha",
                        column: x => x.FkIdFicha,
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
                    FkIdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    Pet = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaPeticoes", x => x.IdPeticoes);
                    table.ForeignKey(
                        name: "FK_FichaPeticoes_FichasPrimeiroContato_FkIdFicha",
                        column: x => x.FkIdFicha,
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
                    FkIdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    Resp = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaRespostas", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaRespostas_FichasPrimeiroContato_FkIdFicha",
                        column: x => x.FkIdFicha,
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
                    FkIdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaResultados", x => x.IdCondicoes);
                    table.ForeignKey(
                        name: "FK_FichaResultados_FichasPrimeiroContato_FkIdFicha",
                        column: x => x.FkIdFicha,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FontesInfo",
                columns: table => new
                {
                    IdFonte = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: false),
                    Idade = table.Column<int>(type: "INTEGER", nullable: false),
                    PapelSocial1 = table.Column<string>(type: "TEXT", nullable: false),
                    PapelSocial2 = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    Extra = table.Column<string>(type: "TEXT", nullable: false),
                    Fk_Id_Ator = table.Column<int>(type: "INTEGER", nullable: false),
                    AtorIdAtores = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FontesInfo", x => x.IdFonte);
                    table.ForeignKey(
                        name: "FK_FontesInfo_Atores_AtorIdAtores",
                        column: x => x.AtorIdAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores");
                    table.ForeignKey(
                        name: "FK_FontesInfo_FichasPrimeiroContato_FkIdFicha",
                        column: x => x.FkIdFicha,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedeEixos",
                columns: table => new
                {
                    IdRedeEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdRede = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdEixo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedeEixos", x => x.IdRedeEixo);
                    table.ForeignKey(
                        name: "FK_RedeEixos_Eixos_FkIdEixo",
                        column: x => x.FkIdEixo,
                        principalTable: "Eixos",
                        principalColumn: "IdEixo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RedeEixos_RedeRecursos_FkIdRede",
                        column: x => x.FkIdRede,
                        principalTable: "RedeRecursos",
                        principalColumn: "IdRede",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VulnerabilidadesEixo",
                columns: table => new
                {
                    IdVEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdEixo = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdVulnerabilidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnerabilidadesEixo", x => x.IdVEixo);
                    table.ForeignKey(
                        name: "FK_VulnerabilidadesEixo_Eixos_FkIdEixo",
                        column: x => x.FkIdEixo,
                        principalTable: "Eixos",
                        principalColumn: "IdEixo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VulnerabilidadesEixo_Vulnerabilidades_FkIdVulnerabilidade",
                        column: x => x.FkIdVulnerabilidade,
                        principalTable: "Vulnerabilidades",
                        principalColumn: "IdVulnerabilidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcoesAtores",
                columns: table => new
                {
                    IdAAtores = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcoesAtores", x => x.IdAAtores);
                    table.ForeignKey(
                        name: "FK_AcoesAtores_Acoes_FkIdAcoes",
                        column: x => x.FkIdAcoes,
                        principalTable: "Acoes",
                        principalColumn: "IdAcoes",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcoesAtores_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiarioAcoes",
                columns: table => new
                {
                    IdDAcoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdAcoes = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdDiario = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioAcoes", x => x.IdDAcoes);
                    table.ForeignKey(
                        name: "FK_DiarioAcoes_Acoes_FkIdAcoes",
                        column: x => x.FkIdAcoes,
                        principalTable: "Acoes",
                        principalColumn: "IdAcoes",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiarioAcoes_DiariosCampo_FkIdDiario",
                        column: x => x.FkIdDiario,
                        principalTable: "DiariosCampo",
                        principalColumn: "IdDCampo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DAAtores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdDDacoes = table.Column<int>(type: "INTEGER", nullable: false),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DAAtores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DAAtores_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DAAtores_DiarioDAcoes_FkIdDDacoes",
                        column: x => x.FkIdDDacoes,
                        principalTable: "DiarioDAcoes",
                        principalColumn: "IdDAcoes",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalhesDAcoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    FkIdDDacoes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalhesDAcoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalhesDAcoes_DiarioDAcoes_FkIdDDacoes",
                        column: x => x.FkIdDDacoes,
                        principalTable: "DiarioDAcoes",
                        principalColumn: "IdDAcoes",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalhesEixos",
                columns: table => new
                {
                    IdDiarioEixo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdDetalhes = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdEixo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalhesEixos", x => x.IdDiarioEixo);
                    table.ForeignKey(
                        name: "FK_DetalhesEixos_DetalhesDAcoes_FkIdDetalhes",
                        column: x => x.FkIdDetalhes,
                        principalTable: "DetalhesDAcoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalhesEixos_Eixos_FkIdEixo",
                        column: x => x.FkIdEixo,
                        principalTable: "Eixos",
                        principalColumn: "IdEixo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Perfis",
                columns: new[] { "IdPerfil", "DtCriacao", "DtModificacao", "FkIdUsuario", "Nome" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Admin" },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Editor" },
                    { 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Colaborador" },
                    { 4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Visualizador" },
                    { 5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Supervisor" }
                });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "IdPermissoes", "FkIdPerfil", "Modulo", "PodeAtualizar", "PodeCriar", "PodeDeletar", "PodeDetalhar", "PodeListar" },
                values: new object[,]
                {
                    { 1, 1, "Usuarios", "S", "S", "S", "S", "S" },
                    { 2, 1, "Perfis", "S", "S", "S", "S", "S" },
                    { 3, 1, "Atividades", "S", "S", "S", "S", "S" },
                    { 4, 1, "Comunidades", "S", "S", "S", "S", "S" },
                    { 5, 1, "Vulnerabilidades", "S", "S", "S", "S", "S" },
                    { 6, 1, "Recursos", "S", "S", "S", "S", "S" },
                    { 7, 1, "DiariosCampo", "S", "S", "S", "S", "S" },
                    { 8, 1, "Atores", "S", "S", "S", "S", "S" },
                    { 9, 1, "Ficha1Contato", "S", "S", "S", "S", "S" },
                    { 10, 1, "DiariosProcessoPessoal", "S", "S", "S", "S", "S" },
                    { 11, 1, "AvaliacoesPessoais", "S", "S", "S", "S", "S" },
                    { 12, 1, "SER", "S", "S", "S", "S", "S" },
                    { 13, 2, "Usuarios", "S", "S", "N", "S", "S" },
                    { 14, 2, "Perfis", "S", "S", "N", "S", "S" },
                    { 15, 2, "Atividades", "S", "S", "N", "S", "S" },
                    { 16, 2, "Comunidades", "S", "S", "N", "S", "S" },
                    { 17, 2, "Vulnerabilidades", "S", "S", "N", "S", "S" },
                    { 18, 2, "Recursos", "S", "S", "N", "S", "S" },
                    { 19, 2, "DiariosCampo", "S", "S", "N", "S", "S" },
                    { 20, 2, "Atores", "S", "S", "N", "S", "S" },
                    { 21, 2, "Ficha1Contato", "S", "S", "N", "S", "S" },
                    { 22, 2, "DiariosProcessoPessoal", "S", "S", "N", "S", "S" },
                    { 23, 2, "AvaliacoesPessoais", "S", "S", "N", "S", "S" },
                    { 24, 2, "SER", "S", "S", "N", "S", "S" },
                    { 25, 3, "Usuarios", "N", "N", "N", "S", "S" },
                    { 26, 3, "Perfis", "N", "N", "N", "S", "S" },
                    { 27, 3, "Atividades", "N", "N", "N", "S", "S" },
                    { 28, 3, "Comunidades", "N", "N", "N", "S", "S" },
                    { 29, 3, "Vulnerabilidades", "N", "N", "N", "S", "S" },
                    { 30, 3, "Recursos", "N", "N", "N", "S", "S" },
                    { 31, 3, "DiariosCampo", "N", "N", "N", "S", "S" },
                    { 32, 3, "Atores", "N", "N", "N", "S", "S" },
                    { 33, 3, "Ficha1Contato", "N", "N", "N", "S", "S" },
                    { 34, 3, "DiariosProcessoPessoal", "N", "N", "N", "S", "S" },
                    { 35, 3, "AvaliacoesPessoais", "N", "N", "N", "S", "S" },
                    { 36, 3, "SER", "N", "N", "N", "S", "S" },
                    { 37, 4, "Usuarios", "N", "N", "N", "N", "S" },
                    { 38, 4, "Perfis", "N", "N", "N", "N", "S" },
                    { 39, 4, "Atividades", "N", "N", "N", "N", "S" },
                    { 40, 4, "Comunidades", "N", "N", "N", "N", "S" },
                    { 41, 4, "Vulnerabilidades", "N", "N", "N", "N", "S" },
                    { 42, 4, "Recursos", "N", "N", "N", "N", "S" },
                    { 43, 4, "DiariosCampo", "N", "N", "N", "N", "S" },
                    { 44, 4, "Atores", "N", "N", "N", "N", "S" },
                    { 45, 4, "Ficha1Contato", "N", "N", "N", "N", "S" },
                    { 46, 4, "DiariosProcessoPessoal", "N", "N", "N", "N", "S" },
                    { 47, 4, "AvaliacoesPessoais", "N", "N", "N", "N", "S" },
                    { 48, 4, "SER", "N", "N", "N", "N", "S" },
                    { 49, 5, "Usuarios", "S", "S", "N", "S", "S" },
                    { 50, 5, "Perfis", "S", "S", "N", "S", "S" },
                    { 51, 5, "Atividades", "S", "S", "N", "S", "S" },
                    { 52, 5, "Comunidades", "S", "S", "N", "S", "S" },
                    { 53, 5, "Vulnerabilidades", "S", "S", "N", "S", "S" },
                    { 54, 5, "Recursos", "S", "S", "N", "S", "S" },
                    { 55, 5, "DiariosCampo", "S", "S", "N", "S", "S" },
                    { 56, 5, "Atores", "S", "S", "N", "S", "S" },
                    { 57, 5, "Ficha1Contato", "S", "S", "N", "S", "S" },
                    { 58, 5, "DiariosProcessoPessoal", "S", "S", "N", "S", "S" },
                    { 59, 5, "AvaliacoesPessoais", "S", "S", "N", "S", "S" },
                    { 60, 5, "SER", "S", "S", "N", "S", "S" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "IdUsuario", "Ativo", "DtAtualizacao", "DtCriacao", "DtNascimento", "Email", "FkIdPerfil", "Foto", "Genero", "Nome", "Ocupacao", "Senha" },
                values: new object[,]
                {
                    { 1, "S", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "joao@email.com", 1, "foto1.jpg", "Masculino", "joao", "Coordenador", "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==" },
                    { 2, "S", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "u2@example.com", 2, "foto2.jpg", "Feminino", "Usuario Dois", "Pesquisador", "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==" },
                    { 3, "S", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "u3@example.com", 3, "foto3.jpg", "Masculino", "Usuario Tres", "Voluntario", "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==" },
                    { 4, "N", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1992, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "u4@example.com", 4, "foto4.jpg", "Feminino", "Usuario Quatro", "Analista", "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==" },
                    { 5, "N", new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "u5@example.com", 5, "foto5.jpg", "Masculino", "Usuario Cinco", "Gerente", "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acoes_FkIdAtividade",
                table: "Acoes",
                column: "FkIdAtividade");

            migrationBuilder.CreateIndex(
                name: "IX_AcoesAtores_FkIdAcoes",
                table: "AcoesAtores",
                column: "FkIdAcoes");

            migrationBuilder.CreateIndex(
                name: "IX_AcoesAtores_FKidAtores",
                table: "AcoesAtores",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_AnexosDiario_FkIdDiario",
                table: "AnexosDiario",
                column: "FkIdDiario");

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_FkIdComunidade",
                table: "Atividades",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_FkIdUsuarioM",
                table: "Atividades",
                column: "FkIdUsuarioM");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesEixo_FkIdAtividade",
                table: "AtividadesEixo",
                column: "FkIdAtividade");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesEixo_FkIdEixo",
                table: "AtividadesEixo",
                column: "FkIdEixo");

            migrationBuilder.CreateIndex(
                name: "IX_AtorComunidades_FKidAtores",
                table: "AtorComunidades",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_AtorComunidades_FkIdComunidade",
                table: "AtorComunidades",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_Atores_FkIdUsuarioM",
                table: "Atores",
                column: "FkIdUsuarioM");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacaoPessoal_FKidAtores",
                table: "AvaliacaoPessoal",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacaoPessoal_FkIdUsuario",
                table: "AvaliacaoPessoal",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Comunidades_FK_Id_UsuarioM",
                table: "Comunidades",
                column: "FK_Id_UsuarioM");

            migrationBuilder.CreateIndex(
                name: "IX_DAAtores_FKidAtores",
                table: "DAAtores",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_DAAtores_FkIdDDacoes",
                table: "DAAtores",
                column: "FkIdDDacoes");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesDAcoes_FkIdDDacoes",
                table: "DetalhesDAcoes",
                column: "FkIdDDacoes");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesEixos_FkIdDetalhes",
                table: "DetalhesEixos",
                column: "FkIdDetalhes");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesEixos_FkIdEixo",
                table: "DetalhesEixos",
                column: "FkIdEixo");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioAcoes_FkIdAcoes",
                table: "DiarioAcoes",
                column: "FkIdAcoes");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioAcoes_FkIdDiario",
                table: "DiarioAcoes",
                column: "FkIdDiario");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioDAcoes_FkIdDiario",
                table: "DiarioDAcoes",
                column: "FkIdDiario");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioEixos_FkIdDiario",
                table: "DiarioEixos",
                column: "FkIdDiario");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioEixos_FkIdEixo",
                table: "DiarioEixos",
                column: "FkIdEixo");

            migrationBuilder.CreateIndex(
                name: "IX_DiariosCampo_FkIdComunidade",
                table: "DiariosCampo",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_DiariosCampo_FkIdUsuario",
                table: "DiariosCampo",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_FichaCondicoes_FkIdFicha",
                table: "FichaCondicoes",
                column: "FkIdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_FichaPeticoes_FkIdFicha",
                table: "FichaPeticoes",
                column: "FkIdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_FichaRespostas_FkIdFicha",
                table: "FichaRespostas",
                column: "FkIdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_FichaResultados_FkIdFicha",
                table: "FichaResultados",
                column: "FkIdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_FichasPrimeiroContato_FKidAtores",
                table: "FichasPrimeiroContato",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_FichasPrimeiroContato_FkIdComunidade",
                table: "FichasPrimeiroContato",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_FichasPrimeiroContato_FkIdUsuario",
                table: "FichasPrimeiroContato",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_FontesInfo_AtorIdAtores",
                table: "FontesInfo",
                column: "AtorIdAtores");

            migrationBuilder.CreateIndex(
                name: "IX_FontesInfo_FkIdFicha",
                table: "FontesInfo",
                column: "FkIdFicha");

            migrationBuilder.CreateIndex(
                name: "IX_Permissoes_FkIdPerfil",
                table: "Permissoes",
                column: "FkIdPerfil");

            migrationBuilder.CreateIndex(
                name: "IX_RedeEixos_FkIdEixo",
                table: "RedeEixos",
                column: "FkIdEixo");

            migrationBuilder.CreateIndex(
                name: "IX_RedeEixos_FkIdRede",
                table: "RedeEixos",
                column: "FkIdRede");

            migrationBuilder.CreateIndex(
                name: "IX_RedeRecursos_FKidAtores",
                table: "RedeRecursos",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_RedeRecursos_FkIdComunidade",
                table: "RedeRecursos",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_RedeRecursos_FkIdUsuario",
                table: "RedeRecursos",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_RedesPrimarias_FkIdAtorPrincipal",
                table: "RedesPrimarias",
                column: "FkIdAtorPrincipal");

            migrationBuilder.CreateIndex(
                name: "IX_RedesPrimarias_FkIdAtorRelacionados",
                table: "RedesPrimarias",
                column: "FkIdAtorRelacionados");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_FkIdPerfil",
                table: "Usuarios",
                column: "FkIdPerfil");

            migrationBuilder.CreateIndex(
                name: "IX_Vulnerabilidades_FkIdComunidade",
                table: "Vulnerabilidades",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilidadesEixo_FkIdEixo",
                table: "VulnerabilidadesEixo",
                column: "FkIdEixo");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilidadesEixo_FkIdVulnerabilidade",
                table: "VulnerabilidadesEixo",
                column: "FkIdVulnerabilidade");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcoesAtores");

            migrationBuilder.DropTable(
                name: "AnexosDiario");

            migrationBuilder.DropTable(
                name: "AtividadesEixo");

            migrationBuilder.DropTable(
                name: "AtorComunidades");

            migrationBuilder.DropTable(
                name: "AvaliacaoPessoal");

            migrationBuilder.DropTable(
                name: "DAAtores");

            migrationBuilder.DropTable(
                name: "DetalhesEixos");

            migrationBuilder.DropTable(
                name: "DiarioAcoes");

            migrationBuilder.DropTable(
                name: "DiarioEixos");

            migrationBuilder.DropTable(
                name: "FichaCondicoes");

            migrationBuilder.DropTable(
                name: "FichaPeticoes");

            migrationBuilder.DropTable(
                name: "FichaRespostas");

            migrationBuilder.DropTable(
                name: "FichaResultados");

            migrationBuilder.DropTable(
                name: "FontesInfo");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "RedeEixos");

            migrationBuilder.DropTable(
                name: "RedesPrimarias");

            migrationBuilder.DropTable(
                name: "VulnerabilidadesEixo");

            migrationBuilder.DropTable(
                name: "DetalhesDAcoes");

            migrationBuilder.DropTable(
                name: "Acoes");

            migrationBuilder.DropTable(
                name: "FichasPrimeiroContato");

            migrationBuilder.DropTable(
                name: "RedeRecursos");

            migrationBuilder.DropTable(
                name: "Eixos");

            migrationBuilder.DropTable(
                name: "Vulnerabilidades");

            migrationBuilder.DropTable(
                name: "DiarioDAcoes");

            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "Atores");

            migrationBuilder.DropTable(
                name: "DiariosCampo");

            migrationBuilder.DropTable(
                name: "Comunidades");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Perfis");
        }
    }
}
