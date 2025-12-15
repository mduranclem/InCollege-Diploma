using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InCollege.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoCursoYDivision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dni",
                table: "Estudiantes");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Estudiantes",
                newName: "NombreCompleto");

            migrationBuilder.RenameColumn(
                name: "Apellido",
                table: "Estudiantes",
                newName: "Division");

            migrationBuilder.AlterColumn<string>(
                name: "Zona",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Usuarios",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "TalleRemera",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TalleAbrigo",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Curso",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Curso",
                table: "Estudiantes");

            migrationBuilder.RenameColumn(
                name: "NombreCompleto",
                table: "Estudiantes",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Division",
                table: "Estudiantes",
                newName: "Apellido");

            migrationBuilder.AlterColumn<string>(
                name: "Zona",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TalleRemera",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TalleAbrigo",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
