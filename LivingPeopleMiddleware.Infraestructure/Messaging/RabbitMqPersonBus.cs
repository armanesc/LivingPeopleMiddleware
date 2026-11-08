using LivingPeopleMiddleware.Application.Abstractions;
using LivingPeopleMiddleware.Infraestructure.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LivingPeopleMiddleware.Infraestructure.Messaging
{
    /// <summary>
    /// Implementación de <see cref="IPersonBus"/> sobre RabbitMQ.
    ///
    /// Decisión clave: la conexión es LAZY. No se conecta en el constructor,
    /// sino en el primer uso (EnsureConnected). Esto permite que la API arranque
    /// aunque RabbitMQ no esté disponible, y que los tests de integración no
    /// dependan del broker. RabbitMQ.Client 6.x tiene AutomaticRecoveryEnabled,
    /// que se activa aquí para reconectar si el broker cae después.
    ///
    /// Registrado como Singleton: mantiene una sola conexión/canal para toda la app.
    /// </summary>
    public class RabbitMqPersonBus : IPersonBus
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPersonBus> _logger;

        private IConnection _connection;
        private IModel _channel;
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor sin side effects: solo guarda configuración y logger.
        /// La conexión se establece en el primer Publish/Subscribe.
        /// </summary>
        public RabbitMqPersonBus(IOptions<BusOptions> options, ILogger<RabbitMqPersonBus> logger)
        {
            _options = options.Value.RabbitMq;
            _logger = logger;
        }

        /// <summary>
        /// Garantiza que exista conexión y canal abiertos. Es idempotente y
        /// thread-safe (doble check con lock). Si el broker no está, lanza
        /// BrokerUnreachableException, que el llamador decide cómo manejar.
        /// </summary>
        private void EnsureConnected()
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                return;

            lock (_lock)
            {
                if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
                    return;

                _logger.LogInformation("Conectando a RabbitMQ en {Host}:5672...", _options.HostName);

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(30)
                };

                _connection = factory.CreateConnection("LivingPeople.Api");
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(_options.InputQueue, durable: false, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(_options.OutputQueue, durable: false, exclusive: false, autoDelete: false);

                _logger.LogInformation("Conexión a RabbitMQ establecida.");
            }
        }





        /// <inheritdoc />
        public Task PublishPersonAsync(int[] person, CancellationToken ct = default)
        {
            EnsureConnected();
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(person));
            _channel.BasicPublish("", _options.InputQueue, null, body);
            _logger.LogInformation("RabbitMQ: persona publicada {Person}", person);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SubscribePersonsAsync(Func<int[], Task> handler, CancellationToken ct = default)
        {
            EnsureConnected();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var person = JsonSerializer.Deserialize<int[]>(ea.Body.ToArray());
                await handler(person);
            };
            _channel.BasicConsume(_options.InputQueue, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task PublishResultAsync(int[] years, CancellationToken ct = default)
        {
            EnsureConnected();
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(years));
            _channel.BasicPublish("", _options.OutputQueue, null, body);
            _logger.LogInformation("RabbitMQ: resultado publicado {Years}", years);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SubscribeResultsAsync(Func<int[], Task> handler, CancellationToken ct = default)
        {
            EnsureConnected();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var years = JsonSerializer.Deserialize<int[]>(ea.Body.ToArray());
                await handler(years);
            };
            _channel.BasicConsume(_options.OutputQueue, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        /// <summary>Libera canal y conexión si fueron creados.</summary>
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

    }
}
