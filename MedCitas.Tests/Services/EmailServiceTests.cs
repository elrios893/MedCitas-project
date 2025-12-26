using MedCitas.Core.Configuration;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace MedCitas.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IOptions<EmailConfiguration>> _configMock;
    private EmailConfiguration _validConfig;

        public EmailServiceTests()
        {
     _loggerMock = new Mock<ILogger<EmailService>>();
            _configMock = new Mock<IOptions<EmailConfiguration>>();
          
   _validConfig = new EmailConfiguration
        {
      SmtpHost = "smtp.test.com",
   SmtpPort = 587,
    SmtpUser = "test@test.com",
     SmtpPassword = "password123",
          FromEmail = "noreply@medcitas.com",
    FromName = "MedCitas",
  EnableSsl = true
       };
        }

        [Fact]
    public void Constructor_ConConfiguracionValida_InicializaCorrectamente()
        {
       // Arrange
  _configMock.Setup(x => x.Value).Returns(_validConfig);

// Act
      var service = new EmailService(_loggerMock.Object, _configMock.Object);

      // Assert
            Assert.NotNull(service);
            _loggerMock.Verify(
  x => x.Log(
          LogLevel.Information,
      It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("EmailService inicializado correctamente")),
    It.IsAny<Exception>(),
It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
          Times.Once);
        }

        [Fact]
        public void Constructor_ConConfiguracionInvalida_LogueaAdvertencia()
  {
    // Arrange
            var invalidConfig = new EmailConfiguration(); // Configuración vacía
     _configMock.Setup(x => x.Value).Returns(invalidConfig);

       // Act
            var service = new EmailService(_loggerMock.Object, _configMock.Object);

       // Assert
Assert.NotNull(service);
    _loggerMock.Verify(
  x => x.Log(
        LogLevel.Warning,
         It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Configuración de Email incompleta")),
It.IsAny<Exception>(),
     It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task EnviarCorreoVerificacionAsync_ConConfiguracionInvalida_NoEnviaEmail()
   {
          // Arrange
    var invalidConfig = new EmailConfiguration();
     _configMock.Setup(x => x.Value).Returns(invalidConfig);
          var service = new EmailService(_loggerMock.Object, _configMock.Object);

  // Act & Assert
   await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await service.EnviarCorreoVerificacionAsync("test@test.com", "token123"));
        }

  [Fact]
        public async Task EnviarOTPAsync_ConConfiguracionInvalida_NoEnviaEmail()
   {
    // Arrange
     var invalidConfig = new EmailConfiguration();
            _configMock.Setup(x => x.Value).Returns(invalidConfig);
            var service = new EmailService(_loggerMock.Object, _configMock.Object);

        // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
       async () => await service.EnviarOTPAsync("test@test.com", "123456", "Juan Perez"));
      }

  [Fact]
 public async Task EnviarCorreoRecuperacionAsync_ConConfiguracionInvalida_NoEnviaEmail()
  {
            // Arrange
            var invalidConfig = new EmailConfiguration();
            _configMock.Setup(x => x.Value).Returns(invalidConfig);
            var service = new EmailService(_loggerMock.Object, _configMock.Object);

        // Act & Assert
 await Assert.ThrowsAsync<InvalidOperationException>(
     async () => await service.EnviarCorreoRecuperacionAsync("test@test.com", "Juan Perez", "https://medcitas.com/reset"));
        }

        [Fact]
        public async Task EnviarCorreoVerificacionAsync_ConDestinatarioValido_LogueaInformacion()
        {
            // Arrange
            var invalidConfig = new EmailConfiguration(); // Usamos config inválida para probar logging
            _configMock.Setup(x => x.Value).Returns(invalidConfig);
var service = new EmailService(_loggerMock.Object, _configMock.Object);

// Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(
    async () => await service.EnviarCorreoVerificacionAsync("test@test.com", "token123"));

 // Verificar que se logueó el intento de envío
      _loggerMock.Verify(
 x => x.Log(
    LogLevel.Information,
  It.IsAny<EventId>(),
     It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enviando email")), // ? Cambiado
  It.IsAny<Exception>(),
         It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
 Times.Once);

  // Verificar que se logueó el error
            _loggerMock.Verify(
     x => x.Log(
       LogLevel.Error,
       It.IsAny<EventId>(),
       It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Configuración de email inválida")),
   It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
      }

        [Fact]
      public async Task EnviarOTPAsync_ConParametrosValidos_LogueaInformacion()
        {
      // Arrange
            var invalidConfig = new EmailConfiguration();
        _configMock.Setup(x => x.Value).Returns(invalidConfig);
   var service = new EmailService(_loggerMock.Object, _configMock.Object);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
       async () => await service.EnviarOTPAsync("test@test.com", "123456", "Juan Perez"));

      // Verificar logging
      _loggerMock.Verify(
      x => x.Log(
 LogLevel.Information,
      It.IsAny<EventId>(),
It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enviando email")), // ? Cambiado
      It.IsAny<Exception>(),
 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
     Times.Once);
        }

        [Fact]
     public async Task EnviarCorreoRecuperacionAsync_ConParametrosValidos_LogueaInformacion()
        {
   // Arrange
      var invalidConfig = new EmailConfiguration();
    _configMock.Setup(x => x.Value).Returns(invalidConfig);
  var service = new EmailService(_loggerMock.Object, _configMock.Object);

    // Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(
     async () => await service.EnviarCorreoRecuperacionAsync("test@test.com", "Juan Perez", "https://medcitas.com/reset"));

        // Verificar logging
    _loggerMock.Verify(
          x => x.Log(
     LogLevel.Information,
         It.IsAny<EventId>(),
       It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enviando email")), // ? Cambiado
            It.IsAny<Exception>(),
         It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
  Times.Once);
    }
    }
}
