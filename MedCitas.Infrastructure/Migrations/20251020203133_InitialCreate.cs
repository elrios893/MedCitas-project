using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "text", nullable: false),
                    TipoDocumento = table.Column<string>(type: "text", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sexo = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Eps = table.Column<string>(type: "text", nullable: false),
                    TipoSangre = table.Column<string>(type: "text", nullable: false),
                    EstaVerificado = table.Column<bool>(type: "boolean", nullable: false),
                    TokenVerificacion = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
