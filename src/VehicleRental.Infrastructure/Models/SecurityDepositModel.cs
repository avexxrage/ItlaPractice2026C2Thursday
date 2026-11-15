using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

/// <summary>Confirmación de retención recibida del proveedor de pagos.</summary>
public sealed record SecurityDepositHoldModel
{
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal HeldAmount { get; init; }

    [Required, StringLength(100)]
    public string PaymentProviderReference { get; init; } = string.Empty;
}

/// <summary>Aplicación del depósito a una reclamación aceptada.</summary>
public sealed record SecurityDepositRetainModel
{
    [Range(1, int.MaxValue)]
    public int DamageClaimId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }

    [Required, StringLength(500)]
    public string Notes { get; init; } = string.Empty;
}

public sealed record SecurityDepositModel(
    int Id,
    int ReservationId,
    decimal CalculatedAmount,
    decimal HeldAmount,
    decimal RetainedAmount,
    SecurityDepositStatus Status,
    string? PaymentProviderReference,
    string? Notes,
    DateTime CreatedAt,
    DateTime? HeldAt,
    DateTime? ReleasedAt);
