using LivingPeopleMiddleware.Application.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivingPeopleMiddleware.Application.UseCases
{
    /// <summary>
    /// Caso de uso del productor aleatorio (Punto 2 del enunciado).
    /// Encapsula la generación de una persona válida y su publicación en el bus.
    /// El BackgroundService de la API solo lo invoca en bucle.
    /// </summary>
    public class GenerateRandomPersonUseCase
    {
        private readonly IPersonBus _bus;
        private readonly Random _random = new Random();

        /// <summary>Inyecta el bus de mensajería.</summary>
        public GenerateRandomPersonUseCase(IPersonBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>
        /// Genera una persona con nacimiento entre 1950 y 2020 y muerte entre
        /// 1 y 79 años después, y la publica en el bus de entrada.
        /// </summary>
        public async Task ExecuteAsync(CancellationToken ct = default)
        {
            int birth = _random.Next(1950, 2020);
            int death = birth + _random.Next(1, 80);
            await _bus.PublishPersonAsync(new[] { birth, death }, ct);
        }
    }
}
