using MedCitas.Core.Entities;
using MedCitas.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MedCitas.Web.Controllers
{
    [SuppressMessage("SonarLint", "S4502", Justification = "Usando enrutamiento MVC de C#")]
#pragma warning disable S6934
    public class PacienteController : Controller
#pragma warning restore S6934
    {
        private readonly PacienteService _pacienteService;
        private readonly ILogger<PacienteController> _logger;

        // Constantes para nombres de acciones
        private const string LoginAction = "Login";
        private const string VerificarOTPView = "VerificarOTP";
        private const string MensajeExitoKey = "MensajeExito";
        private const string ErrorMessageKey = "ErrorMessage";
        private const string PacienteIdSessionKey = "PacienteId";

        public PacienteController(PacienteService pacienteService, ILogger<PacienteController> logger)
        {
            _pacienteService = pacienteService;
            _logger = logger;
        }

        // -------------------------------------
        // GET: /Paciente/Registro
        // -------------------------------------
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        // -------------------------------------
        // POST: /Paciente/Registro
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> Registro(Paciente model, string password, string confirmarPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var nuevoPaciente = await _pacienteService.RegistrarAsync(model, password, confirmarPassword);

                TempData["Mensaje"] = $"¡Registro exitoso! Te hemos enviado un código de verificación a {nuevoPaciente.CorreoElectronico}";
                TempData["CorreoRegistrado"] = nuevoPaciente.CorreoElectronico;

                return RedirectToAction(VerificarOTPView);
            }
            catch (DbUpdateException dbEx)
            {
                // Error específico de base de datos
                ViewBag.Error = $"Error de BD: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ViewBag.Error += $" | Inner: {ex.InnerException.Message}";
                }
                return View(model);
            }
        }

        // -------------------------------------
        // GET: /Paciente/VerificarOTP
        // -------------------------------------
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
                var resultado = await _pacienteService.VerificarOTPAsync(correo, codigoOTP);

                if (resultado)
                {
                    TempData[MensajeExitoKey] = "¡Cuenta verificada exitosamente! Ya puedes iniciar sesión.";
                    return RedirectToAction(LoginAction);
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

        // -------------------------------------
        // POST: /Paciente/ReenviarOTP
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> ReenviarOTP(string correo)
        {
            try
            {
                await _pacienteService.ReenviarOTPAsync(correo);
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

        // -------------------------------------
        // GET: /Paciente/Login
        // -------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Mensaje = TempData[MensajeExitoKey]?.ToString();
            return View();
        }

        // -------------------------------------
        // POST: /Paciente/Login
        // -------------------------------------
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
                var paciente = await _pacienteService.LoginAsync(correoElectronico, password);

                if (paciente == null)
                {
                    ViewBag.Error = "Credenciales incorrectas.";
                    return View();
                }

                HttpContext.Session.SetString(PacienteIdSessionKey, paciente.Id.ToString());
                HttpContext.Session.SetString("PacienteNombre", paciente.NombreCompleto);

                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // -------------------------------------
        // GET: /Paciente/Dashboard
        // -------------------------------------
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
            {
                return RedirectToAction(LoginAction);
            }

            try
            {
                var paciente = await _pacienteService.ObtenerPorIdAsync(Guid.Parse(pacienteId));
                if (paciente == null)
                {
                    return RedirectToAction(LoginAction);
                }

                ViewBag.PacienteNombre = paciente.NombreCompleto;
                ViewBag.Mensaje = TempData[MensajeExitoKey]?.ToString();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard del paciente {PacienteId}", pacienteId);
                TempData["Error"] = "Error al cargar el dashboard";
                return RedirectToAction(LoginAction);
            }
        }

        // -------------------------------------
        // POST: /Paciente/Logout
        // -------------------------------------
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpiar la sesión
            HttpContext.Session.Clear();

            TempData["Mensaje"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Index", "Home");
        }

        // -------------------------------------
        // GET: /Paciente/VerificarCuenta/{token} (método legacy)
        // -------------------------------------
        [HttpGet]
        [Route("Paciente/VerificarCuenta/{token}")]
        public async Task<IActionResult> VerificarCuenta(string token)
        {
            var result = await _pacienteService.ActivarCuentaAsync(token);
            ViewBag.Resultado = result ? "Cuenta activada correctamente." : "Token inválido o expirado.";
            return View();
        }

        // -------------------------------------
        // NUEVO: GET - Mostrar formulario de recuperación de contraseña
        // -------------------------------------
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        // -------------------------------------
        // NUEVO: POST - Procesar solicitud de recuperación
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> RecuperarPassword(string correoElectronico)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Por favor ingresa un correo válido";
                return View();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(correoElectronico))
                {
                    ViewBag.ErrorMessage = "Por favor ingresa tu correo electrónico.";
                    return View();
                }

                // Obtener la URL base
                string urlBase = $"{Request.Scheme}://{Request.Host}";
                _logger.LogInformation("Solicitando recuperación de contraseña para: {Correo} con URL base: {UrlBase}", correoElectronico, urlBase);

                await _pacienteService.SolicitarRecuperacionPasswordAsync(correoElectronico, urlBase);

                ViewBag.SuccessMessage = "Te hemos enviado un enlace de recuperación a tu correo electrónico. Revisa tu bandeja de entrada.";

                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de operación al recuperar contraseña para: {Correo}", correoElectronico);
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar recuperación de contraseña para: {Correo}", correoElectronico);
                ViewBag.ErrorMessage = "Ocurrió un error al procesar tu solicitud. Por favor intenta nuevamente.";
                return View();
            }
        }

        // -------------------------------------
        // NUEVO: GET - Mostrar formulario para restablecer contraseña
        // -------------------------------------
        [HttpGet]
        public IActionResult RestablecerPassword(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Intento de acceso a RestablecerPassword sin token");
                    TempData[ErrorMessageKey] = "El enlace de recuperación es inválido o ha expirado.";
                    return RedirectToAction(LoginAction);
                }

                ViewBag.Token = token;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista RestablecerPassword");
                TempData[ErrorMessageKey] = "Ocurrió un error. Por favor solicita un nuevo enlace de recuperación.";
                return RedirectToAction(LoginAction);
            }
        }

        // -------------------------------------
        // NUEVO: POST - Procesar nueva contraseña
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> RestablecerPassword(string token, string nuevaPassword, string confirmarPassword)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Por favor completa todos los campos correctamente";
                ViewBag.Token = token;
                return View();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Intento de restablecer contraseña sin token");
                    TempData[ErrorMessageKey] = "El enlace de recuperación es inválido.";
                    return RedirectToAction(LoginAction);
                }

                _logger.LogInformation("Intentando restablecer contraseña con token");

                await _pacienteService.RestablecerPasswordAsync(token, nuevaPassword, confirmarPassword);

                TempData[MensajeExitoKey] = "¡Contraseña restablecida exitosamente! Ya puedes iniciar sesión con tu nueva contraseña.";
                _logger.LogInformation("Contraseña restablecida exitosamente");

                return RedirectToAction(LoginAction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al restablecer contraseña");
                ViewBag.Error = ex.Message;
                ViewBag.Token = token;
                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de operación al restablecer contraseña");
                ViewBag.Error = ex.Message;
                ViewBag.Token = token;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al restablecer contraseña");
                ViewBag.Error = "Ocurrió un error al restablecer tu contraseña. Por favor intenta nuevamente.";
                ViewBag.Token = token;
                return View();
            }
        }

        // -------------------------------------
        // NUEVO: GET - Ver perfil del paciente
        // -------------------------------------
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
            {
                return RedirectToAction(LoginAction);
            }

            try
            {
                var paciente = await _pacienteService.ObtenerPorIdAsync(Guid.Parse(pacienteId));
                if (paciente == null)
                {
                    return RedirectToAction(LoginAction);
                }

                return View(paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar perfil del paciente {PacienteId}", pacienteId);
                TempData["Error"] = "Error al cargar el perfil";
                return RedirectToAction("Index", "Home");
            }
        }

        // -------------------------------------
        // NUEVO: POST - Actualizar perfil del paciente
        // -------------------------------------
        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(MedCitas.Core.DTOs.ActualizarPerfilDto dto)
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
            {
                return RedirectToAction(LoginAction);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Por favor corrige los errores del formulario";
                    return View("Perfil", dto);
                }

                await _pacienteService.ActualizarPerfilAsync(Guid.Parse(pacienteId), dto);

                TempData[MensajeExitoKey] = "Perfil actualizado exitosamente";
                return RedirectToAction(nameof(Perfil));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar perfil");
                ViewBag.Error = ex.Message;
                return View("Perfil", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar perfil");
                ViewBag.Error = "Error al actualizar el perfil";
                return View("Perfil", dto);
            }
        }

        // ============================================
        // HISTORIA CLÍNICA
        // ============================================

        private const long MaxPdfSize = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// POST: /Paciente/SubirHistoriaClinica
        /// Solo disponible si la cuenta está verificada
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubirHistoriaClinica(IFormFile archivo)
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
                return RedirectToAction(LoginAction);

            // ✅ Validar que sea PDF
            if (archivo == null || archivo.Length == 0)
            {
                TempData["ErrorHistoria"] = "Por favor selecciona un archivo PDF.";
                return RedirectToAction(nameof(Perfil));
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                TempData["ErrorHistoria"] = "Solo se permiten archivos en formato PDF.";
                return RedirectToAction(nameof(Perfil));
            }

            // ✅ Validar tamaño máximo (10 MB)
            if (archivo.Length > MaxPdfSize)
            {
                TempData["ErrorHistoria"] = "El archivo no puede superar los 10 MB.";
                return RedirectToAction(nameof(Perfil));
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await archivo.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                await _pacienteService.GuardarHistoriaClinicaAsync(
                    Guid.Parse(pacienteId),
                    bytes,
                    archivo.FileName);

                _logger.LogInformation("Historia clínica subida por paciente {PacienteId}, archivo: {Archivo}",
                    pacienteId, archivo.FileName);

                TempData[MensajeExitoKey] = "Historia clínica guardada exitosamente.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al subir historia para paciente {PacienteId}", pacienteId);
                TempData["ErrorHistoria"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al subir historia para paciente {PacienteId}", pacienteId);
                TempData["ErrorHistoria"] = "Ocurrió un error al guardar la historia clínica.";
            }

            return RedirectToAction(nameof(Perfil));
        }

        /// <summary>
        /// POST: /Paciente/EliminarHistoriaClinica
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EliminarHistoriaClinica()
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
                return RedirectToAction(LoginAction);

            try
            {
                await _pacienteService.EliminarHistoriaClinicaAsync(Guid.Parse(pacienteId));

                _logger.LogInformation("Historia clínica eliminada por paciente {PacienteId}", pacienteId);
                TempData[MensajeExitoKey] = "Historia clínica eliminada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar historia para paciente {PacienteId}", pacienteId);
                TempData["ErrorHistoria"] = "Error al eliminar la historia clínica.";
            }

            return RedirectToAction(nameof(Perfil));
        }

        /// <summary>
        /// GET: /Paciente/DescargarHistoriaClinica
        /// Permite al paciente descargar su propio PDF
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarHistoriaClinica()
        {
            var pacienteId = HttpContext.Session.GetString(PacienteIdSessionKey);
            if (string.IsNullOrEmpty(pacienteId))
                return RedirectToAction(LoginAction);

            var resultado = await _pacienteService.ObtenerHistoriaClinicaAsync(Guid.Parse(pacienteId));

            if (resultado == null)
            {
                TempData["ErrorHistoria"] = "No tienes una historia clínica cargada.";
                return RedirectToAction(nameof(Perfil));
            }

            return File(resultado.Value.Archivo, "application/pdf", resultado.Value.NombreArchivo);
        }


    }
}