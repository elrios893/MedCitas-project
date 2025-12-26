using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedCitas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustarLongitudesCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "Pacientes",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Pacientes",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Sexo",
                table: "Pacientes",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Pacientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<int>(
                name: "IntentosOTPFallidos",
                table: "Pacientes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "CodigoOTP",
                table: "Pacientes",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "Pacientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Pacientes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Sexo",
                table: "Pacientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Pacientes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "IntentosOTPFallidos",
                table: "Pacientes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoOTP",
                table: "Pacientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldNullable: true);
        }
    }
}
