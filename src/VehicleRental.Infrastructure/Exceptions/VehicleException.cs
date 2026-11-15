namespace VehicleRental.Infrastructure.Exceptions;

public sealed class VehicleException : PersistenceException
{
    public VehicleException(string message)
        : base(message)
    {
    }

    public static VehicleException DuplicatePlate(string plate)
        => new($"Ya existe un vehículo con la placa {plate}.");

    public static VehicleException HasReservations(int vehicleId)
        => new($"El vehículo {vehicleId} tiene reservas y no puede eliminarse; desactívelo en su lugar.");

    public static VehicleException OverlappingAvailability(int vehicleId)
        => new($"El período se solapa con otro período de disponibilidad del vehículo {vehicleId}.");
}
