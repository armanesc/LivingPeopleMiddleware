using LivingPeopleMiddleware.Application.UseCases;
using LivingPeopleMiddleware.Domain.Interfaces;
using LivingPeopleMiddleware.Domain.Services;
using LivingPeople.Middleware.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LivingPeopleMiddleware.Tests.Integration
{
    /// <summary>
    /// Tests del middleware en aislamiento con DefaultHttpContext.
    /// Verifican detección de ruta, delegación al caso de uso y control del _next.
    /// </summary>
    public class LivingPeopleMiddlewareTests
    {
        /// <summary>Crea un DefaultHttpContext con método, path y body controlados.</summary>
        private static DefaultHttpContext CreateContext(string method, string path, string body)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Method = method;
            ctx.Request.Path = path;
            ctx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            ctx.Response.Body = new MemoryStream();
            return ctx;
        }

        /// <summary>
        /// Caso feliz (ejemplo 1): el middleware procesa y NO llama al _next.
        /// Espera [1983].
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidInput_ReturnsBestYear()
        {
            // Arrange
            var calculator = new LivingPeopleCalculator();
            var useCase = new CalculateLivingPeopleUseCase(calculator);
            var nextCalled = false;

            var mw = new PeopleMiddleware(
                _ => { nextCalled = true; return Task.CompletedTask; },
                useCase);

            var ctx = CreateContext("POST", "/api/living-people",
                "[[1951,2018],[1981,2000],[1983,1984]]");

            // Act
            await mw.InvokeAsync(ctx);

            // Assert
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
            var result = JsonSerializer.Deserialize<int[]>(body);

            Assert.Equal(new[] { 1983 }, result);
            Assert.False(nextCalled);
        }

        /// <summary>
        /// Ruta no coincidente: llama al _next y NO invoca al calculator.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_NonMatchingRoute_CallsNext()
        {
            // Arrange
            var calc = new Mock<ILivingPeopleCalculator>();
            var useCase = new CalculateLivingPeopleUseCase(calc.Object);
            var nextCalled = false;

            var mw = new PeopleMiddleware(
                _ => { nextCalled = true; return Task.CompletedTask; },
                useCase);

            var ctx = CreateContext("GET", "/otra_ruta", "");

            // Act
            await mw.InvokeAsync(ctx);

            // Assert
            Assert.True(nextCalled);
            calc.Verify(c => c.GetBestYears(It.IsAny<int[][]>()), Times.Never);
        }
    }
}
