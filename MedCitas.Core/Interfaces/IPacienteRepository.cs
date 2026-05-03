using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedCitas.Core.Entities;

namespace MedCitas.Core.Interfaces
{
    public interface IPacienteRepository
    {
        Task<Paciente?> ObtenerPorIdAsync(Guid id);
        Task<Paciente?> ObtenerPorDocumentoAsync(string numeroDocumento);
        Task<Paciente?> ObtenerPorCorreoAsync(string correoElectronico);
        Task RegistrarAsync(Paciente paciente);
        Task<bool> ActivarCuentaAsync(string tokenVerificacion);
        Task<bool> VerificarOTPAsync(string correo, string codigoOTP);
        Task ActualizarOTPAsync(Paciente paciente);
        Task ActualizarAsync(Paciente paciente);

        // Métodos para recuperación de contraseña
        Task<Paciente?> ObtenerPorTokenRecuperacionAsync(string token);
        Task ActualizarTokenRecuperacionAsync(Paciente paciente);
        Task ActualizarPasswordAsync(Paciente paciente);

        Task<bool> EliminarAsync(Guid id);

        // Nuevo método para actualizar la historia clínica del paciente desde un interface
        Task ActualizarHistoriaClinicaAsync(Paciente paciente);

    }
}

