using System;
using System.Collections.Generic;
using System.Text;

namespace LivingPeopleMiddleware.Application.Models
{
    /// <summary>
    /// Resultado tipado del caso de uso de cálculo.
    /// Evita devolver int[] crudo desde la capa de aplicación y permite
    /// enriquecer la respuesta sin romper el contrato HTTP.
    /// </summary>
    public sealed class LivingPeopleResult
    {
        /// <summary>Años que alcanzan el máximo de personas vivas.</summary>
        public int[] BestYears { get; }

        /// <summary>Cantidad total de personas procesadas.</summary>
        public int TotalPeople { get; }

        /// <summary>Construye el resultado con los años y el total procesado.</summary>
        public LivingPeopleResult(int[] bestYears, int totalPeople)
        {
            BestYears = bestYears;
            TotalPeople = totalPeople;
        }
    }
}
