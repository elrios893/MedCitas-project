using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MedCitas.Core.DTOs;
using MedCitas.Core.Entities;
using MedCitas.Core.Services;
using MedCitas.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedCitas.Tests.Controllers
{
    public class CitaControllerTests
    {
        private readonly Mock<AppointmentService> _serviceMock;
        private readonly Mock<ILogger<CitaController>> _loggerMock;
        private readonly CitaController _controller;

        public CitaControllerTests()
        {
            _serviceMock = new Mock<AppointmentService>(
        Mock.Of<Core.Interfaces.IAppointmentRepository>(),
        Mock.Of<Core.Interfaces.IPacienteRepository>(),
        Mock.Of<Core.Interfaces.IDoctorRepository>(),
        Mock.Of<Core.Interfaces.IEmailService>());
            _loggerMock = new Mock<ILogger<CitaController>>();

            _controller = new CitaController(
        _serviceMock.Object,
        Mock.Of<Core.Interfaces.ISpecialtyRepository>(),
        Mock.Of<Core.Interfaces.IDoctorRepository>(),
        _loggerMock.Object,
        Mock.Of<Core.Interfaces.IAppointmentRepository>());

            // Configurar sesión simulada
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            httpContext.Session.SetString("PacienteId", Guid.NewGuid().ToString());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Configurar TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
    httpContext,
             Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        #region MisCitas Tests

        [Fact]
        public async Task MisCitas_PacienteAutenticado_RetornaVista()
        {
            // Arrange
            var citas = new List<CitaDto>
{
         new()
             {
     Id = Guid.NewGuid(),
 Especialidad = "Test",
           Medico = "Dr. Test",
        FechaCita = DateTime.Now.AddDays(1),
          Estado = "Agendada"
            }
  };

            _serviceMock.Setup(s => s.ObtenerCitasPacienteAsync(
             It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(citas);

            // Act
            var result = await _controller.MisCitas();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<CitaDto>>(viewResult.Model);
            model.Should().HaveCount(1);
        }

        [Fact]
        public async Task MisCitas_SinAutenticacion_RedirigeALogin()
        {
            // Arrange
            _controller.HttpContext.Session.Clear();

            // Act
            var result = await _controller.MisCitas();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Login");
            redirectResult.ControllerName.Should().Be("Paciente");
        }

        [Fact]
        public async Task MisCitas_ConFiltros_PasaParametros()
        {
            // Arrange
            var estado = "Agendada";
            var desde = DateTime.Now.AddDays(-7);
            var hasta = DateTime.Now.AddDays(7);

            _serviceMock.Setup(s => s.ObtenerCitasPacienteAsync(
              It.IsAny<Guid>(), estado, desde, hasta))
                    .ReturnsAsync(new List<CitaDto>());

            // Act
            await _controller.MisCitas(estado, desde, hasta);

            // Assert
            _serviceMock.Verify(s => s.ObtenerCitasPacienteAsync(
              It.IsAny<Guid>(), estado, desde, hasta), Times.Once);
        }

        #endregion

        #region Detalle Tests

        [Fact]
        public async Task Detalle_CitaExistente_RetornaVista()
        {
            // Arrange
            var citaId = Guid.NewGuid();
            var cita = new CitaDto
            {
                Id = citaId,
                Especialidad = "Test",
                Medico = "Dr. Test"
            };

            _serviceMock.Setup(s => s.ObtenerDetalleCitaAsync(citaId, It.IsAny<Guid>()))
 .ReturnsAsync(cita);

            // Act
            var result = await _controller.Detalle(citaId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CitaDto>(viewResult.Model);
            model.Id.Should().Be(citaId);
        }

        [Fact]
        public async Task Detalle_CitaNoExistente_RedirigeConError()
        {
            // Arrange
            var citaId = Guid.NewGuid();
            _serviceMock.Setup(s => s.ObtenerDetalleCitaAsync(citaId, It.IsAny<Guid>()))
    .ReturnsAsync((CitaDto?)null);

            // Act
            var result = await _controller.Detalle(citaId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("MisCitas");
            _controller.TempData["Error"].Should().NotBeNull();
        }

        #endregion

        #region Agendar Tests

        [Fact]
        public void Agendar_Get_PacienteAutenticado_RetornaVista()
        {
            // Act
            var result = _controller.Agendar();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Agendar_Get_SinAutenticacion_RedirigeALogin()
        {
            // Arrange
            _controller.HttpContext.Session.Clear();

            // Act
            var result = _controller.Agendar();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Login");
        }

        [Fact]
        public async Task Agendar_Post_ConDatosValidos_RedirigeADetalle()
        {
            // Arrange
            var dto = new AgendarCitaDto
            {
                DoctorId = Guid.NewGuid(),
                FechaCita = DateTime.Now.AddDays(2),
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(10, 30, 0),
                Modalidad = "Presencial"
            };

            var cita = new Appointment
            {
                Id = Guid.NewGuid(),
                Estado = "Agendada"
            };

            _serviceMock.Setup(s => s.AgendarCitaAsync(dto, It.IsAny<Guid>()))
      .ReturnsAsync(cita);

            // Act
            var result = await _controller.Agendar(dto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Detalle");
            _controller.TempData["MensajeExito"].Should().NotBeNull();
        }

        [Fact]
        public async Task Agendar_Post_ConErrorDeNegocio_MuestraError()
        {
            // Arrange
            var dto = new AgendarCitaDto();
            _serviceMock.Setup(s => s.AgendarCitaAsync(dto, It.IsAny<Guid>()))
                        .ThrowsAsync(new InvalidOperationException("Horario no disponible"));

            // Act
            var result = await _controller.Agendar(dto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            ((string)_controller.ViewBag.Error).Should().NotBeNull();
        }

        [Fact]
        public async Task Agendar_Post_ConModelStateInvalido_RetornaVista()
        {
            // Arrange
            var dto = new AgendarCitaDto();
            _controller.ModelState.AddModelError("DoctorId", "Required");

            // Act
            var result = await _controller.Agendar(dto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            ((string)_controller.ViewBag.Error).Should().NotBeNull();
        }

        #endregion

        #region Cancelar Tests

        [Fact]
        public async Task Cancelar_CitaValida_RedirigeConExito()
        {
            // Arrange
            var citaId = Guid.NewGuid();
            _serviceMock.Setup(s => s.CancelarCitaAsync(citaId, It.IsAny<Guid>(), It.IsAny<string>()))
     .ReturnsAsync(true);

            // Act
            var result = await _controller.Cancelar(citaId, "Motivo test");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("MisCitas");
            _controller.TempData["MensajeExito"].Should().NotBeNull();
        }

        [Fact]
        public async Task Cancelar_CITAMenosDe24Horas_RedirigeConError()
        {
            // Arrange
            var citaId = Guid.NewGuid();
            _serviceMock.Setup(s => s.CancelarCitaAsync(citaId, It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("No se puede cancelar con menos de 24 horas"));

            // Act
            var result = await _controller.Cancelar(citaId, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Detalle");
            _controller.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public async Task Cancelar_CitaDeOtroPaciente_RedirigeConError()
        {
            // Arrange
            var citaId = Guid.NewGuid();
            _serviceMock.Setup(s => s.CancelarCitaAsync(citaId, It.IsAny<Guid>(), It.IsAny<string>()))
                         .ThrowsAsync(new UnauthorizedAccessException("No autorizado"));

            // Act
            var result = await _controller.Cancelar(citaId, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("MisCitas");
            _controller.TempData["Error"].Should().NotBeNull();
        }

        #endregion
    }

    // Mock de ISession para tests
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[]? value)
        {
            if (_sessionStorage.TryGetValue(key, out var storedValue))
            {
                value = storedValue;
                return true;
            }

            value = null;
            return false;
        }
    }
}
