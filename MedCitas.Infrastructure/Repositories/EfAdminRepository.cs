using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.DataDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Infrastructure.Repositories
{
    public class EfAdminRepository: IAdminRepository
    {
        private readonly MedCitasDbContext _db;
        public EfAdminRepository(MedCitasDbContext db)
        {
            _db = db;
        }

        public async Task RegistrarAsync(Admin admin)
        {
            if (admin.Id == Guid.Empty)
            {
                admin.Id = Guid.NewGuid();
            }
            _db.Admin.Add(admin);
            await _db.SaveChangesAsync();
        }

        public async Task<Admin?> ObtenerPorCorreoAsync(string correoElectronico) =>
         await _db.Admin.FirstOrDefaultAsync(p => p.CorreoElectronico.ToLower() == correoElectronico.ToLower());

        public async Task<Admin> LoginAsync(string correo, string password)
        {
            var admin = await ObtenerPorCorreoAsync(correo);
            if (admin == null)
            {
                throw new InvalidOperationException("Correo o contraseña incorrectos.");
            }
            // Verificar la contraseña usando BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Correo o contraseña incorrectos.");
            }
            return admin;
        }

        public async Task ActualizarAsync(Admin admin)
        {
            ArgumentNullException.ThrowIfNull(admin);

            _db.Admin.Update(admin);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarPasswordAsync(Admin admin)
        {
            ArgumentNullException.ThrowIfNull(admin);

            // Validación adicional específica para contraseña
            if (string.IsNullOrEmpty(admin.PasswordHash))
            {
                throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(admin));
            }

            await ActualizarAsync(admin);
        }

        public async Task<Admin?> ObtenerPorIdAsync(Guid id) =>
            await _db.Admin.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<List<Paciente>> ObtenerTodosPacientesAsync() =>
            await _db.Pacientes
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();

        public async Task<List<Doctor>> ObtenerTodosDoctoresAsync() =>
            await _db.Doctors
                .Include(d => d.Specialty)
                .OrderByDescending(d => d.FechaRegistro)
                .ToListAsync();

        public async Task<List<Admin>> ObtenerTodosAdminsAsync() =>
            await _db.Admin
                .OrderByDescending(a => a.FechaRegistro)
                .ToListAsync();

        public async Task<bool> VerificarOTPAsync(string correo, string codigoOTP)
        {
            var admin = await ObtenerPorCorreoAsync(correo);
            if (admin == null) return false;

            if (admin.CodigoOTP != codigoOTP ||
                admin.OTPExpiracion == null ||
                DateTime.UtcNow > admin.OTPExpiracion)
            {
                admin.IntentosOTPFallidos++;
                await _db.SaveChangesAsync();
                return false;
            }

            admin.EstaVerificado = true;
            admin.CodigoOTP = null;
            admin.OTPExpiracion = null;
            admin.IntentosOTPFallidos = 0;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ActualizarOTPAsync(Admin admin)
        {
            _db.Admin.Update(admin);
            await _db.SaveChangesAsync();
        }

    }
}
