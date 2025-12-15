using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InCollege.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AsignarTaller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EtapaProduccion",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreTaller",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TallerAsignadoId",
                table: "Contratos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtapaProduccion",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "NombreTaller",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "TallerAsignadoId",
                table: "Contratos");
        }
    }
}
