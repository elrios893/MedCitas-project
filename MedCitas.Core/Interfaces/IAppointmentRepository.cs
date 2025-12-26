using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedCitas.Core.Entities;

namespace MedCitas.Core.Interfaces
{
    /// <summary>
    /// Repositorio para la gestión de citas médicas
    /// </summary>
    public interface IAppointmentRepository
    {
        /// <summary>
        /// Obtiene una cita por su ID
        /// </summary>
        Task<Appointment?> ObtenerPorIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las citas de un médico con filtros opcionales
        /// </summary>
        Task<List<Appointment>> ObtenerPorDoctorAsync(
            Guid doctorId,
            string? estado = null,
            DateTime? desde = null,
            DateTime? hasta = null);

        /// <summary>
        /// Obtiene todas las citas de un paciente con filtros opcionales
        /// </summary>
        Task<List<Appointment>> ObtenerPorPacienteAsync(
         Guid pacienteId,
          string? estado = null,
            DateTime? desde = null,
            DateTime? hasta = null);

        /// <summary>
        /// Obtiene la disponibilidad de citas para un médico en una fecha
        /// </summary>
        Task<List<TimeSlot>> ObtenerDisponibilidadAsync(Guid doctorId, DateTime fecha);

        /// <summary>
        /// Valida si un horario está disponible para agendar
        /// </summary>
        Task<bool> ValidarDisponibilidadAsync(
            Guid doctorId,
     DateTime fecha,
     TimeSpan horaInicio,
              TimeSpan horaFin);

        /// <summary>
        /// Verifica si un paciente tiene una cita en el horario especificado
        /// </summary>
        Task<bool> PacienteTieneCitaEnHorarioAsync(
      Guid pacienteId,
 DateTime fecha,
      TimeSpan horaInicio,
 TimeSpan horaFin);

        /// <summary>
        /// Crea una nueva cita
        /// </summary>
        Task CrearAsync(Appointment appointment);

        /// <summary>
        /// Actualiza una cita existente
        /// </summary>
        Task ActualizarAsync(Appointment appointment);

        /// <summary>
        /// Elimina una cita (soft delete cambiando estado)
        /// </summary>
        Task EliminarAsync(Guid id);
    }

    /// <summary>
    /// Representa un slot de tiempo disponible
    /// </summary>
    public class TimeSlot
    {
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool EstaDisponible { get; set; }
    }
}
