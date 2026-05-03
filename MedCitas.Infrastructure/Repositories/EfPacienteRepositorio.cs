using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.DataDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MedCitas.Infrastructure.Repositories
{
    public class EfPacienteRepositorio : IPacienteRepository
    {
        private readonly MedCitasDbContext _db;

        public EfPacienteRepositorio(MedCitasDbContext db) => _db = db;

        public async Task<Paciente?> ObtenerPorIdAsync(Guid id) =>
        await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Paciente?> ObtenerPorDocumentoAsync(string numeroDocumento) =>
           await _db.Pacientes.FirstOrDefaultAsync(p => p.NumeroDocumento == numeroDocumento);

        // ✅ Usar ToLower() - Es traducible a SQL y funciona con InMemory DB
        // Aunque no es lo más eficiente, es compatible con todas las bases de datos
        [SuppressMessage("Performance", "CA1862", Justification = "ToLower() es necesario para compatibilidad con EF Core")]
        public async Task<Paciente?> ObtenerPorCorreoAsync(string correoElectronico) =>
         await _db.Pacientes
       .FirstOrDefaultAsync(p => p.CorreoElectronico.ToLower() == correoElectronico.ToLower());

        public async Task RegistrarAsync(Paciente paciente)
        {
            if (paciente.Id == Guid.Empty)
            {
                paciente.Id = Guid.NewGuid();
            }
            _db.Pacientes.Add(paciente);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ActivarCuentaAsync(string tokenVerificacion)
        {
            var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.TokenVerificacion == tokenVerificacion);
            if (paciente == null)
            {
                return false;
            }
            paciente.EstaVerificado = true;
            paciente.TokenVerificacion = null;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerificarOTPAsync(string correo, string codigoOTP)
        {
            var paciente = await ObtenerPorCorreoAsync(correo);
            if (paciente == null)
            {
                return false;
            }

            if (paciente.CodigoOTP != codigoOTP ||
               paciente.OTPExpiracion == null ||
             DateTime.UtcNow > paciente.OTPExpiracion)
            {
                paciente.IntentosOTPFallidos++;
                await _db.SaveChangesAsync();
                return false;
            }

            paciente.EstaVerificado = true;
            paciente.CodigoOTP = null;
            paciente.OTPExpiracion = null;
            paciente.IntentosOTPFallidos = 0;
            await _db.SaveChangesAsync();
            return true;
        }

        // ✅ Método genérico de actualización
        public async Task ActualizarAsync(Paciente paciente)
        {
            ArgumentNullException.ThrowIfNull(paciente);

            _db.Pacientes.Update(paciente);
            await _db.SaveChangesAsync();
        }

        // ✅ Método específico para OTP que usa el método genérico
        public async Task ActualizarOTPAsync(Paciente paciente)
        {
            ArgumentNullException.ThrowIfNull(paciente);
            await ActualizarAsync(paciente);
        }

        // ✅ IMPLEMENTACIÓN DE MÉTODOS PARA RECUPERACIÓN DE CONTRASEÑA
        public async Task<Paciente?> ObtenerPorTokenRecuperacionAsync(string token) =>
        await _db.Pacientes.FirstOrDefaultAsync(p => p.TokenRecuperacion == token);

        // ✅ Método específico para token que valida y usa el método genérico
        public async Task ActualizarTokenRecuperacionAsync(Paciente paciente)
        {
            ArgumentNullException.ThrowIfNull(paciente);

            // Validación adicional específica para token de recuperación
            if (string.IsNullOrEmpty(paciente.TokenRecuperacion))
            {
                throw new ArgumentException("El token de recuperación no puede estar vacío.", nameof(paciente));
            }

            await ActualizarAsync(paciente);
        }

        // ✅ Método específico para password que valida y usa el método genérico
        public async Task ActualizarPasswordAsync(Paciente paciente)
        {
            ArgumentNullException.ThrowIfNull(paciente);

            // Validación adicional específica para contraseña
            if (string.IsNullOrEmpty(paciente.PasswordHash))
            {
                throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(paciente));
            }

            await ActualizarAsync(paciente);
        }
        
        public async Task<bool> EliminarAsync(Guid id)
        {
            var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null)
            {
                return false;
            }
            _db.Pacientes.Remove(paciente);
            await _db.SaveChangesAsync();
            return true;
        }

        // ✅ Agregar al final de la clase, antes del cierre

        /// <summary>
        /// Actualiza únicamente los campos relacionados a la historia clínica
        /// </summary>
        public async Task ActualizarHistoriaClinicaAsync(Paciente paciente)
        {
            ArgumentNullException.ThrowIfNull(paciente);

            // Actualizar solo los campos de historia clínica (más eficiente que actualizar todo)
            _db.Pacientes.Attach(paciente);
            _db.Entry(paciente).Property(p => p.HistoriaClinica).IsModified = true;
            _db.Entry(paciente).Property(p => p.HistoriaClinicaNombreArchivo).IsModified = true;
            _db.Entry(paciente).Property(p => p.HistoriaClinicaFechaCarga).IsModified = true;

            await _db.SaveChangesAsync();
        }

    }
}
