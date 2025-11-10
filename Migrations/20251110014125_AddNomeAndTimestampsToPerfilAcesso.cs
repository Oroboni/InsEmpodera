using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeAndTimestampsToPerfilAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DtCriacao",
                table: "PerfisAcesso",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtModificacao",
                table: "PerfisAcesso",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "PerfisAcesso",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "IdPAcesso",
                keyValue: 1,
                columns: new[] { "DtCriacao", "DtModificacao", "Nome" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "" });

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "IdPAcesso",
                keyValue: 2,
                columns: new[] { "DtCriacao", "DtModificacao", "Nome" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "" });

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "IdPAcesso",
                keyValue: 3,
                columns: new[] { "DtCriacao", "DtModificacao", "Nome" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "" });

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "IdPAcesso",
                keyValue: 4,
                columns: new[] { "DtCriacao", "DtModificacao", "Nome" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "" });

            migrationBuilder.UpdateData(
                table: "PerfisAcesso",
                keyColumn: "IdPAcesso",
                keyValue: 5,
                columns: new[] { "DtCriacao", "DtModificacao", "Nome" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DtCriacao",
                table: "PerfisAcesso");

            migrationBuilder.DropColumn(
                name: "DtModificacao",
                table: "PerfisAcesso");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "PerfisAcesso");
        }
    }
}
