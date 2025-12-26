using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.DataDb;
using Microsoft.EntityFrameworkCore;

namespace MedCitas.Infrastructure.Repositories
{
    public class EfDoctorRepository : IDoctorRepository
    {
        private readonly MedCitasDbContext _db;

        public EfDoctorRepository(MedCitasDbContext db) => _db = db;

        public async Task<Doctor?> ObtenerPorIdAsync(Guid id) =>
            await _db.Doctors.Include(d => d.Specialty).FirstOrDefaultAsync(d => d.Id == id);

        public async Task<Doctor?> ObtenerPorCorreoAsync(string correoElectronico) =>
           await _db.Doctors.Include(d => d.Specialty).FirstOrDefaultAsync(d => d.CorreoElectronico.ToLower() == correoElectronico.ToLower());

        public async Task<List<Doctor>> ObtenerTodosAsync() =>
            await _db.Doctors.Include(d => d.Specialty).Where(d => d.EstaActivo).OrderBy(d => d.NombreCompleto).ToListAsync();

        public async Task<List<Doctor>> ObtenerPorEspecialidadAsync(Guid especialidadId) =>
            await _db.Doctors.Include(d => d.Specialty).Where(d => d.SpecialtyId == especialidadId && d.EstaActivo).OrderBy(d => d.NombreCompleto).ToListAsync();

        public async Task RegistrarAsync(Doctor doctor)
        {
            if (doctor.Id == Guid.Empty)
            {
                doctor.Id = Guid.NewGuid();
            }
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();
        }

        public async Task<Doctor> LoginAsync(string correo, string password)
        {
            var doctor = await ObtenerPorCorreoAsync(correo);
            if (doctor == null)
            {
                throw new InvalidOperationException("Correo o contraseña incorrectos.");
            }
            // Verificar la contraseña usando BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, doctor.PasswordHash);
            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Correo o contraseña incorrectos.");
            }
            return doctor;
        }

        public async Task ActualizarAsync(Doctor doctor)
        {
            ArgumentNullException.ThrowIfNull(doctor);

            _db.Doctors.Update(doctor);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarPasswordAsync(Doctor doctor)
        {
            ArgumentNullException.ThrowIfNull(doctor);

            // Validación adicional específica para contraseña
            if (string.IsNullOrEmpty(doctor.PasswordHash))
            {
                throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(doctor));
            }

            await ActualizarAsync(doctor);
        }


    }
}
