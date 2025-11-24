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
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Atores",
                columns: table => new
                {
                    IdAtores = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: true),
                    DtNascimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PapelSocial1 = table.Column<string>(type: "TEXT", nullable: true),
                    PapelSocial2 = table.Column<string>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", nullable: true),
                    Extra = table.Column<string>(type: "TEXT", nullable: true),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioIdUsuario = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atores", x => x.IdAtores);
                    table.ForeignKey(
                        name: "FK_Atores_Usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
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
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FkIdUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunidades", x => x.IdComunidade);
                    table.ForeignKey(
                        name: "FK_Comunidades_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
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
                    table.ForeignKey(
                        name: "FK_Perfis_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacoesPessoais",
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
                    table.PrimaryKey("PK_AvaliacoesPessoais", x => x.IdAvaliacao);
                    table.ForeignKey(
                        name: "FK_AvaliacoesPessoais_Atores_FKidAtores",
                        column: x => x.FKidAtores,
                        principalTable: "Atores",
                        principalColumn: "IdAtores",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacoesPessoais_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichasPrimeiroContato",
                columns: table => new
                {
                    IdFicha = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", nullable: true),
                    Emprego = table.Column<string>(type: "TEXT", nullable: false),
                    CEstabeleceu = table.Column<string>(type: "TEXT", nullable: false),
                    NovoParceiro = table.Column<string>(type: "TEXT", nullable: false),
                    FornecidoParceiro = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: true),
                    LContato = table.Column<string>(type: "TEXT", nullable: false),
                    FonteDados = table.Column<string>(type: "TEXT", nullable: false),
                    EstaFamiliar = table.Column<string>(type: "TEXT", nullable: false),
                    EstruFamiliar = table.Column<string>(type: "TEXT", nullable: false),
                    NFIlhos = table.Column<int>(type: "INTEGER", nullable: false),
                    NFilhas = table.Column<int>(type: "INTEGER", nullable: false),
                    AEscolar = table.Column<int>(type: "INTEGER", nullable: false),
                    SLer = table.Column<string>(type: "TEXT", nullable: false),
                    SCalc = table.Column<string>(type: "TEXT", nullable: false),
                    SComp = table.Column<string>(type: "TEXT", nullable: false),
                    QReabili = table.Column<int>(type: "INTEGER", nullable: false),
                    LTrat = table.Column<string>(type: "TEXT", nullable: false),
                    Coment = table.Column<string>(type: "TEXT", nullable: false),
                    DtContato = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraContato = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DtModificacao = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                        name: "FK_FichasPrimeiroContato_Usuarios_FkIdUsuario",
                        column: x => x.FkIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
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
                    UsuarioIdUsuario = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.IdAtividade);
                    table.ForeignKey(
                        name: "FK_Atividades_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "IdComunidade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Atividades_Usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
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
                        principalColumn: "IdComunidade",
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
                        principalColumn: "IdComunidade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiariosCampo_Usuarios_FkIdUsuario",
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
                    FKidAtores = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Dispositivo = table.Column<string>(type: "TEXT", nullable: false),
                    Servicos = table.Column<string>(type: "TEXT", nullable: false),
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
                        principalColumn: "IdComunidade",
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
                        principalColumn: "IdComunidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    IdPermissoes = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdPerfil = table.Column<int>(type: "INTEGER", nullable: false),
                    Permissao = table.Column<string>(type: "TEXT", nullable: false),
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
                table: "Atores",
                columns: new[] { "IdAtores", "Ativo", "DtCriacao", "DtModificacao", "DtNascimento", "Extra", "Genero", "Nome", "PapelSocial1", "PapelSocial2", "Telefone", "UsuarioIdUsuario" },
                values: new object[,]
                {
                    { 1, "S", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 1", "Lider", "Voluntario", "11900000001", null },
                    { 2, "S", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1992, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 2", "Beneficiario", "Membro", "11900000002", null },
                    { 3, "S", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 3", "Parceiro", "Voluntario", "11900000003", null },
                    { 4, "S", new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1991, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 4", "Lider", "Coordenador", "11900000004", null },
                    { 5, "S", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 5", "Beneficiario", "Voluntario", "11900000005", null },
                    { 6, "S", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1993, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 6", "Parceiro", "Membro", "11900000006", null },
                    { 7, "S", new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1994, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 7", "Lider", "Voluntario", "11900000007", null },
                    { 8, "S", new DateTime(2024, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 8", "Beneficiario", "Membro", "11900000008", null },
                    { 9, "S", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1996, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 9", "Parceiro", "Voluntario", "11900000009", null },
                    { 10, "S", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1987, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 10", "Lider", "Coordenador", "11900000010", null },
                    { 11, "S", new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1986, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 11", "Beneficiario", "Membro", "11900000011", null },
                    { 12, "S", new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1989, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 12", "Parceiro", "Voluntario", "11900000012", null },
                    { 13, "S", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1997, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 13", "Membro", "", "11900000013", null },
                    { 14, "S", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1998, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "F", "Ator 14", "Membro", "", "11900000014", null },
                    { 15, "S", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1979, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "M", "Ator 15", "Membro", "", "11900000015", null }
                });

            migrationBuilder.InsertData(
                table: "Eixos",
                columns: new[] { "IdEixo", "Nome" },
                values: new object[,]
                {
                    { 1, "prevenção" },
                    { 2, "ocupação" },
                    { 3, "lazer" },
                    { 4, "segurança social" },
                    { 5, "educação" },
                    { 6, "saúde" },
                    { 7, "assistência básica" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "IdUsuario", "DtAtualizacao", "DtCriacao", "DtNascimento", "Email", "Foto", "Genero", "NivelPermissao", "Nome", "Ocupacao", "Senha" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "joao@email.com", "foto1.jpg", "M", 1, "joao", "Coordenador", "123456" },
                    { 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "u2@example.com", "foto2.jpg", "F", 2, "Usuario Dois", "Pesquisador", "senha2" },
                    { 3, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "u3@example.com", "foto3.jpg", "M", 1, "Usuario Tres", "Voluntario", "senha3" },
                    { 4, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1992, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "u4@example.com", "foto4.jpg", "F", 2, "Usuario Quatro", "Analista", "senha4" },
                    { 5, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "u5@example.com", "foto5.jpg", "M", 3, "Usuario Cinco", "Gerente", "senha5" }
                });

            migrationBuilder.InsertData(
                table: "AvaliacoesPessoais",
                columns: new[] { "IdAvaliacao", "AssBasica", "CCrimes", "DtCriacao", "DtModificacao", "Educacao", "FKidAtores", "FkIdUsuario", "Lazer", "Moradia", "Ocupacao", "Prevencao", "Saude", "Substancias" },
                values: new object[,]
                {
                    { 1, 4, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, 1, 2, 2, 1, 3, 2, 0 },
                    { 2, 3, 0, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 5, 2, 1, 3, 2, 2, 3, 1 },
                    { 3, 2, 2, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 9, 3, 2, 2, 1, 3, 3, 1 },
                    { 4, 4, 0, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 12, 4, 3, 4, 3, 4, 4, 0 },
                    { 5, 1, 3, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 15, 5, 1, 1, 1, 2, 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "Comunidades",
                columns: new[] { "IdComunidade", "Ativo", "Complemento", "Descricao", "DescricaoAcessibilidade", "DtCriacao", "DtModificacao", "FkIdUsuario", "Local", "Nome", "Status" },
                values: new object[,]
                {
                    { 1, "S", "", "Comunidade piloto", "Rampa", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Bairro A", "Comunidade Alpha", "Em Processo" },
                    { 2, "S", "Sala 2", "Comunidade secundária", "Elevador", new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Bairro B", "Comunidade Beta", "Em diagnóstico" },
                    { 3, "S", "", "Comunidade remota", "Rampas", new DateTime(2023, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Bairro C", "Comunidade Gamma", "Em diagnóstico" },
                    { 4, "S", "Anexo", "Comunidade urbana", "Acesso", new DateTime(2023, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Bairro D", "Comunidade Delta", "Em diagnóstico" },
                    { 5, "S", "", "Comunidade rural", "Sem acesso especial", new DateTime(2023, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Bairro E", "Comunidade Epsilon", "Em diagnóstico" }
                });

            migrationBuilder.InsertData(
                table: "FichasPrimeiroContato",
                columns: new[] { "IdFicha", "AEscolar", "CEstabeleceu", "Coment", "Complemento", "DtContato", "DtCriacao", "DtModificacao", "Emprego", "Endereco", "EstaFamiliar", "EstruFamiliar", "FKidAtores", "FkIdUsuario", "FonteDados", "FornecidoParceiro", "HoraContato", "LContato", "LTrat", "NFIlhos", "NFilhas", "NovoParceiro", "QReabili", "SCalc", "SComp", "SLer", "Telefone" },
                values: new object[,]
                {
                    { 1, 12, "Sim", "Pessoa comunicativa, busca oportunidade.", "Apto 101", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Auxiliar Administrativo", "Rua das Flores, 123", "Casado", "Família nuclear", 1, 1, "Cadastro local", "Não", new DateTime(2025, 1, 10, 14, 30, 0, 0, DateTimeKind.Unspecified), "Presencial", "Nenhum", 2, 1, "Não", 0, "Sim", "Sim", "Sim", null },
                    { 2, 16, "Não", "Precisa de acompanhamento psicológico.", "Casa", new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Professor", "Av. Brasil, 457", "Solteiro", "Mora sozinho", 2, 1, "Registro comunitário", "Sim", new DateTime(2025, 1, 5, 9, 45, 0, 0, DateTimeKind.Unspecified), "Telefone", "Fisioterapia", 0, 0, "Sim", 1, "Sim", "Sim", "Sim", null },
                    { 3, 8, "Sim", "Demonstra interesse em programas sociais.", "Bloco B", new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Autônomo", "Rua São João, 998", "Casado", "Família extensa", 3, 2, "Auto-relato", "Não", new DateTime(2025, 1, 3, 11, 15, 0, 0, DateTimeKind.Unspecified), "WhatsApp", "Nenhum", 1, 2, "Não", 0, "Não", "Sim", "Sim", null },
                    { 4, 10, "Não", "Procura recolocação no mercado.", "", new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Desempregado", "Travessa do Sol, 55", "Separado", "Família monoparental", 4, 3, "Centro comunitário", "Não", new DateTime(2025, 1, 2, 15, 0, 0, 0, DateTimeKind.Unspecified), "Presencial", "Nenhum", 3, 0, "Não", 0, "Sim", "Não", "Sim", null },
                    { 5, 14, "Sim", "Interessado em projetos educacionais.", "Sala 5", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Comerciante", "Praça Central, 321", "Viúvo", "Família nuclear", 5, 2, "Instituição parceira", "Sim", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "E-mail", "Nenhum", 1, 1, "Não", 0, "Sim", "Sim", "Sim", null }
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
                table: "RedesPrimarias",
                columns: new[] { "IdRedePrimaria", "FkIdAtorPrincipal", "FkIdAtorRelacionados", "TipoRelacao" },
                values: new object[,]
                {
                    { 1, 1, 2, "Parceria" },
                    { 2, 4, 5, "Suporte" },
                    { 3, 7, 8, "Rede" },
                    { 4, 10, 11, "Par" },
                    { 5, 13, 14, "Ligacao" }
                });

            migrationBuilder.InsertData(
                table: "Atividades",
                columns: new[] { "IdAtividade", "Descricao", "DtCriacao", "DtModificacao", "FkIdComunidade", "Foto", "Nome", "UsuarioIdUsuario" },
                values: new object[,]
                {
                    { 1, "Descricao 1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "a1.jpg", "Ativ 1", null },
                    { 2, "Descricao 2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "a2.jpg", "Ativ 2", null },
                    { 3, "Descricao 3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "a3.jpg", "Ativ 3", null },
                    { 4, "Descricao 4", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "a4.jpg", "Ativ 4", null },
                    { 5, "Descricao 5", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "a5.jpg", "Ativ 5", null }
                });

            migrationBuilder.InsertData(
                table: "AtorComunidades",
                columns: new[] { "IdAtorComunidade", "FKidAtores", "FkIdComunidade" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 4, 2 },
                    { 5, 5, 2 },
                    { 6, 6, 2 },
                    { 7, 7, 3 },
                    { 8, 8, 3 },
                    { 9, 9, 3 },
                    { 10, 10, 4 },
                    { 11, 11, 4 },
                    { 12, 12, 4 },
                    { 13, 13, 5 },
                    { 14, 14, 5 },
                    { 15, 15, 5 }
                });

            migrationBuilder.InsertData(
                table: "DiariosCampo",
                columns: new[] { "IdDCampo", "Data", "Descricao", "DtCriacao", "DtModificacao", "FkIdComunidade", "FkIdUsuario", "Foto", "Localizacao" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Visita inicial", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "d1.jpg", "Ponto A" },
                    { 2, new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reunião", new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, "d2.jpg", "Ponto B" },
                    { 3, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diagnóstico", new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, "d3.jpg", "Ponto C" },
                    { 4, new DateTime(2025, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Intervenção", new DateTime(2025, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 4, "d4.jpg", "Ponto D" },
                    { 5, new DateTime(2025, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Acompanhamento", new DateTime(2025, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 5, "d5.jpg", "Ponto E" }
                });

            migrationBuilder.InsertData(
                table: "FichaCondicoes",
                columns: new[] { "IdCondicoes", "Cond", "FkIdFicha" },
                values: new object[,]
                {
                    { 1, "Cond A", 1 },
                    { 2, "Cond B", 2 },
                    { 3, "Cond C", 3 },
                    { 4, "Cond D", 4 },
                    { 5, "Cond E", 5 }
                });

            migrationBuilder.InsertData(
                table: "FichaPeticoes",
                columns: new[] { "IdPeticoes", "FkIdFicha", "Pet" },
                values: new object[,]
                {
                    { 1, 1, "Pet A" },
                    { 2, 2, "Pet B" },
                    { 3, 3, "Pet C" },
                    { 4, 4, "Pet D" },
                    { 5, 5, "Pet E" }
                });

            migrationBuilder.InsertData(
                table: "FichaRespostas",
                columns: new[] { "IdCondicoes", "FkIdFicha", "Resp" },
                values: new object[,]
                {
                    { 1, 1, "Resp A" },
                    { 2, 2, "Resp B" },
                    { 3, 3, "Resp C" },
                    { 4, 4, "Resp D" },
                    { 5, 5, "Resp E" }
                });

            migrationBuilder.InsertData(
                table: "FichaResultados",
                columns: new[] { "IdCondicoes", "FkIdFicha", "Result" },
                values: new object[,]
                {
                    { 1, 1, "Result A" },
                    { 2, 2, "Result B" },
                    { 3, 3, "Result C" },
                    { 4, 4, "Result D" },
                    { 5, 5, "Result E" }
                });

            migrationBuilder.InsertData(
                table: "FontesInfo",
                columns: new[] { "IdFonte", "AtorIdAtores", "Extra", "FkIdFicha", "Fk_Id_Ator", "Genero", "Idade", "Nome", "PapelSocial1", "PapelSocial2", "Telefone" },
                values: new object[,]
                {
                    { 1, null, "", 1, 1, "M", 40, "Fonte A", "Parente", "", "11911111111" },
                    { 2, null, "", 2, 2, "F", 35, "Fonte B", "Vizin", "", "11922222222" },
                    { 3, null, "", 3, 3, "M", 50, "Fonte C", "Agente", "", "11933333333" },
                    { 4, null, "", 4, 4, "F", 28, "Fonte D", "Amigo", "", "11944444444" },
                    { 5, null, "", 5, 5, "M", 60, "Fonte E", "Lider", "", "11955555555" }
                });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "IdPermissoes", "FkIdPerfil", "Permissao", "PodeAtualizar", "PodeCriar", "PodeDeletar", "PodeDetalhar", "PodeListar" },
                values: new object[,]
                {
                    { 1, 1, "Todas", "S", "S", "S", "S", "S" },
                    { 2, 2, "Conteudo", "S", "S", "N", "S", "S" },
                    { 3, 3, "Campo", "N", "N", "N", "S", "S" },
                    { 4, 4, "Leitura", "N", "N", "N", "S", "S" },
                    { 5, 5, "Gerencia", "S", "S", "N", "S", "S" }
                });

            migrationBuilder.InsertData(
                table: "RedeRecursos",
                columns: new[] { "IdRede", "Dispositivo", "DtCriacao", "DtModificacao", "FKidAtores", "FkIdComunidade", "FkIdUsuario", "Servicos", "Tipo" },
                values: new object[,]
                {
                    { 1, "Router", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 1, "Internet", "Wifi" },
                    { 2, "Switch", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 2, 2, "Conexão", "Ponto" },
                    { 3, "OLT", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 3, 3, "Backbone", "Fibra" },
                    { 4, "Modem", new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, 4, 4, "Dados", "4G" },
                    { 5, "Dish", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, 5, 5, "Satélite", "Sat" }
                });

            migrationBuilder.InsertData(
                table: "Vulnerabilidades",
                columns: new[] { "IdVulnerabilidade", "FkIdComunidade", "Localizacao", "Nome", "Servicos" },
                values: new object[,]
                {
                    { 1, 1, "Local 1", "Vuln 1", "Energia" },
                    { 2, 2, "Local 2", "Vuln 2", "Agua" },
                    { 3, 3, "Local 3", "Vuln 3", "Saude" },
                    { 4, 4, "Local 4", "Vuln 4", "Transporte" },
                    { 5, 5, "Local 5", "Vuln 5", "Comunicacao" }
                });

            migrationBuilder.InsertData(
                table: "Acoes",
                columns: new[] { "IdAcoes", "FkIdAtividade", "Nome", "Provedor", "Quantidade" },
                values: new object[,]
                {
                    { 1, 1, "Ação 1", "Fornecedor A", 10 },
                    { 2, 2, "Ação 2", "Fornecedor B", 5 },
                    { 3, 3, "Ação 3", "Fornecedor C", 8 },
                    { 4, 4, "Ação 4", "Fornecedor D", 12 },
                    { 5, 5, "Ação 5", "Fornecedor E", 7 }
                });

            migrationBuilder.InsertData(
                table: "AnexosDiario",
                columns: new[] { "IdAnexos", "Caminho", "FkIdDiario" },
                values: new object[,]
                {
                    { 1, "anexo1.jpg", 1 },
                    { 2, "anexo2.jpg", 2 },
                    { 3, "anexo3.jpg", 3 },
                    { 4, "anexo4.jpg", 4 },
                    { 5, "anexo5.jpg", 5 }
                });

            migrationBuilder.InsertData(
                table: "AtividadesEixo",
                columns: new[] { "IdAEixo", "FkIdAtividade", "FkIdEixo" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "DiarioDAcoes",
                columns: new[] { "IdDAcoes", "FkIdDiario", "Nome", "PeovedorEx", "Quantidade" },
                values: new object[,]
                {
                    { 1, 1, "Coleta", "Local", 10 },
                    { 2, 2, "Distribuicao", "Externo", 5 },
                    { 3, 3, "Treinamento", "Equipe", 8 },
                    { 4, 4, "Levantamento", "Parceiro", 12 },
                    { 5, 5, "Monitoramento", "Equipe", 7 }
                });

            migrationBuilder.InsertData(
                table: "DiarioEixos",
                columns: new[] { "IdDiarioEixo", "FkIdDiario", "FkIdEixo" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "RedeEixos",
                columns: new[] { "IdRedeEixo", "FkIdEixo", "FkIdRede" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "VulnerabilidadesEixo",
                columns: new[] { "IdVEixo", "FkIdEixo", "FkIdVulnerabilidade" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "AcoesAtores",
                columns: new[] { "IdAAtores", "FKidAtores", "FkIdAcoes" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 4, 2 },
                    { 3, 7, 3 },
                    { 4, 10, 4 },
                    { 5, 13, 5 }
                });

            migrationBuilder.InsertData(
                table: "DAAtores",
                columns: new[] { "Id", "FKidAtores", "FkIdDDacoes" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 4, 2 },
                    { 3, 7, 3 },
                    { 4, 10, 4 },
                    { 5, 13, 5 }
                });

            migrationBuilder.InsertData(
                table: "DetalhesDAcoes",
                columns: new[] { "Id", "FkIdDDacoes", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "Detalhe A" },
                    { 2, 2, "Detalhe B" },
                    { 3, 3, "Detalhe C" },
                    { 4, 4, "Detalhe D" },
                    { 5, 5, "Detalhe E" }
                });

            migrationBuilder.InsertData(
                table: "DiarioAcoes",
                columns: new[] { "IdDAcoes", "FkIdAcoes", "FkIdDiario" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "DetalhesEixos",
                columns: new[] { "IdDiarioEixo", "FkIdDetalhes", "FkIdEixo" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 },
                    { 5, 5, 5 }
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
                name: "IX_Atividades_UsuarioIdUsuario",
                table: "Atividades",
                column: "UsuarioIdUsuario");

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
                name: "IX_Atores_UsuarioIdUsuario",
                table: "Atores",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesPessoais_FKidAtores",
                table: "AvaliacoesPessoais",
                column: "FKidAtores");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesPessoais_FkIdUsuario",
                table: "AvaliacoesPessoais",
                column: "FkIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Comunidades_FkIdUsuario",
                table: "Comunidades",
                column: "FkIdUsuario");

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
                name: "IX_Perfis_FkIdUsuario",
                table: "Perfis",
                column: "FkIdUsuario");

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
                name: "AvaliacoesPessoais");

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
                name: "Perfis");

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
        }
    }
}
