using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddAtorIdToDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtorId",
                table: "DiariosCampo",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DiariosCampo",
                keyColumn: "IdDCampo",
                keyValue: 1,
                column: "AtorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiariosCampo",
                keyColumn: "IdDCampo",
                keyValue: 2,
                column: "AtorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiariosCampo",
                keyColumn: "IdDCampo",
                keyValue: 3,
                column: "AtorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiariosCampo",
                keyColumn: "IdDCampo",
                keyValue: 4,
                column: "AtorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DiariosCampo",
                keyColumn: "IdDCampo",
                keyValue: 5,
                column: "AtorId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_DiariosCampo_AtorId",
                table: "DiariosCampo",
                column: "AtorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiariosCampo_Atores_AtorId",
                table: "DiariosCampo",
                column: "AtorId",
                principalTable: "Atores",
                principalColumn: "IdAtores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiariosCampo_Atores_AtorId",
                table: "DiariosCampo");

            migrationBuilder.DropIndex(
                name: "IX_DiariosCampo_AtorId",
                table: "DiariosCampo");

            migrationBuilder.DropColumn(
                name: "AtorId",
                table: "DiariosCampo");
        }
    }
}
