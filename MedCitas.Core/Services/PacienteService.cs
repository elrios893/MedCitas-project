using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BCrypt.Net;
using MedCitas.Core.Constants;
using MedCitas.Core.Entities;
using MedCitas.Core.Helpers;
using MedCitas.Core.Interfaces;

namespace MedCitas.Core.Services
{
    /// <summary>
    /// Servicio de negocio para gestión de pacientes
    /// </summary>
    public class PacienteService
    {
        private readonly IPacienteRepository _repo;
        private readonly IEmailService _emailService;

        public PacienteService(IPacienteRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        // -----------------------------------------
        // REGISTRO
        // -----------------------------------------
        /// <summary>
        /// Registra un nuevo paciente en el sistema
        /// </summary>
        public async Task<Paciente> RegistrarAsync(Paciente nuevo, string plainPassword, string confirmarPassword)
        {
            // Validaciones básicas
            ArgumentNullException.ThrowIfNull(nuevo);

            ValidarCampos(nuevo, plainPassword, confirmarPassword);

            // Validar duplicados
            var porCorreo = await _repo.ObtenerPorCorreoAsync(nuevo.CorreoElectronico);
            if (porCorreo != null)
            {
                throw new InvalidOperationException("El correo electrónico ya está registrado.");
            }

            var porDoc = await _repo.ObtenerPorDocumentoAsync(nuevo.NumeroDocumento);
            if (porDoc != null)
            {
                throw new InvalidOperationException("El número de documento ya está registrado.");
            }

            // Crear hash seguro
            nuevo.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            // Generar OTP
            nuevo.CodigoOTP = OtpService.GenerarOTP();
            nuevo.OTPExpiracion = OtpService.ObtenerFechaExpiracion();
            nuevo.IntentosOTPFallidos = 0;
            nuevo.EstaVerificado = false;
            nuevo.FechaRegistro = DateTime.UtcNow;

            // Guardar paciente
            await _repo.RegistrarAsync(nuevo);

            // Enviar OTP por correo
            await _emailService.EnviarOTPAsync(
               nuevo.CorreoElectronico,
               nuevo.CodigoOTP,
               nuevo.NombreCompleto
                     );

            return nuevo;
        }

        // -----------------------------------------
        // LOGIN
        // -----------------------------------------
        /// <summary>
        /// Autentica un paciente con correo y contraseña
        /// </summary>
        public async Task<Paciente?> LoginAsync(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Correo y contraseña son obligatorios.");
            }

            var paciente = await _repo.ObtenerPorCorreoAsync(correo);
            if (paciente == null)
            {
                return null;
            }

            if (!paciente.EstaVerificado)
            {
                throw new InvalidOperationException("Cuenta pendiente de verificación.");
            }

            bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(password, paciente.PasswordHash);
            return passwordCorrecto ? paciente : null;
        }

        // -----------------------------------------
        // ACTIVAR CUENTA
        // -----------------------------------------
        /// <summary>
        /// Activa una cuenta usando un token de verificación
        /// </summary>
        public async Task<bool> ActivarCuentaAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token inválido.");
            }

            return await _repo.ActivarCuentaAsync(token);
        }

        // -----------------------------------------
        // VALIDACIONES CENTRALIZADAS
        // -----------------------------------------
        /// <summary>
        /// Valida los campos del paciente y contraseñas
        /// </summary>
        public static void ValidarCampos(Paciente p, string password, string confirmar)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(p.NombreCompleto))
            {
                throw new ArgumentException("El nombre completo es obligatorio.");
            }

            // Validar documento
            if (!ValidationHelper.EsDocumentoValido(p.NumeroDocumento))
            {
                throw new ArgumentException("El número de documento solo debe contener números.");
            }

            // Validar teléfono
            if (!ValidationHelper.EsTelefonoValido(p.Telefono))
            {
                throw new ArgumentException("El teléfono debe contener entre 7 y 15 dígitos.");
            }

            // Validar correo
            if (!ValidationHelper.EsCorreoValido(p.CorreoElectronico))
            {
                throw new ArgumentException("Formato de correo inválido.");
            }

            // Validar coincidencia de contraseñas
            if (!ValidationHelper.PasswordsCoinciden(password, confirmar))
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            // Validar complejidad de contraseña
            if (!ValidationHelper.EsPasswordValido(password))
            {
                throw new ArgumentException(AppConstants.Password.ValidationMessage);
            }
        }

        // -----------------------------------------
        // VERIFICACIÓN OTP
        // -----------------------------------------
        /// <summary>
        /// Verifica el código OTP ingresado por el usuario
        /// </summary>
        public async Task<bool> VerificarOTPAsync(string correo, string codigoOTP)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(codigoOTP))
            {
                throw new ArgumentException("Correo y código OTP son obligatorios.");
            }

            var paciente = await _repo.ObtenerPorCorreoAsync(correo);
            if (paciente == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (OtpService.HaExcedidoIntentos(paciente.IntentosOTPFallidos))
            {
                throw new InvalidOperationException("Demasiados intentos fallidos. Solicita un nuevo código.");
            }

            if (!OtpService.ValidarOTP(codigoOTP, paciente.CodigoOTP!, paciente.OTPExpiracion))
            {
                paciente.IntentosOTPFallidos++;
                await _repo.ActualizarOTPAsync(paciente);
                return false;
            }

            return await _repo.VerificarOTPAsync(correo, codigoOTP);
        }

        /// <summary>
        /// Reenvía un nuevo código OTP al correo del paciente
        /// </summary>
        public async Task ReenviarOTPAsync(string correo)
        {
            var paciente = await _repo.ObtenerPorCorreoAsync(correo);
            if (paciente == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (paciente.EstaVerificado)
            {
                throw new InvalidOperationException("La cuenta ya está verificada.");
            }

            paciente.CodigoOTP = OtpService.GenerarOTP();
            paciente.OTPExpiracion = OtpService.ObtenerFechaExpiracion();
            paciente.IntentosOTPFallidos = 0;

            await _repo.ActualizarOTPAsync(paciente);
            await _emailService.EnviarOTPAsync(correo, paciente.CodigoOTP, paciente.NombreCompleto);
        }

        // -----------------------------------------
        // RECUPERACIÓN DE CONTRASEÑA
        // -----------------------------------------
        /// <summary>
        /// Inicia el proceso de recuperación de contraseña
        /// </summary>
        public async Task SolicitarRecuperacionPasswordAsync(string correo, string urlBase)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ArgumentException("El correo es obligatorio.");
            }

            var paciente = await _repo.ObtenerPorCorreoAsync(correo);
            if (paciente == null)
            {
                throw new InvalidOperationException("No existe una cuenta con este correo.");
            }

            if (!paciente.EstaVerificado)
            {
                throw new InvalidOperationException("La cuenta debe estar verificada para recuperar la contraseña.");
            }

            // Generar token único
            paciente.TokenRecuperacion = GenerarTokenSeguro();
            paciente.TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(AppConstants.RecoveryToken.ExpirationMinutes);

            await _repo.ActualizarTokenRecuperacionAsync(paciente);

            // Crear URL de recuperación
            string urlRecuperacion = $"{urlBase}/Paciente/RestablecerPassword?token={paciente.TokenRecuperacion}";

            // Enviar correo
            await _emailService.EnviarCorreoRecuperacionAsync(
           paciente.CorreoElectronico,
         paciente.NombreCompleto,
           urlRecuperacion
                  );
        }

        /// <summary>
        /// Restablece la contraseña usando un token válido
        /// </summary>
        public async Task<bool> RestablecerPasswordAsync(string token, string nuevaPassword, string confirmarPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token inválido.");
            }

            if (!ValidationHelper.PasswordsCoinciden(nuevaPassword, confirmarPassword))
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            if (!ValidationHelper.EsPasswordValido(nuevaPassword))
            {
                throw new ArgumentException(AppConstants.Password.ValidationMessage);
            }

            var paciente = await _repo.ObtenerPorTokenRecuperacionAsync(token);
            if (paciente == null)
            {
                throw new InvalidOperationException("Token inválido o expirado.");
            }

            if (!paciente.EsTokenRecuperacionValido())
            {
                throw new InvalidOperationException("El enlace ha expirado. Solicita uno nuevo.");
            }

            // Actualizar contraseña
            paciente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
            paciente.TokenRecuperacion = null;
            paciente.TokenRecuperacionExpiracion = null;

            await _repo.ActualizarPasswordAsync(paciente);
            return true;
        }

        // -----------------------------------------
        // MÉTODO AUXILIAR PARA GENERAR TOKEN SEGURO
        // -----------------------------------------
        /// <summary>
        /// Genera un token seguro para recuperación de contraseña
        /// </summary>
        private static string GenerarTokenSeguro()
        {
            byte[] tokenBytes = new byte[AppConstants.RecoveryToken.TokenSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes)
             .Replace("+", "-", StringComparison.Ordinal)
     .Replace("/", "_", StringComparison.Ordinal)
           .Replace("=", "", StringComparison.Ordinal);
        }

        // -----------------------------------------
        // ACTUALIZACIÓN DE PERFIL
        // -----------------------------------------
        /// <summary>
        /// Actualiza el perfil de un paciente autenticado
        /// </summary>
        public async Task<Paciente> ActualizarPerfilAsync(Guid pacienteId, MedCitas.Core.DTOs.ActualizarPerfilDto dto)
        {
            // 1. Validar que el paciente existe
            var paciente = await _repo.ObtenerPorIdAsync(pacienteId);
            ArgumentNullException.ThrowIfNull(paciente);

            // 2. Validar unicidad de correo y documento
            await ValidarUnicidadAsync(paciente, dto);

            // 3. Si cambia contraseña, validar actual y hashear nueva
            await ActualizarPasswordSiEsNecesarioAsync(paciente, dto);

            // 4. Validar teléfono
            if (!ValidationHelper.EsTelefonoValido(dto.Telefono))
            {
                throw new ArgumentException("El teléfono debe tener entre 7 y 15 dígitos");
            }

            // 5. Actualizar campos permitidos
            paciente.NombreCompleto = dto.NombreCompleto;
            paciente.TipoDocumento = dto.TipoDocumento;
            paciente.NumeroDocumento = dto.NumeroDocumento;
            paciente.Telefono = dto.Telefono;

            bool correoCambio = !string.Equals(dto.CorreoElectronico, paciente.CorreoElectronico, StringComparison.OrdinalIgnoreCase);
            paciente.CorreoElectronico = dto.CorreoElectronico;

            // 6. Persistir cambios
            await _repo.ActualizarAsync(paciente);

            // 7. Enviar notificación si cambió email o password
            bool passwordCambio = !string.IsNullOrWhiteSpace(dto.NuevaPassword);
            if (correoCambio || passwordCambio)
            {
                await _emailService.EnviarNotificacionCambiosSensiblesAsync(
                 paciente.CorreoElectronico,
                  paciente.NombreCompleto);
            }

            return paciente;
        }

        private async Task ValidarUnicidadAsync(Paciente paciente, MedCitas.Core.DTOs.ActualizarPerfilDto dto)
        {
            // Validar correo
            bool correoCambio = !string.Equals(dto.CorreoElectronico, paciente.CorreoElectronico, StringComparison.OrdinalIgnoreCase);
            if (correoCambio)
            {
                var existeCorreo = await _repo.ObtenerPorCorreoAsync(dto.CorreoElectronico);
                if (existeCorreo != null && existeCorreo.Id != paciente.Id)
                {
                    throw new InvalidOperationException("El correo ya está registrado");
                }
            }

            // Validar documento
            bool documentoCambio = dto.NumeroDocumento != paciente.NumeroDocumento;
            if (documentoCambio)
            {
                var existeDocumento = await _repo.ObtenerPorDocumentoAsync(dto.NumeroDocumento);
                if (existeDocumento != null && existeDocumento.Id != paciente.Id)
                {
                    throw new InvalidOperationException("El documento ya está registrado");
                }
            }
        }

        private static async Task ActualizarPasswordSiEsNecesarioAsync(Paciente paciente, MedCitas.Core.DTOs.ActualizarPerfilDto dto)
        {
            bool passwordCambio = !string.IsNullOrWhiteSpace(dto.NuevaPassword);
            if (!passwordCambio)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.PasswordActual))
            {
                throw new ArgumentException("Debes ingresar tu contraseña actual");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, paciente.PasswordHash))
            {
                throw new ArgumentException("La contraseña actual es incorrecta");
            }

            if (!ValidationHelper.EsPasswordValido(dto.NuevaPassword!))
            {
                throw new ArgumentException(AppConstants.Password.ValidationMessage);
            }

            paciente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);

            await Task.CompletedTask; // Para mantener la firma async
        }

        /// <summary>
        /// Obtiene un paciente por su ID
        /// </summary>
        public async Task<Paciente?> ObtenerPorIdAsync(Guid id)
        {
            return await _repo.ObtenerPorIdAsync(id);
        }
    }
}


