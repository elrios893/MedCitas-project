using MedCitas.Core.Configuration;
using MedCitas.Core.Constants;
using MedCitas.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MedCitas.Core.DTOs;


namespace MedCitas.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;

            // Validar configuración al inicializar
            if (!_config.IsValid())
            {
                _logger.LogWarning("Configuración de Email incompleta: {Errors}", _config.GetValidationErrors());
            }
            else
            {
                _logger.LogInformation("EmailService inicializado correctamente");
            }


        }

        public async Task EnviarCorreoVerificacionAsync(string destinatario, string tokenVerificacion)
        {
            var asunto = "Verifica tu cuenta - MedCitas";
            var urlVerificacion = $"https://medcitas.com/verificar/{tokenVerificacion}";

            var cuerpoHtml = GenerarHtmlVerificacion(urlVerificacion);

            await EnviarEmailAsync(destinatario, asunto, cuerpoHtml);
        }

        public async Task EnviarOTPAsync(string correo, string codigoOTP, string nombreCompleto)
        {
            var asunto = "Código de Verificación - MedCitas";
            var cuerpoHtml = GenerarHtmlOTP(nombreCompleto, codigoOTP);

            await EnviarEmailAsync(correo, asunto, cuerpoHtml);
        }

        public async Task EnviarCorreoRecuperacionAsync(string correo, string nombreCompleto, string urlRecuperacion)
        {
            var asunto = "Recuperación de Contraseña - MedCitas";
            var cuerpoHtml = GenerarHtmlRecuperacion(nombreCompleto, urlRecuperacion);

            await EnviarEmailAsync(correo, asunto, cuerpoHtml);
        }

        private async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            _logger.LogInformation("Enviando email a {Destinatario}: {Asunto}", destinatario, asunto);

            // Validar configuración ANTES de intentar enviar
            if (!_config.IsValid())
            {
                var errores = _config.GetValidationErrors();
                _logger.LogError("Configuración de email inválida: {Errors}", errores);
                throw new InvalidOperationException($"La configuración de email es inválida: {errores}");
            }

            try
            {
                using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_config.SmtpUser, _config.SmtpPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = AppConstants.Email.SmtpTimeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config.FromEmail, _config.FromName),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(new MailAddress(destinatario));

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email enviado exitosamente a {Destinatario}", destinatario);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "Error SMTP al enviar email a {Destinatario}. StatusCode: {StatusCode}",
                destinatario, smtpEx.StatusCode);
                throw new InvalidOperationException($"Error SMTP al enviar email: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex) when (ex is not SmtpException)
            {
                _logger.LogError(ex, "Error inesperado ({Type}) al enviar email a {Destinatario}",
                ex.GetType().Name, destinatario);
                throw new InvalidOperationException($"Error al enviar email a {destinatario}: {ex.Message}", ex);
            }
        }

        //public async Task EnviarCorreoContactoAsync(ContactoDTO contacto)
        //{
        //    var asunto = $"Contacto desde Web: {contacto.Nombre}";
        //    var cuerpoHtml = GenerarHtmlContacto(contacto);

        //    await EnviarEmailAsync(_config.FromEmail, asunto, cuerpoHtml);
        //}

        public async Task EnviarCorreoContactoAsync(ContactoDTO contacto)
        {
            var asunto = $"Nuevo Mensaje de Contacto: {contacto.Nombre}";
            var cuerpoHtml = GenerarHtmlContacto(contacto);

            // Enviar al correo principal (FromEmail)
            await EnviarEmailAsync(_config.FromEmail, asunto, cuerpoHtml);

            // ✅ NUEVO: Enviar también al correo del administrador si está configurado
            if (!string.IsNullOrWhiteSpace(_config.AdminNotificationEmail) &&
                _config.AdminNotificationEmail != _config.FromEmail)
            {
                _logger.LogInformation("Enviando copia de contacto a administrador: {AdminEmail}",
                    _config.AdminNotificationEmail);

                await EnviarEmailAsync(_config.AdminNotificationEmail, asunto, cuerpoHtml);
            }
        }

        private static string GenerarHtmlContacto(ContactoDTO contacto)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .info-box {{ background: white; border-left: 4px solid #667eea; padding: 15px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'><h1>📧 Nuevo Mensaje de Contacto</h1></div>
                    <div class='content'>
                        <div class='info-box'>
                            <p><strong>👤 Nombre:</strong> {contacto.Nombre}</p>
                            <p><strong>📧 Email:</strong> {contacto.Email}</p>
                        </div>
                        <div class='info-box'>
                            <p><strong>💬 Mensaje:</strong></p>
                            <p>{contacto.Mensaje}</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        // Métodos privados para generar HTML (sin cambios en la lógica)
        private static string GenerarHtmlVerificacion(string urlVerificacion)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                 .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                 .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'><h1>Bienvenido a MedCitas</h1></div>
            <div class='content'>
                        <h2>Verifica tu cuenta</h2>
             <p>Gracias por registrarte. Para activar tu cuenta, haz clic en el botón:</p>
               <p style='text-align: center;'><a href='{urlVerificacion}' class='button'>Verificar mi cuenta</a></p>
                    </div>
                    <div class='footer'><p>© 2025 MedCitas</p></div>
                </div>
            </body>
            </html>";
        }

        private static string GenerarHtmlOTP(string nombreCompleto, string codigoOTP)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
              .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .otp-box {{ background: white; border: 2px solid #667eea; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 10px; color: #667eea; margin: 20px 0; border-radius: 10px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'><h1>Código de Verificación</h1></div>
                 <div class='content'>
                        <h2>Hola, {nombreCompleto}</h2>
                   <p>Tu código de verificación es:</p>
                   <div class='otp-box'>{codigoOTP}</div>
                        <div class='warning'><strong>Importante:</strong> Este código expira en {AppConstants.Otp.ExpirationMinutes} minutos.</div>
                    </div>
                  <div class='footer'><p>© 2025 MedCitas</p></div>
                </div>
            </body>
            </html>";
        }

        private static string GenerarHtmlRecuperacion(string nombreCompleto, string urlRecuperacion)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
  <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
 .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
      .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
  .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
  .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
      .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>Recuperación de Contraseña</h1></div>
    <div class='content'>
     <h2>Hola, {nombreCompleto}</h2>
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
    <p style='text-align: center;'><a href='{urlRecuperacion}' class='button'>Restablecer Contraseña</a></p>
       <div class='warning'><strong>Importante:</strong> Este enlace expira en {AppConstants.RecoveryToken.ExpirationMinutes} minutos.</div>
    </div>
        <div class='footer'><p>© 2025 MedCitas</p></div>
 </div>
</body>
</html>";
        }

        public async Task EnviarNotificacionCambiosSensiblesAsync(string correo, string nombreCompleto)
        {
            var asunto = "Cambios en tu cuenta - MedCitas";
            var cuerpoHtml = GenerarHtmlCambiosSensibles(nombreCompleto);
            await EnviarEmailAsync(correo, asunto, cuerpoHtml);
        }

        public async Task EnviarConfirmacionCitaAsync(
       string correo,
    string nombrePaciente,
          string nombreDoctor,
    string especialidad,
          DateTime fechaCita,
   TimeSpan horaCita)
        {
            var asunto = "Confirmación de Cita Médica - MedCitas";
            var cuerpoHtml = GenerarHtmlConfirmacionCita(nombrePaciente, nombreDoctor, especialidad, fechaCita, horaCita);
            await EnviarEmailAsync(correo, asunto, cuerpoHtml);
        }

        public async Task EnviarNotificacionCancelacionAsync(
        string correo,
            string nombrePaciente,
            string nombreDoctor,
    string especialidad,
   DateTime fechaCita,
TimeSpan horaCita)
        {
            var asunto = "Cita Cancelada - MedCitas";
            var cuerpoHtml = GenerarHtmlCancelacionCita(nombrePaciente, nombreDoctor, especialidad, fechaCita, horaCita);
            await EnviarEmailAsync(correo, asunto, cuerpoHtml);
        }

        private static string GenerarHtmlCambiosSensibles(string nombreCompleto)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .alert {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
              <div class='header'><h1>🔒 Cambios en tu Cuenta</h1></div>
             <div class='content'>
                 <h2>Hola, {nombreCompleto}</h2>
                        <p>Te informamos que se han realizado cambios en tu cuenta de MedCitas.</p>
                        <div class='alert'>
                <strong>⚠️ Atención:</strong> Si no fuiste tú quien realizó estos cambios, por favor contacta inmediatamente con soporte.
                      </div>
                        <p>Cambios que generan esta notificación:</p>
                      <ul>
              <li>Cambio de correo electrónico</li>
                 <li>Cambio de contraseña</li>
                        </ul>
                    </div>
                    <div class='footer'><p>© 2025 MedCitas</p></div>
                </div>
            </body>
            </html>";
        }

        private static string GenerarHtmlConfirmacionCita(
            string nombrePaciente,
            string nombreDoctor,
            string especialidad,
            DateTime fechaCita,
            TimeSpan horaCita)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset='UTF-8'>
                <style>
            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .cita-info {{ background: white; border: 2px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 10px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'><h1>✅ Cita Confirmada</h1></div>
                    <div class='content'>
                        <h2>Hola, {nombrePaciente}</h2>
                      <p>Tu cita médica ha sido confirmada exitosamente.</p>
                     <div class='cita-info'>
              <p><strong>📅 Fecha:</strong> {fechaCita:dddd, dd 'de' MMMM 'de' yyyy}</p>
             <p><strong>🕐 Hora:</strong> {horaCita:hh\:mm}</p>
              <p><strong>👨‍⚕️ Médico:</strong> {nombreDoctor}</p>
               <p><strong>🏥 Especialidad:</strong> {especialidad}</p>
                      </div>
              <p>Por favor, llega 15 minutos antes de tu cita.</p>
                    </div>
                    <div class='footer'><p>© 2025 MedCitas</p></div>
                </div>
            </body>
            </html>";
        }

        private static string GenerarHtmlCancelacionCita(
            string nombrePaciente,
            string nombreDoctor,
            string especialidad,
            DateTime fechaCita,
            TimeSpan horaCita)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .cita-info {{ background: white; border: 2px solid #dc3545; padding: 20px; margin: 20px 0; border-radius: 10px; }}
                .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                <div class='header'><h1>❌ Cita Cancelada</h1></div>
                    <div class='content'>
                   <h2>Hola, {nombrePaciente}</h2>
                  <p>Tu cita médica ha sido cancelada.</p>
                    <div class='cita-info'>
                        <p><strong>📅 Fecha:</strong> {fechaCita:dddd, dd 'de' MMMM 'de' yyyy}</p>
                <p><strong>🕐 Hora:</strong> {horaCita:hh\:mm}</p>
                  <p><strong>👨‍⚕️ Médico:</strong> {nombreDoctor}</p>
                     <p><strong>🏥 Especialidad:</strong> {especialidad}</p>
                    </div>
                 <p>Si deseas reagendar, por favor ingresa a tu cuenta en MedCitas.</p>
                    </div>
               <div class='footer'><p>© 2025 MedCitas</p></div>
              </div>
            </body>
            </html>";
        }
    }
}
