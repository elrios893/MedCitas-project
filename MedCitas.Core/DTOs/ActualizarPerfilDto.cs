using System;
using System.ComponentModel.DataAnnotations;

namespace MedCitas.Core.DTOs
{
    /// <summary>
    /// DTO para actualizar el perfil de un paciente
  /// </summary>
 public class ActualizarPerfilDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de documento es requerido")]
        public string TipoDocumento { get; set; } = string.Empty;

   [Required(ErrorMessage = "El número de documento es requerido")]
      [StringLength(20, MinimumLength = 6, ErrorMessage = "El documento debe tener entre 6 y 20 caracteres")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El documento solo debe contener números")]
    public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
   [StringLength(15, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 15 dígitos")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono solo debe contener números")]
   public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido")]
[EmailAddress(ErrorMessage = "El formato del correo es inválido")]
        [MaxLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;

    // Campos opcionales para cambio de contraseña
  public string? PasswordActual { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? NuevaPassword { get; set; }

        [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmarPassword { get; set; }
    }
}
