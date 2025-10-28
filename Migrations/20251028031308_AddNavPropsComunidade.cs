using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddNavPropsComunidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Redes_ComunidadeId",
                table: "Redes",
                column: "ComunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiariosCampo_ComunidadeId",
                table: "DiariosCampo",
                column: "ComunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_AtorComunidades_ComunidadeId",
                table: "AtorComunidades",
                column: "ComunidadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AtorComunidades_Comunidades_ComunidadeId",
                table: "AtorComunidades",
                column: "ComunidadeId",
                principalTable: "Comunidades",
                principalColumn: "IdComunidade",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiariosCampo_Comunidades_ComunidadeId",
                table: "DiariosCampo",
                column: "ComunidadeId",
                principalTable: "Comunidades",
                principalColumn: "IdComunidade",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Redes_Comunidades_ComunidadeId",
                table: "Redes",
                column: "ComunidadeId",
                principalTable: "Comunidades",
                principalColumn: "IdComunidade",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AtorComunidades_Comunidades_ComunidadeId",
                table: "AtorComunidades");

            migrationBuilder.DropForeignKey(
                name: "FK_DiariosCampo_Comunidades_ComunidadeId",
                table: "DiariosCampo");

            migrationBuilder.DropForeignKey(
                name: "FK_Redes_Comunidades_ComunidadeId",
                table: "Redes");

            migrationBuilder.DropIndex(
                name: "IX_Redes_ComunidadeId",
                table: "Redes");

            migrationBuilder.DropIndex(
                name: "IX_DiariosCampo_ComunidadeId",
                table: "DiariosCampo");

            migrationBuilder.DropIndex(
                name: "IX_AtorComunidades_ComunidadeId",
                table: "AtorComunidades");
        }
    }
}
