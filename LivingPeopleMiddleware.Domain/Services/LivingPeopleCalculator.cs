using LivingPeopleMiddleware.Domain.Interfaces;
using System;
using System.Linq;

namespace LivingPeopleMiddleware.Domain.Services
{
        /// <summary>
        /// Implementación O(n + rango) del cálculo de personas vivas por año.
        ///
        /// Estrategia (Delta Array):
        ///   - Por cada persona, se marca +1 en su año de nacimiento y -1 en el
        ///     año inmediatamente posterior a su muerte.
        ///   - La suma acumulativa del array de deltas da el número de personas
        ///     vivas en cada año.
        ///   - Se recorre una sola vez para encontrar el primer año con el máximo.
        ///
        /// Complejidad: O(n + (maxYear - minYear)) en tiempo, O(maxYear - minYear) en espacio.
        /// Es stateless y thread-safe: seguro para registrar como Singleton.
        /// </summary>
        public sealed class LivingPeopleCalculator : ILivingPeopleCalculator
        {
            /// <inheritdoc />
            public int[] GetBestYears(int[][] people)
            {
                if (people == null || people.Length == 0)
                    return Array.Empty<int>();

                int minYear = people.Min(p => p[0]);
                int maxYear = people.Max(p => p[1]);

                // +2: uno por el offset (minYear -> índice 0) y otro para el
                // decremento en (deathYear + 1) sin salirnos del array.
                int[] delta = new int[maxYear - minYear + 2];

                foreach (var p in people)
                {
                    delta[p[0] - minYear]++;
                    delta[p[1] - minYear + 1]--;
                }

                int maxAlive = 0, currentAlive = 0, bestYear = minYear;
                for (int i = 0; i < delta.Length - 1; i++)
                {
                    currentAlive += delta[i];
                    if (currentAlive > maxAlive)
                    {
                        maxAlive = currentAlive;
                        bestYear = minYear + i;
                    }
                }

                return new[] { bestYear };
            }
        }
    }
