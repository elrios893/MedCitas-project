using MedCitas.Core.DTOs;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedCitas.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly RagService _rag;
        private readonly IPacienteRepository _pacientes;
        private readonly ILogger<ChatbotController> _logger;

        private const string PacienteIdSessionKey = "PacienteId";
        private const string LoginAction = "Login";
        private const string LoginController = "Paciente";

        public ChatbotController(
            RagService rag,
            IPacienteRepository pacientes,
            ILogger<ChatbotController> logger)
        {
            _rag = rag;
            _pacientes = pacientes;
            _logger = logger;
        }

        private Guid? ObtenerPacienteIdSesion()
        {
            var idStr = HttpContext.Session.GetString(PacienteIdSessionKey);
            return Guid.TryParse(idStr, out var id) ? id : null;
        }

        // ============================================
        // GET: /Chatbot/Chat
        // Renderiza la vista del chat
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            var pacienteId = ObtenerPacienteIdSesion();
            if (pacienteId == null)
                return RedirectToAction(LoginAction, LoginController);

            var paciente = await _pacientes.ObtenerPorIdAsync(pacienteId.Value);
            if (paciente == null)
                return RedirectToAction(LoginAction, LoginController);

            ViewBag.PacienteNombre = paciente.NombreCompleto;
            ViewBag.TieneHistoria = paciente.TieneHistoriaClinica;
            ViewBag.ApiDisponible = await _rag.HealthCheckAsync();

            return View();
        }

        // ============================================
        // POST: /Chatbot/Consultar  (respuesta completa, sin streaming)
        // ============================================
        [HttpPost]
        public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
        {
            var pacienteId = ObtenerPacienteIdSesion();
            if (pacienteId == null)
                return Unauthorized(new { message = "Sesión expirada" });

            try
            {
                var paciente = await _pacientes.ObtenerPorIdAsync(pacienteId.Value);
                if (paciente == null)
                    return NotFound(new { message = "Paciente no encontrado" });

                // Siempre asignar el ID real desde sesión
                request.PacienteId = pacienteId.Value.ToString();

                // Enriquecer contexto clínico con datos del paciente
                request.ContextoClinico ??= new ContextoClinico();
                request.ContextoClinico.Nombre = paciente.NombreCompleto;
                request.ContextoClinico.Sexo = paciente.Sexo;
                request.ContextoClinico.Edad = paciente.CalcularEdad();

                _logger.LogInformation("Consulta chatbot paciente {PacienteId}: {Pregunta}",
                    pacienteId, request.Pregunta);

                var resultado = await _rag.ConsultarAsync(request);
                return Ok(resultado);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión RAG para paciente {PacienteId}", pacienteId);
                return StatusCode(503, new { message = "El asistente no está disponible en este momento." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en chatbot para paciente {PacienteId}", pacienteId);
                return StatusCode(500, new { message = "Error al procesar tu consulta." });
            }
        }

        // ============================================
        // GET: /Chatbot/Stream?pregunta=...
        // Endpoint SSE: relay token a token desde el microservicio Python
        // ============================================
        [HttpGet]
        public async Task ConsultarStream([FromQuery] string pregunta)
        {
            // ✅ Verificar sesión — si no hay sesión, responder SSE de error y terminar
            var pacienteId = ObtenerPacienteIdSesion();
            if (pacienteId == null)
            {
                Response.StatusCode = 401;
                return;
            }

            if (string.IsNullOrWhiteSpace(pregunta))
            {
                Response.StatusCode = 400;
                return;
            }

            // ✅ Configurar respuesta como SSE
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            // ✅ Desactivar buffering para que los tokens lleguen inmediatamente al cliente
            var bufferingFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
            bufferingFeature?.DisableBuffering();

            _logger.LogInformation("Stream chatbot iniciado para paciente {PacienteId}: {Pregunta}",
                pacienteId, pregunta);

            try
            {
                // ✅ Pasar Response.Body (Stream) en lugar de HttpResponse
                await _rag.StreamConsultaAsync(
                    pregunta,
                    pacienteId.Value.ToString(),
                    Response.Body,
                    HttpContext.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                // Cliente desconectó — es normal, no es un error
                _logger.LogInformation("Stream cancelado por el cliente para paciente {PacienteId}", pacienteId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión RAG (stream) para paciente {PacienteId}", pacienteId);
                // Enviar evento de error en formato SSE para que el JS lo capture
                var error = System.Text.Encoding.UTF8.GetBytes("data: {\"error\":\"El asistente no está disponible en este momento.\"}\n\n");
                await Response.Body.WriteAsync(error);
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en stream para paciente {PacienteId}", pacienteId);
                var error = System.Text.Encoding.UTF8.GetBytes("data: {\"error\":\"Error inesperado al procesar tu consulta.\"}\n\n");
                await Response.Body.WriteAsync(error);
                await Response.Body.FlushAsync();
            }
        }

        // ============================================
        // GET: /Chatbot/Health
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Health()
        {
            var disponible = await _rag.HealthCheckAsync();
            return disponible
                ? Ok(new { status = "online" })
                : StatusCode(503, new { status = "offline" });
        }
    }
}
