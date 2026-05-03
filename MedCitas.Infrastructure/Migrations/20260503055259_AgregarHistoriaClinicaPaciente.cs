using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistoriaClinicaPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "HistoriaClinica",
                table: "Pacientes",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HistoriaClinicaFechaCarga",
                table: "Pacientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoriaClinicaNombreArchivo",
                table: "Pacientes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HistoriaClinica",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "HistoriaClinicaFechaCarga",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "HistoriaClinicaNombreArchivo",
                table: "Pacientes");
        }
    }
}
