using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MedCitas.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly ISpecialtyRepository _specialtyRepo;
        private readonly IPacienteRepository _pacienteRepo;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminRepository _adminRepository;


        private const string VerificarOTPView = "VerificarOTP";
        private const string MensajeExitoKey = "MensajeExito";
        public AdminController(AdminService adminService, ISpecialtyRepository specialtyRepo, IPacienteRepository pacienteRepo, IDoctorRepository doctorRepository, IAdminRepository adminRepository, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _specialtyRepo = specialtyRepo;
            _pacienteRepo = pacienteRepo;
            _doctorRepository = doctorRepository;
            _adminRepository = adminRepository;
            _logger = logger;
        }

        // ============================================
        // LOGIN
        // ============================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correoElectronico, string password)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Por favor ingresa tu correo y contraseña";
                return View();
            }

            try
            {
                var admin = await _adminService.LoginAsync(correoElectronico, password);

                if (admin == null)
                {
                    ViewBag.Error = "Credenciales incorrectas.";
                    return View();
                }

                HttpContext.Session.SetString("AdminId", admin.Id.ToString());
                HttpContext.Session.SetString("AdminNombre", admin.NombreCompleto);

                return RedirectToAction("Dashboard");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error durante el inicio de sesión: {ex.Message}";
                return View();
            }
        }

        // ============================================
        // DASHBOARD
        // ============================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            try
            {
                ViewBag.AdminNombre = HttpContext.Session.GetString("AdminNombre");

                // Obtener estadísticas
                var pacientes = await _adminService.ObtenerTodosPacientesAsync();
                var doctores = await _adminService.ObtenerTodosDoctoresAsync();
                var admins = await _adminService.ObtenerTodosAdminsAsync();

                ViewBag.TotalPacientes = pacientes.Count;
                ViewBag.TotalDoctores = doctores.Count;
                ViewBag.TotalAdmins = admins.Count;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar el dashboard: {ex.Message}";
                return View();
            }
        }

        // ============================================
        // LISTAR USUARIOS
        // ============================================

        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            try
            {
                ViewBag.Pacientes = await _adminService.ObtenerTodosPacientesAsync();
                ViewBag.Doctores = await _adminService.ObtenerTodosDoctoresAsync();
                ViewBag.Admins = await _adminService.ObtenerTodosAdminsAsync();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar usuarios: {ex.Message}";
                return View();
            }
        }

        // ============================================
        // REGISTRAR ADMIN
        // ============================================

        [HttpGet]
        public IActionResult RegistrarAdmin()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAdmin(Admin model, string password, string confirmarPassword)
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.RegistrarAsync(model, password, confirmarPassword);

                TempData["MensajeExito"] = "Administrador registrado exitosamente. Se ha enviado un código OTP al correo.";
                return RedirectToAction("VerificarOTP");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al registrar administrador: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerificarOTP()
        {
            ViewBag.Correo = TempData["CorreoRegistrado"]?.ToString() ?? "";
            ViewBag.Mensaje = TempData["Mensaje"]?.ToString();
            return View();
        }

        // -------------------------------------
        // POST: /Paciente/VerificarOTP
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> VerificarOTP(string correo, string codigoOTP)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Datos inválidos";
                ViewBag.Correo = correo;
                return View();
            }

            try
            {
                var resultado = await _adminService.VerificarOTPAsync(correo, codigoOTP);

                if (resultado)
                {
                    TempData[MensajeExitoKey]= "¡Cuenta verificada exitosamente! Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Código OTP inválido o expirado. Intenta nuevamente.";
                    ViewBag.Correo = correo;
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Correo = correo;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReenviarOTP(string correo)
        {
            try
            {
                await _adminService.ReenviarOTPAsync(correo);
                ViewBag.Mensaje = "Código reenviado exitosamente. Revisa tu correo.";
                ViewBag.Correo = correo;
                return View(VerificarOTPView);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Correo = correo;
                return View(VerificarOTPView);
            }
        }

        // ============================================
        // REGISTRAR DOCTOR
        // ============================================

        [HttpGet]
        public async Task<IActionResult> RegistrarMed()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Especialidades = await _specialtyRepo.ObtenerTodasAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarMed(
            string nombreCompleto,
            Guid specialtyId,
            string numeroLicencia,
            string correoElectronico,
            string telefono,
            string password,
            string confirmarPassword)
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Especialidades = await _specialtyRepo.ObtenerTodasAsync();
                return View();
            }

            try
            {
                await _adminService.RegistrarDoctorAsync(
                    nombreCompleto,
                    specialtyId,
                    numeroLicencia,
                    correoElectronico,
                    telefono,
                    password,
                    confirmarPassword
                );

                TempData["MensajeExito"] = "Doctor registrado exitosamente.";
                return RedirectToAction("ListarUsuarios");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Especialidades = await _specialtyRepo.ObtenerTodasAsync();
                return View();
            }
            catch (ArgumentException ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Especialidades = await _specialtyRepo.ObtenerTodasAsync();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al registrar doctor: {ex.Message}";
                ViewBag.Especialidades = await _specialtyRepo.ObtenerTodasAsync();
                return View();
            }
        }

        // ============================================
        // REGISTRAR PACIENTE
        // ============================================

        [HttpGet]
        public IActionResult RegistrarPaciente()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPaciente(
            string nombreCompleto,
            string tipoDocumento,
            string numeroDocumento,
            DateTime fechaNacimiento,
            string sexo,
            string telefono,
            string correoElectronico,
            string eps,
            string tipoSangre,
            string password,
            string confirmarPassword)
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                await _adminService.RegistrarPacienteAsync(
                    nombreCompleto,
                    tipoDocumento,
                    numeroDocumento,
                    fechaNacimiento,
                    sexo,
                    telefono,
                    correoElectronico,
                    eps,
                    tipoSangre,
                    password,
                    confirmarPassword
                );

                TempData["MensajeExito"] = "Paciente registrado exitosamente.";
                return RedirectToAction("ListarUsuarios");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
            catch (ArgumentException ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al registrar paciente: {ex.Message}";
                return View();
            }
        }

        // ============================================
        // LOGOUT
        // ============================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPaciente(Guid id)
        {

            var adminId = HttpContext.Session.GetString("AdminId");
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrEmpty(adminId))
            {
                return Json(new { success = false, message = "Sesión expirada. Por favor, inicia sesión nuevamente." });
            }

            try
            {
                _logger.LogInformation("Intentando eliminar paciente {PacienteId}", id);

                var resultado = await _pacienteRepo.EliminarAsync(id);

                if (!resultado)
                {
                    _logger.LogWarning("Paciente {PacienteId} no encontrado", id);
                    return Json(new { success = false, message = "Paciente no encontrado" });
                }

                _logger.LogInformation("Paciente {PacienteId} eliminado exitosamente", id);
                return Json(new { success = true, message = "Paciente eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paciente {PacienteId}", id);
                return Json(new { success = false, message = $"Error al eliminar paciente: {ex.Message}" });
            }
        }
    }
}