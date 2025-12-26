using MedCitas.Core.Configuration;
using Xunit;

namespace MedCitas.Tests.Configuration
{
    public class EmailConfigurationTests
    {
 #region IsValid

        [Fact]
        public void IsValid_ConConfiguracionCompleta_DeberiaRetornarTrue()
      {
      // Arrange
    var config = new EmailConfiguration
         {
     SmtpHost = "smtp.gmail.com",
      SmtpPort = 587,
    SmtpUser = "test@example.com",
        SmtpPassword = "password123",
    FromEmail = "noreply@example.com",
FromName = "Test App",
       EnableSsl = true
};

   // Act
     var resultado = config.IsValid();

     // Assert
  Assert.True(resultado);
     }

        [Theory]
     [InlineData("", 587, "user", "pass", "from")]  // SmtpHost vacío
        [InlineData("host", 0, "user", "pass", "from")]   // SmtpPort inválido
    [InlineData("host", -1, "user", "pass", "from")]  // SmtpPort negativo
 [InlineData("host", 587, "", "pass", "from")]   // SmtpUser vacío
        [InlineData("host", 587, "user", "", "from")]   // SmtpPassword vacía
[InlineData("host", 587, "user", "pass", "")]   // FromEmail vacío
  [InlineData("", 0, "", "", "")]      // Todo vacío
public void IsValid_ConConfiguracionIncompleta_DeberiaRetornarFalse(
   string host, int port, string user, string password, string fromEmail)
        {
  // Arrange
            var config = new EmailConfiguration
  {
     SmtpHost = host,
    SmtpPort = port,
                SmtpUser = user,
             SmtpPassword = password,
   FromEmail = fromEmail
        };

      // Act
       var resultado = config.IsValid();

     // Assert
       Assert.False(resultado);
   }

      [Fact]
      public void IsValid_ConSmtpHostNull_DeberiaRetornarFalse()
 {
     // Arrange
var config = new EmailConfiguration
       {
SmtpHost = null!,
      SmtpPort = 587,
     SmtpUser = "user",
SmtpPassword = "pass",
          FromEmail = "from"
   };

// Act
  var resultado = config.IsValid();

            // Assert
   Assert.False(resultado);
   }

 [Fact]
  public void IsValid_ConSmtpHostSoloEspacios_DeberiaRetornarFalse()
 {
   // Arrange
            var config = new EmailConfiguration
      {
         SmtpHost = "   ",
  SmtpPort = 587,
       SmtpUser = "user",
SmtpPassword = "pass",
     FromEmail = "from"
   };

     // Act
       var resultado = config.IsValid();

       // Assert
            Assert.False(resultado);
   }

    #endregion

    #region GetValidationErrors

        [Fact]
        public void GetValidationErrors_ConConfiguracionValida_DeberiaRetornarCadenaVacia()
   {
         // Arrange
       var config = new EmailConfiguration
       {
    SmtpHost = "smtp.gmail.com",
     SmtpPort = 587,
    SmtpUser = "test@example.com",
     SmtpPassword = "password123",
       FromEmail = "noreply@example.com"
   };

 // Act
 var errores = config.GetValidationErrors();

 // Assert
   Assert.Empty(errores);
        }

        [Fact]
   public void GetValidationErrors_ConSmtpHostVacio_DeberiaIncluirError()
 {
// Arrange
     var config = new EmailConfiguration
            {
      SmtpHost = "",
    SmtpPort = 587,
    SmtpUser = "user",
  SmtpPassword = "pass",
 FromEmail = "from"
            };

   // Act
var errores = config.GetValidationErrors();

     // Assert
Assert.Contains("SmtpHost es requerido", errores);
        }

        [Fact]
public void GetValidationErrors_ConSmtpPortInvalido_DeberiaIncluirError()
 {
     // Arrange
var config = new EmailConfiguration
  {
    SmtpHost = "host",
       SmtpPort = 0,
SmtpUser = "user",
       SmtpPassword = "pass",
   FromEmail = "from"
    };

     // Act
  var errores = config.GetValidationErrors();

 // Assert
 Assert.Contains("SmtpPort debe ser mayor a 0", errores);
 }

 [Fact]
   public void GetValidationErrors_ConSmtpUserVacio_DeberiaIncluirError()
    {
    // Arrange
     var config = new EmailConfiguration
   {
     SmtpHost = "host",
         SmtpPort = 587,
       SmtpUser = "",
       SmtpPassword = "pass",
     FromEmail = "from"
      };

  // Act
  var errores = config.GetValidationErrors();

            // Assert
Assert.Contains("SmtpUser es requerido", errores);
        }

  [Fact]
   public void GetValidationErrors_ConSmtpPasswordVacia_DeberiaIncluirError()
        {
        // Arrange
  var config = new EmailConfiguration
{
    SmtpHost = "host",
       SmtpPort = 587,
       SmtpUser = "user",
  SmtpPassword = "",
       FromEmail = "from"
};

   // Act
       var errores = config.GetValidationErrors();

// Assert
 Assert.Contains("SmtpPassword es requerido", errores);
 }

  [Fact]
        public void GetValidationErrors_ConFromEmailVacio_DeberiaIncluirError()
   {
   // Arrange
  var config = new EmailConfiguration
            {
        SmtpHost = "host",
 SmtpPort = 587,
         SmtpUser = "user",
         SmtpPassword = "pass",
  FromEmail = ""
   };

// Act
    var errores = config.GetValidationErrors();

       // Assert
  Assert.Contains("FromEmail es requerido", errores);
 }

    [Fact]
 public void GetValidationErrors_ConMultiplesErrores_DeberiaIncluirTodos()
 {
    // Arrange
   var config = new EmailConfiguration
       {
    SmtpHost = "",
   SmtpPort = 0,
       SmtpUser = "",
     SmtpPassword = "",
    FromEmail = ""
  };

       // Act
   var errores = config.GetValidationErrors();

      // Assert
 Assert.Contains("SmtpHost es requerido", errores);
Assert.Contains("SmtpPort debe ser mayor a 0", errores);
    Assert.Contains("SmtpUser es requerido", errores);
       Assert.Contains("SmtpPassword es requerido", errores);
   Assert.Contains("FromEmail es requerido", errores);
   }

     [Fact]
   public void GetValidationErrors_DeberiaUsarComoSeparador()
  {
    // Arrange
  var config = new EmailConfiguration
            {
    SmtpHost = "",
      SmtpUser = ""
            };

// Act
    var errores = config.GetValidationErrors();

     // Assert
Assert.Contains(", ", errores);
   }

        #endregion

    #region Propiedades Por Defecto

   [Fact]
   public void Constructor_DeberiaInicializarPropiedadesConValoresPorDefecto()
  {
// Arrange & Act
   var config = new EmailConfiguration();

            // Assert
   Assert.Equal(string.Empty, config.SmtpHost);
  Assert.Equal(0, config.SmtpPort);
Assert.Equal(string.Empty, config.SmtpUser);
        Assert.Equal(string.Empty, config.SmtpPassword);
   Assert.Equal(string.Empty, config.FromEmail);
  Assert.Equal(string.Empty, config.FromName);
Assert.True(config.EnableSsl);
        }

 #endregion
}
}
