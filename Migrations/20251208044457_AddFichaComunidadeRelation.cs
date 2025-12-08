using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddFichaComunidadeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ficha1oContatoComunidades",
                columns: table => new
                {
                    IdFichaComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdFicha = table.Column<int>(type: "INTEGER", nullable: false),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ficha1oContatoComunidades", x => x.IdFichaComunidade);
                    table.ForeignKey(
                        name: "FK_Ficha1oContatoComunidades_Comunidades_FkIdComunidade",
                        column: x => x.FkIdComunidade,
                        principalTable: "Comunidades",
                        principalColumn: "IdComunidade",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ficha1oContatoComunidades_FichasPrimeiroContato_IdFicha",
                        column: x => x.IdFicha,
                        principalTable: "FichasPrimeiroContato",
                        principalColumn: "IdFicha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ficha1oContatoComunidades_FkIdComunidade",
                table: "Ficha1oContatoComunidades",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_Ficha1oContatoComunidades_IdFicha",
                table: "Ficha1oContatoComunidades",
                column: "IdFicha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ficha1oContatoComunidades");
        }
    }
}
