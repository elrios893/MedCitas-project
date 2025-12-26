using System;
using System.Security.Cryptography;
using MedCitas.Core.Constants;

namespace MedCitas.Core.Services
{
    /// <summary>
    /// Servicio para generación y validación de códigos OTP (One-Time Password)
    /// </summary>
    public static class OtpService
    {
        /// <summary>
        /// Genera un código OTP de 6 dígitos usando RandomNumberGenerator seguro
        /// </summary>
        public static string GenerarOTP()
        {
            return RandomNumberGenerator.GetInt32(
                 AppConstants.Otp.MinValue,
                   AppConstants.Otp.MaxValue
                ).ToString();
        }

        /// <summary>
        /// Obtiene la fecha de expiración del OTP
        /// </summary>
        public static DateTime ObtenerFechaExpiracion()
        {
            return DateTime.UtcNow.AddMinutes(AppConstants.Otp.ExpirationMinutes);
        }

        /// <summary>
        /// Valida si el OTP ingresado coincide con el almacenado y no ha expirado
        /// </summary>
        /// <param name="otpIngresado">OTP ingresado por el usuario</param>
        /// <param name="otpAlmacenado">OTP almacenado en la base de datos</param>
        /// <param name="expiracion">Fecha de expiración del OTP</param>
        /// <returns>True si el OTP es válido, False en caso contrario</returns>
        public static bool ValidarOTP(string otpIngresado, string otpAlmacenado, DateTime? expiracion)
        {
            if (string.IsNullOrWhiteSpace(otpIngresado) ||
                 string.IsNullOrWhiteSpace(otpAlmacenado) ||
               expiracion == null)
                 return false;

            if (DateTime.UtcNow > expiracion)
                return false;

            return otpIngresado == otpAlmacenado;
        }

        /// <summary>
        /// Verifica si el número de intentos fallidos ha excedido el máximo permitido
        /// </summary>
        public static bool HaExcedidoIntentos(int intentosFallidos)
        {
            return intentosFallidos >= AppConstants.Otp.MaxFailedAttempts;
        }
    }
}
