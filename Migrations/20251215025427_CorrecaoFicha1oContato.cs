using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoFicha1oContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ficha1oContatoComunidades");

            migrationBuilder.AddColumn<int>(
                name: "FkIdComunidade",
                table: "FichasPrimeiroContato",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 1,
                columns: new[] { "FkIdComunidade", "Status" },
                values: new object[] { 1, "EmProgresso" });

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 2,
                columns: new[] { "FkIdComunidade", "Status" },
                values: new object[] { 2, "EmProgresso" });

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 3,
                columns: new[] { "FkIdComunidade", "Status" },
                values: new object[] { 3, "EmProgresso" });

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 4,
                columns: new[] { "FkIdComunidade", "Status" },
                values: new object[] { 4, "EmProgresso" });

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 5,
                columns: new[] { "FkIdComunidade", "Status" },
                values: new object[] { 5, "EmProgresso" });

            migrationBuilder.CreateIndex(
                name: "IX_FichasPrimeiroContato_FkIdComunidade",
                table: "FichasPrimeiroContato",
                column: "FkIdComunidade");

            migrationBuilder.AddForeignKey(
                name: "FK_FichasPrimeiroContato_Comunidades_FkIdComunidade",
                table: "FichasPrimeiroContato",
                column: "FkIdComunidade",
                principalTable: "Comunidades",
                principalColumn: "IdComunidade",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FichasPrimeiroContato_Comunidades_FkIdComunidade",
                table: "FichasPrimeiroContato");

            migrationBuilder.DropIndex(
                name: "IX_FichasPrimeiroContato_FkIdComunidade",
                table: "FichasPrimeiroContato");

            migrationBuilder.DropColumn(
                name: "FkIdComunidade",
                table: "FichasPrimeiroContato");

            migrationBuilder.CreateTable(
                name: "Ficha1oContatoComunidades",
                columns: table => new
                {
                    IdFichaComunidade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FkIdComunidade = table.Column<int>(type: "INTEGER", nullable: false),
                    IdFicha = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 1,
                column: "Status",
                value: null);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 2,
                column: "Status",
                value: null);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 3,
                column: "Status",
                value: null);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 4,
                column: "Status",
                value: null);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 5,
                column: "Status",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Ficha1oContatoComunidades_FkIdComunidade",
                table: "Ficha1oContatoComunidades",
                column: "FkIdComunidade");

            migrationBuilder.CreateIndex(
                name: "IX_Ficha1oContatoComunidades_IdFicha",
                table: "Ficha1oContatoComunidades",
                column: "IdFicha");
        }
    }
}
