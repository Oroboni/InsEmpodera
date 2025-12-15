using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AjusteModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 1,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 1" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 2,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 2" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 3,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 3" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 4,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 4" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 5,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 5" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 6,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 6" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 7,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 7" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 8,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 8" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 9,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 9" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 10,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 10" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 11,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 11" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 12,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 12" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 13,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 13" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 14,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "F", "Ator 14" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 15,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "M", "Ator 15" });

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

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 1,
                column: "Genero",
                value: "M");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 2,
                column: "Genero",
                value: "F");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 3,
                column: "Genero",
                value: "M");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 4,
                column: "Genero",
                value: "F");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 5,
                column: "Genero",
                value: "M");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "Genero",
                value: "M");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "Genero",
                value: "F");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 3,
                column: "Genero",
                value: "M");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 4,
                column: "Genero",
                value: "F");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 5,
                column: "Genero",
                value: "M");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 1,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Caio Nascimento" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 2,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Clara Veloso" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 3,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Milton Nascimento" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 4,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Marisa Monte" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 5,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "César Lattes" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 6,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Teresa Leite" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 7,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Cássio Drummond" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 8,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Lia Guimarães" });

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
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Marisa Prestes" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 11,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Ana Marighella" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 12,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Sara Assis" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 13,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Masculino", "Caio Freire" });

            migrationBuilder.UpdateData(
                table: "Atores",
                keyColumn: "IdAtores",
                keyValue: 14,
                columns: new[] { "Genero", "Nome" },
                values: new object[] { "Feminino", "Paula Souza" });

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

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 1,
                column: "Genero",
                value: "Masculino");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 2,
                column: "Genero",
                value: "Feminino");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 3,
                column: "Genero",
                value: "Masculino");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 4,
                column: "Genero",
                value: "Feminino");

            migrationBuilder.UpdateData(
                table: "FontesInfo",
                keyColumn: "IdFonte",
                keyValue: 5,
                column: "Genero",
                value: "Masculino");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "Genero",
                value: "Masculino");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "Genero",
                value: "Feminino");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 3,
                column: "Genero",
                value: "Masculino");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 4,
                column: "Genero",
                value: "Feminino");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 5,
                column: "Genero",
                value: "Masculino");
        }
    }
}
