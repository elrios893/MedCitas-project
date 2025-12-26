using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAdminYActualizarDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Doctors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "text", nullable: false),
                    CorreoElectronico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstaActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CodigoOTP = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    OTPExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IntentosOTPFallidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EstaVerificado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TokenVerificacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_CorreoElectronico",
                table: "Admin",
                column: "CorreoElectronico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Doctors");
        }
    }
}
