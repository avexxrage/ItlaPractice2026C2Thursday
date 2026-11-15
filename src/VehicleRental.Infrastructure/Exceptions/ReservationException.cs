namespace VehicleRental.Infrastructure.Exceptions;

public sealed class ReservationException : PersistenceException
{
    public ReservationException(string message)
        : base(message)
    {
    }

    public static ReservationException RenterNotAllowed(int renterId)
        => new($"El usuario {renterId} no está activo o no ha completado la verificación de arrendatario.");

    public static ReservationException VehicleNotAvailable(int vehicleId)
        => new($"El vehículo {vehicleId} no está disponible en las fechas solicitadas.");

    public static ReservationException OverlappingReservation(int vehicleId)
        => new($"El vehículo {vehicleId} ya tiene una reserva en esas fechas.");

    public static ReservationException OwnVehicle()
        => new("Un proveedor no puede reservar su propio vehículo.");
}
