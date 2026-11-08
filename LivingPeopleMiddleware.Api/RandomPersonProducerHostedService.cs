using LivingPeopleMiddleware.Application.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivingPeopleMiddleware.Api
{
    /// <summary>
    /// Productor aleatorio (Punto 2). Solo se registra si Producer:Enabled = true.
    /// Delega la lógica en GenerateRandomPersonUseCase para mantener el host delgado.
    /// Tolera fallos del broker: si publicar lanza excepción, loguea y continúa.
    /// </summary>
    public class RandomPersonProducerHostedService : BackgroundService
    {
        private readonly GenerateRandomPersonUseCase _useCase;
        private readonly ILogger<RandomPersonProducerHostedService> _logger;

        /// <summary>Inyecta el caso de uso y el logger.</summary>
        public RandomPersonProducerHostedService(
            GenerateRandomPersonUseCase useCase,
            ILogger<RandomPersonProducerHostedService> logger)
        {
            _useCase = useCase;
            _logger = logger;
        }


        /// <summary>
        /// Bucle principal: publica una persona por segundo hasta que se cancele.
        /// Captura excepciones del broker para no tumbar el host.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _useCase.ExecuteAsync(stoppingToken);
                    _logger.LogInformation("Persona aleatoria publicada en el bus.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo publicar la persona. ¿Está el broker disponible?");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
