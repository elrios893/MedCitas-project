using MedCitas.Core.Constants;
using MedCitas.Core.Entities;
using MedCitas.Core.Helpers;
using MedCitas.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Services
{
    public class DoctorService
    {
        // Implementación del servicio para la gestión de médicos
        private readonly IDoctorRepository _repo;
        private readonly IEmailService _emailService;

        public DoctorService(IDoctorRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task<Doctor> RegistrarAsync(Doctor nuevo, string plainPassword, string confirmarPassword)
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
            
            // Fecha en la que se registra
            nuevo.FechaRegistro = DateTime.UtcNow;

            // Guardar paciente
            await _repo.RegistrarAsync(nuevo);


            return nuevo;
        }

        public async Task<Doctor?> LoginAsync(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Correo y contraseña son obligatorios.");
            }

            var doctor = await _repo.ObtenerPorCorreoAsync(correo);
            if (doctor == null)
            {
                return null;
            }

            bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(password, doctor.PasswordHash);
            return passwordCorrecto ? doctor : null;
        }

        public static void ValidarCampos(Doctor p, string password, string confirmar)
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

            // Validar número de licencia
            if (string.IsNullOrWhiteSpace(p.NumeroLicencia))
            {
                throw new ArgumentException("El número de licencia es obligatorio.");
            }

            //Validar especialidad id
            if (p.SpecialtyId == Guid.Empty)
            {
                throw new ArgumentException("La especialidad es obligatoria.");
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
    }
}
