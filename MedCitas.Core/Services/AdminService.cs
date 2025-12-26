using MedCitas.Core.Constants;
using MedCitas.Core.Entities;
using MedCitas.Core.Helpers;
using MedCitas.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Services
{
    public class AdminService
    {
        private readonly IAdminRepository _repo;
        private readonly IEmailService _emailService;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IPacienteRepository _pacienteRepo;

        // Implementar métodos para la gestión de administradores

        public AdminService(IAdminRepository repo, IEmailService emailService, IPacienteRepository pacienteRepo, IDoctorRepository doctorRepo)
        {
            _repo = repo;
            _emailService = emailService;
            _doctorRepo = doctorRepo;
            _pacienteRepo = pacienteRepo;
        }

        public async Task<Admin> RegistrarAsync(Admin nuevo, string plainPassword, string confirmarPassword)
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


        public async Task<Admin?> LoginAsync(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Correo y contraseña son obligatorios.");
            }

            var admin = await _repo.ObtenerPorCorreoAsync(correo);
            if (admin == null)
            {
                return null;
            }

            if (!admin.EstaVerificado)
            {
                throw new InvalidOperationException("Cuenta pendiente de verificación.");
            }

            bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
            return passwordCorrecto ? admin : null;
        }

        // ========================================
        // ✅ REGISTRO DE DOCTOR (SOLO ADMIN)
        // ========================================

        public async Task<Doctor> RegistrarDoctorAsync(
            string nombreCompleto,
            Guid specialtyId,
            string numeroLicencia,
            string? correoElectronico,
            string? telefono,
            string plainPassword,
            string confirmarPassword)
        {
            // Validar contraseñas
            if (!ValidationHelper.PasswordsCoinciden(plainPassword, confirmarPassword))
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            if (!ValidationHelper.EsPasswordValido(plainPassword))
            {
                throw new ArgumentException(AppConstants.Password.ValidationMessage);
            }

            // Validar correo si se proporciona
            if (!string.IsNullOrEmpty(correoElectronico) &&
                !ValidationHelper.EsCorreoValido(correoElectronico))
            {
                throw new ArgumentException("Formato de correo inválido.");
            }

            // Validar teléfono si se proporciona
            if (!string.IsNullOrEmpty(telefono) &&
                !ValidationHelper.EsTelefonoValido(telefono))
            {
                throw new ArgumentException("El teléfono debe contener entre 7 y 15 dígitos.");
            }

            // Verificar que no exista doctor con el mismo correo
            if (!string.IsNullOrEmpty(correoElectronico))
            {
                var existente = await _doctorRepo.ObtenerPorCorreoAsync(correoElectronico);
                if (existente != null)
                {
                    throw new InvalidOperationException("Ya existe un doctor con este correo.");
                }
            }

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                NombreCompleto = nombreCompleto,
                SpecialtyId = specialtyId,
                NumeroLicencia = numeroLicencia,
                CorreoElectronico = correoElectronico,
                Telefono = telefono,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                EstaActivo = true,
                FechaRegistro = DateTime.UtcNow
            };

            await _doctorRepo.RegistrarAsync(doctor);

            return doctor;
        }

        // ========================================
        // ✅ REGISTRO DE PACIENTE (ALTERNATIVA ADMIN)
        // ========================================

        public async Task<Paciente> RegistrarPacienteAsync(
            string nombreCompleto,
            string tipoDocumento,
            string numeroDocumento,
            DateTime fechaNacimiento,
            string sexo,
            string telefono,
            string correoElectronico,
            string eps,
            string tipoSangre,
            string plainPassword,
            string confirmarPassword)
        {
            // Validar contraseñas
            if (!ValidationHelper.PasswordsCoinciden(plainPassword, confirmarPassword))
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            if (!ValidationHelper.EsPasswordValido(plainPassword))
            {
                throw new ArgumentException(AppConstants.Password.ValidationMessage);
            }

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                throw new ArgumentException("El nombre completo es obligatorio.");
            }

            if (!ValidationHelper.EsCorreoValido(correoElectronico))
            {
                throw new ArgumentException("Formato de correo inválido.");
            }

            if (!ValidationHelper.EsTelefonoValido(telefono))
            {
                throw new ArgumentException("El teléfono debe contener entre 7 y 15 dígitos.");
            }

            if (!ValidationHelper.EsDocumentoValido(numeroDocumento))
            {
                throw new ArgumentException("El número de documento solo debe contener números.");
            }

            // Verificar duplicados por correo
            var existenteCorreo = await _pacienteRepo.ObtenerPorCorreoAsync(correoElectronico);
            if (existenteCorreo != null)
            {
                throw new InvalidOperationException("Ya existe un paciente con este correo.");
            }

            // Verificar duplicados por documento
            var existenteDoc = await _pacienteRepo.ObtenerPorDocumentoAsync(numeroDocumento);
            if (existenteDoc != null)
            {
                throw new InvalidOperationException("Ya existe un paciente con este documento.");
            }

            var paciente = new Paciente
            {
                Id = Guid.NewGuid(),
                NombreCompleto = nombreCompleto,
                TipoDocumento = tipoDocumento,
                NumeroDocumento = numeroDocumento,
                FechaNacimiento = fechaNacimiento,
                Sexo = sexo,
                Telefono = telefono,
                CorreoElectronico = correoElectronico,
                Eps = eps,
                TipoSangre = tipoSangre,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                EstaVerificado = true, // ✅ Admin registra, cuenta auto-verificada
                FechaRegistro = DateTime.UtcNow
            };

            await _pacienteRepo.RegistrarAsync(paciente);

            return paciente;
        }

        public async Task<List<Paciente>> ObtenerTodosPacientesAsync() =>
            await _repo.ObtenerTodosPacientesAsync();

        public async Task<List<Doctor>> ObtenerTodosDoctoresAsync() =>
            await _repo.ObtenerTodosDoctoresAsync();

        public async Task<List<Admin>> ObtenerTodosAdminsAsync() =>
            await _repo.ObtenerTodosAdminsAsync();


        public static void ValidarCampos(Admin p, string password, string confirmar)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(p.NombreCompleto))
            {
                throw new ArgumentException("El nombre completo es obligatorio.");
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


        public async Task<bool> VerificarOTPAsync(string correo, string codigoOTP)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(codigoOTP))
            {
                throw new ArgumentException("Correo y código OTP son obligatorios.");
            }

            var admin = await _repo.ObtenerPorCorreoAsync(correo);
            if (admin == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (OtpService.HaExcedidoIntentos(admin.IntentosOTPFallidos))
            {
                throw new InvalidOperationException("Demasiados intentos fallidos. Solicita un nuevo código.");
            }

            if (!OtpService.ValidarOTP(codigoOTP, admin.CodigoOTP!, admin.OTPExpiracion))
            {
                admin.IntentosOTPFallidos++;
                await _repo.ActualizarOTPAsync(admin);
                return false;
            }

            return await _repo.VerificarOTPAsync(correo, codigoOTP);
        }

        public async Task ReenviarOTPAsync(string correo)
        {
            var admin = await _repo.ObtenerPorCorreoAsync(correo);
            if (admin == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (admin.EstaVerificado)
            {
                throw new InvalidOperationException("La cuenta ya está verificada.");
            }

            admin.CodigoOTP = OtpService.GenerarOTP();
            admin.OTPExpiracion = OtpService.ObtenerFechaExpiracion();
            admin.IntentosOTPFallidos = 0;

            await _repo.ActualizarOTPAsync(admin);
            await _emailService.EnviarOTPAsync(correo, admin.CodigoOTP, admin.NombreCompleto);
        }

    }
}
