using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCitas.Core.DTOs
{
    public class ContactoDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [MinLength(10, ErrorMessage = "El mensaje debe tener al menos 10 caracteres")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
