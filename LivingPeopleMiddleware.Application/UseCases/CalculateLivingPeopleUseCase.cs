using LivingPeopleMiddleware.Application.Models;
using LivingPeopleMiddleware.Domain.Interfaces;

namespace LivingPeopleMiddleware.Application.UseCases
{
    /// <summary>
    /// Caso de uso del cálculo de personas vivas.
    /// Orquesta la validación de entrada y la delegación al dominio.
    /// Es la pieza que consumen el middleware y el controlador HTTP.
    /// </summary>
    public class CalculateLivingPeopleUseCase
    {
        private readonly ILivingPeopleCalculator _calculator;

        /// <summary>Inyecta el cálculo del dominio.</summary>
        public CalculateLivingPeopleUseCase(ILivingPeopleCalculator calculator)
        {
            _calculator = calculator;
        }

        /// <summary>
        /// Ejecuta el cálculo. Devuelve un resultado vacío si la entrada es nula
        /// o no contiene personas.
        /// </summary>
        public LivingPeopleResult Execute(int[][] people)
        {
            if (people == null || people.Length == 0)
                return new LivingPeopleResult(System.Array.Empty<int>(), 0);

            var bestYears = _calculator.GetBestYears(people);
            return new LivingPeopleResult(bestYears, people.Length);
        }
    }
}
