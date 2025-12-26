using System;
using System.Linq;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Infrastructure.DataDb;
using MedCitas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedCitas.Tests.Repositories
{
    public class EfDoctorRepositoryTests : IDisposable
    {
        private readonly MedCitasDbContext _context;
    private readonly EfDoctorRepository _repository;
        private readonly Guid _specialtyId1 = Guid.NewGuid();
        private readonly Guid _specialtyId2 = Guid.NewGuid();

        public EfDoctorRepositoryTests()
      {
       var options = new DbContextOptionsBuilder<MedCitasDbContext>()
 .UseInMemoryDatabase(databaseName: $"TestDb_Doctor_{Guid.NewGuid()}")
      .Options;

     _context = new MedCitasDbContext(options);
_repository = new EfDoctorRepository(_context);

  SeedTestData();
   }

 private void SeedTestData()
    {
    var specialty1 = new Specialty
 {
Id = _specialtyId1,
        Nombre = "Cardiología",
        Descripcion = "Especialidad en corazón"
};

     var specialty2 = new Specialty
    {
   Id = _specialtyId2,
                Nombre = "Pediatría",
      Descripcion = "Especialidad en niños"
   };

      var doctors = new[]
     {
new Doctor
  {
     Id = Guid.NewGuid(),
        NombreCompleto = "Dr. Carlos Rodríguez",
NumeroLicencia = "LIC001",
      SpecialtyId = _specialtyId1,
    Specialty = specialty1,
      Telefono = "3001234567",
      CorreoElectronico = "carlos@test.com",
EstaActivo = true
     },
      new Doctor
     {
     Id = Guid.NewGuid(),
   NombreCompleto = "Dra. María González",
      NumeroLicencia = "LIC002",
  SpecialtyId = _specialtyId1,
  Specialty = specialty1,
    Telefono = "3009876543",
  CorreoElectronico = "maria@test.com",
 EstaActivo = true
  },
     new Doctor
 {
   Id = Guid.NewGuid(),
   NombreCompleto = "Dr. Juan Martínez",
         NumeroLicencia = "LIC003",
       SpecialtyId = _specialtyId2,
       Specialty = specialty2,
   Telefono = "3005555555",
   CorreoElectronico = "juan@test.com",
        EstaActivo = true
 },
      new Doctor
 {
         Id = Guid.NewGuid(),
         NombreCompleto = "Dr. Pedro Inactivo",
 NumeroLicencia = "LIC004",
  SpecialtyId = _specialtyId1,
    Specialty = specialty1,
       Telefono = "3004444444",
     CorreoElectronico = "pedro@test.com",
  EstaActivo = false // Inactivo
     }
         };

_context.Specialties.AddRange(specialty1, specialty2);
    _context.Doctors.AddRange(doctors);
         _context.SaveChanges();
   }

        public void Dispose()
 {
       _context.Database.EnsureDeleted();
         _context.Dispose();
        }

  #region Tests de ObtenerPorIdAsync

   [Fact]
        public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarDoctor()
        {
     // Arrange
  var doctorEsperado = _context.Doctors.First();

     // Act
   var resultado = await _repository.ObtenerPorIdAsync(doctorEsperado.Id);

            // Assert
      Assert.NotNull(resultado);
            Assert.Equal(doctorEsperado.Id, resultado.Id);
      Assert.Equal(doctorEsperado.NombreCompleto, resultado.NombreCompleto);
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

  [Fact]
        public async Task ObtenerPorIdAsync_DeberiaIncluirEspecialidad()
     {
  // Arrange
     var doctorEsperado = _context.Doctors.First();

            // Act
       var resultado = await _repository.ObtenerPorIdAsync(doctorEsperado.Id);

        // Assert
            Assert.NotNull(resultado);
       Assert.NotNull(resultado.Specialty);
      Assert.Equal(doctorEsperado.SpecialtyId, resultado.Specialty.Id);
  }

        #endregion

     #region Tests de ObtenerTodosAsync

   [Fact]
        public async Task ObtenerTodosAsync_DeberiaRetornarSoloDoctoresActivos()
 {
// Act
   var resultado = await _repository.ObtenerTodosAsync();

       // Assert
   Assert.Equal(3, resultado.Count); // Solo los 3 activos
       Assert.All(resultado, d => Assert.True(d.EstaActivo));
 }

        [Fact]
        public async Task ObtenerTodosAsync_DeberiaOrdenarPorNombreCompleto()
     {
  // Act
       var resultado = await _repository.ObtenerTodosAsync();

       // Assert
   Assert.Equal("Dr. Carlos Rodríguez", resultado[0].NombreCompleto);
   Assert.Equal("Dr. Juan Martínez", resultado[1].NombreCompleto);
Assert.Equal("Dra. María González", resultado[2].NombreCompleto);
    }

        [Fact]
        public async Task ObtenerTodosAsync_DeberiaIncluirEspecialidades()
        {
     // Act
            var resultado = await _repository.ObtenerTodosAsync();

// Assert
Assert.All(resultado, d => Assert.NotNull(d.Specialty));
        }

        #endregion

        #region Tests de ObtenerPorEspecialidadAsync

 [Fact]
public async Task ObtenerPorEspecialidadAsync_ConEspecialidadValida_DeberiaRetornarDoctoresFiltrados()
{
  // Act
          var resultado = await _repository.ObtenerPorEspecialidadAsync(_specialtyId1);

   // Assert
       Assert.Equal(2, resultado.Count); // Carlos y María (Pedro está inactivo)
Assert.All(resultado, d => Assert.Equal(_specialtyId1, d.SpecialtyId));
       Assert.All(resultado, d => Assert.True(d.EstaActivo));
 }

        [Fact]
        public async Task ObtenerPorEspecialidadAsync_DeberiaOrdenarPorNombreCompleto()
        {
 // Act
            var resultado = await _repository.ObtenerPorEspecialidadAsync(_specialtyId1);

    // Assert
Assert.Equal("Dr. Carlos Rodríguez", resultado[0].NombreCompleto);
 Assert.Equal("Dra. María González", resultado[1].NombreCompleto);
 }

  [Fact]
        public async Task ObtenerPorEspecialidadAsync_ConEspecialidadSinDoctores_DeberiaRetornarListaVacia()
  {
   // Act
   var resultado = await _repository.ObtenerPorEspecialidadAsync(Guid.NewGuid());

// Assert
       Assert.Empty(resultado);
        }

        [Fact]
    public async Task ObtenerPorEspecialidadAsync_DeberiaIncluirEspecialidades()
        {
      // Act
     var resultado = await _repository.ObtenerPorEspecialidadAsync(_specialtyId1);

  // Assert
            Assert.All(resultado, d => Assert.NotNull(d.Specialty));
        }

        [Fact]
        public async Task ObtenerPorEspecialidadAsync_NoDeberiaRetornarDoctoresInactivos()
   {
 // Arrange
            // Ya tenemos un doctor inactivo en _specialtyId1

 // Act
      var resultado = await _repository.ObtenerPorEspecialidadAsync(_specialtyId1);

     // Assert
 Assert.DoesNotContain(resultado, d => d.NombreCompleto == "Dr. Pedro Inactivo");
    }

        #endregion
    }
}
