using MedCitas.Core.DTOs;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MedCitas.Web.Controllers
{
    public class CitaController : Controller
    {
        private readonly AppointmentService _appointmentService;
        private readonly ISpecialtyRepository _specialtyRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ILogger<CitaController> _logger;

        private const string LoginAction = "Login";
        private const string LoginController = "Paciente";
        private const string MensajeExitoKey = "MensajeExito";
        private const string ErrorKey = "Error";

        public CitaController(
            AppointmentService appointmentService,
            ISpecialtyRepository specialtyRepo,
            IDoctorRepository doctorRepo,
            ILogger<CitaController> logger, IAppointmentRepository appointmentRepo)
        {
            _appointmentService = appointmentService;
            _specialtyRepo = specialtyRepo;
            _doctorRepo = doctorRepo;
            _logger = logger;
            _appointmentRepo = appointmentRepo;
        }

        // ============================================
        // API ENDPOINTS PARA AJAX
        // ============================================

        /// <summary>
        /// GET: /Cita/ObtenerEspecialidades
        /// Retorna todas las especialidades activas en formato JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerEspecialidades()
        {
            try
            {
                var especialidades = await _specialtyRepo.ObtenerTodasAsync();

                var resultado = especialidades.Select(e => new
                {
                    id = e.Id,
                    nombre = e.Nombre,
                    descripcion = e.Descripcion,
                    duracion = e.DuracionConsultaMinutos
                }).ToList();

                return Json(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especialidades");
                return Json(new { error = "Error al cargar especialidades" });
            }
        }

        /// <summary>
        /// GET: /Cita/ObtenerDoctoresPorEspecialidad/{especialidadId}
        /// Retorna todos los médicos activos de una especialidad
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerDoctoresPorEspecialidad(Guid especialidadId)
        {
            try
            {
                var doctores = await _doctorRepo.ObtenerPorEspecialidadAsync(especialidadId);

                var resultado = doctores
                    .Where(d => d.EstaActivo)
                    .Select(d => new
                    {
                        id = d.Id,
                        nombre = d.NombreCompleto,
                        licencia = d.NumeroLicencia,
                        correo = d.CorreoElectronico,
                        telefono = d.Telefono
                    }).ToList();

                return Json(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctores de especialidad {EspecialidadId}", especialidadId);
                return Json(new { error = "Error al cargar médicos" });
            }
        }

        /// <summary>
        /// GET: /Cita/ObtenerDisponibilidad
        /// Retorna los horarios disponibles de un médico en una fecha
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerDisponibilidad(Guid doctorId, DateTime fecha)
        {
            try
            {
                _logger.LogInformation("Obteniendo disponibilidad para doctor {DoctorId} en fecha {Fecha}", doctorId, fecha);

                if (fecha.Date <= DateTime.Now.Date)
                {
                    _logger.LogWarning("Intento de obtener disponibilidad para fecha pasada: {Fecha}", fecha);
                    return Json(new { error = "La fecha debe ser futura" });
                }

                var citasExistentes = await _appointmentRepo.ObtenerPorDoctorAsync(
                    doctorId,
                    "Agendada",
                    fecha.Date,
                    fecha.Date.AddDays(1));

                _logger.LogInformation("Doctor tiene {Count} citas agendadas para esa fecha", citasExistentes.Count);

                var slots = new List<object>();
                var horaInicio = new TimeSpan(8, 0, 0);
                var horaFin = new TimeSpan(18, 0, 0);

                while (horaInicio < horaFin)
                {
                    var horaFinSlot = horaInicio.Add(TimeSpan.FromMinutes(30));

                    var estaOcupado = citasExistentes.Any(c =>
                    {
                        var citaInicio = c.HoraInicio;
                        var citaFin = c.HoraFin;

                        return (horaInicio >= citaInicio && horaInicio < citaFin) ||
                               (horaFinSlot > citaInicio && horaFinSlot <= citaFin) ||
                               (horaInicio <= citaInicio && horaFinSlot >= citaFin);
                    });

                    slots.Add(new
                    {
                        // ✅ CAMBIO CRÍTICO: Usar formato 24 horas
                        inicio = horaInicio.ToString(@"hh\:mm"),  // ✅ HH en lugar de hh
                        fin = horaFinSlot.ToString(@"hh\:mm"),    // ✅ HH en lugar de hh
                        disponible = !estaOcupado
                    });

                    horaInicio = horaFinSlot;
                }

                _logger.LogInformation("Se generaron {Count} slots de disponibilidad", slots.Count);
                return Json(slots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener disponibilidad para doctor {DoctorId} en fecha {Fecha}", doctorId, fecha);
                return Json(new { error = "Error al cargar disponibilidad" });
            }
        }

        // -------------------------------------
        // HELPER: Obtener Paciente ID de la sesión
        // -------------------------------------
        private Guid? ObtenerPacienteIdSesion()
        {
            var pacienteIdStr = HttpContext.Session.GetString("PacienteId");
            return string.IsNullOrEmpty(pacienteIdStr) ? null : Guid.Parse(pacienteIdStr);
        }

        // -------------------------------------
        // GET: /Cita/MisCitas
        // -------------------------------------
        [HttpGet]
        public async Task<IActionResult> MisCitas(string? estado = null, DateTime? desde = null, DateTime? hasta = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var pacienteId = ObtenerPacienteIdSesion();
            if (!pacienteId.HasValue)
            {
                return RedirectToAction(LoginAction, LoginController);
            }

            try
            {
                var citas = await _appointmentService.ObtenerCitasPacienteAsync(
                       pacienteId.Value, estado, desde, hasta);

                return View(citas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del paciente {PacienteId}", pacienteId);
                ViewBag.Error = "Error al cargar las citas";
                return View();
            }
        }

        // -------------------------------------
        // GET: /Cita/Detalle/{id}
        // -------------------------------------
        [HttpGet]
        public async Task<IActionResult> Detalle(Guid id)
        {
            if (!ModelState.IsValid)
            {
                TempData[ErrorKey] = "Datos inválidos";
                return RedirectToAction(nameof(MisCitas));
            }

            var pacienteId = ObtenerPacienteIdSesion();
            if (!pacienteId.HasValue)
            {
                return RedirectToAction(LoginAction, LoginController);
            }

            try
            {
                var cita = await _appointmentService.ObtenerDetalleCitaAsync(id, pacienteId.Value);
                if (cita == null)
                {
                    TempData[ErrorKey] = "Cita no encontrada";
                    return RedirectToAction(nameof(MisCitas));
                }

                return View(cita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de cita {CitaId}", id);
                TempData[ErrorKey] = "Error al cargar el detalle de la cita";
                return RedirectToAction(nameof(MisCitas));
            }
        }

        // -------------------------------------
        // GET: /Cita/Agendar
        // -------------------------------------
        [HttpGet]
        public IActionResult Agendar()
        {
            var pacienteId = ObtenerPacienteIdSesion();
            if (!pacienteId.HasValue)
            {
                return RedirectToAction(LoginAction, LoginController);
            }

            return View();
        }

        // -------------------------------------
        // POST: /Cita/Agendar
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> Agendar(AgendarCitaDto dto)
        {
            var pacienteId = ObtenerPacienteIdSesion();
            if (!pacienteId.HasValue)
            {
                return RedirectToAction(LoginAction, LoginController);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("ModelState inválido: {Errors}", string.Join(", ", errors));
                    ViewBag.Error = "Por favor corrige los errores del formulario: " + string.Join(", ", errors);
                    return View(dto);
                }

                _logger.LogInformation(
                    "Intentando agendar cita para paciente {PacienteId} - Doctor: {DoctorId}, Fecha: {Fecha}, Hora: {HoraInicio}-{HoraFin}",
                    pacienteId, dto.DoctorId, dto.FechaCita, dto.HoraInicio, dto.HoraFin);

                var cita = await _appointmentService.AgendarCitaAsync(dto, pacienteId.Value);

                _logger.LogInformation("Cita agendada exitosamente con ID {CitaId}", cita.Id);

                TempData[MensajeExitoKey] = "¡Cita agendada exitosamente!";
                return RedirectToAction(nameof(MisCitas));
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Error de formato al agendar cita");
                ViewBag.Error = $"Error en el formato de los datos: {ex.Message}";
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al agendar cita para paciente {PacienteId}", pacienteId);
                ViewBag.Error = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al agendar cita para paciente {PacienteId}", pacienteId);
                ViewBag.Error = $"Error al agendar la cita: {ex.Message}";
                return View(dto);
            }
        }
        // -------------------------------------
        // POST: /Cita/Cancelar/{id}
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> Cancelar(Guid id, string? motivoCancelacion)
        {
            if (!ModelState.IsValid)
            {
                TempData[ErrorKey] = "Datos inválidos";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var pacienteId = ObtenerPacienteIdSesion();
            if (!pacienteId.HasValue)
            {
                return RedirectToAction(LoginAction, LoginController);
            }

            try
            {
                await _appointmentService.CancelarCitaAsync(id, pacienteId.Value, motivoCancelacion);

                TempData[MensajeExitoKey] = "Cita cancelada exitosamente";
                return RedirectToAction(nameof(MisCitas));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al cancelar cita {CitaId}", id);
                TempData[ErrorKey] = ex.Message;
                return RedirectToAction(nameof(Detalle), new { id });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al cancelar cita {CitaId}", id);
                TempData[ErrorKey] = ex.Message;
                return RedirectToAction(nameof(MisCitas));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cancelar cita {CitaId}", id);
                TempData[ErrorKey] = "Error al cancelar la cita";
                return RedirectToAction(nameof(Detalle), new { id });
            }

        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest request)
        {
            try
            {
                _logger.LogInformation("Cambiando estado de cita {CitaId} a {NuevoEstado}", id, request.Estado);

                // Validar estado
                var estadosValidos = new[] { "Agendada", "Completada", "Cancelada", "NoAsistio" };
                if (!estadosValidos.Contains(request.Estado))
                {
                    return BadRequest(new { error = "Estado inválido" });
                }

                // Obtener la cita
                var cita = await _appointmentRepo.ObtenerPorIdAsync(id);
                if (cita == null)
                {
                    return NotFound(new { error = "Cita no encontrada" });
                }

                // Actualizar estado
                cita.Estado = request.Estado;

                if (request.Estado == "Cancelada")
                {
                    cita.FechaCancelacion = DateTime.UtcNow;
                    cita.MotivoCancelacion = request.Motivo ?? "Cancelada por el médico";
                }

                await _appointmentRepo.ActualizarAsync(cita);

                _logger.LogInformation("Estado de cita {CitaId} actualizado exitosamente a {NuevoEstado}", id, request.Estado);

                return Ok(new { success = true, mensaje = $"Cita marcada como {request.Estado}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de cita {CitaId}", id);
                return StatusCode(500, new { error = "Error al procesar la solicitud" });
            }
        }
        // DTO para recibir la solicitud de cambio de estado
        public class CambiarEstadoRequest
        {
            public string Estado { get; set; } = string.Empty;
            public string? Motivo { get; set; }
        }

    }
}
