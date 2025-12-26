using System;
using MedCitas.Core.Helpers;
using Xunit;

namespace MedCitas.Tests.Helpers
{
    public class ValidationHelperTests
    {
   #region EsDocumentoValido

[Theory]
[InlineData("12345678")]
     [InlineData("1")]
    [InlineData("123456789012345")]
     public void EsDocumentoValido_ConSoloNumeros_DeberiaRetornarTrue(string documento)
   {
   // Act
var resultado = ValidationHelper.EsDocumentoValido(documento);

     // Assert
  Assert.True(resultado);
   }

   [Theory]
[InlineData("")]
      [InlineData("   ")]
   [InlineData("12345ABC")]
   [InlineData("ABC12345")]
     [InlineData("123-456")]
  [InlineData("123.456")]
  [InlineData("12 34 56")]
   public void EsDocumentoValido_ConFormatoInvalido_DeberiaRetornarFalse(string documento)
 {
    // Act
 var resultado = ValidationHelper.EsDocumentoValido(documento);

       // Assert
    Assert.False(resultado);
      }

#endregion

    #region EsTelefonoValido

   [Theory]
    [InlineData("1234567")]   // 7 dígitos (mínimo)
  [InlineData("12345678")]       // 8 dígitos
   [InlineData("3001234567")]     // 10 dígitos
  [InlineData("123456789012345")] // 15 dígitos (máximo)
 public void EsTelefonoValido_ConFormatoValido_DeberiaRetornarTrue(string telefono)
{
    // Act
   var resultado = ValidationHelper.EsTelefonoValido(telefono);

   // Assert
  Assert.True(resultado);
  }

     [Theory]
     [InlineData("")]
   [InlineData("   ")]
[InlineData("123456")] // Muy corto (6 dígitos)
  [InlineData("1234567890123456")] // Muy largo (16 dígitos)
  [InlineData("300-123-4567")]  // Con guiones
   [InlineData("300 123 4567")]    // Con espacios
   [InlineData("3001234ABC")] // Con letras
   [InlineData("+573001234567")]   // Con símbolo
     public void EsTelefonoValido_ConFormatoInvalido_DeberiaRetornarFalse(string telefono)
  {
 // Act
    var resultado = ValidationHelper.EsTelefonoValido(telefono);

      // Assert
Assert.False(resultado);
   }

  #endregion

     #region EsCorreoValido

   [Theory]
   [InlineData("user@example.com")]
   [InlineData("user.name@example.com")]
 [InlineData("user+tag@example.co.uk")]
     [InlineData("user_name@example-domain.com")]
      [InlineData("123@example.com")]
   [InlineData("user@subdomain.example.com")]
public void EsCorreoValido_ConFormatoValido_DeberiaRetornarTrue(string correo)
 {
         // Act
  var resultado = ValidationHelper.EsCorreoValido(correo);

      // Assert
  Assert.True(resultado);
   }

   [Theory]
  [InlineData("")]
   [InlineData("   ")]
   [InlineData("correo-invalido")]
    [InlineData("@example.com")]
   [InlineData("user@")]
   [InlineData("user @example.com")]
   [InlineData("user@.com")]
        public void EsCorreoValido_ConFormatoInvalido_DeberiaRetornarFalse(string correo)
     {
         // Act
     var resultado = ValidationHelper.EsCorreoValido(correo);

// Assert
   Assert.False(resultado);
        }

        #endregion

        #region EsPasswordValido

 [Theory]
   [InlineData("Prueba123!")]
  [InlineData("MiPassword2024@")]
   [InlineData("Segura#Pass123")]
  [InlineData("P@ssw0rd!")]
      [InlineData("Abc123!@#")]
      public void EsPasswordValido_ConFormatoValido_DeberiaRetornarTrue(string password)
      {
  // Act
     var resultado = ValidationHelper.EsPasswordValido(password);

     // Assert
   Assert.True(resultado);
        }

    [Theory]
   [InlineData("")]
  [InlineData("   ")]
      [InlineData("1234567")]      // Muy corta
[InlineData("password")]  // Sin mayúsculas, números o símbolos
      [InlineData("PASSWORD123")] // Sin minúsculas ni símbolos
      [InlineData("Passwor")]      // Sin número ni símbolo
   [InlineData("Pass123")]     // Sin carácter especial
[InlineData("Password!")]      // Sin número
     [InlineData("password123!")]// Sin mayúscula
     [InlineData("PASSWORD123!")]// Sin minúscula
    public void EsPasswordValido_ConFormatoInvalido_DeberiaRetornarFalse(string password)
   {
 // Act
      var resultado = ValidationHelper.EsPasswordValido(password);

      // Assert
  Assert.False(resultado);
   }

   #endregion

  #region SanitizarInput

        [Fact]
     public void SanitizarInput_ConCaracteresXSS_DeberiaEscaparlos()
     {
 // Arrange
     var input = "<script>alert('XSS')</script>";
      var esperado = "&lt;script&gt;alert(&#x27;XSS&#x27;)&lt;&#x2F;script&gt;";

        // Act
  var resultado = ValidationHelper.SanitizarInput(input);

   // Assert
        Assert.Equal(esperado, resultado);
 }

        [Fact]
        public void SanitizarInput_ConComillasYSlash_DeberiaEscaparlos()
 {
   // Arrange
    var input = "Test \"quotes\" and /slashes/";
      var esperado = "Test &quot;quotes&quot; and &#x2F;slashes&#x2F;";

       // Act
 var resultado = ValidationHelper.SanitizarInput(input);

   // Assert
      Assert.Equal(esperado, resultado);
 }

    [Theory]
    [InlineData("", "")]
   [InlineData("   ", "")]
   public void SanitizarInput_ConInputVacio_DeberiaRetornarVacio(string input, string esperado)
 {
    // Act
      var resultado = ValidationHelper.SanitizarInput(input);

       // Assert
Assert.Equal(esperado, resultado);
}

 [Fact]
        public void SanitizarInput_ConEspaciosAlrededor_DeberiaTrimearlos()
     {
  // Arrange
        var input = "  texto con espacios  ";
 var esperado = "texto con espacios";

  // Act
    var resultado = ValidationHelper.SanitizarInput(input);

          // Assert
     Assert.Equal(esperado, resultado);
    }

        #endregion

        #region PasswordsCoinciden

  [Fact]
     public void PasswordsCoinciden_ConPasswordsIguales_DeberiaRetornarTrue()
 {
  // Arrange
   var password = "MiPassword123!";
  var confirmar = "MiPassword123!";

      // Act
var resultado = ValidationHelper.PasswordsCoinciden(password, confirmar);

      // Assert
            Assert.True(resultado);
   }

   [Theory]
        [InlineData("Password123!", "Password456!")]
  [InlineData("Password123!", "password123!")]  // Case sensitive
 [InlineData("Password123!", "Password123! ")]  // Espacios extra
   public void PasswordsCoinciden_ConPasswordsDiferentes_DeberiaRetornarFalse(string password, string confirmar)
 {
      // Act
  var resultado = ValidationHelper.PasswordsCoinciden(password, confirmar);

  // Assert
   Assert.False(resultado);
      }

   [Theory]
     [InlineData("", "")]
   [InlineData("   ", "   ")]
     [InlineData("Password123!", "")]
     [InlineData("", "Password123!")]
   public void PasswordsCoinciden_ConPasswordsVacias_DeberiaRetornarFalse(string password, string confirmar)
        {
// Act
      var resultado = ValidationHelper.PasswordsCoinciden(password, confirmar);

      // Assert
      Assert.False(resultado);
  }

   #endregion
    }
}
