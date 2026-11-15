using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Infrastructure.Models;

public sealed record VehicleAvailabilityCreateModel
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    /// <summary>true = disponible; false = bloqueado (no se puede reservar).</summary>
    public bool IsAvailable { get; init; } = true;

    [StringLength(250)]
    public string? Notes { get; init; }
}

public sealed record VehicleAvailabilityUpdateModel
{
    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public bool IsAvailable { get; init; } = true;

    [StringLength(250)]
    public string? Notes { get; init; }
}

public sealed record VehicleAvailabilityModel(
    int Id,
    int VehicleId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAvailable,
    string? Notes);
