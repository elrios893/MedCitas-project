using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedCitas.Core.Entities;

namespace MedCitas.Core.Interfaces
{
    /// <summary>
  /// Repositorio para la gestión de especialidades médicas
    /// </summary>
    public interface ISpecialtyRepository
    {
        /// <summary>
        /// Obtiene una especialidad por su ID
        /// </summary>
    Task<Specialty?> ObtenerPorIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las especialidades activas
        /// </summary>
   Task<List<Specialty>> ObtenerTodasAsync();
    }
}
