using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LivingPeopleMiddleware.Tests.Integration
{
    /// <summary>
    /// Tests del pipeline HTTP completo (middleware → caso de uso → dominio).
    /// Usan TestWebApplicationFactory, por lo que no requieren broker.
    /// </summary>
    public class ApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        /// <summary>xUnit inyecta la fábrica y construye el cliente en memoria.</summary>
        public ApiIntegrationTests(TestWebApplicationFactory factory)
            => _client = factory.CreateClient();

        /// <summary>
        /// Caso feliz: ejemplo 2 del enunciado. Valida wiring DI, pipeline HTTP,
        /// serialización y resultado correcto [1981].
        /// </summary>
        [Fact]
        public async Task Post_LivingPeople_ReturnsExpectedYear()
        {
            var json = "[[1951,2018],[1981,2000],[1980,1982],[1983,1984]]";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/living-people", content);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<int[]>(body);

            Assert.Equal(new[] { 1981 }, result);
        }

        /// <summary>
        /// JSON inválido: el middleware devuelve 400 controlado, no 500.
        /// </summary>
        [Fact]
        public async Task Post_InvalidJson_ReturnsBadRequest()
        {
            var content = new StringContent("no-es-json", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/living-people", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
