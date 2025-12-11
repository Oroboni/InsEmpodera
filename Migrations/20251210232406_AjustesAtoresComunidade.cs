using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AjustesAtoresComunidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 1,
                column: "Nome",
                value: "Caio Nascimento");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 2,
                column: "Nome",
                value: "Clara Veloso");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 3,
                column: "Nome",
                value: "Milton Nascimento");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 4,
                column: "Nome",
                value: "Marisa Monte");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 5,
                column: "Nome",
                value: "César Lattes");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 6,
                column: "Nome",
                value: "Teresa Leite");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 7,
                column: "Nome",
                value: "Cássio Drummond");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 8,
                column: "Nome",
                value: "Lia Guimarães");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 9,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Transgênero", "Djair Sócrates" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 10,
                column: "Nome",
                value: "Marisa Prestes");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 11,
                column: "Nome",
                value: "Ana Marighella");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 12,
                column: "Nome",
                value: "Sara Assis");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 13,
                column: "Nome",
                value: "Caio Freire");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 14,
                column: "Nome",
                value: "Paula Souza");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 15,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Darcy Fernandes" });

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 1,
                column: "Nome",
                value: "Comunidade Alva-Branda");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 2,
                column: "Nome",
                value: "Comunidade Brenda Luxemburgo");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 3,
                column: "Nome",
                value: "Comunidade Cachoeirinha");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 4,
                column: "Nome",
                value: "Comunidade Divina Luz");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 5,
                column: "Nome",
                value: "Comunidade Estrela d’Alva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 1,
                column: "Nome",
                value: "Ator 1");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 2,
                column: "Nome",
                value: "Ator 2");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 3,
                column: "Nome",
                value: "Ator 3");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 4,
                column: "Nome",
                value: "Ator 4");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 5,
                column: "Nome",
                value: "Ator 5");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 6,
                column: "Nome",
                value: "Ator 6");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 7,
                column: "Nome",
                value: "Ator 7");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 8,
                column: "Nome",
                value: "Ator 8");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 9,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Ator 9" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 10,
                column: "Nome",
                value: "Ator 10");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 11,
                column: "Nome",
                value: "Ator 11");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 12,
                column: "Nome",
                value: "Ator 12");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 13,
                column: "Nome",
                value: "Ator 13");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 14,
                column: "Nome",
                value: "Ator 14");

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 15,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Ator 15" });

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 1,
                column: "Nome",
                value: "Comunidade Alpha");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 2,
                column: "Nome",
                value: "Comunidade Beta");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 3,
                column: "Nome",
                value: "Comunidade Gamma");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 4,
                column: "Nome",
                value: "Comunidade Delta");

            migrationBuilder.UpdateData(
                table: "Comunidades",
                keyColumn: "IdComunidade",
                keyValue: 5,
                column: "Nome",
                value: "Comunidade Epsilon");
        }
    }
}
