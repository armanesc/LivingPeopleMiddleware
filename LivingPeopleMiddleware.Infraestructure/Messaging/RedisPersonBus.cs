using LivingPeopleMiddleware.Application.Abstractions;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using LivingPeopleMiddleware.Infraestructure.Options;

namespace LivingPeopleMiddleware.Infraestructure.Messaging
{
    /// <summary>
    /// Implementación de <see cref="IPersonBus"/> sobre Redis Pub/Sub.
    ///
    /// Redis Pub/Sub es "fire and forget": si el suscriptor no está, el mensaje
    /// se pierde. Es suficiente para este examen porque el foco es el cálculo
    /// y la integración, no la garantía de entrega.
    ///
    /// Igual que RabbitMQ, la conexión es lazy y tolerante a fallos.
    /// </summary>
    public class RedisPersonBus : IPersonBus
    {
        private readonly RedisOptions _options;
        private readonly ILogger<RedisPersonBus> _logger;

        private IConnectionMultiplexer _redis;
        private ISubscriber _subscriber;
        private readonly object _lock = new object();

        /// <summary>Constructor sin side effects: solo guarda configuración y logger.</summary>
        public RedisPersonBus(IOptions<BusOptions> options, ILogger<RedisPersonBus> logger)
        {
            _options = options.Value.Redis;
            _logger = logger;
        }

        /// <summary>
        /// Garantiza conexión y suscriptor activos. Idempotente y thread-safe.
        /// Si Redis no está disponible, lanza RedisConnectionException.
        /// </summary>
        private void EnsureConnected()
        {
            if (_redis != null && _redis.IsConnected && _subscriber != null)
                return;

            lock (_lock)
            {
                if (_redis != null && _redis.IsConnected && _subscriber != null)
                    return;

                _logger.LogInformation("Conectando a Redis en {Connection}...", _options.ConnectionString);
                _redis = ConnectionMultiplexer.Connect(_options.ConnectionString);
                _subscriber = _redis.GetSubscriber();
                _logger.LogInformation("Conexión a Redis establecida.");
            }
        }

        /// <inheritdoc />
        public async Task PublishPersonAsync(int[] person, CancellationToken ct = default)
        {
            EnsureConnected();
            var payload = JsonSerializer.Serialize(person);
            await _subscriber.PublishAsync(_options.InputChannel, payload);
            _logger.LogInformation("Redis: persona publicada {Person}", person);
        }

        /// <inheritdoc />
        public async Task SubscribePersonsAsync(Func<int[], Task> handler, CancellationToken ct = default)
        {
            EnsureConnected();
            await _subscriber.SubscribeAsync(_options.InputChannel, async (_, message) =>
            {
                var person = JsonSerializer.Deserialize<int[]>(message);
                await handler(person);
            });
        }

        /// <inheritdoc />
        public async Task PublishResultAsync(int[] years, CancellationToken ct = default)
        {
            EnsureConnected();
            var payload = JsonSerializer.Serialize(years);
            await _subscriber.PublishAsync(_options.OutputChannel, payload);
            _logger.LogInformation("Redis: resultado publicado {Years}", years);
        }

        /// <inheritdoc />
        public async Task SubscribeResultsAsync(Func<int[], Task> handler, CancellationToken ct = default)
        {
            EnsureConnected();
            await _subscriber.SubscribeAsync(_options.OutputChannel, async (_, message) =>
            {
                var years = JsonSerializer.Deserialize<int[]>(message);
                await handler(years);
            });
        }

        /// <summary>Libera la conexión a Redis si fue creada.</summary>
        public void Dispose() => _redis?.Dispose();
    }
}
