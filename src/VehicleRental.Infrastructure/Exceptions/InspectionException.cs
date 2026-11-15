namespace VehicleRental.Infrastructure.Exceptions;

public sealed class InspectionException : PersistenceException
{
    public InspectionException(string message)
        : base(message)
    {
    }

    public static InspectionException MileageBelowVehicle(int recordedMileage)
        => new($"El kilometraje de la inspección no puede ser menor que el registrado en el vehículo ({recordedMileage} km).");
}
