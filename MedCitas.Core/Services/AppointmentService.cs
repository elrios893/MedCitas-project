using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedCitas.Core.DTOs;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;

namespace MedCitas.Core.Services
{
    /// <summary>
    /// Servicio para la gestión de citas médicas
    /// </summary>
    public class AppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IPacienteRepository _pacienteRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IEmailService _emailService;

        public AppointmentService(
          IAppointmentRepository appointmentRepo,
    IPacienteRepository pacienteRepo,
            IDoctorRepository doctorRepo,
       IEmailService emailService)
        {
            _appointmentRepo = appointmentRepo;
            _pacienteRepo = pacienteRepo;
            _doctorRepo = doctorRepo;
            _emailService = emailService;
        }

        /// <summary>
        /// Agenda una nueva cita médica
        /// </summary>
        public virtual async Task<Appointment> AgendarCitaAsync(AgendarCitaDto dto, Guid pacienteId)
        {
            // 1. Validar que el paciente existe
            var paciente = await _pacienteRepo.ObtenerPorIdAsync(pacienteId);
            if (paciente == null)
            {
                throw new InvalidOperationException("Paciente no encontrado");
            }

            // 2. Validar que el médico existe y está activo
            var doctor = await _doctorRepo.ObtenerPorIdAsync(dto.DoctorId);
            if (doctor == null || !doctor.EstaActivo)
            {
                throw new InvalidOperationException("Médico no disponible");
            }

            // 3. Validar que la fecha/hora está disponible
            var estaDisponible = await _appointmentRepo.ValidarDisponibilidadAsync(
                dto.DoctorId, dto.FechaCita, dto.HoraInicio, dto.HoraFin);

            if (!estaDisponible)
            {
                throw new InvalidOperationException("El horario seleccionado ya no está disponible");
            }

            // 4. Validar que el paciente no tenga cita en la misma hora
            var tieneCitaSimultanea = await _appointmentRepo.PacienteTieneCitaEnHorarioAsync(
          pacienteId, dto.FechaCita, dto.HoraInicio, dto.HoraFin);

            if (tieneCitaSimultanea)
            {
                throw new InvalidOperationException("Ya tienes una cita agendada en ese horario");
            }

            // 5. Crear la cita
            var cita = new Appointment
            {
                Id = Guid.NewGuid(),
                PacienteId = pacienteId,
                DoctorId = dto.DoctorId,
                SpecialtyId = doctor.SpecialtyId,
                FechaCita = dto.FechaCita,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Modalidad = dto.Modalidad,
                MotivoConsulta = dto.MotivoConsulta,
                Estado = "Agendada",
                FechaCreacion = DateTime.UtcNow
            };

            // 6. Persistir
            await _appointmentRepo.CrearAsync(cita);

            // 7. Enviar confirmación por correo
            await _emailService.EnviarConfirmacionCitaAsync(
                 paciente.CorreoElectronico,
             paciente.NombreCompleto,
               doctor.NombreCompleto,
               doctor.Specialty.Nombre,
                  cita.FechaCita,
             cita.HoraInicio);

            return cita;
        }

        /// <summary>
        /// Obtiene todas las citas de un paciente con filtros opcionales
        /// </summary>
        public virtual async Task<List<CitaDto>> ObtenerCitasPacienteAsync(
              Guid pacienteId,
       string? estado = null,
             DateTime? desde = null,
      DateTime? hasta = null)
        {
            var citas = await _appointmentRepo.ObtenerPorPacienteAsync(
         pacienteId, estado, desde, hasta);

            return citas.Select(c => new CitaDto
            {
                Id = c.Id,
                Especialidad = c.Specialty.Nombre,
                Medico = c.Doctor.NombreCompleto,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                HoraFin = c.HoraFin,
                Modalidad = c.Modalidad,
                Estado = c.Estado,
                MotivoConsulta = c.MotivoConsulta,
                Observaciones = c.Observaciones,
                HorasHastaCita = c.HorasHastaCita(),
                PuedeCancelarse = c.PuedeSerCancelada()
            }).ToList();
        }

        /// <summary>
        /// Obtiene el detalle de una cita específica
        /// </summary>
        public virtual async Task<CitaDto?> ObtenerDetalleCitaAsync(Guid citaId, Guid pacienteId)
        {
            var cita = await _appointmentRepo.ObtenerPorIdAsync(citaId);

            if (cita == null || cita.PacienteId != pacienteId)
            {
                return null;
            }

            return new CitaDto
            {
                Id = cita.Id,
                Especialidad = cita.Specialty.Nombre,
                Medico = cita.Doctor.NombreCompleto,
                FechaCita = cita.FechaCita,
                HoraInicio = cita.HoraInicio,
                HoraFin = cita.HoraFin,
                Modalidad = cita.Modalidad,
                Estado = cita.Estado,
                MotivoConsulta = cita.MotivoConsulta,
                Observaciones = cita.Observaciones,
                HorasHastaCita = cita.HorasHastaCita(),
                PuedeCancelarse = cita.PuedeSerCancelada()
            };
        }

        /// <summary>
        /// Cancela una cita (solo si faltan más de 24 horas)
        /// </summary>
        public virtual async Task<bool> CancelarCitaAsync(Guid citaId, Guid pacienteId, string? motivoCancelacion = null)
        {

            // 1. Obtener la cita
            var cita = await _appointmentRepo.ObtenerPorIdAsync(citaId);
            if (cita == null)
            {
                throw new InvalidOperationException("Cita no encontrada");
            }

            // 2. Validar que pertenece al paciente
            if (cita.PacienteId != pacienteId)
            {
                throw new UnauthorizedAccessException("No tienes permiso para cancelar esta cita");
            }

            // 3. Validar que no esté ya cancelada
            if (cita.Estado == "Cancelada")
            {
                throw new InvalidOperationException("La cita ya está cancelada");
            }

            // 4. REGLA: Solo se puede cancelar con más de 24 horas de anticipación
            var horasRestantes = cita.HorasHastaCita();

            if (horasRestantes < 24)
            {
                throw new InvalidOperationException(
                       $"No se puede cancelar con menos de 24 horas de anticipación. " +
                     $"Quedan {Math.Round(horasRestantes, 1)} horas para la cita.");
            }

            // 5. Cancelar
            cita.Estado = "Cancelada";
            cita.FechaCancelacion = DateTime.UtcNow;
            cita.MotivoCancelacion = motivoCancelacion;
            await _appointmentRepo.ActualizarAsync(cita);

            // 6. Notificar a paciente y médico
            await _emailService.EnviarNotificacionCancelacionAsync(
             cita.Paciente.CorreoElectronico,
              cita.Paciente.NombreCompleto,
          cita.Doctor.NombreCompleto,
             cita.Specialty.Nombre,
            cita.FechaCita,
         cita.HoraInicio);

            return true;
        }

        /// <summary>
        /// Obtiene la disponibilidad de horarios para un médico
        /// </summary>
        public virtual async Task<List<TimeSlot>> ObtenerDisponibilidadAsync(Guid doctorId, DateTime fecha)
        {
            return await _appointmentRepo.ObtenerDisponibilidadAsync(doctorId, fecha);
        }

    }
}
