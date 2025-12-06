using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InCollege.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeroCuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroCuota",
                table: "Pagos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroCuota",
                table: "Pagos");
        }
    }
}
