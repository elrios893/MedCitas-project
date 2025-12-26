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
    public class EfAppointmentRepository : IAppointmentRepository
    {
        private readonly MedCitasDbContext _db;

        public EfAppointmentRepository(MedCitasDbContext db) => _db = db;

        public async Task<Appointment?> ObtenerPorIdAsync(Guid id) =>
         await _db.Appointments
             .Include(a => a.Paciente)
       .Include(a => a.Doctor)
         .Include(a => a.Specialty)
         .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<List<Appointment>> ObtenerPorPacienteAsync(
       Guid pacienteId,
          string? estado = null,
     DateTime? desde = null,
        DateTime? hasta = null)
        {
            var query = _db.Appointments
                       .Include(a => a.Doctor)
            .Include(a => a.Specialty)
            .Where(a => a.PacienteId == pacienteId);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(a => a.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
            }

            if (desde.HasValue)
            {
                query = query.Where(a => a.FechaCita >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(a => a.FechaCita <= hasta.Value);
            }

            return await query
       .OrderBy(a => a.FechaCita)
       .ThenBy(a => a.HoraInicio)
        .ToListAsync();
        }

        public async Task<List<Appointment>> ObtenerPorDoctorAsync(
            Guid doctorId,
            string? estado = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            var query = _db.Appointments
                .Include(a => a.Paciente)
                .Include(a => a.Doctor)
                .Include(a => a.Specialty)
                .Where(a => a.DoctorId == doctorId);

            // Aplicar filtro de estado si se proporciona
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(a => a.Estado == estado);
            }

            // Aplicar filtro de fecha desde
            if (desde.HasValue)
            {
                query = query.Where(a => a.FechaCita >= desde.Value);
            }

            // Aplicar filtro de fecha hasta
            if (hasta.HasValue)
            {
                query = query.Where(a => a.FechaCita <= hasta.Value);
            }

            // Ordenar por fecha y hora
            return await query
                .OrderBy(a => a.FechaCita)
                .ThenBy(a => a.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<TimeSlot>> ObtenerDisponibilidadAsync(Guid doctorId, DateTime fecha)
        {
            // Obtener todas las citas del médico para esa fecha
            var citasDelDia = await _db.Appointments.Where(a => a.DoctorId == doctorId && a.FechaCita.Date == fecha.Date && a.Estado == "Agendada").OrderBy(a => a.HoraInicio).ToListAsync();

            // Generar slots de 30 minutos desde las 8:00 hasta las 18:00
            var slots = new List<TimeSlot>();
            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFin = new TimeSpan(18, 0, 0);

            while (horaInicio < horaFin)
            {
                var slotFin = horaInicio.Add(TimeSpan.FromMinutes(30));
                var estaOcupado = citasDelDia.Any(c =>
                    (horaInicio >= c.HoraInicio && horaInicio < c.HoraFin) ||
                 (slotFin > c.HoraInicio && slotFin <= c.HoraFin) ||
                  (horaInicio <= c.HoraInicio && slotFin >= c.HoraFin));

                slots.Add(new TimeSlot
                {
                    HoraInicio = horaInicio,
                    HoraFin = slotFin,
                    EstaDisponible = !estaOcupado
                });

                horaInicio = slotFin;
            }

            return slots;
        }

        public async Task<bool> ValidarDisponibilidadAsync(
        Guid doctorId,
  DateTime fecha,
      TimeSpan horaInicio,
       TimeSpan horaFin)
        {
            var citaExistente = await _db.Appointments
                .Where(a => a.DoctorId == doctorId &&
               a.FechaCita.Date == fecha.Date &&
               a.Estado == "Agendada")
                 .AnyAsync(a =>
               (horaInicio >= a.HoraInicio && horaInicio < a.HoraFin) ||
                   (horaFin > a.HoraInicio && horaFin <= a.HoraFin) ||
                (horaInicio <= a.HoraInicio && horaFin >= a.HoraFin));

            return !citaExistente;
        }

        public async Task<bool> PacienteTieneCitaEnHorarioAsync(
         Guid pacienteId,
       DateTime fecha,
          TimeSpan horaInicio,
      TimeSpan horaFin)
        {
            return await _db.Appointments
               .Where(a => a.PacienteId == pacienteId &&
              a.FechaCita.Date == fecha.Date &&
             a.Estado == "Agendada")
               .AnyAsync(a =>
                (horaInicio >= a.HoraInicio && horaInicio < a.HoraFin) ||
                      (horaFin > a.HoraInicio && horaFin <= a.HoraFin) ||
              (horaInicio <= a.HoraInicio && horaFin >= a.HoraFin));
        }

        public async Task CrearAsync(Appointment appointment)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Appointment appointment)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync();
        }

        public async Task EliminarAsync(Guid id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Estado = "Cancelada";
                await _db.SaveChangesAsync();
            }
        }
    }
}
