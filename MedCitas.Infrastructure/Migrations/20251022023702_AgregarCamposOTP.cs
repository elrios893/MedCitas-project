using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposOTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoOTP",
                table: "Pacientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosOTPFallidos",
                table: "Pacientes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPExpiracion",
                table: "Pacientes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoOTP",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "IntentosOTPFallidos",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "OTPExpiracion",
                table: "Pacientes");
        }
    }
}
