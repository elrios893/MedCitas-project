using System;
using MedCitas.Core.Constants;
using MedCitas.Core.Services;
using Xunit;

namespace MedCitas.Tests.Services
{
    public class OtpServiceTests
    {
        #region GenerarOTP

        [Fact]
public void GenerarOTP_DeberiaRetornarCodigo6Digitos()
 {
        // Act
var otp = OtpService.GenerarOTP();

   // Assert
Assert.NotNull(otp);
  Assert.Equal(6, otp.Length);
     Assert.True(int.TryParse(otp, out _), "OTP debe ser numérico");
}

 [Fact]
   public void GenerarOTP_DeberiaEstarEnRangoValido()
    {
  // Act
   var otp = OtpService.GenerarOTP();
   var otpNumerico = int.Parse(otp);

  // Assert
       Assert.InRange(otpNumerico, AppConstants.Otp.MinValue, AppConstants.Otp.MaxValue - 1);
   }

[Fact]
    public void GenerarOTP_DeberiaSerDiferente_EnLlamadasSucesivas()
{
   // Act
   var otp1 = OtpService.GenerarOTP();
   var otp2 = OtpService.GenerarOTP();
      var otp3 = OtpService.GenerarOTP();

     // Assert - Al menos uno debe ser diferente (probabilidad muy alta)
        Assert.True(otp1 != otp2 || otp2 != otp3 || otp1 != otp3,
        "Los OTPs generados deberían ser diferentes");
    }

     #endregion

        #region ObtenerFechaExpiracion

    [Fact]
   public void ObtenerFechaExpiracion_DeberiaRetornarFechaFutura()
 {
         // Arrange
      var ahora = DateTime.UtcNow;

   // Act
  var expiracion = OtpService.ObtenerFechaExpiracion();

// Assert
     Assert.True(expiracion > ahora);
 }

        [Fact]
    public void ObtenerFechaExpiracion_DeberiaAgregarMinutosCorrectos()
 {
   // Arrange
        var ahora = DateTime.UtcNow;

     // Act
       var expiracion = OtpService.ObtenerFechaExpiracion();
 var diferencia = (expiracion - ahora).TotalMinutes;

// Assert
 Assert.InRange(diferencia, AppConstants.Otp.ExpirationMinutes - 0.1, 
      AppConstants.Otp.ExpirationMinutes + 0.1);
   }

    #endregion

    #region ValidarOTP

[Fact]
 public void ValidarOTP_ConOTPCorrecto_DeberiaRetornarTrue()
   {
   // Arrange
var otpIngresado = "123456";
   var otpAlmacenado = "123456";
    var expiracion = DateTime.UtcNow.AddMinutes(5);

    // Act
       var resultado = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);

    // Assert
Assert.True(resultado);
 }

  [Fact]
  public void ValidarOTP_ConOTPIncorrecto_DeberiaRetornarFalse()
 {
  // Arrange
   var otpIngresado = "123456";
   var otpAlmacenado = "654321";
       var expiracion = DateTime.UtcNow.AddMinutes(5);

    // Act
       var resultado = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);

    // Assert
  Assert.False(resultado);
     }

      [Fact]
 public void ValidarOTP_ConOTPExpirado_DeberiaRetornarFalse()
 {
  // Arrange
   var otpIngresado = "123456";
       var otpAlmacenado = "123456";
    var expiracion = DateTime.UtcNow.AddMinutes(-1); // Expirado hace 1 minuto

   // Act
    var resultado = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);

 // Assert
  Assert.False(resultado);
    }

 [Theory]
[InlineData("", "123456")]
        [InlineData("123456", "")]
        public void ValidarOTP_ConOTPVacio_DeberiaRetornarFalse(string otpIngresado, string otpAlmacenado)
 {
     // Arrange
    var expiracion = DateTime.UtcNow.AddMinutes(5);

// Act
var resultado = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);

            // Assert
         Assert.False(resultado);
        }

 [Fact]
  public void ValidarOTP_ConExpiracionNull_DeberiaRetornarFalse()
  {
  // Arrange
 var otpIngresado = "123456";
      var otpAlmacenado = "123456";
    DateTime? expiracion = null;

    // Act
   var resultado = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);

        // Assert
  Assert.False(resultado);
   }

     [Fact]
     public void ValidarOTP_CaseSensitive_DeberiaSerExacto()
  {
      // Arrange - aunque OTP es numérico, validamos que sea exacto
    var otpIngresado = "123456";
   var otpAlmacenado = "123456";
     var expiracion = DateTime.UtcNow.AddMinutes(5);

      // Act
       var resultadoCorrecto = OtpService.ValidarOTP(otpIngresado, otpAlmacenado, expiracion);
  var resultadoIncorrecto = OtpService.ValidarOTP("023456", otpAlmacenado, expiracion);

     // Assert
    Assert.True(resultadoCorrecto);
   Assert.False(resultadoIncorrecto);
        }

        #endregion

    #region HaExcedidoIntentos

 [Fact]
    public void HaExcedidoIntentos_ConMenosDeTresIntentos_DeberiaRetornarFalse()
{
   // Arrange & Act & Assert
 Assert.False(OtpService.HaExcedidoIntentos(0));
 Assert.False(OtpService.HaExcedidoIntentos(1));
Assert.False(OtpService.HaExcedidoIntentos(2));
     }

     [Fact]
        public void HaExcedidoIntentos_ConTresOMasIntentos_DeberiaRetornarTrue()
  {
   // Arrange & Act & Assert
  Assert.True(OtpService.HaExcedidoIntentos(3));
    Assert.True(OtpService.HaExcedidoIntentos(4));
       Assert.True(OtpService.HaExcedidoIntentos(10));
      }

    [Fact]
 public void HaExcedidoIntentos_ConMaximoExacto_DeberiaRetornarTrue()
   {
 // Arrange
 var maxIntentos = AppConstants.Otp.MaxFailedAttempts;

      // Act
   var resultado = OtpService.HaExcedidoIntentos(maxIntentos);

  // Assert
  Assert.True(resultado);
  }

 [Fact]
 public void HaExcedidoIntentos_ConMaximoMenosUno_DeberiaRetornarFalse()
    {
// Arrange
    var intentos = AppConstants.Otp.MaxFailedAttempts - 1;

 // Act
   var resultado = OtpService.HaExcedidoIntentos(intentos);

      // Assert
   Assert.False(resultado);
     }

        #endregion
    }
}
