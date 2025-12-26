using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using MedCitas.Web.Controllers;
using MedCitas.Web.Models;

namespace MedCitas.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _controller = new HomeController();
        }

        [Fact]
        public void Index_DeberiaRetornarViewResult()
        {
            // Act
            var resultado = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public void Privacy_DeberiaRetornarViewResult()
        {
            // Act
            var resultado = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(resultado);
        }

        [Fact]
        public void Error_DeberiaRetornarViewResultConErrorViewModel()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var resultado = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(resultado);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void Error_ConActivityActual_DeberiaUsarActivityId()
        {
            // Arrange
            var activity = new Activity("test-activity");
            activity.Start();

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var resultado = _controller.Error();
            activity.Stop();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(resultado);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
            Assert.Equal(activity.Id, model.RequestId);
        }

        [Fact]
        public void Error_SinActivityActual_DeberiaUsarTraceIdentifier()
        {
            // Arrange
            var expectedTraceId = "expected-trace-123";
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = expectedTraceId;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Detener cualquier activity actual
            Activity.Current?.Stop();

            // Act
            var resultado = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(resultado);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(expectedTraceId, model.RequestId);
        }

        [Fact]
        public void Constructor_DeberiaInicializarController()
        {
            // Arrange & Act
            var controller = new HomeController();

            // Assert
            Assert.NotNull(controller);
        }
    }
}
