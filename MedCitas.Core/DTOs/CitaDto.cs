using System;

namespace MedCitas.Core.DTOs
{
    /// <summary>
    /// DTO para mostrar información de una cita
    /// </summary>
    public class CitaDto
    {
        public Guid Id { get; set; }

        // Información del paciente
        public string PacienteNombre { get; set; } = string.Empty;

        // Información del doctor
        public string DoctorNombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Modalidad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? MotivoConsulta { get; set; }
        public string? Observaciones { get; set; }
        public double HorasHastaCita { get; set; }
        public bool PuedeCancelarse { get; set; }
    }
}
