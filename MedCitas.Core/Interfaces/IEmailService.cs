using MedCitas.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de envío de correos electrónicos
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo con el enlace de verificación de cuenta
        /// </summary>
        Task EnviarCorreoVerificacionAsync(string destinatario, string tokenVerificacion);

        /// <summary>
        /// Envía un código OTP por correo electrónico
        /// </summary>
        Task EnviarOTPAsync(string correo, string codigoOTP, string nombreCompleto);

        /// <summary>
        /// Envía un correo con el enlace para recuperación de contraseña
        /// </summary>
        Task EnviarCorreoRecuperacionAsync(string correo, string nombreCompleto, string urlRecuperacion);

        /// <summary>
        /// Envía notificación cuando se realizan cambios sensibles en la cuenta
        /// </summary>
        Task EnviarNotificacionCambiosSensiblesAsync(string correo, string nombreCompleto);

        /// <summary>
        /// Envía confirmación de cita agendada
        /// </summary>
        Task EnviarConfirmacionCitaAsync(
                string correo,
        string nombrePaciente,
                string nombreDoctor,
             string especialidad,
         DateTime fechaCita,
                TimeSpan horaCita);

         /// <summary>
        /// Envía notificación de cancelación de cita
        /// </summary>
        Task EnviarNotificacionCancelacionAsync(string correo, string nombrePaciente, string nombreDoctor, string especialidad,
        DateTime fechaCita, TimeSpan horaCita);

        /// <summary>
        /// Envía un correo de contacto desde el formulario web
        /// </summary>
        Task EnviarCorreoContactoAsync(ContactoDTO contacto);

    }

}
