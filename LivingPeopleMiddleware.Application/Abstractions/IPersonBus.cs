using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivingPeopleMiddleware.Application.Abstractions
{
    /// <summary>
    /// Abstracción del bus de mensajería (Strategy).
    /// Vive en Application para respetar la regla de dependencias de Clean
    /// Architecture: Infrastructure implementa esta interfaz, no al revés.
    ///
    /// Permite intercambiar RabbitMQ por Redis (o cualquier otro broker) sin
    /// tocar los casos de uso.
    /// </summary>
    public interface IPersonBus : IDisposable
    {

        /// <summary>Publica una persona (arreglo [birth, death]) en el bus de entrada.</summary>
        Task PublishPersonAsync(int[] person, CancellationToken ct = default);

        /// <summary>Suscribe un handler que recibe las personas del bus de entrada.</summary>
        Task SubscribePersonsAsync(Func<int[], Task> handler, CancellationToken ct = default);

        /// <summary>Publica el resultado (años con más vivos) en el bus de salida.</summary>
        Task PublishResultAsync(int[] years, CancellationToken ct = default);

        /// <summary>Suscribe un handler al bus de salida (servicio que consume resultados).</summary>
        Task SubscribeResultsAsync(Func<int[], Task> handler, CancellationToken ct = default);
    }
}
