using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.DataDb;
using MedCitas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedCitas.Tests.Repositories
{
    public class EfAppointmentRepositoryTests : IDisposable
    {
        private readonly MedCitasDbContext _context;
      private readonly EfAppointmentRepository _repository;
    private readonly Guid _pacienteId = Guid.NewGuid();
    private readonly Guid _doctorId = Guid.NewGuid();
    private readonly Guid _specialtyId = Guid.NewGuid();

        public EfAppointmentRepositoryTests()
        {
 var options = new DbContextOptionsBuilder<MedCitasDbContext>()
    .UseInMemoryDatabase(databaseName: $"TestDb_Appointment_{Guid.NewGuid()}")
                .Options;

     _context = new MedCitasDbContext(options);
 _repository = new EfAppointmentRepository(_context);

            // Seed data
       SeedTestData();
    }

        private void SeedTestData()
   {
   var paciente = new Paciente
      {
             Id = _pacienteId,
          NombreCompleto = "Test Paciente",
        TipoDocumento = "CC",
  NumeroDocumento = "1234567890",
  FechaNacimiento = DateTime.Now.AddYears(-30),
                Sexo = "M",
Telefono = "3001234567",
   CorreoElectronico = "paciente@test.com",
     PasswordHash = "hash",
   Eps = "Test EPS",
   TipoSangre = "O+",
      EstaVerificado = true,
     FechaRegistro = DateTime.UtcNow
         };

 var specialty = new Specialty
       {
    Id = _specialtyId,
  Nombre = "Cardiología",
     Descripcion = "Especialidad en corazón"
        };

var doctor = new Doctor
    {
        Id = _doctorId,
 NombreCompleto = "Dr. Test",
     NumeroLicencia = "LIC123",
        SpecialtyId = _specialtyId,
       Specialty = specialty,
   Telefono = "3009876543",
     CorreoElectronico = "doctor@test.com",
 EstaActivo = true
  };

         _context.Pacientes.Add(paciente);
_context.Specialties.Add(specialty);
          _context.Doctors.Add(doctor);
     _context.SaveChanges();
        }

        public void Dispose()
        {
         _context.Database.EnsureDeleted();
      _context.Dispose();
        }

  #region Tests de ObtenerPorIdAsync

[Fact]
        public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarCita()
        {
       // Arrange
    var appointment = new Appointment
    {
       Id = Guid.NewGuid(),
            PacienteId = _pacienteId,
       DoctorId = _doctorId,
     SpecialtyId = _specialtyId,
          FechaCita = DateTime.Now.AddDays(5),
          HoraInicio = new TimeSpan(10, 0, 0),
            HoraFin = new TimeSpan(10, 30, 0),
      Modalidad = "Presencial",
            Estado = "Agendada",
       FechaCreacion = DateTime.UtcNow
      };
    _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

 // Act
  var resultado = await _repository.ObtenerPorIdAsync(appointment.Id);

   // Assert
        Assert.NotNull(resultado);
       Assert.Equal(appointment.Id, resultado.Id);
Assert.NotNull(resultado.Paciente);
Assert.NotNull(resultado.Doctor);
            Assert.NotNull(resultado.Specialty);
        }

   [Fact]
        public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaRetornarNull()
        {
// Act
            var resultado = await _repository.ObtenerPorIdAsync(Guid.NewGuid());

     // Assert
 Assert.Null(resultado);
     }

        #endregion

        #region Tests de ObtenerPorPacienteAsync

        [Fact]
  public async Task ObtenerPorPacienteAsync_SinFiltros_DeberiaRetornarTodasLasCitas()
        {
 // Arrange
       await CrearCitasDeTest();

         // Act
 var resultado = await _repository.ObtenerPorPacienteAsync(_pacienteId);

   // Assert
          Assert.Equal(3, resultado.Count);
        }

  [Fact]
     public async Task ObtenerPorPacienteAsync_ConFiltroEstado_DeberiaRetornarCitasFiltradas()
{
 // Arrange
     await CrearCitasDeTest();

   // Act
       var resultado = await _repository.ObtenerPorPacienteAsync(_pacienteId, "Agendada");

            // Assert
   Assert.Equal(2, resultado.Count);
       Assert.All(resultado, c => Assert.Equal("Agendada", c.Estado));
     }

        [Fact]
        public async Task ObtenerPorPacienteAsync_ConFiltroDesde_DeberiaRetornarCitasFiltradas()
 {
        // Arrange
await CrearCitasDeTest();
         var fechaDesde = DateTime.Now.AddDays(3);

// Act
   var resultado = await _repository.ObtenerPorPacienteAsync(_pacienteId, null, fechaDesde);

// Assert
  Assert.Equal(2, resultado.Count);
     Assert.All(resultado, c => Assert.True(c.FechaCita >= fechaDesde));
        }

        [Fact]
        public async Task ObtenerPorPacienteAsync_ConFiltroHasta_DeberiaRetornarCitasFiltradas()
     {
        // Arrange
   await CrearCitasDeTest();
   var fechaHasta = DateTime.Now.AddDays(3);

 // Act
var resultado = await _repository.ObtenerPorPacienteAsync(_pacienteId, null, null, fechaHasta);

            // Assert
            Assert.Single(resultado);
    Assert.All(resultado, c => Assert.True(c.FechaCita <= fechaHasta));
        }

  [Fact]
        public async Task ObtenerPorPacienteAsync_DeberiaOrdenarPorFechaYHora()
    {
// Arrange
     await CrearCitasDeTest();

            // Act
       var resultado = await _repository.ObtenerPorPacienteAsync(_pacienteId);

    // Assert
       Assert.True(resultado[0].FechaCita <= resultado[1].FechaCita);
if (resultado[0].FechaCita == resultado[1].FechaCita)
    {
      Assert.True(resultado[0].HoraInicio <= resultado[1].HoraInicio);
    }
  }

    private async Task CrearCitasDeTest()
  {
  var citas = new List<Appointment>
            {
     new()
    {
  Id = Guid.NewGuid(),
  PacienteId = _pacienteId,
        DoctorId = _doctorId,
        SpecialtyId = _specialtyId,
     FechaCita = DateTime.Now.AddDays(2),
     HoraInicio = new TimeSpan(10, 0, 0),
 HoraFin = new TimeSpan(10, 30, 0),
  Modalidad = "Presencial",
Estado = "Agendada",
   FechaCreacion = DateTime.UtcNow
    },
      new()
                {
       Id = Guid.NewGuid(),
       PacienteId = _pacienteId,
     DoctorId = _doctorId,
  SpecialtyId = _specialtyId,
   FechaCita = DateTime.Now.AddDays(5),
       HoraInicio = new TimeSpan(14, 0, 0),
        HoraFin = new TimeSpan(14, 30, 0),
   Modalidad = "Virtual",
       Estado = "Agendada",
     FechaCreacion = DateTime.UtcNow
     },
 new()
     {
   Id = Guid.NewGuid(),
      PacienteId = _pacienteId,
   DoctorId = _doctorId,
       SpecialtyId = _specialtyId,
    FechaCita = DateTime.Now.AddDays(10),
       HoraInicio = new TimeSpan(11, 0, 0),
    HoraFin = new TimeSpan(11, 30, 0),
         Modalidad = "Presencial",
Estado = "Cancelada",
     FechaCreacion = DateTime.UtcNow
 }
            };

   _context.Appointments.AddRange(citas);
         await _context.SaveChangesAsync();
        }

     #endregion

        #region Tests de ObtenerDisponibilidadAsync

  [Fact]
  public async Task ObtenerDisponibilidadAsync_SinCitas_DeberiaTenerTodoDisponible()
  {
    // Arrange
  var fecha = DateTime.Now.AddDays(10);

            // Act
        var slots = await _repository.ObtenerDisponibilidadAsync(_doctorId, fecha);

// Assert
     Assert.NotEmpty(slots);
  Assert.All(slots, s => Assert.True(s.EstaDisponible));
        }

   [Fact]
        public async Task ObtenerDisponibilidadAsync_ConCitas_DeberiaMarcarOcupados()
        {
       // Arrange
     var fecha = DateTime.Now.AddDays(10);
  var appointment = new Appointment
       {
 Id = Guid.NewGuid(),
     PacienteId = _pacienteId,
     DoctorId = _doctorId,
SpecialtyId = _specialtyId,
       FechaCita = fecha,
   HoraInicio = new TimeSpan(10, 0, 0),
     HoraFin = new TimeSpan(10, 30, 0),
    Modalidad = "Presencial",
         Estado = "Agendada",
  FechaCreacion = DateTime.UtcNow
     };
  _context.Appointments.Add(appointment);
     await _context.SaveChangesAsync();

     // Act
         var slots = await _repository.ObtenerDisponibilidadAsync(_doctorId, fecha);

       // Assert
     var slotOcupado = slots.FirstOrDefault(s => s.HoraInicio == new TimeSpan(10, 0, 0));
       Assert.NotNull(slotOcupado);
    Assert.False(slotOcupado.EstaDisponible);
    }

  [Fact]
        public async Task ObtenerDisponibilidadAsync_DeberiaGenerarSlotsDe30Minutos()
{
       // Arrange
     var fecha = DateTime.Now.AddDays(10);

  // Act
  var slots = await _repository.ObtenerDisponibilidadAsync(_doctorId, fecha);

   // Assert
Assert.All(slots, s =>
  {
 var duracion = s.HoraFin - s.HoraInicio;
               Assert.Equal(TimeSpan.FromMinutes(30), duracion);
      });
  }

    #endregion

        #region Tests de ValidarDisponibilidadAsync

        [Fact]
  public async Task ValidarDisponibilidadAsync_HorarioLibre_DeberiaRetornarTrue()
     {
    // Arrange
            var fecha = DateTime.Now.AddDays(10);
  var horaInicio = new TimeSpan(10, 0, 0);
   var horaFin = new TimeSpan(10, 30, 0);

 // Act
     var resultado = await _repository.ValidarDisponibilidadAsync(_doctorId, fecha, horaInicio, horaFin);

        // Assert
 Assert.True(resultado);
  }

        [Fact]
        public async Task ValidarDisponibilidadAsync_HorarioOcupado_DeberiaRetornarFalse()
 {
        // Arrange
       var fecha = DateTime.Now.AddDays(10);
       var horaInicio = new TimeSpan(10, 0, 0);
       var horaFin = new TimeSpan(10, 30, 0);
  
            var appointment = new Appointment
            {
   Id = Guid.NewGuid(),
          PacienteId = _pacienteId,
   DoctorId = _doctorId,
SpecialtyId = _specialtyId,
   FechaCita = fecha,
HoraInicio = horaInicio,
   HoraFin = horaFin,
   Modalidad = "Presencial",
    Estado = "Agendada",
       FechaCreacion = DateTime.UtcNow
 };
 _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

       // Act
       var resultado = await _repository.ValidarDisponibilidadAsync(_doctorId, fecha, horaInicio, horaFin);

  // Assert
 Assert.False(resultado);
    }

        #endregion

        #region Tests de PacienteTieneCitaEnHorarioAsync

  [Fact]
public async Task PacienteTieneCitaEnHorarioAsync_SinCitas_DeberiaRetornarFalse()
        {
       // Arrange
    var fecha = DateTime.Now.AddDays(10);
       var horaInicio = new TimeSpan(10, 0, 0);
            var horaFin = new TimeSpan(10, 30, 0);

        // Act
       var resultado = await _repository.PacienteTieneCitaEnHorarioAsync(_pacienteId, fecha, horaInicio, horaFin);

         // Assert
       Assert.False(resultado);
        }

        [Fact]
   public async Task PacienteTieneCitaEnHorarioAsync_ConCita_DeberiaRetornarTrue()
        {
// Arrange
    var fecha = DateTime.Now.AddDays(10);
   var horaInicio = new TimeSpan(10, 0, 0);
    var horaFin = new TimeSpan(10, 30, 0);
            
            var appointment = new Appointment
       {
       Id = Guid.NewGuid(),
     PacienteId = _pacienteId,
     DoctorId = _doctorId,
      SpecialtyId = _specialtyId,
     FechaCita = fecha,
       HoraInicio = horaInicio,
   HoraFin = horaFin,
     Modalidad = "Presencial",
    Estado = "Agendada",
     FechaCreacion = DateTime.UtcNow
     };
   _context.Appointments.Add(appointment);
    await _context.SaveChangesAsync();

       // Act
            var resultado = await _repository.PacienteTieneCitaEnHorarioAsync(_pacienteId, fecha, horaInicio, horaFin);

// Assert
       Assert.True(resultado);
     }

      #endregion

    #region Tests de CrearAsync

     [Fact]
        public async Task CrearAsync_ConCitaValida_DeberiaGuardarEnBD()
    {
       // Arrange
var appointment = new Appointment
      {
      Id = Guid.NewGuid(),
          PacienteId = _pacienteId,
    DoctorId = _doctorId,
       SpecialtyId = _specialtyId,
              FechaCita = DateTime.Now.AddDays(5),
  HoraInicio = new TimeSpan(10, 0, 0),
   HoraFin = new TimeSpan(10, 30, 0),
  Modalidad = "Presencial",
        Estado = "Agendada",
   FechaCreacion = DateTime.UtcNow
            };

            // Act
await _repository.CrearAsync(appointment);

            // Assert
     var citaGuardada = await _context.Appointments.FindAsync(appointment.Id);
        Assert.NotNull(citaGuardada);
    }

     [Fact]
        public async Task CrearAsync_ConCitaNull_DebeLanzarArgumentNullException()
     {
            // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CrearAsync(null!));
        }

  #endregion

#region Tests de ActualizarAsync

[Fact]
        public async Task ActualizarAsync_ConCitaValida_DeberiaActualizarEnBD()
        {
  // Arrange
      var appointment = new Appointment
   {
      Id = Guid.NewGuid(),
           PacienteId = _pacienteId,
  DoctorId = _doctorId,
  SpecialtyId = _specialtyId,
  FechaCita = DateTime.Now.AddDays(5),
     HoraInicio = new TimeSpan(10, 0, 0),
            HoraFin = new TimeSpan(10, 30, 0),
       Modalidad = "Presencial",
       Estado = "Agendada",
       FechaCreacion = DateTime.UtcNow
 };
 _context.Appointments.Add(appointment);
    await _context.SaveChangesAsync();

       // Act
        appointment.Estado = "Cancelada";
       appointment.MotivoCancelacion = "Test cancelación";
         await _repository.ActualizarAsync(appointment);

// Assert
     var citaActualizada = await _context.Appointments.FindAsync(appointment.Id);
     Assert.Equal("Cancelada", citaActualizada?.Estado);
            Assert.Equal("Test cancelación", citaActualizada?.MotivoCancelacion);
        }

      [Fact]
        public async Task ActualizarAsync_ConCitaNull_DebeLanzarArgumentNullException()
     {
   // Act & Assert
       await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.ActualizarAsync(null!));
  }

        #endregion

        #region Tests de EliminarAsync

   [Fact]
  public async Task EliminarAsync_ConIdValido_DeberiaCancelarCita()
        {
    // Arrange
  var appointment = new Appointment
        {
  Id = Guid.NewGuid(),
     PacienteId = _pacienteId,
       DoctorId = _doctorId,
 SpecialtyId = _specialtyId,
         FechaCita = DateTime.Now.AddDays(5),
HoraInicio = new TimeSpan(10, 0, 0),
          HoraFin = new TimeSpan(10, 30, 0),
Modalidad = "Presencial",
   Estado = "Agendada",
                FechaCreacion = DateTime.UtcNow
   };
 _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

 // Act
            await _repository.EliminarAsync(appointment.Id);

   // Assert
            var citaEliminada = await _context.Appointments.FindAsync(appointment.Id);
      Assert.Equal("Cancelada", citaEliminada?.Estado);
   }

      [Fact]
        public async Task EliminarAsync_ConIdInexistente_NoDebeLanzarExcepcion()
        {
            // Act & Assert (no debe lanzar excepción)
       await _repository.EliminarAsync(Guid.NewGuid());
  }

        #endregion
    }
}
