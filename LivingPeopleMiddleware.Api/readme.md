# LivingPeopleMiddleware – Examen Técnico .NET Core 3.1

## 📋 Descripción

Solución que implementa el cálculo del año con mayor número de personas vivas a partir
de un arreglo de años de nacimiento/muerte, junto con un productor y consumidor sobre
RabbitMQ o Redis (intercambiables). Incluye Swagger, tests unitarios e integración, y
está diseñada para **ejecutarse y testearse sin necesidad de instalar ningún broker**.

## 🏗️ Arquitectura

Clean Architecture pragmática con 4 capas y 2 proyectos de tests:

| Proyecto | Tipo | Responsabilidad |
|---|---|---|
| `LivingPeopleMiddleware.Domain` | Class Library | Lógica pura (`LivingPeopleCalculator`). Sin dependencias. |
| `LivingPeopleMiddleware.Application` | Class Library | Casos de uso y abstracciones (`IPersonBus`). |
| `LivingPeopleMiddleware.Infrastructure` | Class Library | Implementaciones RabbitMQ y Redis. |
| `LivingPeopleMiddleware.Api` | Web API | Host, middleware, Swagger y wiring DI. |
| `LivingPeopleMiddleware.Tests.Unit` | xUnit | Algoritmo y casos de uso. |
| `LivingPeopleMiddleware.Tests.Integration` | xUnit | Pipeline HTTP y middleware. |

Regla de dependencias: `Api → Application + Infrastructure`; `Infrastructure → Application + Domain`; `Application → Domain`; `Domain → (nada)`.

## ⚙️ Requisitos previos

- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- Visual Studio 2019 (16.4+) o VS Code
- **Docker (opcional)** — solo para probar el flujo de mensajería real
- **No se requiere RabbitMQ ni Redis instalados** para ejecutar la API ni los tests

## 🚀 Cómo ejecutar la API (sin broker)

```bash
dotnet run --project LivingPeopleMiddleware.Api


La API arranca en https://localhost:44334/ (o el puerto configurado). Verás en el log
un warning de RabbitMQ no disponible; es esperado y no afecta al endpoint.

Probar en Swagger
Abre https://localhost:44334/swagger en el navegador. El endpoint POST /api/living-people
está documentado y se puede ejecutar desde la UI.

Probar en Postman
text
POST https://localhost:44334/api/living-people
Content-Type: application/json
body:
[[1951,2018],[1981,2000],[1980,1982],[1983,1984]]
Respuesta esperada: [1981].

Si Postman rechaza el certificado autofirmado, desactiva SSL certificate verification
en Settings, o ejecuta la API con dotnet run --project LivingPeople.Api --urls "http://localhost:5000".

##Habilitar el flujo de mensajería (opcional)
RabbitMQ
bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
Consola web: http://localhost:15672 (guest/guest). Colas: person_births, living_people_result.

Redis
bash
docker run -d --name redis -p 6379:6379 redis
Canales Pub/Sub: person_births_channel, living_people_result_channel.


##Activar el productor aleatorio
Por defecto Producer:Enabled = false. Para activarlo:

bash
dotnet run --project LivingPeople.Api --Producer:Enabled=true
O cambiar en appsettings.Development.json.

Cambiar de RabbitMQ a Redis
Cambiar en appsettings.json:

json
"Bus": { "Provider": "Redis" }
El Strategy en Startup.ConfigureServices inyecta RedisPersonBus automáticamente.

##📡 Endpoints
POST /api/living-people
Calcula el año con más personas vivas.

Request:

json
[[1951, 2018], [1981, 2000], [1983, 1984]]
Response (200):

json
[1983]
Response (400): JSON inválido o arreglo vacío.

##📝 Decisiones de diseño
Algoritmo O(n + rango) con delta array: +1 en nacimiento, -1 en muerte+1, suma acumulativa.

Middleware delgado: solo HTTP. Lógica en CalculateLivingPeopleUseCase → LivingPeopleCalculator.

Strategy para el bus: IPersonBus abstrae RabbitMQ/Redis; se intercambia por configuración.

Conexión lazy al broker: RabbitMqPersonBus y RedisPersonBus no conectan en el constructor.
Esto permite que la API arranque sin broker y que los tests no dependan de Docker.

Arranque tolerante: Startup.Configure captura fallos de suscripción; la API sigue operando.

TestWebApplicationFactory con NoOpPersonBus: los tests de integración no requieren broker.

Swagger documenta el endpoint: el examinador puede probar sin Postman.