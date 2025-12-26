using System;
using System.Text.RegularExpressions;
using MedCitas.Core.Constants;

namespace MedCitas.Core.Helpers
{
    /// <summary>
    /// Clase centralizada para validaciones con timeout de regex para prevenir ReDoS
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(AppConstants.Validation.RegexTimeoutMs);

        /// <summary>
        /// Valida que un número de documento contenga solo dígitos
        /// </summary>
        public static bool EsDocumentoValido(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(documento, AppConstants.Validation.DocumentPattern, RegexOptions.None, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que un teléfono tenga entre 7 y 15 dígitos
        /// </summary>
        public static bool EsTelefonoValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(telefono, AppConstants.Validation.PhonePattern, RegexOptions.None, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que un correo electrónico tenga formato válido
        /// </summary>
        public static bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(correo, AppConstants.Validation.EmailPattern, RegexOptions.None, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que una contraseña cumpla con los requisitos de seguridad
        /// </summary>
        public static bool EsPasswordValido(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < AppConstants.Password.MinLength)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(password, AppConstants.Password.ValidationPattern, RegexOptions.None, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Sanitiza una cadena removiendo caracteres peligrosos para prevenir XSS
        /// </summary>
        public static string SanitizarInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Remover caracteres peligrosos comunes en XSS
            return input
                .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
             .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#x27;", StringComparison.Ordinal)
          .Replace("/", "&#x2F;", StringComparison.Ordinal)
       .Trim();
        }

        /// <summary>
        /// Valida que dos contraseñas coincidan
        /// </summary>
        public static bool PasswordsCoinciden(string password, string confirmar)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmar))
            {
                return false;
            }

            return string.Equals(password, confirmar, StringComparison.Ordinal);
        }
    }
}
