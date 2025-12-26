using MedCitas.Web.Models;
using Xunit;

namespace MedCitas.Tests.Models
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void RequestId_PuedeSerAsignado()
        {
 // Arrange
  var model = new ErrorViewModel();
     var requestId = "test-request-id-123";

       // Act
  model.RequestId = requestId;

            // Assert
         Assert.Equal(requestId, model.RequestId);
 }

    [Fact]
        public void RequestId_PuedeSerNull()
        {
     // Arrange
  var model = new ErrorViewModel();

            // Act
 model.RequestId = null;

  // Assert
         Assert.Null(model.RequestId);
    }

        [Fact]
        public void ShowRequestId_ConRequestIdNulo_RetornaFalse()
        {
  // Arrange
   var model = new ErrorViewModel
        {
 RequestId = null
    };

      // Act
 var result = model.ShowRequestId;

            // Assert
      Assert.False(result);
      }

        [Fact]
        public void ShowRequestId_ConRequestIdVacio_RetornaFalse()
        {
            // Arrange
     var model = new ErrorViewModel
 {
              RequestId = string.Empty
            };

  // Act
         var result = model.ShowRequestId;

         // Assert
            Assert.False(result);
  }

        [Fact]
        public void ShowRequestId_ConRequestIdValido_RetornaTrue()
        {
            // Arrange
    var model = new ErrorViewModel
            {
                RequestId = "test-request-id"
   };

         // Act
  var result = model.ShowRequestId;

            // Assert
  Assert.True(result);
 }

        [Fact]
        public void ShowRequestId_ConRequestIdConEspacios_RetornaTrue()
        {
          // Arrange
   var model = new ErrorViewModel
            {
                RequestId = "   "
            };

            // Act
            var result = model.ShowRequestId;

      // Assert
            Assert.True(result);
     }
    }
}
