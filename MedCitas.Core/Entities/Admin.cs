using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Entities
{
    public class Admin
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)] // Cambiado de 10 a 15
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(100)] // Cambiado de 64 a 100 para BCrypt
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public bool EstaActivo { get; set; } = true;

        //Validación OTP y verificación de cuenta
        public string? CodigoOTP { get; set; }
        public DateTime? OTPExpiracion { get; set; }
        public int IntentosOTPFallidos { get; set; } = 0;

        public bool EstaVerificado { get; set; } = false;
        public string? TokenVerificacion { get; set; }

    }
}
