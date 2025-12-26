namespace MedCitas.Core.Constants
{
    /// <summary>
    /// Constantes de la aplicación para evitar magic numbers y strings
    /// </summary>
    public static class AppConstants
{
        /// <summary>
        /// Constantes relacionadas con OTP (One-Time Password)
     /// </summary>
        public static class Otp
        {
     public const int ExpirationMinutes = 15;
            public const int MaxFailedAttempts = 3;
   public const int MinValue = 100000;
          public const int MaxValue = 999999;
  }

   /// <summary>
        /// Constantes de validación de contraseñas
        /// </summary>
        public static class Password
    {
 public const int MinLength = 8;
    public const string ValidationPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$";
      public const string ValidationMessage = "La contraseña debe tener mínimo 8 caracteres, con mayúscula, minúscula, número y carácter especial.";
    }

        /// <summary>
 /// Constantes de validación de campos
        /// </summary>
        public static class Validation
        {
    public const string DocumentPattern = @"^\d+$";
 public const string PhonePattern = @"^\d{7,15}$";
 public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            public const int RegexTimeoutMs = 100;
        }

        /// <summary>
/// Constantes de tokens de recuperación
        /// </summary>
        public static class RecoveryToken
        {
   public const int ExpirationMinutes = 15;
    public const int TokenSizeBytes = 32;
      }

        /// <summary>
        /// Constantes de sesión
        /// </summary>
        public static class Session
     {
  public const int TimeoutMinutes = 30;
            public const string PacienteIdKey = "PacienteId";
        public const string PacienteNombreKey = "PacienteNombre";
        }

        /// <summary>
 /// Constantes de email
        /// </summary>
   public static class Email
        {
public const int SmtpTimeout = 30000; // 30 segundos
    public const string DefaultFromName = "MedCitas";
        }
    }
}
