using LivingPeople.Middleware.Api.Middleware;
using LivingPeopleMiddleware.Application.Abstractions;
using LivingPeopleMiddleware.Application.UseCases;
using LivingPeopleMiddleware.Domain.Interfaces;
using LivingPeopleMiddleware.Domain.Services;
using LivingPeopleMiddleware.Infraestructure.Messaging;
using LivingPeopleMiddleware.Infraestructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;

namespace LivingPeopleMiddleware.Api
{
    /// <summary>
    /// Configuración del host: registro de DI y pipeline HTTP.
    /// El orden y los lifetimes están pensados para que la API arranque sin broker.
    /// </summary>
    public class Startup
    {
        /// <summary>Constructor estándar de ASP.NET Core.</summary>
        public Startup(IConfiguration configuration) => Configuration = configuration;

        /// <summary>Configuración de la aplicación.</summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Registra servicios de todas las capas.
        /// - Domain: calculator Singleton (stateless, thread-safe).
        /// - Application: casos de uso Singleton (sin estado salvo el buffer del consumidor).
        /// - Infrastructure: IPersonBus Strategy según Bus:Provider.
        /// - Swagger.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            // ----- Opciones -----
            services.Configure<BusOptions>(Configuration.GetSection("Bus"));

            // ----- Domain -----
            services.AddSingleton<ILivingPeopleCalculator, LivingPeopleCalculator>();

            // ----- Application (casos de uso) -----
            services.AddSingleton<CalculateLivingPeopleUseCase>();
            services.AddSingleton<ProcessPersonMessageUseCase>();
            services.AddSingleton<GenerateRandomPersonUseCase>();

            // ----- Infrastructure (Strategy según configuración) -----
            var provider = Configuration.GetSection("Bus:Provider").Value;
            if (string.Equals(provider, "Redis", StringComparison.OrdinalIgnoreCase))
                services.AddSingleton<IPersonBus, RedisPersonBus>();
            else
                services.AddSingleton<IPersonBus, RabbitMqPersonBus>();

            // ----- Hosted service del productor (opcional) -----
            var producerEnabled = Configuration.GetValue<bool>("Producer:Enabled");
            if (producerEnabled)
                services.AddHostedService<RandomPersonProducerHostedService>();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LivingPeople API",
                    Version = "v1",
                    Description = "Cálculo del año con mayor número de personas vivas."
                });
            });


        }

        /// <summary>
        /// Configura el pipeline HTTP.
        /// El arranque del consumidor está envuelto en try/catch: si el broker no
        /// está disponible, la API sigue operando para el endpoint de cálculo.
        /// Swagger se expone en /swagger.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ProcessPersonMessageUseCase processUseCase, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Arranque tolerante del consumidor: no tumba la API si no hay broker.
            try
            {
                processUseCase.StartAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "No se pudo suscribir al bus al arrancar. La API seguirá operando sin consumidor.");
            }

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LivingPeople API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();            
            app.UseRouting();            
            app.UseMiddleware<PeopleMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
