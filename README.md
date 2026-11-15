# VehicleRental: plataforma de alquiler de vehículos (RD)

Capas **Domain** e **Infrastructure** de una plataforma tipo Airbnb para alquilar vehículos de
propietarios particulares y empresas Rent a Car, consumidas por una **API REST**.

- .NET 11, ASP.NET Core (controladores)
- Entity Framework Core 11 + **SQLite** (`vehicle_rental.db`, se crea al iniciar la API)
- xUnit para pruebas (SQLite en memoria, mismo motor que la API)

## Estructura

```
VehicleRental.slnx
src/
  VehicleRental.Domain/            (sin dependencias de EF Core ni de Infrastructure)
    Core/          BaseEntity, Person, DomainException, Guard, Enums/,
                   SecurityDepositCalculator + SecurityDepositSettings (servicio de dominio)
    Entities/      User, Provider, Company, Vehicle, VehicleDocument, VehicleAvailability,
                   Reservation, SecurityDeposit, VehicleInspection, InspectionEvidence, DamageClaim
    Repository/    IBaseRepository<TEntity> (contrato genérico)
  VehicleRental.Infrastructure/
    Context/       VehicleRentalContext + Configurations/ (Fluent API por entidad)
    Core/          BaseRepository<TEntity>, SensitiveData
    Exceptions/    PersistenceException, EntityNotFoundException y una excepción por área
    Interfaces/    IUserRepository, IVehicleRepository, IReservationRepository... (heredan IBaseRepository)
    Models/        DTOs de creación, actualización y respuesta
    Repositories/  Implementaciones concretas
  VehicleRental.API/
    Controllers/   Un controlador por recurso
    Extensions/    Registro de DbContext, repositorios y servicios (DI)
    Middleware/    Traducción de excepciones a ProblemDetails (400/404/409)
    Services/      ReservationService (confirmación + depósito dinámico)
tests/
  VehicleRental.Tests/  Pruebas de dominio y de repositorios
```

Igual que en la referencia (`School.Domain` / `School.Infrastructure`), el contrato genérico vive en
`Domain/Repository` y los contratos específicos de cada entidad en `Infrastructure/Interfaces`.
Así no se duplica ningún contrato.

## Reglas de negocio implementadas

- Un usuario alquila solo si está **activo y verificado** como arrendatario.
- Un proveedor es **particular** (ligado a un usuario) o **Rent a Car** (ligado a una empresa). Una
  empresa solo se verifica como proveedor si la empresa ya está verificada.
- Un vehículo pertenece a un proveedor, tiene placa única y solo se publica si el proveedor está verificado.
- Las reservas no se solapan con otras reservas activas ni con períodos bloqueados del vehículo.
  Un proveedor no puede reservar su propio vehículo.
- Ciclo de la reserva: `Pending → Confirmed → Active → Completed` o `Cancelled`.
  - **Confirmar** crea el **depósito obligatorio**, calculado dinámicamente.
  - **Iniciar** exige el depósito retenido y la inspección de entrega.
  - **Completar** exige la inspección de devolución y actualiza el kilometraje del vehículo.
- Las **reclamaciones** pasan por revisión y solo se aceptan si hay una inspección vinculada **con evidencias**.
  Nunca se declara responsable al arrendatario de forma automática.
- El depósito se libera solo con la reserva finalizada y sin reclamaciones abiertas. Si hay una
  reclamación aceptada, se **retiene** como máximo el importe reclamado.

### Depósito de garantía

`SecurityDepositCalculator` combina el valor comercial, la categoría, la duración y el riesgo con
parámetros de la sección `SecurityDeposit` del `appsettings.json`. **Los valores incluidos son
provisionales**: la fórmula definitiva debe acordarse con el área financiera y legal.

La plataforma **no mueve dinero**: solo registra lo que confirma el proveedor de pagos autorizado
(importe retenido y referencia). El depósito no es el límite de la responsabilidad del arrendatario.

## Ejecutar

1. Abrir `VehicleRental.slnx` en Visual Studio y marcar **VehicleRental.API** como proyecto de inicio.
2. Ejecutar. La base SQLite se crea automáticamente y el documento OpenAPI queda en `/openapi/v1.json`.
3. `src/VehicleRental.API/VehicleRental.API.http` trae un recorrido completo de ejemplo.
4. Pruebas: **Test > Run All Tests** (o `dotnet test`).

## Decisiones

- **Punto de partida:** el repositorio solo tenía el commit inicial (sin solución ni rama `develop`).
  Por eso la solución se creó aquí, siguiendo la estructura de la referencia.
- **Base de datos:** SQLite, porque no requiere servidor. Se usa `EnsureCreated` mientras no haya
  migraciones. Para adoptarlas: `dotnet ef migrations add InitialCreate` y cambiar a `Database.MigrateAsync()`.
- **Datos sensibles:** no se guardan contraseñas ni datos de tarjeta. La cédula se devuelve enmascarada
  y los documentos y evidencias se guardan solo como referencia al archivo.
