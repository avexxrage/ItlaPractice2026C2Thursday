using VehicleRental.Domain.Core;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Período en el que el proveedor marca el vehículo como disponible o bloqueado.
/// </summary>
public sealed class VehicleAvailability : BaseEntity
{
    public const int NotesMaxLength = 250;

    private VehicleAvailability()
    {
    }

    public VehicleAvailability(int vehicleId, DateTime startDate, DateTime endDate, bool isAvailable, string? notes)
    {
        VehicleId = Guard.ValidId(vehicleId, nameof(VehicleId));
        Update(startDate, endDate, isAvailable, notes);
    }

    public int VehicleId { get; private set; }

    public Vehicle? Vehicle { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public bool IsAvailable { get; private set; }

    public string? Notes { get; private set; }

    public void Update(DateTime startDate, DateTime endDate, bool isAvailable, string? notes)
    {
        Guard.DateRange(startDate, endDate, "inicial", "final");
        StartDate = startDate;
        EndDate = endDate;
        IsAvailable = isAvailable;
        Notes = Guard.Optional(notes, nameof(Notes), NotesMaxLength);
    }

    public bool Overlaps(DateTime start, DateTime end) => StartDate < end && start < EndDate;
}
