using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsEmpodera.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToFichaPrimeiroContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FichasPrimeiroContato",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 1,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 2,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 3,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 4,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FichasPrimeiroContato",
                keyColumn: "IdFicha",
                keyValue: 5,
                column: "Status",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FichasPrimeiroContato");
        }
    }
}
