using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsuariosTeste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "IdUsuario", "DtCriacao", "DtNascimento", "Email", "Foto", "Genero", "NivelPermissao", "Nome", "Ocupacao", "Senha" },
                values: new object[,]
                {
                    { 6, new DateTime(2025, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1998, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "teste.operador@exemplo.com", "placeholder-ator.png", "Não Informado", 1, "Usuario Teste Operador", "Estagiário", "teste" },
                    { 7, new DateTime(2025, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "teste.admin@exemplo.com", "placeholder-ator.png", "Não Informado", 2, "Usuario Teste Admin", "Coordenador", "teste" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 7);
        }
    }
}
