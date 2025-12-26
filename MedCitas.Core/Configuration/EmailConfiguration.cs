using System.Collections.Generic;

namespace MedCitas.Core.Configuration
{
    /// <summary>
    /// Configuración de email para evitar hardcodear valores
    /// </summary>
    public class EmailConfiguration
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string AdminNotificationEmail { get; set; } = string.Empty;

        /// <summary>
        /// Valida que la configuración esté completa
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SmtpHost) &&
                SmtpPort > 0 &&
                !string.IsNullOrWhiteSpace(SmtpUser) &&
                !string.IsNullOrWhiteSpace(SmtpPassword) &&
                !string.IsNullOrWhiteSpace(FromEmail);
        }

        /// <summary>
        /// Obtiene mensajes de validación si la configuración es inválida
        /// </summary>
        public string GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SmtpHost))
            {
                errors.Add("SmtpHost es requerido");
            }

            if (SmtpPort <= 0)
            {
                errors.Add("SmtpPort debe ser mayor a 0");
            }

            if (string.IsNullOrWhiteSpace(SmtpUser))
            {
                errors.Add("SmtpUser es requerido");
            }

            if (string.IsNullOrWhiteSpace(SmtpPassword))
            {
                errors.Add("SmtpPassword es requerido");
            }

            if (string.IsNullOrWhiteSpace(FromEmail))
            {
                errors.Add("FromEmail es requerido");
            }

            return string.Join(", ", errors);
        }
    }
}
