using LivingPeopleMiddleware.Application.UseCases;
using LivingPeopleMiddleware.Domain.Interfaces;
using Moq;
using Xunit;

namespace LivingPeopleMiddleware.Tests.Unit
{
    /// <summary>
    /// Tests del caso de uso. Verifican la orquestación (validación + delegación)
    /// sin depender de la implementación del cálculo.
    /// </summary>
    public class CalculateLivingPeopleUseCaseTests
    {
        /// <summary>Entrada vacía: no invoca al calculator y devuelve resultado vacío.</summary>
        [Fact]
        public void Execute_EmptyInput_ReturnsEmptyWithoutCallingCalculator()
        {
            var calc = new Mock<ILivingPeopleCalculator>();
            var sut = new CalculateLivingPeopleUseCase(calc.Object);

            var result = sut.Execute(System.Array.Empty<int[]>());

            Assert.Empty(result.BestYears);
            Assert.Equal(0, result.TotalPeople);
            calc.Verify(c => c.GetBestYears(It.IsAny<int[][]>()), Times.Never);
        }

        /// <summary>Entrada válida: delega al calculator una vez y reporta el total.</summary>
        [Fact]
        public void Execute_ValidInput_DelegatesToCalculator()
        {
            var calc = new Mock<ILivingPeopleCalculator>();
            calc.Setup(c => c.GetBestYears(It.IsAny<int[][]>())).Returns(new[] { 1983 });
            var sut = new CalculateLivingPeopleUseCase(calc.Object);

            var input = new[] { new[] { 1951, 2018 }, new[] { 1983, 1984 } };
            var result = sut.Execute(input);

            Assert.Equal(new[] { 1983 }, result.BestYears);
            Assert.Equal(2, result.TotalPeople);
            calc.Verify(c => c.GetBestYears(input), Times.Once);
        }
    }
}
