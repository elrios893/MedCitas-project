using System;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Infrastructure.DataDb;
using MedCitas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedCitas.Tests.Repositories
{
    public class EfPacienteRepositorioTests :IDisposable
    {
        private readonly MedCitasDbContext _context;
        private readonly EfPacienteRepositorio _repositorio;

        public EfPacienteRepositorioTests()
        {
            // Configurar In-Memory Database
            var options = new DbContextOptionsBuilder<MedCitasDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // BD única por test
                .Options;

            _context = new MedCitasDbContext(options);
            _repositorio = new EfPacienteRepositorio(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region ObtenerPorDocumentoAsync

        [Fact]
        public async Task ObtenerPorDocumento_CuandoExiste_DeberiaRetornarPaciente()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com"
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.ObtenerPorDocumentoAsync("123456789");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("123456789", resultado.NumeroDocumento);
        }

        [Fact]
        public async Task ObtenerPorDocumento_CuandoNoExiste_DeberiaRetornarNull()
        {
            // Act
            var resultado = await _repositorio.ObtenerPorDocumentoAsync("999999999");

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region ObtenerPorCorreoAsync

        [Fact]
        public async Task ObtenerPorCorreo_CuandoExiste_DeberiaRetornarPaciente()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com"
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.ObtenerPorCorreoAsync("test@example.com");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("test@example.com", resultado.CorreoElectronico);
        }

        [Fact]
        public async Task ObtenerPorCorreo_DeberiaSer_CaseInsensitive()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "TEST@EXAMPLE.COM"
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.ObtenerPorCorreoAsync("test@example.com");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("TEST@EXAMPLE.COM", resultado.CorreoElectronico);
        }

        [Fact]
        public async Task ObtenerPorCorreo_CuandoNoExiste_DeberiaRetornarNull()
        {
            // Act
            var resultado = await _repositorio.ObtenerPorCorreoAsync("noexiste@example.com");

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region RegistrarAsync

        [Fact]
        public async Task Registrar_DeberiaGuardarPacienteEnBaseDeDatos()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "987654321",
                NombreCompleto = "Nuevo Paciente",
                CorreoElectronico = "nuevo@example.com",
                TipoDocumento = "CC",
                FechaNacimiento = DateTime.Now.AddYears(-30),
                Sexo = "M",
                Telefono = "3001234567",
                Eps = "Sura",
                TipoSangre = "O+"
            };

            // Act
            await _repositorio.RegistrarAsync(paciente);

            // Assert
            var pacienteGuardado = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.NumeroDocumento == "987654321");
            Assert.NotNull(pacienteGuardado);
            Assert.Equal("Nuevo Paciente", pacienteGuardado.NombreCompleto);
        }

        [Fact]
        public async Task Registrar_SiIdEsVacio_DeberiaGenerarGuid()
        {
            // Arrange
            var paciente = new Paciente
            {
                Id = Guid.Empty,
                NumeroDocumento = "111222333",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@test.com",
                TipoDocumento = "CC",
                FechaNacimiento = DateTime.Now.AddYears(-25),
                Sexo = "F",
                Telefono = "3009998877",
                Eps = "Sanitas",
                TipoSangre = "AB+"
            };

            // Act
            await _repositorio.RegistrarAsync(paciente);

            // Assert
            Assert.NotEqual(Guid.Empty, paciente.Id);
        }

        #endregion

        #region ActivarCuentaAsync

        [Fact]
        public async Task ActivarCuenta_ConTokenValido_DeberiaActivarCuenta()
        {
            // Arrange
            var token = "token-valido-123";
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com",
                TokenVerificacion = token,
                EstaVerificado = false
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.ActivarCuentaAsync(token);

            // Assert
            Assert.True(resultado);
            var pacienteActualizado = await _context.Pacientes.FindAsync(paciente.Id);
            Assert.True(pacienteActualizado!.EstaVerificado);
            Assert.Null(pacienteActualizado.TokenVerificacion);
        }

        [Fact]
        public async Task ActivarCuenta_ConTokenInvalido_DeberiaRetornarFalse()
        {
            // Act
            var resultado = await _repositorio.ActivarCuentaAsync("token-inexistente");

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region VerificarOTPAsync

        [Fact]
        public async Task VerificarOTP_ConOTPValido_DeberiaActivarCuenta()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com",
                CodigoOTP = "123456",
                OTPExpiracion = DateTime.UtcNow.AddMinutes(10),
                EstaVerificado = false,
                IntentosOTPFallidos = 0
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.VerificarOTPAsync("test@example.com", "123456");

            // Assert
            Assert.True(resultado);
            var pacienteActualizado = await _context.Pacientes.FindAsync(paciente.Id);
            Assert.True(pacienteActualizado!.EstaVerificado);
            Assert.Null(pacienteActualizado.CodigoOTP);
            Assert.Null(pacienteActualizado.OTPExpiracion);
            Assert.Equal(0, pacienteActualizado.IntentosOTPFallidos);
        }

        [Fact]
        public async Task VerificarOTP_ConOTPIncorrecto_DeberiaIncrementarIntentosFallidos()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com",
                CodigoOTP = "123456",
                OTPExpiracion = DateTime.UtcNow.AddMinutes(10),
                EstaVerificado = false,
                IntentosOTPFallidos = 0
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.VerificarOTPAsync("test@example.com", "000000");

            // Assert
            Assert.False(resultado);
            var pacienteActualizado = await _context.Pacientes.FindAsync(paciente.Id);
            Assert.Equal(1, pacienteActualizado!.IntentosOTPFallidos);
            Assert.False(pacienteActualizado.EstaVerificado);
        }

        [Fact]
        public async Task VerificarOTP_ConOTPExpirado_DeberiaRetornarFalse()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com",
                CodigoOTP = "123456",
                OTPExpiracion = DateTime.UtcNow.AddMinutes(-1), // Expirado hace 1 minuto
                EstaVerificado = false,
                IntentosOTPFallidos = 0
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repositorio.VerificarOTPAsync("test@example.com", "123456");

            // Assert
            Assert.False(resultado);
            var pacienteActualizado = await _context.Pacientes.FindAsync(paciente.Id);
            Assert.Equal(1, pacienteActualizado!.IntentosOTPFallidos);
        }

        [Fact]
        public async Task VerificarOTP_ConCorreoInexistente_DeberiaRetornarFalse()
        {
            // Act
            var resultado = await _repositorio.VerificarOTPAsync("noexiste@example.com", "123456");

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region ActualizarOTPAsync

        [Fact]
        public async Task ActualizarOTP_DeberiaActualizarDatosPaciente()
        {
            // Arrange
            var paciente = new Paciente
            {
                NumeroDocumento = "123456789",
                NombreCompleto = "Test User",
                CorreoElectronico = "test@example.com",
                CodigoOTP = "111111",
                OTPExpiracion = DateTime.UtcNow.AddMinutes(5),
                IntentosOTPFallidos = 2
            };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            // Act
            paciente.CodigoOTP = "999999";
            paciente.OTPExpiracion = DateTime.UtcNow.AddMinutes(15);
            paciente.IntentosOTPFallidos = 0;
            await _repositorio.ActualizarOTPAsync(paciente);

            // Assert
            var pacienteActualizado = await _context.Pacientes.FindAsync(paciente.Id);
            Assert.Equal("999999", pacienteActualizado!.CodigoOTP);
            Assert.Equal(0, pacienteActualizado.IntentosOTPFallidos);
        }

        #endregion
    }
}