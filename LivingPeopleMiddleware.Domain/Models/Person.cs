using System;

namespace LivingPeopleMiddleware.Domain.Models
{
    /// <summary>
    /// Modelo de dominio que representa a una persona con su rango de vida.
    /// Se declara como struct readonly para evitar allocations en bucles de cálculo.
    /// </summary>
    public readonly struct Person
    {
        /// <summary>Año de nacimiento (inclusive).</summary>
        public int BirthYear { get; }

        /// <summary>Año de muerte (inclusive).</summary>
        public int DeathYear { get; }

        /// <summary>
        /// Construye una persona validando que la muerte no sea anterior al nacimiento.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Si <paramref name="deathYear"/> es menor que <paramref name="birthYear"/>.
        /// </exception>
        public Person(int birthYear, int deathYear)
        {
            if (deathYear < birthYear)
                throw new ArgumentException("El año de fallecimiento no puede ser anterior al año de nacimiento.");
            BirthYear = birthYear;
            DeathYear = deathYear;
        }
    }
}
