using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Entities
{
    public class Paciente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        public string Sexo { get; set; } = string.Empty;

        [Required, MaxLength(15)] // ✅ Cambiado de 10 a 15
        public string Telefono { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string CorreoElectronico { get; set; } = string.Empty;

        [MaxLength(100)] // ✅ Cambiado de 64 a 100 para BCrypt
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Eps { get; set; } = string.Empty;

        [Required]
        public string TipoSangre { get; set; } = string.Empty;

        // Agregar estas propiedades a la clase Paciente:
        public string? CodigoOTP { get; set; }
        public DateTime? OTPExpiracion { get; set; }
        public int IntentosOTPFallidos { get; set; } = 0;

        public bool EstaVerificado { get; set; } = false;
        public string? TokenVerificacion { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // ✅ NUEVAS PROPIEDADES PARA RECUPERACIÓN DE CONTRASEÑA
        public string? TokenRecuperacion { get; set; }
        public DateTime? TokenRecuperacionExpiracion { get; set; }

        public string ToStringPaciente()
        {
            
            return $"Paciente: {NombreCompleto}, Documento: {TipoDocumento} {NumeroDocumento}, Nacimiento: {FechaNacimiento.ToShortDateString()}, Sexo: {Sexo}, Teléfono: {Telefono}, Correo: {CorreoElectronico}, EPS: {Eps}, Tipo de Sangre: {TipoSangre}, Verificado: {EstaVerificado}, Fecha de Registro: {FechaRegistro}";
        }

        public int CalcularEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        public bool EsMayorDeEdad()
        {
            return CalcularEdad() >= 18;
        }

        public bool EsPacientePreferencial()
        {
            var edad = CalcularEdad();
            return edad >= 65 || edad <= 12;
        }

        public string ObtenerResumenContacto()
        {
            return $"{NombreCompleto} - Tel: {Telefono}, Email: {CorreoElectronico}";
        }

        public void ActualizarDatosContacto(string telefono, string correo)
        {
            Telefono = telefono;
            CorreoElectronico = correo;
        }

        // MÉTODO PARA VALIDAR TOKEN DE RECUPERACIÓN
        public bool EsTokenRecuperacionValido()
        {
            if (string.IsNullOrEmpty(TokenRecuperacion) || !TokenRecuperacionExpiracion.HasValue)
                return false;

            return DateTime.UtcNow <= TokenRecuperacionExpiracion.Value;
        }
    }

}

