using LivingPeopleMiddleware.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace LivingPeopleMiddleware.Api.Controllers
{
    /// <summary>
    /// Controlador HTTP del endpoint de cálculo. Documentado con Swagger.
    ///
    /// IMPORTANTE: si el middleware está activo, este controlador queda como
    /// respaldo documentado en Swagger; el middleware se ejecuta antes y gana.
    /// Para usar solo el controlador, comenta el UseMiddleware en Startup.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LivingPeopleController : ControllerBase
    {
        private readonly CalculateLivingPeopleUseCase _useCase;

        /// <summary>Inyecta el caso de uso.</summary>
        public LivingPeopleController(CalculateLivingPeopleUseCase useCase)
        {
            _useCase = useCase;
        }

        /// <summary>
        /// Calcula el año con mayor número de personas vivas.
        /// </summary>
        /// <param name="people">Arreglo de [añoNacimiento, añoMuerte].</param>
        /// <returns>Arreglo con el año de máximo.</returns>
        /// <response code="200">Cálculo exitoso.</response>
        /// <response code="400">Input inválido o vacío.</response>
        [HttpPost]
        [ProducesResponseType(typeof(int[]), 200)]
        [ProducesResponseType(400)]
        public IActionResult Post([FromBody] int[][] people)
        {
            if (people == null || people.Length == 0)
                return BadRequest("Input inválido.");

            var result = _useCase.Execute(people);
            return Ok(result.BestYears);
        }

    }
}
