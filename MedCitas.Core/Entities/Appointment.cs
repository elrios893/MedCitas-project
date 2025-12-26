using System;
using System.ComponentModel.DataAnnotations;

namespace MedCitas.Core.Entities
{
    /// <summary>
    /// Representa una cita médica en el sistema
    /// </summary>
    public class Appointment
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PacienteId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid SpecialtyId { get; set; }

        [Required]
        public DateTime FechaCita { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        [Required]
        [MaxLength(20)]
        public string Modalidad { get; set; } = "Presencial"; // Presencial, Virtual

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Agendada"; // Agendada, Cancelada, Completada, NoAsistio

        [MaxLength(500)]
        public string? MotivoConsulta { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaCancelacion { get; set; }

        [MaxLength(200)]
        public string? MotivoCancelacion { get; set; }

        // Navigation properties
        public Paciente Paciente { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Specialty Specialty { get; set; } = null!;

        /// <summary>
        /// Calcula las horas que faltan para la cita
        /// </summary>
        public double HorasHastaCita()
        {
            var fechaHoraCita = FechaCita.Add(HoraInicio);
            return (fechaHoraCita - DateTime.Now).TotalHours;
        }

        /// <summary>
        /// Verifica si la cita puede ser cancelada (más de 24 horas de anticipación)
        /// </summary>
        public bool PuedeSerCancelada()
        {
            return HorasHastaCita() > 24 && Estado == "Agendada";
        }
    }
}
