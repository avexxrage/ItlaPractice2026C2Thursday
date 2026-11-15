using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record VehicleCreateModel
{
    [Range(1, int.MaxValue)]
    public int ProviderId { get; init; }

    [Required, StringLength(50)]
    public string Brand { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string Model { get; init; } = string.Empty;

    [Range(1980, 2100)]
    public int Year { get; init; }

    [Required, StringLength(15, MinimumLength = 5)]
    public string LicensePlate { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string Color { get; init; } = string.Empty;

    public TransmissionType Transmission { get; init; }

    public FuelType FuelType { get; init; }

    public VehicleCategory Category { get; init; }

    [Range(0, int.MaxValue)]
    public int Mileage { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal DailyRate { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal EstimatedCommercialValue { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }
}

public sealed record VehicleUpdateModel
{
    [Required, StringLength(50)]
    public string Brand { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string Model { get; init; } = string.Empty;

    [Range(1980, 2100)]
    public int Year { get; init; }

    [Required, StringLength(30)]
    public string Color { get; init; } = string.Empty;

    public TransmissionType Transmission { get; init; }

    public FuelType FuelType { get; init; }

    public VehicleCategory Category { get; init; }

    [Range(0, int.MaxValue)]
    public int Mileage { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal DailyRate { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal EstimatedCommercialValue { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }
}

/// <summary>Filtros opcionales para buscar vehículos.</summary>
public sealed record VehicleFilterModel
{
    public string? Brand { get; init; }

    public VehicleCategory? Category { get; init; }

    public VehicleStatus? Status { get; init; }

    public int? ProviderId { get; init; }
}

public sealed record VehicleModel(
    int Id,
    int ProviderId,
    ProviderType ProviderType,
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string Color,
    TransmissionType Transmission,
    FuelType FuelType,
    VehicleCategory Category,
    int Mileage,
    decimal DailyRate,
    decimal EstimatedCommercialValue,
    string? Description,
    VehicleStatus Status,
    DateTime CreatedAt);
