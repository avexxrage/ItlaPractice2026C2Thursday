using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Reserva de un vehículo por un arrendatario. Controla su ciclo de vida:
/// Pendiente → Confirmada → Activa → Completada, o Cancelada.
/// </summary>
public sealed class Reservation : BaseEntity
{
    public const int CancellationReasonMaxLength = 500;

    private readonly List<VehicleInspection> _inspections = new();
    private readonly List<DamageClaim> _damageClaims = new();

    private Reservation()
    {
    }

    public Reservation(
        int vehicleId,
        int renterId,
        int providerId,
        DateTime startDate,
        DateTime endDate,
        decimal dailyRate,
        RiskLevel riskLevel)
    {
        VehicleId = Guard.ValidId(vehicleId, nameof(VehicleId));
        RenterId = Guard.ValidId(renterId, nameof(RenterId));
        ProviderId = Guard.ValidId(providerId, nameof(ProviderId));
        Guard.DateRange(startDate, endDate, "de inicio", "de finalización");

        if (startDate.Date < DateTime.UtcNow.Date)
        {
            throw new DomainException("La reserva no puede comenzar en una fecha pasada.");
        }

        StartDate = startDate;
        EndDate = endDate;
        DailyRate = Guard.Positive(dailyRate, "precio por día");
        RiskLevel = Guard.Defined(riskLevel, nameof(RiskLevel));
        TotalPrice = DailyRate * RentalDays;
        Status = ReservationStatus.Pending;
    }

    public int VehicleId { get; private set; }

    public Vehicle? Vehicle { get; private set; }

    /// <summary>Usuario que alquila (arrendatario).</summary>
    public int RenterId { get; private set; }

    public User? Renter { get; private set; }

    public int ProviderId { get; private set; }

    public Provider? Provider { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    /// <summary>Precio por día acordado al momento de reservar.</summary>
    public decimal DailyRate { get; private set; }

    public decimal TotalPrice { get; private set; }

    public RiskLevel RiskLevel { get; private set; }

    public ReservationStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public SecurityDeposit? SecurityDeposit { get; private set; }

    public IReadOnlyCollection<VehicleInspection> Inspections => _inspections;

    public IReadOnlyCollection<DamageClaim> DamageClaims => _damageClaims;

    /// <summary>Días facturables (fracciones de día cuentan como día completo).</summary>
    public int RentalDays => Math.Max(1, (int)Math.Ceiling((EndDate - StartDate).TotalDays));

    public bool Overlaps(DateTime start, DateTime end) => StartDate < end && start < EndDate;

    /// <summary>
    /// Confirma la reserva. Toda reserva confirmada debe tener un depósito obligatorio, cuyo importe
    /// se calcula dinámicamente fuera de la entidad (<see cref="SecurityDepositCalculator"/>).
    /// </summary>
    public SecurityDeposit Confirm(decimal calculatedDepositAmount)
    {
        EnsureStatus(ReservationStatus.Pending, "confirmar");
        SecurityDeposit = new SecurityDeposit(this, calculatedDepositAmount);
        Status = ReservationStatus.Confirmed;
        return SecurityDeposit;
    }

    /// <summary>Entrega del vehículo: requiere depósito retenido e inspección de entrega.</summary>
    public void Start()
    {
        EnsureStatus(ReservationStatus.Confirmed, "iniciar");

        if (SecurityDeposit is null || SecurityDeposit.Status != SecurityDepositStatus.Held)
        {
            throw new DomainException("El depósito de garantía debe estar retenido antes de entregar el vehículo.");
        }

        if (!_inspections.Any(i => i.Type == InspectionType.Delivery))
        {
            throw new DomainException("Debe registrarse la inspección de entrega antes de iniciar la reserva.");
        }

        Status = ReservationStatus.Active;
    }

    /// <summary>Devolución del vehículo: requiere inspección de devolución.</summary>
    public void Complete()
    {
        EnsureStatus(ReservationStatus.Active, "completar");

        if (!_inspections.Any(i => i.Type == InspectionType.Return))
        {
            throw new DomainException("Debe registrarse la inspección de devolución antes de completar la reserva.");
        }

        Status = ReservationStatus.Completed;
    }

    public void Cancel(string reason)
    {
        if (Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
        {
            throw new DomainException("Solo se pueden cancelar reservas pendientes o confirmadas.");
        }

        CancellationReason = Guard.NotEmpty(reason, "motivo de cancelación", CancellationReasonMaxLength);
        Status = ReservationStatus.Cancelled;
    }

    public VehicleInspection RegisterInspection(InspectionType type, int mileage, int fuelLevelPercentage, string? observations)
    {
        var allowedStatus = type == InspectionType.Delivery ? ReservationStatus.Confirmed : ReservationStatus.Active;
        if (Status != allowedStatus)
        {
            throw new DomainException(type == InspectionType.Delivery
                ? "La inspección de entrega solo se registra en reservas confirmadas."
                : "La inspección de devolución solo se registra en reservas activas.");
        }

        if (_inspections.Any(i => i.Type == type))
        {
            throw new DomainException("Ya existe una inspección de ese tipo para la reserva.");
        }

        var delivery = _inspections.FirstOrDefault(i => i.Type == InspectionType.Delivery);
        if (type == InspectionType.Return && delivery is not null && mileage < delivery.Mileage)
        {
            throw new DomainException("El kilometraje de devolución no puede ser menor que el de entrega.");
        }

        var inspection = new VehicleInspection(this, type, mileage, fuelLevelPercentage, observations);
        _inspections.Add(inspection);
        return inspection;
    }

    /// <summary>Abre una reclamación por daños. No declara responsable al arrendatario: inicia la evaluación.</summary>
    public DamageClaim OpenDamageClaim(string description, decimal claimedAmount)
    {
        if (Status is not (ReservationStatus.Active or ReservationStatus.Completed))
        {
            throw new DomainException("Solo se pueden reclamar daños de reservas activas o completadas.");
        }

        var claim = new DamageClaim(this, description, claimedAmount);
        _damageClaims.Add(claim);
        return claim;
    }

    private void EnsureStatus(ReservationStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new DomainException($"No se puede {action} una reserva en estado {Status}.");
        }
    }
}
