using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MedCitas.Core.DTOs;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using Moq;
using Xunit;

namespace MedCitas.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
private readonly Mock<IPacienteRepository> _pacienteRepoMock;
        private readonly Mock<IDoctorRepository> _doctorRepoMock;
     private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _appointmentRepoMock = new Mock<IAppointmentRepository>();
          _pacienteRepoMock = new Mock<IPacienteRepository>();
    _doctorRepoMock = new Mock<IDoctorRepository>();
      _emailServiceMock = new Mock<IEmailService>();

       _service = new AppointmentService(
    _appointmentRepoMock.Object,
    _pacienteRepoMock.Object,
    _doctorRepoMock.Object,
       _emailServiceMock.Object);
        }

    #region AgendarCitaAsync Tests

      [Fact]
public async Task AgendarCita_ConDatosValidos_CreaExitosamente()
        {
  // Arrange
            var pacienteId = Guid.NewGuid();
    var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();
            
         var paciente = new Paciente
            {
     Id = pacienteId,
                NombreCompleto = "Juan Pérez",
      CorreoElectronico = "juan@test.com"
 };

 var doctor = new Doctor
     {
      Id = doctorId,
                NombreCompleto = "Dr. Carlos Rodríguez",
                SpecialtyId = specialtyId,
    EstaActivo = true,
          Specialty = new Specialty { Id = specialtyId, Nombre = "Medicina General" }
         };

  var dto = new AgendarCitaDto
  {
           DoctorId = doctorId,
          FechaCita = DateTime.Now.AddDays(2),
           HoraInicio = new TimeSpan(10, 0, 0),
     HoraFin = new TimeSpan(10, 30, 0),
          Modalidad = "Presencial",
            MotivoConsulta = "Control general"
    };

    _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(pacienteId))
                .ReturnsAsync(paciente);
            _doctorRepoMock.Setup(r => r.ObtenerPorIdAsync(doctorId))
           .ReturnsAsync(doctor);
          _appointmentRepoMock.Setup(r => r.ValidarDisponibilidadAsync(
 It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
          .ReturnsAsync(true);
            _appointmentRepoMock.Setup(r => r.PacienteTieneCitaEnHorarioAsync(
    It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
          .ReturnsAsync(false);
     _appointmentRepoMock.Setup(r => r.CrearAsync(It.IsAny<Appointment>()))
       .Returns(Task.CompletedTask);
     _emailServiceMock.Setup(e => e.EnviarConfirmacionCitaAsync(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
          It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
     .Returns(Task.CompletedTask);

         // Act
         var resultado = await _service.AgendarCitaAsync(dto, pacienteId);

            // Assert
   resultado.Should().NotBeNull();
      resultado.PacienteId.Should().Be(pacienteId);
            resultado.DoctorId.Should().Be(doctorId);
        resultado.Estado.Should().Be("Agendada");
          _appointmentRepoMock.Verify(r => r.CrearAsync(It.IsAny<Appointment>()), Times.Once);
_emailServiceMock.Verify(e => e.EnviarConfirmacionCitaAsync(
  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
      It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()), Times.Once);
        }

    [Fact]
     public async Task AgendarCita_ConPacienteNoExistente_LanzaExcepcion()
        {
       // Arrange
    var pacienteId = Guid.NewGuid();
          var dto = new AgendarCitaDto { DoctorId = Guid.NewGuid() };

         _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(pacienteId))
  .ReturnsAsync((Paciente?)null);

   // Act & Assert
       await Assert.ThrowsAsync<InvalidOperationException>(
          async () => await _service.AgendarCitaAsync(dto, pacienteId));
     }

        [Fact]
        public async Task AgendarCita_ConMedicoNoDisponible_LanzaExcepcion()
      {
     // Arrange
    var pacienteId = Guid.NewGuid();
     var doctorId = Guid.NewGuid();
     var dto = new AgendarCitaDto { DoctorId = doctorId };

            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(pacienteId))
                .ReturnsAsync(new Paciente { Id = pacienteId });
    _doctorRepoMock.Setup(r => r.ObtenerPorIdAsync(doctorId))
                .ReturnsAsync((Doctor?)null);

  // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.AgendarCitaAsync(dto, pacienteId));
        }

        [Fact]
        public async Task AgendarCita_ConHorarioNoDisponible_LanzaExcepcion()
        {
        // Arrange
       var pacienteId = Guid.NewGuid();
     var doctorId = Guid.NewGuid();
            var dto = new AgendarCitaDto
            {
     DoctorId = doctorId,
         FechaCita = DateTime.Now.AddDays(2),
         HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(10, 30, 0)
            };

     _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(pacienteId))
   .ReturnsAsync(new Paciente { Id = pacienteId });
_doctorRepoMock.Setup(r => r.ObtenerPorIdAsync(doctorId))
         .ReturnsAsync(new Doctor { Id = doctorId, EstaActivo = true, Specialty = new Specialty() });
_appointmentRepoMock.Setup(r => r.ValidarDisponibilidadAsync(
      It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
       .ReturnsAsync(false);

      // Act & Assert
   var ex = await Assert.ThrowsAsync<InvalidOperationException>(
       async () => await _service.AgendarCitaAsync(dto, pacienteId));
            ex.Message.Should().Contain("horario");
        }

        [Fact]
        public async Task AgendarCita_ConCitaSimultanea_LanzaExcepcion()
   {
        // Arrange
   var pacienteId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
      var dto = new AgendarCitaDto
            {
      DoctorId = doctorId,
            FechaCita = DateTime.Now.AddDays(2),
HoraInicio = new TimeSpan(10, 0, 0),
      HoraFin = new TimeSpan(10, 30, 0)
   };

       _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(pacienteId))
     .ReturnsAsync(new Paciente { Id = pacienteId });
   _doctorRepoMock.Setup(r => r.ObtenerPorIdAsync(doctorId))
     .ReturnsAsync(new Doctor { Id = doctorId, EstaActivo = true, Specialty = new Specialty() });
  _appointmentRepoMock.Setup(r => r.ValidarDisponibilidadAsync(
      It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
        .ReturnsAsync(true);
     _appointmentRepoMock.Setup(r => r.PacienteTieneCitaEnHorarioAsync(
          It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
     .ReturnsAsync(true);

     // Act & Assert
      var ex = await Assert.ThrowsAsync<InvalidOperationException>(
       async () => await _service.AgendarCitaAsync(dto, pacienteId));
     ex.Message.Should().Contain("Ya tienes");
        }

    #endregion

        #region CancelarCitaAsync Tests

        [Fact]
        public async Task CancelarCita_Con25HorasAnticipacion_CancelaExitosamente()
   {
            // Arrange
  var citaId = Guid.NewGuid();
          var pacienteId = Guid.NewGuid();
       var fecha = DateTime.Now.AddDays(2);
            
            var cita = new Appointment
  {
       Id = citaId,
        PacienteId = pacienteId,
         DoctorId = Guid.NewGuid(),
      SpecialtyId = Guid.NewGuid(),
        FechaCita = fecha.Date,
                HoraInicio = new TimeSpan(10, 0, 0),
  HoraFin = new TimeSpan(10, 30, 0),
            Estado = "Agendada",
     Paciente = new Paciente { CorreoElectronico = "test@test.com", NombreCompleto = "Test" },
  Doctor = new Doctor { NombreCompleto = "Dr. Test" },
                Specialty = new Specialty { Nombre = "Test" }
      };

            _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
                .ReturnsAsync(cita);
 _appointmentRepoMock.Setup(r => r.ActualizarAsync(It.IsAny<Appointment>()))
        .Returns(Task.CompletedTask);
     _emailServiceMock.Setup(e => e.EnviarNotificacionCancelacionAsync(
  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
       It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
     .Returns(Task.CompletedTask);

            // Act
var resultado = await _service.CancelarCitaAsync(citaId, pacienteId);

      // Assert
            resultado.Should().BeTrue();
            _appointmentRepoMock.Verify(r => r.ActualizarAsync(It.Is<Appointment>(a => a.Estado == "Cancelada")), Times.Once);
        _emailServiceMock.Verify(e => e.EnviarNotificacionCancelacionAsync(
    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
     It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
     public async Task CancelarCita_Con23HorasAnticipacion_LanzaExcepcion()
{
    // Arrange
   var citaId = Guid.NewGuid();
      var pacienteId = Guid.NewGuid();
var fecha = DateTime.Now.AddHours(23);
       
     var cita = new Appointment
 {
       Id = citaId,
     PacienteId = pacienteId,
                FechaCita = fecha.Date,
              HoraInicio = fecha.TimeOfDay,
        HoraFin = fecha.AddMinutes(30).TimeOfDay,
     Estado = "Agendada",
        Paciente = new Paciente(),
         Doctor = new Doctor(),
          Specialty = new Specialty()
 };

            _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
      .ReturnsAsync(cita);

       // Act & Assert
       var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CancelarCitaAsync(citaId, pacienteId));
     ex.Message.Should().Contain("24 horas");
  }

        [Fact]
        public async Task CancelarCita_YaCancelada_LanzaExcepcion()
        {
            // Arrange
     var citaId = Guid.NewGuid();
         var pacienteId = Guid.NewGuid();
            
        var cita = new Appointment
       {
        Id = citaId,
   PacienteId = pacienteId,
 Estado = "Cancelada",
                FechaCita = DateTime.Now.AddDays(2),
            HoraInicio = new TimeSpan(10, 0, 0)
            };

      _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
            .ReturnsAsync(cita);

       // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
     async () => await _service.CancelarCitaAsync(citaId, pacienteId));
        }

        [Fact]
        public async Task CancelarCita_DeOtroPaciente_LanzaUnauthorized()
  {
          // Arrange
        var citaId = Guid.NewGuid();
         var pacienteId = Guid.NewGuid();
            var otroPacienteId = Guid.NewGuid();
        
   var cita = new Appointment
            {
       Id = citaId,
                PacienteId = otroPacienteId,
     Estado = "Agendada",
       FechaCita = DateTime.Now.AddDays(2),
      HoraInicio = new TimeSpan(10, 0, 0)
         };

            _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
                .ReturnsAsync(cita);

    // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
     async () => await _service.CancelarCitaAsync(citaId, pacienteId));
        }

        #endregion

        #region ObtenerCitasPacienteAsync Tests

     [Fact]
     public async Task ObtenerCitasPaciente_RetornaListaOrdenada()
    {
        // Arrange
            var pacienteId = Guid.NewGuid();
            var citas = new List<Appointment>
       {
      new()
    {
     Id = Guid.NewGuid(),
PacienteId = pacienteId,
         FechaCita = DateTime.Now.AddDays(1),
           HoraInicio = new TimeSpan(10, 0, 0),
        HoraFin = new TimeSpan(10, 30, 0),
           Estado = "Agendada",
           Doctor = new Doctor { NombreCompleto = "Dr. Test" },
     Specialty = new Specialty { Nombre = "Test" }
  }
          };

            _appointmentRepoMock.Setup(r => r.ObtenerPorPacienteAsync(
           pacienteId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
       .ReturnsAsync(citas);

   // Act
      var resultado = await _service.ObtenerCitasPacienteAsync(pacienteId);

            // Assert
            resultado.Should().NotBeEmpty();
       resultado.Should().HaveCount(1);
     }

#endregion

        #region ObtenerDetalleCitaAsync Tests

        [Fact]
  public async Task ObtenerDetalleCita_CitaExistente_RetornaDto()
        {
 // Arrange
            var citaId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
            
            var cita = new Appointment
    {
       Id = citaId,
      PacienteId = pacienteId,
       FechaCita = DateTime.Now.AddDays(2),
     HoraInicio = new TimeSpan(10, 0, 0),
    HoraFin = new TimeSpan(10, 30, 0),
     Estado = "Agendada",
      Doctor = new Doctor { NombreCompleto = "Dr. Test" },
          Specialty = new Specialty { Nombre = "Test" }
            };

         _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
    .ReturnsAsync(cita);

            // Act
   var resultado = await _service.ObtenerDetalleCitaAsync(citaId, pacienteId);

     // Assert
 resultado.Should().NotBeNull();
   resultado!.Id.Should().Be(citaId);
      }

        [Fact]
        public async Task ObtenerDetalleCita_CitaDeOtroPaciente_RetornaNull()
        {
      // Arrange
  var citaId = Guid.NewGuid();
            var pacienteId = Guid.NewGuid();
            var otroPacienteId = Guid.NewGuid();
            
 var cita = new Appointment
            {
      Id = citaId,
                PacienteId = otroPacienteId,
                Doctor = new Doctor(),
     Specialty = new Specialty()
            };

          _appointmentRepoMock.Setup(r => r.ObtenerPorIdAsync(citaId))
    .ReturnsAsync(cita);

  // Act
            var resultado = await _service.ObtenerDetalleCitaAsync(citaId, pacienteId);

      // Assert
        resultado.Should().BeNull();
        }

        #endregion
    }
}
