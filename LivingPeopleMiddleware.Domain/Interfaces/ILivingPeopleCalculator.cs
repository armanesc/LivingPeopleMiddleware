namespace LivingPeopleMiddleware.Domain.Interfaces
{
    /// <summary>
    /// Contrato del cálculo de años con mayor número de personas vivas.
    /// Se abstrae para permitir mockearlo en tests del middleware y del caso de uso.
    /// </summary>
    public interface ILivingPeopleCalculator
    {
        /// <summary>
        /// Calcula el/los años con el mayor número de personas vivas simultáneamente.
        /// </summary>
        /// <param name="people">
        /// Arreglo de arreglos donde cada elemento es [añoNacimiento, añoMuerte].
        /// </param>
        /// <returns>
        /// Arreglo con el año (o años) que alcanzan el máximo. Si la entrada es
        /// nula o vacía, devuelve un arreglo vacío.
        /// </returns>
        int[] GetBestYears(int[][] people);
    }
}
