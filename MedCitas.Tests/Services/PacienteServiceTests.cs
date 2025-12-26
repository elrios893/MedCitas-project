using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using Moq;

namespace MedCitas.Tests.Services
{
    public class PacienteServiceTests
    {
        private readonly Mock<IPacienteRepository> _pacienteRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly PacienteService _service;

        public PacienteServiceTests()
        {
            _pacienteRepoMock = new Mock<IPacienteRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _service = new PacienteService(_pacienteRepoMock.Object, _emailServiceMock.Object);
        }

        #region Registro - Casos Exitosos

        #endregion

        #region Registro - Validaciones de Duplicados

        [Fact]
        public async Task RegistrarPaciente_DeberiaFallarSiCorreoYaExiste()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            var pacienteExistente = new Paciente { CorreoElectronico = "carlos@example.com" };
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync(pacienteExistente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Equal("El correo electrónico ya está registrado.", exception.Message);
            _pacienteRepoMock.Verify(r => r.RegistrarAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPaciente_DeberiaFallarSiDocumentoYaExiste()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "nuevo@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            var pacienteExistente = new Paciente { NumeroDocumento = "123456789" };
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("nuevo@example.com"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync(pacienteExistente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Equal("El número de documento ya está registrado.", exception.Message);
            _pacienteRepoMock.Verify(r => r.RegistrarAsync(It.IsAny<Paciente>()), Times.Never);
        }

        #endregion

        #region Registro - Validaciones de Contraseña

        [Theory]
        [InlineData("1234567")]           // muy corta (menos de 8 caracteres)
        [InlineData("password")]          // sin mayúsculas, números o símbolos
        [InlineData("PASSWORD123")]       // sin minúsculas ni símbolos
        [InlineData("Passwor")]           // sin número ni símbolo
        [InlineData("Pass123")]           // sin carácter especial
        [InlineData("Password!")]         // sin número
        public async Task RegistrarPaciente_DeberiaFallarSiContraseñaNoValida(string contraseña)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "nuevo@example.com"
            };

            var confirmarPassword = contraseña;

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("nuevo@example.com"))
                .ReturnsAsync((Paciente?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, contraseña, confirmarPassword));
            
            Assert.Contains("contraseña", exception.Message.ToLower());
            _pacienteRepoMock.Verify(r => r.RegistrarAsync(It.IsAny<Paciente>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPaciente_DeberiaFallarSiContraseñasNoCoinciden()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba456!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync((Paciente?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Equal("Las contraseñas no coinciden.", exception.Message);
            _pacienteRepoMock.Verify(r => r.RegistrarAsync(It.IsAny<Paciente>()), Times.Never);
        }

        #endregion

        #region Registro - Validaciones de Campos

        [Fact]
        public async Task RegistrarPaciente_DeberiaFallarSiNombreEstaVacio()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Equal("El nombre completo es obligatorio.", exception.Message);
        }

        [Theory]
        [InlineData("12345ABC")]          // contiene letras
        [InlineData("123-456")]           // contiene guion
        [InlineData("")]                  // vacío
        public async Task RegistrarPaciente_DeberiaFallarSiDocumentoNoValido(string documento)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = documento,
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Contains("documento", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("123456")]            // muy corto
        [InlineData("12345678901234567")] // muy largo (más de 15 dígitos)
        [InlineData("301-555-9999")]      // contiene guiones
        [InlineData("3015559999ABC")]     // contiene letras
        public async Task RegistrarPaciente_DeberiaFallarSiTelefonoNoValido(string telefono)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = telefono,
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Contains("teléfono", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("correo-invalido")]           // sin @
        [InlineData("@example.com")]              // sin parte local
        [InlineData("carlos@")]                   // sin dominio
        [InlineData("carlos @example.com")]       // con espacios
        public async Task RegistrarPaciente_DeberiaFallarSiCorreoNoValido(string correo)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = correo
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegistrarAsync(paciente, password, confirmarPassword));
            
            Assert.Contains("correo", exception.Message.ToLower());
        }

        #endregion

        #region Registro - Validaciones de Null

        [Fact]
        public async Task RegistrarPaciente_DeberiaFallarSiPacienteEsNull()
        {
            // Arrange
            Paciente? paciente = null;
            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.RegistrarAsync(paciente!, password, confirmarPassword));
        }

        #endregion

        #region Login

        [Fact]
        public async Task LoginAsync_DeberiaFallarSiCorreoEsNullOVacio()
        {
            // Arrange
            string correo = "";
            var password = "Prueba123!";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.LoginAsync(correo, password));
            
            Assert.Contains("Correo y contraseña son obligatorios", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_DeberiaFallarSiPasswordEsNullOVacio()
        {
            // Arrange
            var correo = "carlos@example.com";
            string password = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.LoginAsync(correo, password));
            
            Assert.Contains("Correo y contraseña son obligatorios", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarPacienteSiCredencialesSonCorrectas()
        {
            // Arrange
            var correo = "carlos@example.com";
            var password = "Prueba123!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var paciente = new Paciente
            {
                CorreoElectronico = correo,
                PasswordHash = passwordHash,
                EstaVerificado = true
            };

            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync(correo))
                .ReturnsAsync(paciente);

            // Act
            var resultado = await _service.LoginAsync(correo, password);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(correo, resultado.CorreoElectronico);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarNullSiCorreoNoExiste()
        {
            // Arrange
            var correo = "noexiste@example.com";
            var password = "Prueba123!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync(correo))
                .ReturnsAsync((Paciente?)null);

            // Act
            var resultado = await _service.LoginAsync(correo, password);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task LoginAsync_DeberiaRetornarNullSiContraseñaEsIncorrecta()
        {
            // Arrange
            var correo = "carlos@example.com";
            var password = "Prueba123!";
            var passwordIncorrecto = "Incorrecta456!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var paciente = new Paciente
            {
                CorreoElectronico = correo,
                PasswordHash = passwordHash,
                EstaVerificado = true
            };

            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync(correo))
                .ReturnsAsync(paciente);

            // Act
            var resultado = await _service.LoginAsync(correo, passwordIncorrecto);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task LoginAsync_DeberiaFallarSiCuentaNoEstaVerificada()
        {
            // Arrange
            var correo = "carlos@example.com";
            var password = "Prueba123!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var paciente = new Paciente
            {
                CorreoElectronico = correo,
                PasswordHash = passwordHash,
                EstaVerificado = false
            };

            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync(correo))
                .ReturnsAsync(paciente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.LoginAsync(correo, password));
            
            Assert.Equal("Cuenta pendiente de verificación.", exception.Message);
        }

        #endregion

        #region Activar Cuenta

        [Fact]
        public async Task ActivarCuentaAsync_DeberiaRetornarTrueSiTokenEsValido()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();

            _pacienteRepoMock.Setup(r => r.ActivarCuentaAsync(token))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.ActivarCuentaAsync(token);

            // Assert
            Assert.True(resultado);
            _pacienteRepoMock.Verify(r => r.ActivarCuentaAsync(token), Times.Once);
        }

        [Fact]
        public async Task ActivarCuentaAsync_DeberiaRetornarFalseSiTokenNoEsValido()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();

            _pacienteRepoMock.Setup(r => r.ActivarCuentaAsync(token))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.ActivarCuentaAsync(token);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task ActivarCuentaAsync_DeberiaFallarSiTokenEstaVacio()
        {
            // Arrange
            var token = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ActivarCuentaAsync(token));
            
            Assert.Equal("Token inválido.", exception.Message);
        }

        [Fact]
        public async Task ActivarCuentaAsync_DeberiaFallarSiTokenEsNull()
        {
            // Arrange
            string? token = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ActivarCuentaAsync(token!));
            
            Assert.Equal("Token inválido.", exception.Message);
        }

        [Fact]
        public async Task ActivarCuentaAsync_DeberiaFallarSiTokenEsSoloEspacios()
        {
            // Arrange
            var token = "   ";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ActivarCuentaAsync(token));
            
            Assert.Equal("Token inválido.", exception.Message);
        }

        #endregion

        #region Registro - Edge Cases Adicionales

        [Fact]
        public async Task RegistrarPaciente_DeberiaAsignarFechaRegistroCorrectamente()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.EnviarCorreoVerificacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var antes = DateTime.UtcNow;

            // Act
            var resultado = await _service.RegistrarAsync(paciente, password, confirmarPassword);

            var despues = DateTime.UtcNow;

            // Assert
            Assert.True(resultado.FechaRegistro >= antes && resultado.FechaRegistro <= despues);
        }

        [Theory]
        [InlineData("1234567890")]        // 10 dígitos
        [InlineData("12345678901")]       // 11 dígitos
        [InlineData("123456789012345")]   // 15 dígitos
        public async Task RegistrarPaciente_DeberiaAceptarDocumentosDeVariasLongitudes(string documento)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = documento,
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync(documento))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.EnviarCorreoVerificacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.RegistrarAsync(paciente, password, confirmarPassword);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(documento, resultado.NumeroDocumento);
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name@example.com")]
        [InlineData("user+tag@example.co.uk")]
        [InlineData("user_name@example-domain.com")]
        public async Task RegistrarPaciente_DeberiaAceptarCorreosValidos(string correo)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = correo
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync(correo))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.EnviarCorreoVerificacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.RegistrarAsync(paciente, password, confirmarPassword);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(correo, resultado.CorreoElectronico);
        }

        [Theory]
        [InlineData("1234567")]           // 7 dígitos (mínimo)
        [InlineData("12345678")]          // 8 dígitos
        [InlineData("123456789012345")]   // 15 dígitos (máximo)
        public async Task RegistrarPaciente_DeberiaAceptarTelefonosValidos(string telefono)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = telefono,
                CorreoElectronico = "carlos@example.com"
            };

            var password = "Prueba123!";
            var confirmarPassword = "Prueba123!";

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.EnviarCorreoVerificacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.RegistrarAsync(paciente, password, confirmarPassword);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(telefono, resultado.Telefono);
        }

        [Theory]
        [InlineData("Prueba123!")]
        [InlineData("MiPassword2024@")]
        [InlineData("Segura#Pass123")]
        [InlineData("P@ssw0rd!")]
        public async Task RegistrarPaciente_DeberiaAceptarContraseñasValidas(string password)
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Carlos Jimenez",
                TipoDocumento = "CC",
                NumeroDocumento = "123456789",
                FechaNacimiento = new DateTime(2003, 10, 15),
                Sexo = "M",
                Telefono = "3015559999",
                CorreoElectronico = "carlos@example.com"
            };

            var confirmarPassword = password;

            _pacienteRepoMock.Setup(r => r.ObtenerPorDocumentoAsync("123456789"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.ObtenerPorCorreoAsync("carlos@example.com"))
                .ReturnsAsync((Paciente?)null);
            _pacienteRepoMock.Setup(r => r.RegistrarAsync(It.IsAny<Paciente>()))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.EnviarCorreoVerificacionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.RegistrarAsync(paciente, password, confirmarPassword);

            // Assert
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.PasswordHash);
            Assert.NotEqual(password, resultado.PasswordHash); // Hash debe ser diferente
        }
        #endregion
    }
}

