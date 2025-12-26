using System;
using Xunit;
using MedCitas.Core.Entities;

namespace MedCitas.Tests.Entities
{
    public class PacienteTests
    {
        #region CalcularEdad

        [Theory]
        [InlineData("2000-01-01", 25)]  // Nació en 2000, hoy es 2025
        [InlineData("1990-06-15", 35)]  // 35 años
        [InlineData("2010-12-31", 14)]  // 14 años
        [InlineData("1980-03-20", 45)]  // 45 años
        public void CalcularEdad_ConDiferentesFechas_DeberiaRetornarEdadCorrecta(string fechaNacimiento, int edadEsperada)
        {
            // Arrange
            var paciente = new Paciente
            {
                FechaNacimiento = DateTime.Parse(fechaNacimiento)
            };

            // Act
            var edad = paciente.CalcularEdad();

            // Assert
            Assert.Equal(edadEsperada, edad);
        }

        [Fact]
        public void CalcularEdad_CuandoCumpleañosNoHaPasado_DeberiaRestarUnAño()
        {
            // Arrange
            var hoy = DateTime.Today;
            var fechaNacimiento = new DateTime(hoy.Year - 30, hoy.Month, hoy.Day).AddDays(1); // Cumpleaños es mañana
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var edad = paciente.CalcularEdad();

            // Assert
            Assert.Equal(29, edad); // Aún tiene 29 porque no ha cumplido
        }

        [Fact]
        public void CalcularEdad_CuandoEsElDiaDelCumpleaños_DeberiaRetornarEdadCompleta()
        {
            // Arrange
            var hoy = DateTime.Today;
            var fechaNacimiento = new DateTime(hoy.Year - 25, hoy.Month, hoy.Day); // Cumple hoy
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var edad = paciente.CalcularEdad();

            // Assert
            Assert.Equal(25, edad);
        }

        [Fact]
        public void CalcularEdad_RecienNacido_DeberiaRetornarCero()
        {
            // Arrange
            var paciente = new Paciente
            {
                FechaNacimiento = DateTime.Today
            };

            // Act
            var edad = paciente.CalcularEdad();

            // Assert
            Assert.Equal(0, edad);
        }

        #endregion

        #region EsMayorDeEdad

        [Theory]
        [InlineData("2006-01-01", true)]  // 19 años
        [InlineData("1990-01-01", true)]  // 35 años
        [InlineData("2010-01-01", false)] // 15 años
        public void EsMayorDeEdad_ConDiferentesEdades_DeberiaRetornarResultadoCorrecto(string fechaNacimiento, bool esperado)
        {
            // Arrange
            var paciente = new Paciente
            {
                FechaNacimiento = DateTime.Parse(fechaNacimiento)
            };

            // Act
            var resultado = paciente.EsMayorDeEdad();

            // Assert
            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void EsMayorDeEdad_Con18AñosExactos_DeberiaRetornarTrue()
        {
            // Arrange
            var fechaNacimiento = DateTime.Today.AddYears(-18);
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var resultado = paciente.EsMayorDeEdad();

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void EsMayorDeEdad_Con17Años11Meses_DeberiaRetornarFalse()
        {
            // Arrange
            var fechaNacimiento = DateTime.Today.AddYears(-17).AddMonths(-11);
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var resultado = paciente.EsMayorDeEdad();

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region EsPacientePreferencial

        [Theory]
        [InlineData("1950-01-01", true)]  // 75 años - preferencial (>= 65)
        [InlineData("1960-01-01", true)]  // 65 años - preferencial (>= 65)
        [InlineData("2015-01-01", true)]  // 10 años - preferencial (<= 12)
        [InlineData("2013-01-01", true)]  // 12 años - preferencial (<= 12)
        [InlineData("1990-01-01", false)] // 35 años - no preferencial
        [InlineData("2005-01-01", false)] // 20 años - no preferencial
        [InlineData("2011-01-01", false)] // 13 o 14 años - no preferencial
        public void EsPacientePreferencial_ConDiferentesEdades_DeberiaRetornarResultadoCorrecto(
            string fechaNacimiento,
            bool esperado)
        {
            // Arrange
            var paciente = new Paciente
            {
                FechaNacimiento = DateTime.Parse(fechaNacimiento)
            };

            // Act
            var resultado = paciente.EsPacientePreferencial();

            // Assert
            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void EsPacientePreferencial_RecienNacido_DeberiaRetornarTrue()
        {
            // Arrange
            var paciente = new Paciente
            {
                FechaNacimiento = DateTime.Today
            };

            // Act
            var resultado = paciente.EsPacientePreferencial();

            // Assert
            Assert.True(resultado); // 0 años es <= 12
        }

        [Fact]
        public void EsPacientePreferencial_Con65AñosExactos_DeberiaRetornarTrue()
        {
            // Arrange
            var fechaNacimiento = DateTime.Today.AddYears(-65);
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var resultado = paciente.EsPacientePreferencial();

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void EsPacientePreferencial_Con12AñosExactos_DeberiaRetornarTrue()
        {
            // Arrange
            var fechaNacimiento = DateTime.Today.AddYears(-12);
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var resultado = paciente.EsPacientePreferencial();

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void EsPacientePreferencial_Con13Años_DeberiaRetornarFalse()
        {
            // Arrange
            var fechaNacimiento = DateTime.Today.AddYears(-13);
            var paciente = new Paciente
            {
                FechaNacimiento = fechaNacimiento
            };

            // Act
            var resultado = paciente.EsPacientePreferencial();

            // Assert
            Assert.False(resultado);
        }

        #endregion

        #region ToStringPaciente

        [Fact]
        public void ToStringPaciente_DeberiaRetornarInformacionCompleta()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Juan Perez",
                TipoDocumento = "CC",
                NumeroDocumento = "12345678",
                FechaNacimiento = new DateTime(1990, 1, 1),
                Sexo = "M",
                Telefono = "3001234567",
                CorreoElectronico = "juan@test.com",
                Eps = "SURA",
                TipoSangre = "O+",
                EstaVerificado = true,
                FechaRegistro = new DateTime(2024, 1, 1)
            };

            // Act
            var result = paciente.ToStringPaciente();

            // Assert
            Assert.Contains("Juan Perez", result);
            Assert.Contains("CC", result);
            Assert.Contains("12345678", result);
            Assert.Contains("3001234567", result);
            Assert.Contains("juan@test.com", result);
            Assert.Contains("SURA", result);
            Assert.Contains("O+", result);
            Assert.Contains("True", result);
        }

        #endregion

        #region ObtenerResumenContacto

        [Fact]
        public void ObtenerResumenContacto_DeberiaRetornarFormatoEsperado()
        {
            // Arrange
            var paciente = new Paciente
            {
                NombreCompleto = "Maria Garcia",
                Telefono = "3009876543",
                CorreoElectronico = "maria@test.com"
            };

            // Act
            var resumen = paciente.ObtenerResumenContacto();

            // Assert
            Assert.Contains("Maria Garcia", resumen);
            Assert.Contains("3009876543", resumen);
            Assert.Contains("maria@test.com", resumen);
            Assert.Contains("Tel:", resumen);
            Assert.Contains("Email:", resumen);
        }

        #endregion

        #region ActualizarDatosContacto

        [Fact]
        public void ActualizarDatosContacto_DeberiaActualizarTelefonoYCorreo()
        {
            // Arrange
            var paciente = new Paciente
            {
                Telefono = "3001111111",
                CorreoElectronico = "old@test.com"
            };

            // Act
            paciente.ActualizarDatosContacto("3002222222", "new@test.com");

            // Assert
            Assert.Equal("3002222222", paciente.Telefono);
            Assert.Equal("new@test.com", paciente.CorreoElectronico);
        }

        #endregion

        #region EsTokenRecuperacionValido

        [Fact]
        public void EsTokenRecuperacionValido_ConTokenNulo_DeberiaRetornarFalse()
        {
            // Arrange
            var paciente = new Paciente
            {
                TokenRecuperacion = null,
                TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(15)
            };

            // Act
            var result = paciente.EsTokenRecuperacionValido();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EsTokenRecuperacionValido_ConTokenVacio_DeberiaRetornarFalse()
        {
            // Arrange
            var paciente = new Paciente
            {
                TokenRecuperacion = string.Empty,
                TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(15)
            };

            // Act
            var result = paciente.EsTokenRecuperacionValido();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EsTokenRecuperacionValido_ConExpiracionNula_DeberiaRetornarFalse()
        {
            // Arrange
            var paciente = new Paciente
            {
                TokenRecuperacion = "token123",
                TokenRecuperacionExpiracion = null
            };

            // Act
            var result = paciente.EsTokenRecuperacionValido();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EsTokenRecuperacionValido_ConTokenExpirado_DeberiaRetornarFalse()
        {
            // Arrange
            var paciente = new Paciente
            {
                TokenRecuperacion = "token123",
                TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(-1)
            };

            // Act
            var result = paciente.EsTokenRecuperacionValido();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EsTokenRecuperacionValido_ConTokenValido_DeberiaRetornarTrue()
        {
            // Arrange
            var paciente = new Paciente
            {
                TokenRecuperacion = "token123",
                TokenRecuperacionExpiracion = DateTime.UtcNow.AddMinutes(15)
            };

            // Act
            var result = paciente.EsTokenRecuperacionValido();

            // Assert
            Assert.True(result);
        }

        #endregion

        #region PropiedadesPorDefecto

        [Fact]
        public void Constructor_DeberiaInicializarPropiedadesPorDefecto()
        {
            // Act
            var paciente = new Paciente();

            // Assert
            Assert.NotEqual(Guid.Empty, paciente.Id);
            Assert.Equal(0, paciente.IntentosOTPFallidos);
            Assert.False(paciente.EstaVerificado);
            Assert.True(paciente.FechaRegistro <= DateTime.UtcNow);
            Assert.True(paciente.FechaRegistro > DateTime.UtcNow.AddSeconds(-5));
            Assert.Empty(paciente.NombreCompleto);
            Assert.Empty(paciente.TipoDocumento);
            Assert.Empty(paciente.NumeroDocumento);
            Assert.Empty(paciente.Sexo);
            Assert.Empty(paciente.Telefono);
            Assert.Empty(paciente.CorreoElectronico);
            Assert.Empty(paciente.PasswordHash);
            Assert.Empty(paciente.Eps);
            Assert.Empty(paciente.TipoSangre);
        }

        #endregion
    }
}