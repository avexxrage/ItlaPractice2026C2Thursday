using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

/// <summary>
/// Solicitud de reserva. El precio se toma del vehículo; las fechas no se editan:
/// para cambiarlas se cancela y se crea una nueva reserva.
/// </summary>
public sealed record ReservationCreateModel
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; init; }

    [Range(1, int.MaxValue)]
    public int RenterId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public RiskLevel RiskLevel { get; init; } = RiskLevel.Medium;
}

/// <summary>Filtros opcionales para listar reservas.</summary>
public sealed record ReservationFilterModel
{
    public int? RenterId { get; init; }

    public int? VehicleId { get; init; }

    public int? ProviderId { get; init; }

    public ReservationStatus? Status { get; init; }
}

public sealed record ReservationModel(
    int Id,
    int VehicleId,
    string VehicleDescription,
    int RenterId,
    string RenterFullName,
    int ProviderId,
    DateTime StartDate,
    DateTime EndDate,
    int RentalDays,
    decimal DailyRate,
    decimal TotalPrice,
    RiskLevel RiskLevel,
    ReservationStatus Status,
    string? CancellationReason,
    int? SecurityDepositId,
    decimal? SecurityDepositAmount,
    SecurityDepositStatus? SecurityDepositStatus,
    DateTime CreatedAt);

/// <summary>Datos que necesita el cálculo dinámico del depósito.</summary>
public sealed record ReservationDepositInputModel(
    int ReservationId,
    decimal EstimatedCommercialValue,
    VehicleCategory Category,
    int RentalDays,
    RiskLevel RiskLevel);
