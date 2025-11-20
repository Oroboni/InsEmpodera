using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaEixos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 1,
                column: "Nome",
                value: "Rede primária");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 2,
                column: "Nome",
                value: "Segurança Social");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 3,
                column: "Nome",
                value: "Substâncias");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 4,
                column: "Nome",
                value: "Moradia");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 5,
                column: "Nome",
                value: "Prevenção");

            migrationBuilder.InsertData(
                table: "Eixos",
                columns: new[] { "IdEixo", "Nome" },
                values: new object[,]
                {
                    { 6, "Assistência Básica" },
                    { 7, "Educação" },
                    { 8, "Saúde" },
                    { 9, "Ocupação" },
                    { 10, "Lazer" },
                    { 11, "Cultura" },
                    { 12, "Cidadania" },
                    { 13, "Meio Ambiente" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 1,
                column: "Nome",
                value: "Educação");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 2,
                column: "Nome",
                value: "Saúde");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 3,
                column: "Nome",
                value: "Segurança");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 4,
                column: "Nome",
                value: "Cultura");

            migrationBuilder.UpdateData(
                table: "Eixos",
                keyColumn: "IdEixo",
                keyValue: 5,
                column: "Nome",
                value: "Infraestrutura");
        }
    }
}
