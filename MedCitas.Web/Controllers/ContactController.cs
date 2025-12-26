using MedCitas.Core.Interfaces;
using MedCitas.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MedCitas.Core.DTOs;


namespace MedCitas.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;

        public ContactController(ILogger<ContactController> logger, IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contacto(ContactoDTO modelo)
        {
            if (!ModelState.IsValid)
            {
                return View("Contacto", modelo);
            }

            try
            {
                await _emailService.EnviarCorreoContactoAsync(modelo);
                TempData["Mensaje"] = "Tu mensaje ha sido enviado exitosamente.";
                return RedirectToAction("Contacto");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al enviar: {ex.Message}";
                return View("Contacto", modelo);
            }
        }

    }
}
