# ExchangeRate API — Documentación

API en ASP.NET Core (.NET 10) que aplica un tipo de cambio a un monto. Incluye autenticación JWT,
persistencia en SQLite en memoria y ejecución en Docker.

---

## 1. Arquitectura

Clean Architecture en 3 proyectos. **La dependencia siempre apunta hacia el Core.**

```
ExchangeRate (Api)  ──►  ExchangeRate.Core  ◄──  ExchangeRate.Infrastructure
        │                                                    ▲
        └────────────── (registra las dependencias) ─────────┘
```

| Proyecto | Responsabilidad | Depende de |
|---|---|---|
| **ExchangeRate.Core** | Entidad, interfaces, DTOs y la lógica de negocio (`ExchangeService`). | Nadie |
| **ExchangeRate.Infrastructure** | Acceso a datos: EF Core + SQLite, `AppDbContext` y repositorio. | Core |
| **ExchangeRate** (Api) | Controllers, JWT, configuración y `Program.cs`. | Core, Infrastructure |

### Estructura

```
ExchangeRate/
├─ ExchangeRate.slnx
├─ docker-compose.yml
├─ ExchangeRate.postman_collection.json
├─ ExchangeRate.Core/
│   ├─ Entities/CurrencyRate.cs
│   ├─ Interfaces/ICurrencyRateRepository.cs, IExchangeService.cs
│   ├─ Services/ExchangeService.cs
│   └─ Dtos/ExchangeDtos.cs
├─ ExchangeRate.Infrastructure/
│   ├─ Persistence/AppDbContext.cs
│   ├─ Repositories/CurrencyRateRepository.cs
│   └─ DependencyInjection.cs
└─ ExchangeRate/
    ├─ Controllers/AuthController.cs, ExchangeController.cs
    ├─ Settings/AuthSettings.cs
    ├─ Program.cs, appsettings.json, Dockerfile
```

### Patrones y principios

| Patrón / principio | Dónde |
|---|---|
| Clean Architecture (regla de dependencia) | Separación en Core / Infrastructure / Api |
| Repository | `ICurrencyRateRepository` (Core) → `CurrencyRateRepository` (Infrastructure) |
| Service Layer | `ExchangeService` contiene la lógica; el controller solo traduce HTTP |
| DTO | La entidad nunca se expone; la API usa records de `Dtos/` |
| Dependency Injection / Inversión de dependencias | El servicio depende de la interfaz del repositorio, no de EF Core |
| Options Pattern | `AuthSettings` se lee de la sección `Auth` de `appsettings.json` |

### Flujo de una conversión

```
POST /api/exchange/convert  (Bearer JWT)
  → ExchangeController
    → IExchangeService.ConvertAsync
      → ICurrencyRateRepository.GetAsync(from, to)   (SQLite)
    ← ConvertResponse   (o 404 si no existe el par)
```

### Reglas de negocio

- Los códigos de moneda se normalizan a mayúsculas (`usd` → `USD`).
- Se busca el par exacto (`USD→PEN`). Si no existe, se responde `404`.
- `montoConvertido = monto × tasa`, redondeado a **2 decimales** (`MidpointRounding.AwayFromZero`).
- Al **actualizar** una tasa se guarda también su **inverso** (`1 / tasa`, 6 decimales), para que ambos
  sentidos sean consistentes. Ej.: `USD→PEN = 2` guarda además `PEN→USD = 0.5`. Si el par no existe, se crea.
- Origen y destino deben ser distintos al actualizar (`400` si son iguales).

### Persistencia

SQLite **en memoria** con una conexión compartida (singleton). Los datos se reinician al reiniciar la app.

Tasas iniciales (seed), en ambos sentidos:

| From | To | Rate |
|---|---|---|
| USD | PEN | 3.75 |
| PEN | USD | 0.2667 |
| EUR | PEN | 4.05 |
| PEN | EUR | 0.2469 |
| USD | EUR | 0.92 |
| EUR | USD | 1.087 |

---

## 2. Entidad

### `CurrencyRate`

Representa el tipo de cambio de una moneda origen a una moneda destino.

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Id` | `int` | Identificador (clave primaria). |
| `FromCurrency` | `string` (3) | Moneda origen, código ISO de 3 letras (ej. `USD`). |
| `ToCurrency` | `string` (3) | Moneda destino (ej. `PEN`). |
| `Rate` | `decimal(18,6)` | Cuántas unidades de destino equivalen a 1 de origen. |
| `UpdatedAt` | `DateTime` (UTC) | Fecha de la última actualización. |

Restricción: índice **único** sobre (`FromCurrency`, `ToCurrency`).

---

## 3. Endpoints

Base URL: `http://localhost:8080` (con Docker o con `dotnet run`, usan el mismo puerto).

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | No | Devuelve un JWT. |
| POST | `/api/exchange/convert` | JWT | Aplica el tipo de cambio a un monto. |
| POST | `/api/exchange/rate` | JWT | Crea o actualiza un tipo de cambio. |

Los endpoints protegidos requieren el header `Authorization: Bearer <token>`.

### POST `/api/auth/login`

Usuario de demostración: `admin` / `admin123` (configurable en `appsettings.json`).

Request:
```json
{ "username": "admin", "password": "admin123" }
```
Response `200`:
```json
{ "token": "eyJhbGciOi...", "expiresAt": "2026-11-11T02:47:48Z" }
```
Errores: `400` (campos faltantes), `401` (credenciales inválidas).

### POST `/api/exchange/convert`

Request:
```json
{ "amount": 100, "fromCurrency": "USD", "toCurrency": "PEN" }
```
Response `200`:
```json
{
  "amount": 100,
  "convertedAmount": 375.00,
  "fromCurrency": "USD",
  "toCurrency": "PEN",
  "exchangeRate": 3.75
}
```

| Campo | Validación |
|---|---|
| `amount` | Entre 0.01 y 1 000 000 000 000 |
| `fromCurrency`, `toCurrency` | Obligatorios, exactamente 3 letras |

Errores: `400` (validación), `401` (sin token o token inválido), `404` (no existe tipo de cambio para el par).

### POST `/api/exchange/rate`

Request:
```json
{ "fromCurrency": "USD", "toCurrency": "PEN", "rate": 3.80 }
```
Response `200`:
```json
{ "fromCurrency": "USD", "toCurrency": "PEN", "rate": 3.80, "updatedAt": "2026-11-11T01:47:48Z" }
```

| Campo | Validación |
|---|---|
| `fromCurrency`, `toCurrency` | Obligatorios, exactamente 3 letras |
| `rate` | Mayor a 0 (mínimo 0.000001) |

Además de guardar el par indicado, guarda automáticamente el inverso (`1 / rate`).

Errores: `400` (validación, o monedas iguales), `401` (sin token o token inválido).

### Códigos de error

Los errores de validación devuelven el formato estándar `ProblemDetails`:
```json
{ "title": "One or more validation errors occurred.", "status": 400,
  "errors": { "Amount": ["El monto debe ser mayor a 0."] } }
```

---

## 4. Ejecución

### Ejecución rápida desde la terminal

Ejecutar siempre desde la **carpeta raíz de la solución** (donde están `ExchangeRate.slnx` y `docker-compose.yml`).
`ExchangeRate.Core` es una biblioteca de clases, no se ejecuta: los proyectos ejecutables son la API
(`ExchangeRate`) y el front (`ExchangeRate.Web`). Levantar primero la API.

```powershell
cd "C:\ruta\a\ExchangeRate"

# Terminal 1: API  →  http://localhost:8080
dotnet run --project ExchangeRate --launch-profile http

# Terminal 2: Front  →  http://localhost:5242
dotnet run --project ExchangeRate.Web --launch-profile http
```

Con Docker (levanta API y front a la vez): `docker compose up --build`.

| Opción | Significado |
|---|---|
| `--project <carpeta>` | Proyecto a ejecutar (la solución tiene varios). |
| `--launch-profile http` | Perfil de `Properties/launchSettings.json` (define URL y entorno `Development`). |

### Con Docker
```bash
docker compose up -d --build     # levanta la API en http://localhost:8080
docker compose logs -f           # ver logs
docker compose down              # detener y eliminar el contenedor
```

### Sin Docker
```bash
dotnet run --project ExchangeRate/ExchangeRate.csproj --launch-profile http   # http://localhost:8080
```

### Configuración (`appsettings.json`, sección `Auth`)

| Clave | Descripción |
|---|---|
| `Key` | Clave de firma del JWT (mínimo 32 caracteres). **Cambiar en producción.** |
| `Issuer`, `Audience` | Emisor y audiencia del token. |
| `ExpirationMinutes` | Vigencia del token (60 por defecto). |
| `Username`, `Password` | Usuario de demostración. |

En Docker, la clave se sobrescribe con la variable `AUTH_KEY` (`Auth__Key` en el contenedor).

### Front (Blazor Server)
```bash
# Con Docker (API + Web):
docker compose up -d --build        # API en :8080, Web en http://localhost:5242

# Sin Docker (dos terminales):
dotnet run --project ExchangeRate/ExchangeRate.csproj --launch-profile http          # API en :8080
dotnet run --project ExchangeRate.Web/ExchangeRate.Web.csproj --launch-profile http  # Web en :5242
```
Local y Docker usan los mismos puertos (API `8080`, Web `5242`), por lo que `Api:BaseUrl` en
`ExchangeRate.Web/appsettings.json` (`http://localhost:8080`) funciona en ambos modos. No se pueden
usar los dos modos a la vez porque comparten puertos. En Docker, el compose lo sobrescribe con
`http://exchangerate-api:8080`.

### Postman

Importar `ExchangeRate.postman_collection.json`, ejecutar primero **1. Login** (guarda el token en
`{{token}}`) y luego el resto de las peticiones. `baseUrl` es `http://localhost:8080` por defecto.

---

## 5. Evolución prevista

El diseño permite crecer sin reescribir:

- **Separar Core** en `Domain` + `Application` moviendo carpetas (entidades e interfaz del repositorio
  a Domain; servicio y DTOs a Application).
- **Cambiar de base de datos** (SQL Server, PostgreSQL) tocando solo `Infrastructure`.
- **Agregar CQRS/MediatR** handler por handler si el sistema crece.

---

## 6. Front — ExchangeRate.Web (Blazor Server)

Cliente web que consume la API. Es **Blazor Server**: el navegador solo habla con Blazor, y Blazor
llama a la API desde el servidor. Por eso no requiere CORS y el JWT nunca llega al navegador.

```
ExchangeRate.Web/
├─ Components/
│   ├─ Layout/MainLayout.razor        (barra superior con usuario y "Cerrar sesión")
│   └─ Pages/
│       ├─ Login.razor                (/login)
│       └─ Home.razor                 (/  → convertir monto + actualizar tipo de cambio)
├─ Services/
│   ├─ ExchangeApiClient.cs           (HttpClient tipado hacia la API)
│   └─ AuthState.cs                   (token de la sesión, Scoped por conexión)
├─ Models/Forms.cs                    (modelos de formulario con validación)
└─ appsettings.json                   (Api:BaseUrl)
```

- Reutiliza los DTOs de `ExchangeRate.Core` para requests y responses.
- Si la API responde 401 (token vencido), se cierra la sesión y se vuelve al login.
- El token se guarda en memoria: al recargar la página (F5) se pide login de nuevo.
