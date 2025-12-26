using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedCitas.Core.Entities;

namespace MedCitas.Infrastructure.DataDb
{
    public class MedCitasDbContext : DbContext
    {
        // Constantes para tipos de columna PostgreSQL
        private const string TimestampColumnType = "timestamp with time zone";
        private const string DateColumnType = "date";

        public MedCitasDbContext(DbContextOptions<MedCitasDbContext> options) : base(options) { }

        public DbSet<Paciente> Pacientes { get; set; } = null!;
        public DbSet<Specialty> Specialties { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Admin> Admin { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Paciente
            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCompleto).IsRequired();
                entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(5);
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FechaNacimiento).IsRequired().HasColumnType(DateColumnType);
                entity.Property(e => e.Sexo).IsRequired().HasMaxLength(1);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(15);
                entity.Property(e => e.CorreoElectronico).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).HasMaxLength(100);
                entity.Property(e => e.Eps).IsRequired();
                entity.Property(e => e.TipoSangre).IsRequired();
                entity.Property(e => e.EstaVerificado);
                entity.Property(e => e.TokenVerificacion);
                entity.Property(e => e.FechaRegistro).HasColumnType(TimestampColumnType);
                entity.Property(e => e.CodigoOTP).HasMaxLength(6);
                entity.Property(e => e.OTPExpiracion).HasColumnType(TimestampColumnType);
                entity.Property(e => e.IntentosOTPFallidos).HasDefaultValue(0);
                entity.Property(e => e.TokenRecuperacion).HasMaxLength(64);
                entity.Property(e => e.TokenRecuperacionExpiracion).HasColumnType(TimestampColumnType);

                // Índices únicos
                entity.HasIndex(e => e.CorreoElectronico).IsUnique();
                entity.HasIndex(e => e.NumeroDocumento).IsUnique();
            });

            // Configuración de Specialty
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.DuracionConsultaMinutos).IsRequired();
                entity.Property(e => e.EstaActiva).HasDefaultValue(true);

                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Configuración de Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NumeroLicencia).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(15);
                entity.Property(e => e.EstaActivo).HasDefaultValue(true);
                entity.Property(e => e.PasswordHash).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasColumnType(TimestampColumnType);

                entity.HasOne(d => d.Specialty)
                    .WithMany(s => s.Doctors)
                    .HasForeignKey(d => d.SpecialtyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.NumeroLicencia).IsUnique();
            });

            // Configuración de Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaCita).IsRequired().HasColumnType(DateColumnType);
                entity.Property(e => e.HoraInicio).IsRequired();
                entity.Property(e => e.HoraFin).IsRequired();
                entity.Property(e => e.Modalidad).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MotivoConsulta).HasMaxLength(500);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.FechaCreacion).HasColumnType(TimestampColumnType);
                entity.Property(e => e.FechaCancelacion).HasColumnType(TimestampColumnType);
                entity.Property(e => e.MotivoCancelacion).HasMaxLength(200);

                entity.HasOne(a => a.Paciente)
                    .WithMany()
                    .HasForeignKey(a => a.PacienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Specialty)
                    .WithMany()
                    .HasForeignKey(a => a.SpecialtyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices para mejorar consultas
                entity.HasIndex(e => new { e.DoctorId, e.FechaCita, e.Estado });
                entity.HasIndex(e => new { e.PacienteId, e.FechaCita, e.Estado });
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreCompleto).IsRequired();
                entity.Property(e => e.CorreoElectronico).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(15);
                entity.Property(e => e.PasswordHash).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasColumnType(TimestampColumnType);
                entity.Property(e => e.EstaActivo).HasDefaultValue(true);
                entity.Property(e => e.EstaVerificado).HasDefaultValue(false);
                entity.Property(e => e.CodigoOTP).HasMaxLength(6);
                entity.Property(e => e.OTPExpiracion).HasColumnType(TimestampColumnType);
                entity.Property(e => e.IntentosOTPFallidos).HasDefaultValue(0);

                entity.HasIndex(e => e.CorreoElectronico).IsUnique();

            });

            base.OnModelCreating(modelBuilder);
        }
    }
}