using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedCitas.Core.Entities
{
    /// <summary>
    /// Representa una especialidad médica en el sistema
    /// </summary>
    public class Specialty
    {
    public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
      public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Duración estándar de una consulta en minutos
        /// </summary>
        [Range(15, 120)]
        public int DuracionConsultaMinutos { get; set; } = 30;

        public bool EstaActiva { get; set; } = true;

        // Navigation properties
        public List<Doctor> Doctors { get; set; } = [];
    }
}
