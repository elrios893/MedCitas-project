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
    public class EfSpecialtyRepositoryTests : IDisposable
    {
    private readonly MedCitasDbContext _context;
        private readonly EfSpecialtyRepository _repository;

        public EfSpecialtyRepositoryTests()
   {
            var options = new DbContextOptionsBuilder<MedCitasDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Specialty_{Guid.NewGuid()}")
        .Options;

         _context = new MedCitasDbContext(options);
  _repository = new EfSpecialtyRepository(_context);

         SeedTestData();
        }

     private void SeedTestData()
   {
            var specialties = new[]
       {
          new Specialty
      {
  Id = Guid.NewGuid(),
        Nombre = "Cardiología",
    Descripcion = "Especialidad en corazón y sistema cardiovascular",
            EstaActiva = true
     },
                new Specialty
      {
      Id = Guid.NewGuid(),
  Nombre = "Pediatría",
      Descripcion = "Especialidad en salud infantil",
       EstaActiva = true
                },
  new Specialty
     {
      Id = Guid.NewGuid(),
               Nombre = "Dermatología",
          Descripcion = "Especialidad en piel",
     EstaActiva = true
   },
          new Specialty
       {
     Id = Guid.NewGuid(),
      Nombre = "Especialidad Inactiva",
  Descripcion = "Esta especialidad no está disponible",
      EstaActiva = false // Inactiva
                }
   };

            _context.Specialties.AddRange(specialties);
            _context.SaveChanges();
  }

     public void Dispose()
        {
         _context.Database.EnsureDeleted();
   _context.Dispose();
        }

        #region Tests de ObtenerPorIdAsync

        [Fact]
      public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarEspecialidad()
        {
    // Arrange
      var especialidadEsperada = _context.Specialties.First();

            // Act
            var resultado = await _repository.ObtenerPorIdAsync(especialidadEsperada.Id);

        // Assert
      Assert.NotNull(resultado);
Assert.Equal(especialidadEsperada.Id, resultado.Id);
   Assert.Equal(especialidadEsperada.Nombre, resultado.Nombre);
 Assert.Equal(especialidadEsperada.Descripcion, resultado.Descripcion);
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
        public async Task ObtenerPorIdAsync_DeberiaRetornarEspecialidadInactiva()
        {
            // Arrange
            var especialidadInactiva = _context.Specialties.First(s => !s.EstaActiva);

    // Act
            var resultado = await _repository.ObtenerPorIdAsync(especialidadInactiva.Id);

        // Assert
     Assert.NotNull(resultado);
            Assert.False(resultado.EstaActiva);
        }

    #endregion

        #region Tests de ObtenerTodasAsync

        [Fact]
   public async Task ObtenerTodasAsync_DeberiaRetornarSoloEspecialidadesActivas()
        {
            // Act
    var resultado = await _repository.ObtenerTodasAsync();

            // Assert
    Assert.Equal(3, resultado.Count); // Solo las 3 activas
        Assert.All(resultado, s => Assert.True(s.EstaActiva));
 }

        [Fact]
        public async Task ObtenerTodasAsync_DeberiaOrdenarPorNombre()
        {
            // Act
   var resultado = await _repository.ObtenerTodasAsync();

   // Assert
      Assert.Equal("Cardiología", resultado[0].Nombre);
      Assert.Equal("Dermatología", resultado[1].Nombre);
            Assert.Equal("Pediatría", resultado[2].Nombre);
        }

     [Fact]
        public async Task ObtenerTodasAsync_NoDeberiaRetornarEspecialidadesInactivas()
     {
   // Act
            var resultado = await _repository.ObtenerTodasAsync();

            // Assert
     Assert.DoesNotContain(resultado, s => s.Nombre == "Especialidad Inactiva");
   }

        [Fact]
        public async Task ObtenerTodasAsync_ConTodasInactivas_DeberiaRetornarListaVacia()
        {
// Arrange
            var todasLasEspecialidades = await _context.Specialties.ToListAsync();
     foreach (var spec in todasLasEspecialidades)
       {
    spec.EstaActiva = false;
            }
        await _context.SaveChangesAsync();

    // Act
     var resultado = await _repository.ObtenerTodasAsync();

   // Assert
      Assert.Empty(resultado);
        }

        #endregion
 }
}
