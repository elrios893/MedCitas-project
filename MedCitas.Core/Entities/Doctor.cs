using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedCitas.Core.Entities
{
    /// <summary>
    /// Representa un médico en el sistema
  /// </summary>
    public class Doctor
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public Guid SpecialtyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroLicencia { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? CorreoElectronico { get; set; }

        [MaxLength(15)]
        public string? Telefono { get; set; }

        public bool EstaActivo { get; set; } = true;

        [MaxLength(100)] // ✅ Cambiado de 64 a 100 para BCrypt
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Specialty Specialty { get; set; } = null!;
        public List<Appointment> Appointments { get; set; } = [];
    }
}
