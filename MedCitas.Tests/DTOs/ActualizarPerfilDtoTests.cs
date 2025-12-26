using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedCitas.Core.DTOs;
using Xunit;

namespace MedCitas.Tests.DTOs
{
    public class ActualizarPerfilDtoTests
    {
        private static List<ValidationResult> ValidateDto(ActualizarPerfilDto dto)
     {
     var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(dto);
      Validator.TryValidateObject(dto, validationContext, validationResults, true);
            return validationResults;
        }

  #region Tests de Validación Básica

        [Fact]
      public void ActualizarPerfilDto_ConDatosValidos_NoDeberiaGenerarErrores()
        {
       // Arrange
         var dto = new ActualizarPerfilDto
      {
         NombreCompleto = "Juan Pérez García",
              TipoDocumento = "CC",
  NumeroDocumento = "1234567890",
    Telefono = "3001234567",
    CorreoElectronico = "juan.perez@example.com"
            };

 // Act
    var validationResults = ValidateDto(dto);

  // Assert
            Assert.Empty(validationResults);
}

        [Fact]
        public void ActualizarPerfilDto_NombreCompletoVacio_DeberiaGenerarError()
      {
            // Arrange
  var dto = new ActualizarPerfilDto
            {
                NombreCompleto = "",
                TipoDocumento = "CC",
          NumeroDocumento = "1234567890",
                Telefono = "3001234567",
       CorreoElectronico = "test@example.com"
     };

        // Act
            var validationResults = ValidateDto(dto);

  // Assert
  Assert.Contains(validationResults, v => v.MemberNames.Contains("NombreCompleto"));
        }

        [Fact]
  public void ActualizarPerfilDto_NombreCompletoMuyCorto_DeberiaGenerarError()
        {
    // Arrange
     var dto = new ActualizarPerfilDto
            {
              NombreCompleto = "AB",
         TipoDocumento = "CC",
           NumeroDocumento = "1234567890",
          Telefono = "3001234567",
  CorreoElectronico = "test@example.com"
            };

            // Act
            var validationResults = ValidateDto(dto);

            // Assert
  Assert.Contains(validationResults, v => v.ErrorMessage.Contains("entre 3 y 100 caracteres"));
        }

        [Fact]
        public void ActualizarPerfilDto_NombreCompletoMuyLargo_DeberiaGenerarError()
        {
         // Arrange
      var dto = new ActualizarPerfilDto
     {
      NombreCompleto = new string('A', 101),
           TipoDocumento = "CC",
             NumeroDocumento = "1234567890",
 Telefono = "3001234567",
      CorreoElectronico = "test@example.com"
            };

            // Act
  var validationResults = ValidateDto(dto);

   // Assert
   Assert.Contains(validationResults, v => v.ErrorMessage.Contains("entre 3 y 100 caracteres"));
        }

        #endregion

  #region Tests de Tipo de Documento

        [Fact]
        public void ActualizarPerfilDto_TipoDocumentoVacio_DeberiaGenerarError()
   {
            // Arrange
            var dto = new ActualizarPerfilDto
            {
      NombreCompleto = "Juan Pérez",
      TipoDocumento = "",
             NumeroDocumento = "1234567890",
    Telefono = "3001234567",
     CorreoElectronico = "test@example.com"
          };

        // Act
   var validationResults = ValidateDto(dto);

            // Assert
          Assert.Contains(validationResults, v => v.MemberNames.Contains("TipoDocumento"));
        }

        #endregion

        #region Tests de Número de Documento

        [Fact]
        public void ActualizarPerfilDto_NumeroDocumentoVacio_DeberiaGenerarError()
        {
            // Arrange
    var dto = new ActualizarPerfilDto
          {
            NombreCompleto = "Juan Pérez",
     TipoDocumento = "CC",
                NumeroDocumento = "",
    Telefono = "3001234567",
       CorreoElectronico = "test@example.com"
            };

 // Act
            var validationResults = ValidateDto(dto);

            // Assert
    Assert.Contains(validationResults, v => v.MemberNames.Contains("NumeroDocumento"));
     }

      [Fact]
        public void ActualizarPerfilDto_NumeroDocumentoMuyCorto_DeberiaGenerarError()
        {
     // Arrange
   var dto = new ActualizarPerfilDto
         {
                NombreCompleto = "Juan Pérez",
   TipoDocumento = "CC",
 NumeroDocumento = "12345",
            Telefono = "3001234567",
  CorreoElectronico = "test@example.com"
            };

    // Act
      var validationResults = ValidateDto(dto);

// Assert
      Assert.Contains(validationResults, v => v.ErrorMessage.Contains("entre 6 y 20 caracteres"));
  }

        [Fact]
        public void ActualizarPerfilDto_NumeroDocumentoConLetras_DeberiaGenerarError()
  {
            // Arrange
            var dto = new ActualizarPerfilDto
            {
                NombreCompleto = "Juan Pérez",
                TipoDocumento = "CC",
NumeroDocumento = "1234ABC890",
  Telefono = "3001234567",
        CorreoElectronico = "test@example.com"
  };

      // Act
var validationResults = ValidateDto(dto);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("solo debe contener números"));
     }

      #endregion

  #region Tests de Teléfono

        [Fact]
        public void ActualizarPerfilDto_TelefonoVacio_DeberiaGenerarError()
{
            // Arrange
        var dto = new ActualizarPerfilDto
 {
    NombreCompleto = "Juan Pérez",
     TipoDocumento = "CC",
 NumeroDocumento = "1234567890",
     Telefono = "",
    CorreoElectronico = "test@example.com"
         };

            // Act
   var validationResults = ValidateDto(dto);

            // Assert
       Assert.Contains(validationResults, v => v.MemberNames.Contains("Telefono"));
        }

        [Fact]
        public void ActualizarPerfilDto_TelefonoMuyCorto_DeberiaGenerarError()
        {
       // Arrange
            var dto = new ActualizarPerfilDto
  {
      NombreCompleto = "Juan Pérez",
        TipoDocumento = "CC",
    NumeroDocumento = "1234567890",
                Telefono = "123456",
       CorreoElectronico = "test@example.com"
  };

 // Act
            var validationResults = ValidateDto(dto);

            // Assert
       Assert.Contains(validationResults, v => v.ErrorMessage.Contains("entre 7 y 15 dígitos"));
        }

     [Fact]
        public void ActualizarPerfilDto_TelefonoConLetras_DeberiaGenerarError()
        {
  // Arrange
     var dto = new ActualizarPerfilDto
{
       NombreCompleto = "Juan Pérez",
     TipoDocumento = "CC",
    NumeroDocumento = "1234567890",
            Telefono = "300ABC1234",
          CorreoElectronico = "test@example.com"
            };

 // Act
    var validationResults = ValidateDto(dto);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("solo debe contener números"));
        }

        #endregion

        #region Tests de Correo Electrónico

    [Fact]
        public void ActualizarPerfilDto_CorreoVacio_DeberiaGenerarError()
        {
 // Arrange
            var dto = new ActualizarPerfilDto
     {
  NombreCompleto = "Juan Pérez",
     TipoDocumento = "CC",
          NumeroDocumento = "1234567890",
    Telefono = "3001234567",
      CorreoElectronico = ""
        };

   // Act
     var validationResults = ValidateDto(dto);

        // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("CorreoElectronico"));
      }

 [Fact]
     public void ActualizarPerfilDto_CorreoInvalido_DeberiaGenerarError()
     {
 // Arrange
 var dto = new ActualizarPerfilDto
            {
         NombreCompleto = "Juan Pérez",
         TipoDocumento = "CC",
     NumeroDocumento = "1234567890",
   Telefono = "3001234567",
  CorreoElectronico = "correo-invalido"
      };

  // Act
       var validationResults = ValidateDto(dto);

            // Assert
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("formato del correo"));
        }

        #endregion

        #region Tests de Cambio de Contraseña

    [Fact]
public void ActualizarPerfilDto_ConCambioDeContrasena_ConDatosValidos_NoDeberiaGenerarErrores()
        {
        // Arrange
            var dto = new ActualizarPerfilDto
            {
                NombreCompleto = "Juan Pérez",
       TipoDocumento = "CC",
                NumeroDocumento = "1234567890",
         Telefono = "3001234567",
     CorreoElectronico = "test@example.com",
             PasswordActual = "OldPassword123!",
    NuevaPassword = "NewPassword123!",
        ConfirmarPassword = "NewPassword123!"
      };

         // Act
            var validationResults = ValidateDto(dto);

            // Assert
   Assert.Empty(validationResults);
  }

        [Fact]
        public void ActualizarPerfilDto_NuevaPasswordMuyCorta_DeberiaGenerarError()
        {
        // Arrange
  var dto = new ActualizarPerfilDto
    {
              NombreCompleto = "Juan Pérez",
        TipoDocumento = "CC",
         NumeroDocumento = "1234567890",
                Telefono = "3001234567",
      CorreoElectronico = "test@example.com",
 PasswordActual = "OldPassword123!",
                NuevaPassword = "Short1!",
      ConfirmarPassword = "Short1!"
      };

   // Act
          var validationResults = ValidateDto(dto);

            // Assert
   Assert.Contains(validationResults, v => v.ErrorMessage.Contains("al menos 8 caracteres"));
 }

     [Fact]
        public void ActualizarPerfilDto_PasswordsNoCoinciden_DeberiaGenerarError()
        {
   // Arrange
            var dto = new ActualizarPerfilDto
            {
     NombreCompleto = "Juan Pérez",
        TipoDocumento = "CC",
             NumeroDocumento = "1234567890",
        Telefono = "3001234567",
             CorreoElectronico = "test@example.com",
PasswordActual = "OldPassword123!",
  NuevaPassword = "NewPassword123!",
  ConfirmarPassword = "DifferentPassword123!"
    };

  // Act
      var validationResults = ValidateDto(dto);

     // Assert
Assert.Contains(validationResults, v => v.ErrorMessage.Contains("no coinciden"));
        }

        [Fact]
        public void ActualizarPerfilDto_SinCambioDeContrasena_CamposPasswordNull_NoDeberiaGenerarErrores()
        {
         // Arrange
 var dto = new ActualizarPerfilDto
    {
        NombreCompleto = "Juan Pérez",
             TipoDocumento = "CC",
          NumeroDocumento = "1234567890",
          Telefono = "3001234567",
      CorreoElectronico = "test@example.com",
       PasswordActual = null,
NuevaPassword = null,
    ConfirmarPassword = null
 };

        // Act
         var validationResults = ValidateDto(dto);

        // Assert
            Assert.Empty(validationResults);
        }

  #endregion

      #region Tests de Propiedades Inicializadas

   [Fact]
        public void ActualizarPerfilDto_PropiedadesString_DeberianInicializarseVacias()
    {
            // Arrange & Act
            var dto = new ActualizarPerfilDto();

    // Assert
        Assert.Equal(string.Empty, dto.NombreCompleto);
            Assert.Equal(string.Empty, dto.TipoDocumento);
    Assert.Equal(string.Empty, dto.NumeroDocumento);
            Assert.Equal(string.Empty, dto.Telefono);
    Assert.Equal(string.Empty, dto.CorreoElectronico);
   }

      [Fact]
        public void ActualizarPerfilDto_PropiedadesPassword_DeberianInicializarseNull()
        {
      // Arrange & Act
   var dto = new ActualizarPerfilDto();

   // Assert
          Assert.Null(dto.PasswordActual);
     Assert.Null(dto.NuevaPassword);
Assert.Null(dto.ConfirmarPassword);
 }

        #endregion
    }
}
