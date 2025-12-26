using MedCitas.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.Interfaces
{
    public interface IAdminRepository
    {
        Task RegistrarAsync(Admin admin);
        Task<Admin?> ObtenerPorCorreoAsync(string correoElectronico);
        Task<Admin> LoginAsync(string correo, string password);
        Task ActualizarAsync(Admin admin);
        Task ActualizarPasswordAsync(Admin admin);
        Task<Admin?> ObtenerPorIdAsync(Guid id);
        Task<List<Paciente>> ObtenerTodosPacientesAsync();
        Task<List<Doctor>> ObtenerTodosDoctoresAsync();
        Task<List<Admin>> ObtenerTodosAdminsAsync();
        Task<bool> VerificarOTPAsync(string correo, string codigoOTP);
        Task ActualizarOTPAsync(Admin admin);
    }
}
