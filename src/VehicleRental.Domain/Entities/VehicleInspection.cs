using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Inspección del vehículo al entregarlo o al devolverlo.
/// </summary>
public sealed class VehicleInspection : BaseEntity
{
    public const int ObservationsMaxLength = 1000;

    private readonly List<InspectionEvidence> _evidences = new();

    private VehicleInspection()
    {
    }

    internal VehicleInspection(Reservation reservation, InspectionType type, int mileage, int fuelLevelPercentage, string? observations)
    {
        if (fuelLevelPercentage is < 0 or > 100)
        {
            throw new DomainException("El nivel de combustible debe estar entre 0 y 100.");
        }

        Reservation = reservation;
        ReservationId = reservation.Id;
        Type = Guard.Defined(type, nameof(Type));
        Mileage = Guard.NotNegative(mileage, nameof(Mileage));
        FuelLevelPercentage = fuelLevelPercentage;
        Observations = Guard.Optional(observations, nameof(Observations), ObservationsMaxLength);
        InspectedAt = DateTime.UtcNow;
    }

    public int ReservationId { get; private set; }

    public Reservation? Reservation { get; private set; }

    public InspectionType Type { get; private set; }

    public int Mileage { get; private set; }

    public int FuelLevelPercentage { get; private set; }

    public string? Observations { get; private set; }

    public DateTime InspectedAt { get; private set; }

    public IReadOnlyCollection<InspectionEvidence> Evidences => _evidences;

    public InspectionEvidence AddEvidence(string fileReference, EvidenceType evidenceType, string? description)
    {
        var evidence = new InspectionEvidence(this, fileReference, evidenceType, description);
        _evidences.Add(evidence);
        return evidence;
    }
}
