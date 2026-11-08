namespace LivingPeopleMiddleware.Infraestructure.Options
{
    /// <summary>
    /// Opciones tipadas del bus, enlazadas desde la sección "Bus" de appsettings.
    /// </summary>
    public class BusOptions
    {
        /// <summary>Proveedor activo: "RabbitMQ" o "Redis".</summary>
        public string Provider { get; set; } = "RabbitMQ";

        /// <summary>Configuración de RabbitMQ.</summary>
        public RabbitMqOptions RabbitMq { get; set; } = new RabbitMqOptions();

        /// <summary>Configuración de Redis.</summary>
        public RedisOptions Redis { get; set; } = new RedisOptions();
    }

    /// <summary>Configuración específica de RabbitMQ.</summary>
    public class RabbitMqOptions
    {
        /// <summary>Host del broker.</summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>Cola de entrada de personas.</summary>
        public string InputQueue { get; set; } = "person_births";

        /// <summary>Cola de salida de resultados.</summary>
        public string OutputQueue { get; set; } = "living_people_result";
    }

    /// <summary>Configuración específica de Redis.</summary>
    public class RedisOptions
    {
        /// <summary>Cadena de conexión (host:puerto).</summary>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>Canal Pub/Sub de entrada.</summary>
        public string InputChannel { get; set; } = "person_births_channel";

        /// <summary>Canal Pub/Sub de salida.</summary>
        public string OutputChannel { get; set; } = "living_people_result_channel";
    }
}
