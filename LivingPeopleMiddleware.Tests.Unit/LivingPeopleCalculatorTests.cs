using LivingPeopleMiddleware.Domain.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace LivingPeopleMiddleware.Tests.Unit
{
    /// <summary>
    /// Tests del algoritmo de dominio. Cubren los 3 ejemplos del enunciado y
    /// los edge cases: vacío, null, tie-breaker y muerte compartida.
    /// </summary>
    public class LivingPeopleCalculatorTests
    {
        private readonly LivingPeopleCalculator _sut = new LivingPeopleCalculator();

        /// <summary>Casos exactos del enunciado del examen.</summary>
        public static IEnumerable<object[]> EnunciadoCases()
        {
            yield return new object[]
            {
                new[] { new[] { 1951, 2018 }, new[] { 1981, 2000 }, new[] { 1983, 1984 } },
                new[] { 1983 }
            };
            yield return new object[]
            {
                new[] { new[] { 1951, 2018 }, new[] { 1981, 2000 }, new[] { 1980, 1982 }, new[] { 1983, 1984 } },
                new[] { 1981 }
            };
            yield return new object[]
            {
                new[]
                {
                    new[] { 1951, 2018 }, new[] { 1981, 2000 }, new[] { 1980, 1982 },
                    new[] { 1983, 1984 }, new[] { 2000, 2018 }, new[] { 2003, 2016 },
                    new[] { 2005, 2015 }
                },
                new[] { 2005 }
            };
        }

        /// <summary>Verifica los 3 ejemplos del enunciado.</summary>
        [Theory]
        [MemberData(nameof(EnunciadoCases))]
        public void GetBestYears_EnunciadoCases_ReturnsExpected(int[][] input, int[] expected)
            => Assert.Equal(expected, _sut.GetBestYears(input));

        /// <summary>Entrada vacía devuelve arreglo vacío sin lanzar.</summary>
        [Fact]
        public void GetBestYears_EmptyArray_ReturnsEmpty()
            => Assert.Empty(_sut.GetBestYears(Array.Empty<int[]>()));

        /// <summary>Entrada null devuelve arreglo vacío sin lanzar.</summary>
        [Fact]
        public void GetBestYears_Null_ReturnsEmpty()
            => Assert.Empty(_sut.GetBestYears(null));

        /// <summary>Una sola persona devuelve su año de nacimiento.</summary>
        [Fact]
        public void GetBestYears_SinglePerson_ReturnsBirthYear()
            => Assert.Equal(new[] { 1951 }, _sut.GetBestYears(new[] { new[] { 1951, 2018 } }));

        /// <summary>En empate, elige el año más antiguo que alcanza el máximo.</summary>
        [Fact]
        public void GetBestYears_TieBreaker_PicksEarliestMax()
        {
            var input = new[] { new[] { 1990, 1995 }, new[] { 1991, 1995 } };
            Assert.Equal(new[] { 1991 }, _sut.GetBestYears(input));
        }

        /// <summary>Mismo año de muerte: el -1 en death+1 resta a ambas correctamente.</summary>
        [Fact]
        public void GetBestYears_SameDeathYear_HandlesDeltaCorrectly()
        {
            var input = new[] { new[] { 1990, 2000 }, new[] { 1995, 2000 } };
            Assert.Equal(new[] { 1995 }, _sut.GetBestYears(input));
        }
    }
}
