using LivingPeopleMiddleware.Application.UseCases;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivingPeople.Middleware.Api.Middleware
{
    /// <summary>
    /// Middleware HTTP que atiende POST /api/living-people.
    ///
    /// Responsabilidad única: parsear el body, validar, delegar en el caso de uso
    /// y serializar la respuesta. La lógica de negocio NO vive aquí.
    ///
    /// Si la ruta no coincide, llama a _next para que el pipeline continúe.
    /// </summary>
    public class PeopleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CalculateLivingPeopleUseCase _useCase;

        /// <summary>Inyecta el siguiente middleware y el caso de uso.</summary>
        public PeopleMiddleware(RequestDelegate next, CalculateLivingPeopleUseCase useCase)
        {
            _next = next;
            _useCase = useCase;
        }

        /// <summary>
        /// Procesa la petición si es POST a /api/living-people. En caso contrario,
        /// delega al siguiente middleware.
        /// Devuelve 400 si el body es JSON inválido o el arreglo está vacío.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            //debes de escribir esta ruta en una peticion POST [https://localhost:44334/api/living-people]
            //pj desde postaman para que entre al middleware y no se vaya al controller            
            bool isMatch = context.Request.Method == "POST"
                           && context.Request.Path.StartsWithSegments("/api/living-people");

            if (!isMatch)
            {
                await _next(context);
                return;
            }

            int[][] input;
            try
            {
                input = await JsonSerializer.DeserializeAsync<int[][]>(context.Request.Body);
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("JSON inválido.");
                return;
            }

            if (input == null || input.Length == 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Input inválido: se esperaba un arreglo no vacío.");
                return;
            }

            var result = _useCase.Execute(input);
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(result.BestYears));
        }
    }
}
