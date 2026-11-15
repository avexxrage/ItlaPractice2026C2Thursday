using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record VehicleInspectionCreateModel
{
    [Range(1, int.MaxValue)]
    public int ReservationId { get; init; }

    public InspectionType Type { get; init; }

    [Range(0, int.MaxValue)]
    public int Mileage { get; init; }

    [Range(0, 100)]
    public int FuelLevelPercentage { get; init; }

    [StringLength(1000)]
    public string? Observations { get; init; }
}

public sealed record InspectionEvidenceCreateModel
{
    [Required, StringLength(500)]
    public string FileReference { get; init; } = string.Empty;

    public EvidenceType EvidenceType { get; init; }

    [StringLength(250)]
    public string? Description { get; init; }
}

public sealed record InspectionEvidenceModel(
    int Id,
    int VehicleInspectionId,
    string FileReference,
    EvidenceType EvidenceType,
    string? Description,
    DateTime RegisteredAt);

public sealed record VehicleInspectionModel(
    int Id,
    int ReservationId,
    InspectionType Type,
    int Mileage,
    int FuelLevelPercentage,
    string? Observations,
    DateTime InspectedAt,
    IReadOnlyList<InspectionEvidenceModel> Evidences);
