using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InCollege.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarModelosFinalv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Pagos",
                newName: "FechaPago");

            migrationBuilder.AlterColumn<Guid>(
                name: "EstudianteId",
                table: "Pagos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                table: "Pagos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concepto",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pagos");

            migrationBuilder.RenameColumn(
                name: "FechaPago",
                table: "Pagos",
                newName: "Fecha");

            migrationBuilder.AlterColumn<Guid>(
                name: "EstudianteId",
                table: "Pagos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
