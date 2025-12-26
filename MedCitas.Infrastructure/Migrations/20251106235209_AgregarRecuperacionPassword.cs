using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecuperacionPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacion",
                table: "Pacientes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenRecuperacionExpiracion",
                table: "Pacientes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenRecuperacion",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacionExpiracion",
                table: "Pacientes");
        }
    }
}
