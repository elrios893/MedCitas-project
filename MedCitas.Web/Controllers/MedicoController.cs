using MedCitas.Core.DTOs;
using MedCitas.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MedCitas.Web.Controllers
{
    public class MedicoController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IDoctorRepository _doctorRepo;

        public MedicoController(
            IAppointmentRepository appointmentRepo,
            IDoctorRepository doctorRepo)
        {
            _appointmentRepo = appointmentRepo;
            _doctorRepo = doctorRepo;
        }

        // GET: /Medico/Login
        [HttpGet]
        public IActionResult Login()
        {   
            return View(); 
        }

        // POST: /Medico/Login
        [HttpPost]
        public async Task<IActionResult> Login(string correoElectronico, string password)
        {
            try
            {
                var medico = await _doctorRepo.LoginAsync(correoElectronico, password);
                if (medico == null || !ModelState.IsValid)
                {
                    ViewBag.Error = "Credenciales incorrectas.";
                    return View();
                }
                else
                {
                    HttpContext.Session.SetString("DoctorId", medico.Id.ToString());
                    HttpContext.Session.SetString("DoctorNombre", medico.NombreCompleto);
                    return RedirectToAction("Calendario", "Medico");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error durante el inicio de sesión: {ex.Message}";
                return View();
            }
        }

        // GET: /Medico/Calendario
        [HttpGet]
        public async Task<IActionResult> Calendario()
        {
            // Obtener ID del médico desde la sesión
            var medicoIdStr = HttpContext.Session.GetString("DoctorId");
            if (string.IsNullOrEmpty(medicoIdStr))
            {
                TempData["Error"] = "Debe iniciar sesión como médico.";
                return RedirectToAction("Login", "Medico");
            }

            var medicoId = Guid.Parse(medicoIdStr);

            // Obtener información del médico
            var medico = await _doctorRepo.ObtenerPorIdAsync(medicoId);
            if (medico == null)
            {
                return NotFound();
            }

            ViewBag.MedicoNombre = medico.NombreCompleto;

            // Obtener todas las citas del médico
            var citas = await _appointmentRepo.ObtenerPorDoctorAsync(medicoId);

            // Convertir a DTOs
            var citasDto = citas.Select(c => new CitaDto
            {
                Id = c.Id,
                PacienteNombre = c.Paciente.NombreCompleto,
                DoctorNombre = c.Doctor.NombreCompleto,
                Especialidad = c.Specialty.Nombre,
                FechaCita = c.FechaCita/*.ToString("yyyy-MM-dd")*/,
                HoraInicio = c.HoraInicio/*.ToString(@"hh\:mm")*/,
                HoraFin = c.HoraFin/*.ToString(@"hh\:mm")*/,
                Modalidad = c.Modalidad,
                Estado = c.Estado,
                MotivoConsulta = c.MotivoConsulta,
                Observaciones = c.Observaciones
            }).ToList();

            return View(citasDto);
        }

        // GET: /Medico/MisPacientes
        [HttpGet]
        public async Task<IActionResult> MisPacientes()
        {
            return View();
        }


        // GET: /Medico/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpiar la sesión
            HttpContext.Session.Clear();

            TempData["Mensaje"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}