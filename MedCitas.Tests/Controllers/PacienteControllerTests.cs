using System;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using MedCitas.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MedCitas.Infrastructure.Services;

namespace MedCitas.Tests.Controllers
{
  public class PacienteControllerTests
    {
   private readonly Mock<IPacienteRepository> _pacienteRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<PacienteController>> _loggerMock;
        private readonly PacienteService _pacienteService;
        private readonly PacienteController _controller;

   public PacienteControllerTests()
     {
   _pacienteRepositoryMock = new Mock<IPacienteRepository>();
       _emailServiceMock = new Mock<IEmailService>();
       _loggerMock = new Mock<ILogger<PacienteController>>();
            
  _pacienteService = new PacienteService(
     _pacienteRepositoryMock.Object,
     _emailServiceMock.Object);

            // ✅ Agregar Mock de RagService (requerido por el nuevo constructor)
            var ragServiceMock = new Mock<RagService>(Mock.Of<HttpClient>());

            _controller = new PacienteController(_pacienteService, _loggerMock.Object, ragServiceMock.Object);

            // Configurar HttpContext y TempData
         var httpContext = new DefaultHttpContext();
            httpContext.Session = new Mock<ISession>().Object;
            httpContext.Request.Scheme = "https";
       httpContext.Request.Host = new HostString("localhost");
    
 var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
            _controller.ControllerContext = new ControllerContext
       {
  HttpContext = httpContext
            };
        }

        #region Registro GET

        [Fact]
        public void RegistroGet_DeberiaRetornarView()
        {
            var resultado = _controller.Registro();
            Assert.IsType<ViewResult>(resultado);
        }

        #endregion

      #region Registro POST

        [Fact]
      public async Task RegistroPost_ConDatosValidos_DeberiaRedirigirAVerificarOTP()
     {
      var paciente = new Paciente
  {
  CorreoElectronico = "test@example.com",
      NombreCompleto = "Test User",
       NumeroDocumento = "12345678",
      Telefono = "3001234567",
     TipoDocumento = "CC"
   };

    _pacienteRepositoryMock
   .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
           .ReturnsAsync((Paciente?)null);

         _pacienteRepositoryMock
          .Setup(r => r.ObtenerPorDocumentoAsync(It.IsAny<string>()))
   .ReturnsAsync((Paciente?)null);

    _pacienteRepositoryMock
        .Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);

      _emailServiceMock
   .Setup(e => e.EnviarOTPAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

         var resultado = await _controller.Registro(paciente, "Password123!", "Password123!");

       var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
    Assert.Equal("VerificarOTP", redirectResult.ActionName);
       Assert.Equal("test@example.com", _controller.TempData["CorreoRegistrado"]);
        }

        #endregion

        #region Login

        [Fact]
public void LoginGet_DeberiaRetornarView()
        {
          var resultado = _controller.Login();
        Assert.IsType<ViewResult>(resultado);
  }

      [Fact]
        public async Task LoginPost_ConCredencialesValidas_DeberiaRedirigirAHome()
   {
  var paciente = new Paciente
{
   Id = Guid.NewGuid(),
  NombreCompleto = "Test User",
      CorreoElectronico = "test@example.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
        EstaVerificado = true
   };

       _pacienteRepositoryMock
       .Setup(r => r.ObtenerPorCorreoAsync("test@example.com"))
  .ReturnsAsync(paciente);

          var resultado = await _controller.Login("test@example.com", "Password123!");

 var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
     Assert.Equal("Dashboard", redirectResult.ActionName);
   Assert.Null(redirectResult.ControllerName); // Dashboard está en el mismo controlador (Paciente)
        }

        #endregion

        #region VerificarOTP

        [Fact]
        public void VerificarOTPGet_DeberiaRetornarView()
        {
        var resultado = _controller.VerificarOTP();
 Assert.IsType<ViewResult>(resultado);
        }

        #endregion

        #region RecuperarPassword

        [Fact]
        public void RecuperarPasswordGet_DeberiaRetornarView()
        {
            var resultado = _controller.RecuperarPassword();
       Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public async Task RecuperarPasswordPost_ConCorreoVacio_DeberiaRetornarViewConError()
        {
       var resultado = await _controller.RecuperarPassword("");

       var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.NotNull(_controller.ViewBag.ErrorMessage);
  }

     [Fact]
        public async Task RecuperarPasswordPost_ConCorreoValido_DeberiaEnviarCorreo()
     {
            var paciente = new Paciente
    {
                CorreoElectronico = "test@example.com",
   NombreCompleto = "Test User",
        EstaVerificado = true
            };

            _pacienteRepositoryMock
                .Setup(r => r.ObtenerPorCorreoAsync("test@example.com"))
    .ReturnsAsync(paciente);

     _pacienteRepositoryMock
   .Setup(r => r.ActualizarTokenRecuperacionAsync(It.IsAny<Paciente>()))
    .Returns(Task.CompletedTask);

  _emailServiceMock
       .Setup(e => e.EnviarCorreoRecuperacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      var resultado = await _controller.RecuperarPassword("test@example.com");

  var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.NotNull(_controller.ViewBag.SuccessMessage);
     }

        #endregion

        #region RestablecerPassword

        [Fact]
        public void RestablecerPasswordGet_ConTokenVacio_DeberiaRedirigirALogin()
        {
            var resultado = _controller.RestablecerPassword("");

    var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
    Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
 public void RestablecerPasswordGet_ConTokenValido_DeberiaRetornarView()
        {
  var resultado = _controller.RestablecerPassword("token-valido");

            var viewResult = Assert.IsType<ViewResult>(resultado);
    Assert.Equal("token-valido", _controller.ViewBag.Token);
        }

 [Fact]
        public async Task RestablecerPasswordPost_ConTokenVacio_DeberiaRedirigirALogin()
        {
    var resultado = await _controller.RestablecerPassword("", "NewPass123!", "NewPass123!");

   var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
     Assert.Equal("Login", redirectResult.ActionName);
  }

        [Fact]
        public async Task RestablecerPasswordPost_Exitoso_DeberiaRedirigirALogin()
        {
      var paciente = new Paciente
            {
      CorreoElectronico = "test@example.com",
             TokenRecuperacion = "token-valido",
        TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(15)
      };

            _pacienteRepositoryMock
    .Setup(r => r.ObtenerPorTokenRecuperacionAsync("token-valido"))
       .ReturnsAsync(paciente);

            _pacienteRepositoryMock
 .Setup(r => r.ActualizarPasswordAsync(It.IsAny<Paciente>()))
 .Returns(Task.CompletedTask);

       var resultado = await _controller.RestablecerPassword("token-valido", "NewPass123!", "NewPass123!");

            var redirectResult = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Login", redirectResult.ActionName);
     Assert.NotNull(_controller.TempData["MensajeExito"]);
        }

        #endregion

        #region Registro POST - Tests Adicionales

      [Fact]
        public async Task RegistroPost_ConModelStateInvalido_DeberiaRetornarView()
        {
       // Arrange
var paciente = new Paciente();
  _controller.ModelState.AddModelError("NombreCompleto", "Required");

            // Act
            var resultado = await _controller.Registro(paciente, "Password123!", "Password123!");

  // Assert
 var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal(paciente, viewResult.Model);
        }

        [Fact]
        public async Task RegistroPost_ConDbUpdateException_DeberiaRetornarViewConError()
        {
         // Arrange
        var paciente = new Paciente
  {
      CorreoElectronico = "test@example.com",
            NombreCompleto = "Test User",
         NumeroDocumento = "12345678",
       Telefono = "3001234567",
  TipoDocumento = "CC"
         };

  _pacienteRepositoryMock
       .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
                .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("DB Error"));

// Act
            var resultado = await _controller.Registro(paciente, "Password123!", "Password123!");

       // Assert
     var viewResult = Assert.IsType<ViewResult>(resultado);
   Assert.NotNull(_controller.ViewBag.Error);
            Assert.Contains("Error de BD", _controller.ViewBag.Error.ToString());
}

        [Fact]
  public async Task RegistroPost_ConExcepcionConInnerException_DeberiaIncluirInnerEnError()
        {
            // Arrange
   var paciente = new Paciente
        {
                CorreoElectronico = "test@example.com",
         NombreCompleto = "Test User",
         NumeroDocumento = "12345678",
     Telefono = "3001234567",
    TipoDocumento = "CC"
         };

        var innerEx = new Exception("Inner error");
         var outerEx = new Exception("Outer error", innerEx);

       _pacienteRepositoryMock
         .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
       .ThrowsAsync(outerEx);

      // Act
            var resultado = await _controller.Registro(paciente, "Password123!", "Password123!");

    // Assert
    var viewResult = Assert.IsType<ViewResult>(resultado);
      Assert.NotNull(_controller.ViewBag.Error);
       Assert.Contains("Inner", _controller.ViewBag.Error.ToString());
      }

        #endregion

        #region VerificarOTP - Tests Adicionales

        [Fact]
    public async Task VerificarOTPPost_ConExcepcion_DeberiaRetornarViewConError()
        {
            // Arrange
            _pacienteRepositoryMock
  .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
    .ThrowsAsync(new Exception("Error de test"));

       // Act
   var resultado = await _controller.VerificarOTP("test@example.com", "123456");

     // Assert
     var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal("Error de test", _controller.ViewBag.Error);
  }

        [Fact]
        public async Task VerificarOTPPost_ConOTPInvalido_DeberiaRetornarViewConError()
        {
            // Arrange
     _pacienteRepositoryMock
     .Setup(r => r.VerificarOTPAsync("test@example.com", "000000"))
                .ReturnsAsync(false);

          _pacienteRepositoryMock
    .Setup(r => r.ObtenerPorCorreoAsync("test@example.com"))
                .ReturnsAsync(new Paciente { CodigoOTP = "123456" });

    // Act
  var resultado = await _controller.VerificarOTP("test@example.com", "000000");

            // Assert
        var viewResult = Assert.IsType<ViewResult>(resultado);
    Assert.Contains("inválido", _controller.ViewBag.Error.ToString());
        }

        #endregion

      #region ReenviarOTP - Tests Adicionales

        [Fact]
        public async Task ReenviarOTP_Exitoso_DeberiaRetornarViewConMensaje()
      {
     // Arrange
            var paciente = new Paciente
 {
            CorreoElectronico = "test@example.com",
 NombreCompleto = "Test User",
     EstaVerificado = false
 };

          _pacienteRepositoryMock
 .Setup(r => r.ObtenerPorCorreoAsync("test@example.com"))
         .ReturnsAsync(paciente);

 _pacienteRepositoryMock
  .Setup(r => r.ActualizarOTPAsync(It.IsAny<Paciente>()))
     .Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(e => e.EnviarOTPAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

// Act
            var resultado = await _controller.ReenviarOTP("test@example.com");

            // Assert
   var viewResult = Assert.IsType<ViewResult>(resultado);
         Assert.Equal("VerificarOTP", viewResult.ViewName);
       Assert.Contains("reenviado", _controller.ViewBag.Mensaje.ToString());
        }

        #endregion

        #region Login - Tests Adicionales

   [Fact]
     public async Task LoginPost_ConExcepcion_DeberiaRetornarViewConError()
        {
            // Arrange
    _pacienteRepositoryMock
.Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
      .ThrowsAsync(new Exception("Error de prueba"));

     // Act
      var resultado = await _controller.Login("test@example.com", "password");

 // Assert
   var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.Equal("Error de prueba", _controller.ViewBag.Error);
        }

  #endregion

        #region VerificarCuenta - Tests Adicionales

        [Fact]
        public async Task VerificarCuenta_ConTokenValido_DeberiaRetornarViewConExito()
        {
  // Arrange
   _pacienteRepositoryMock
      .Setup(r => r.ActivarCuentaAsync("token-valido"))
      .ReturnsAsync(true);

          // Act
            var resultado = await _controller.VerificarCuenta("token-valido");

            // Assert
      var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.Contains("activada correctamente", _controller.ViewBag.Resultado.ToString());
        }

        [Fact]
        public async Task VerificarCuenta_ConTokenInvalido_DeberiaRetornarViewConError()
    {
     // Arrange
            _pacienteRepositoryMock
  .Setup(r => r.ActivarCuentaAsync("token-invalido"))
           .ReturnsAsync(false);

         // Act
    var resultado = await _controller.VerificarCuenta("token-invalido");

     // Assert
   var viewResult = Assert.IsType<ViewResult>(resultado);
 Assert.Contains("inválido", _controller.ViewBag.Resultado.ToString());
        }

        #endregion

    #region RecuperarPassword - Tests Adicionales

        [Fact]
        public async Task RecuperarPasswordPost_ConInvalidOperationException_DeberiaRetornarViewConError()
        {
            // Arrange
         _pacienteRepositoryMock
            .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
      .ThrowsAsync(new InvalidOperationException("Usuario no verificado"));

            // Act
            var resultado = await _controller.RecuperarPassword("test@example.com");

            // Assert
       var viewResult = Assert.IsType<ViewResult>(resultado);
   Assert.Equal("Usuario no verificado", _controller.ViewBag.ErrorMessage);
        }

        [Fact]
        public async Task RecuperarPasswordPost_ConExcepcionGenerica_DeberiaRetornarViewConError()
        {
 // Arrange
            _pacienteRepositoryMock
  .Setup(r => r.ObtenerPorCorreoAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error inesperado"));

   // Act
          var resultado = await _controller.RecuperarPassword("test@example.com");

       // Assert
   var viewResult = Assert.IsType<ViewResult>(resultado);
            Assert.NotNull(_controller.ViewBag.ErrorMessage);
        }

        #endregion

        #region RestablecerPassword - Tests Adicionales

        [Fact]
 public async Task RestablecerPasswordPost_ConArgumentException_DeberiaRetornarViewConError()
        {
         // Arrange
  var paciente = new Paciente
    {
      TokenRecuperacion = "token",
       TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(15)
     };

      _pacienteRepositoryMock
 .Setup(r => r.ObtenerPorTokenRecuperacionAsync("token"))
     .ReturnsAsync(paciente);

     // Act (contraseñas no coinciden)
 var resultado = await _controller.RestablecerPassword("token", "Pass1!", "Pass2!");

 // Assert
  var viewResult = Assert.IsType<ViewResult>(resultado);
   Assert.NotNull(_controller.ViewBag.Error);
        }

      [Fact]
   public async Task RestablecerPasswordPost_ConInvalidOperationException_DeberiaRetornarViewConError()
      {
  // Arrange
  _pacienteRepositoryMock
       .Setup(r => r.ObtenerPorTokenRecuperacionAsync("token"))
     .ReturnsAsync((Paciente?)null);

    // Act
    var resultado = await _controller.RestablecerPassword("token", "Pass123!", "Pass123!");

// Assert
 var viewResult = Assert.IsType<ViewResult>(resultado);
        Assert.NotNull(_controller.ViewBag.Error);
     }

      [Fact]
        public async Task RestablecerPasswordPost_ConExcepcionGenerica_DeberiaRetornarViewConError()
    {
   // Arrange
     _pacienteRepositoryMock
 .Setup(r => r.ObtenerPorTokenRecuperacionAsync(It.IsAny<string>()))
 .ThrowsAsync(new Exception("Error inesperado"));

     // Act
 var resultado = await _controller.RestablecerPassword("token", "Pass123!", "Pass123!");

          // Assert
   var viewResult = Assert.IsType<ViewResult>(resultado);
     Assert.NotNull(_controller.ViewBag.Error);
   }

     #endregion
  }
}
