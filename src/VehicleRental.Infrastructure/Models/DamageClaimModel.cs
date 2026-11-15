using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record DamageClaimCreateModel
{
    [Range(1, int.MaxValue)]
    public int ReservationId { get; init; }

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal ClaimedAmount { get; init; }

    /// <summary>Inspección que sustenta la reclamación (opcional al crearla).</summary>
    [Range(1, int.MaxValue)]
    public int? VehicleInspectionId { get; init; }
}

public sealed record DamageClaimLinkInspectionModel
{
    [Range(1, int.MaxValue)]
    public int VehicleInspectionId { get; init; }
}

public sealed record DamageClaimModel(
    int Id,
    int ReservationId,
    int? VehicleInspectionId,
    string Description,
    decimal ClaimedAmount,
    DamageClaimStatus Status,
    string? ResolutionNotes,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
