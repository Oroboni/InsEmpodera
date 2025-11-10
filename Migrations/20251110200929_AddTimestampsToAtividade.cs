using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampsToAtividade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DtCriacao",
                table: "Atividades",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtModificacao",
                table: "Atividades",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AtividadeIdAtividade",
                table: "AtividadeEixos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AtividadeEixos",
                keyColumn: "IdAEixo",
                keyValue: 1,
                column: "AtividadeIdAtividade",
                value: null);

            migrationBuilder.UpdateData(
                table: "AtividadeEixos",
                keyColumn: "IdAEixo",
                keyValue: 2,
                column: "AtividadeIdAtividade",
                value: null);

            migrationBuilder.UpdateData(
                table: "AtividadeEixos",
                keyColumn: "IdAEixo",
                keyValue: 3,
                column: "AtividadeIdAtividade",
                value: null);

            migrationBuilder.UpdateData(
                table: "AtividadeEixos",
                keyColumn: "IdAEixo",
                keyValue: 4,
                column: "AtividadeIdAtividade",
                value: null);

            migrationBuilder.UpdateData(
                table: "AtividadeEixos",
                keyColumn: "IdAEixo",
                keyValue: 5,
                column: "AtividadeIdAtividade",
                value: null);

            migrationBuilder.UpdateData(
                table: "Atividades",
                keyColumn: "IdAtividade",
                keyValue: 1,
                columns: new[] { "DtCriacao", "DtModificacao" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Atividades",
                keyColumn: "IdAtividade",
                keyValue: 2,
                columns: new[] { "DtCriacao", "DtModificacao" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Atividades",
                keyColumn: "IdAtividade",
                keyValue: 3,
                columns: new[] { "DtCriacao", "DtModificacao" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Atividades",
                keyColumn: "IdAtividade",
                keyValue: 4,
                columns: new[] { "DtCriacao", "DtModificacao" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Atividades",
                keyColumn: "IdAtividade",
                keyValue: 5,
                columns: new[] { "DtCriacao", "DtModificacao" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_AtividadeEixos_AtividadeIdAtividade",
                table: "AtividadeEixos",
                column: "AtividadeIdAtividade");

            migrationBuilder.AddForeignKey(
                name: "FK_AtividadeEixos_Atividades_AtividadeIdAtividade",
                table: "AtividadeEixos",
                column: "AtividadeIdAtividade",
                principalTable: "Atividades",
                principalColumn: "IdAtividade");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AtividadeEixos_Atividades_AtividadeIdAtividade",
                table: "AtividadeEixos");

            migrationBuilder.DropIndex(
                name: "IX_AtividadeEixos_AtividadeIdAtividade",
                table: "AtividadeEixos");

            migrationBuilder.DropColumn(
                name: "DtCriacao",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "DtModificacao",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "AtividadeIdAtividade",
                table: "AtividadeEixos");
        }
    }
}
