using System;
using System.ComponentModel.DataAnnotations;

namespace MedCitas.Core.DTOs
{
    /// <summary>
    /// DTO para agendar una nueva cita
    /// </summary>
    public class AgendarCitaDto
    {
        [Required(ErrorMessage = "El médico es requerido")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime FechaCita { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es requerida")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "La modalidad es requerida")]
        [RegularExpression("^(Presencial|Virtual)$", ErrorMessage = "Modalidad inválida")]
        public string Modalidad { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "El motivo no debe exceder 500 caracteres")]
        public string? MotivoConsulta { get; set; }

        // ✅ NUEVO: Método para validar y convertir strings a TimeSpan
        public static TimeSpan ParseTimeSpan(string time)
        {
            if (TimeSpan.TryParse(time, out var result))
            {
                return result;
            }
            throw new FormatException($"El formato de hora '{time}' no es válido. Use formato HH:mm");
        }
    }
}