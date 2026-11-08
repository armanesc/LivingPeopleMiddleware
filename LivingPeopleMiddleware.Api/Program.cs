using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LivingPeopleMiddleware.Api
{
    /// <summary>
    /// Punto de entrada del host. Configura el builder por defecto y arranca Startup.
    /// </summary>
    public class Program
    {
        /// <summary>Arranca el host web.</summary>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary>Crea el host builder con Startup como clase de configuración.</summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
