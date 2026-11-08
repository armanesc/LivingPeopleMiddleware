using LivingPeopleMiddleware.Application.Abstractions;
using LivingPeopleMiddleware.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LivingPeopleMiddleware.Application.UseCases
{
    /// <summary>
    /// Caso de uso del consumidor (Punto 3 del enunciado).
    /// Se suscribe al bus de entrada, acumula las personas recibidas, recalcula
    /// el mejor año y publica el resultado en el bus de salida.
    ///
    /// Nota de diseño: el buffer es en memoria y se recalcula por mensaje.
    /// En un entorno productivo se implementaría una ventana de tiempo o
    /// tamaño de lote para evitar recalcular en cada mensaje.
    /// </summary>
    public class ProcessPersonMessageUseCase
    {
        private readonly IPersonBus _bus;
        private readonly ILivingPeopleCalculator _calculator;
        private readonly ILogger<ProcessPersonMessageUseCase> _logger;

        private readonly List<int[]> _buffer = new List<int[]>();
        private readonly object _lock = new object();

        public ProcessPersonMessageUseCase(
            IPersonBus bus,
            ILivingPeopleCalculator calculator,
            ILogger<ProcessPersonMessageUseCase> logger)
        {
            _bus = bus;
            _calculator = calculator;
            _logger = logger;
        }

        /// <summary>
        /// Inicia la suscripción al bus de entrada. Puede lanzar excepción si el
        /// broker no está disponible; el llamador (Startup) la captura para
        /// permitir que la API arranque sin broker.
        /// </summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            await _bus.SubscribePersonsAsync(OnPersonReceived, ct);
            _logger.LogInformation("ProcessPersonMessageUseCase suscrito al bus de entrada.");
        }

        /// <summary>
        /// Handler interno: agrega la persona al buffer, recalcula y publica el resultado.
        /// </summary>
        private async Task OnPersonReceived(int[] person)
        {
            int[][] snapshot;
            lock (_lock)
            {
                _buffer.Add(person);
                snapshot = _buffer.ToArray();
            }

            var bestYears = _calculator.GetBestYears(snapshot);
            await _bus.PublishResultAsync(bestYears);

            _logger.LogInformation(
                "Procesadas {Count} personas. Mejor año: {Year}",
                snapshot.Length, bestYears.Length > 0 ? bestYears[0] : 0);
        }
    }
}
