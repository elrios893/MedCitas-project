using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntidadesCitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DuracionConsultaMinutos = table.Column<int>(type: "integer", nullable: false),
                    EstaActiva = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroLicencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    EstaActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaCita = table.Column<DateTime>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Modalidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MotivoConsulta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_CorreoElectronico",
                table: "Pacientes",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_NumeroDocumento",
                table: "Pacientes",
                column: "NumeroDocumento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_FechaCita_Estado",
                table: "Appointments",
                columns: new[] { "DoctorId", "FechaCita", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PacienteId_FechaCita_Estado",
                table: "Appointments",
                columns: new[] { "PacienteId", "FechaCita", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SpecialtyId",
                table: "Appointments",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_NumeroLicencia",
                table: "Doctors",
                column: "NumeroLicencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecialtyId",
                table: "Doctors",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Nombre",
                table: "Specialties",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_CorreoElectronico",
                table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_NumeroDocumento",
                table: "Pacientes");
        }
    }
}
