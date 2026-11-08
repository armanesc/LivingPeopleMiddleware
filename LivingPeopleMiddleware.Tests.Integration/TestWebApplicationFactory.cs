using LivingPeopleMiddleware.Api;
using LivingPeopleMiddleware.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LivingPeopleMiddleware.Tests.Integration
{
    /// <summary>
    /// Fábrica de host de pruebas. Arranca el Startup real, pero reemplaza el
    /// IPersonBus por un NoOp para no requerir RabbitMQ ni Redis en CI.
    ///
    /// Gracias al arranque tolerante de Startup.Configure, incluso si el
    /// NoOp no estuviera, el host arrancaría; pero el reemplazo garantiza
    /// que ningún test intente conectar al broker.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        /// <summary>Reemplaza el bus real por el no-op.</summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Reemplaza el bus real por un no-op para no requerir broker en CI.
                services.AddSingleton<IPersonBus, NoOpPersonBus>();
            });
        }

        /// <summary>Bus no-op: todas las operaciones completan sin efecto.</summary>
        private class NoOpPersonBus : IPersonBus
        {
            public Task PublishPersonAsync(int[] person, CancellationToken ct = default)
                => Task.CompletedTask;

            public Task SubscribePersonsAsync(Func<int[], Task> handler, CancellationToken ct = default)
                => Task.CompletedTask;

            public Task PublishResultAsync(int[] years, CancellationToken ct = default)
                => Task.CompletedTask;

            public Task SubscribeResultsAsync(Func<int[], Task> handler, CancellationToken ct = default)
                => Task.CompletedTask;

            public void Dispose() { }
        }
    }
}
