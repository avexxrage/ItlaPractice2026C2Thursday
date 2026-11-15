using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Vehículo publicado por un proveedor (particular o empresa).
/// </summary>
public sealed class Vehicle : BaseEntity
{
    public const int BrandMaxLength = 50;
    public const int ModelMaxLength = 50;
    public const int LicensePlateMaxLength = 10;
    public const int ColorMaxLength = 30;
    public const int DescriptionMaxLength = 1000;
    public const int MinYear = 1980;

    private readonly List<VehicleDocument> _documents = new();
    private readonly List<VehicleAvailability> _availabilities = new();
    private readonly List<Reservation> _reservations = new();

    private Vehicle()
    {
    }

    public Vehicle(
        int providerId,
        string brand,
        string model,
        int year,
        string licensePlate,
        string color,
        TransmissionType transmission,
        FuelType fuelType,
        VehicleCategory category,
        int mileage,
        decimal dailyRate,
        decimal estimatedCommercialValue,
        string? description)
    {
        ProviderId = Guard.ValidId(providerId, nameof(ProviderId));
        LicensePlate = NormalizePlate(licensePlate);
        Mileage = Guard.NotNegative(mileage, nameof(Mileage));
        UpdateDetails(brand, model, year, color, transmission, fuelType, category, dailyRate, estimatedCommercialValue, description);
        Status = VehicleStatus.Draft;
    }

    public int ProviderId { get; private set; }

    public Provider? Provider { get; private set; }

    public string Brand { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public int Year { get; private set; }

    /// <summary>Placa / matrícula. Única en la plataforma.</summary>
    public string LicensePlate { get; private set; } = string.Empty;

    public string Color { get; private set; } = string.Empty;

    public TransmissionType Transmission { get; private set; }

    public FuelType FuelType { get; private set; }

    public VehicleCategory Category { get; private set; }

    public int Mileage { get; private set; }

    /// <summary>Precio de alquiler por día.</summary>
    public decimal DailyRate { get; private set; }

    public decimal EstimatedCommercialValue { get; private set; }

    public string? Description { get; private set; }

    public VehicleStatus Status { get; private set; }

    public IReadOnlyCollection<VehicleDocument> Documents => _documents;

    public IReadOnlyCollection<VehicleAvailability> Availabilities => _availabilities;

    public IReadOnlyCollection<Reservation> Reservations => _reservations;

    public void UpdateDetails(
        string brand,
        string model,
        int year,
        string color,
        TransmissionType transmission,
        FuelType fuelType,
        VehicleCategory category,
        decimal dailyRate,
        decimal estimatedCommercialValue,
        string? description)
    {
        var maxYear = DateTime.UtcNow.Year + 1;
        if (year < MinYear || year > maxYear)
        {
            throw new DomainException($"El año del vehículo debe estar entre {MinYear} y {maxYear}.");
        }

        Brand = Guard.NotEmpty(brand, nameof(Brand), BrandMaxLength);
        Model = Guard.NotEmpty(model, nameof(Model), ModelMaxLength);
        Year = year;
        Color = Guard.NotEmpty(color, nameof(Color), ColorMaxLength);
        Transmission = Guard.Defined(transmission, nameof(Transmission));
        FuelType = Guard.Defined(fuelType, nameof(FuelType));
        Category = Guard.Defined(category, nameof(Category));
        DailyRate = Guard.Positive(dailyRate, "precio de alquiler");
        EstimatedCommercialValue = Guard.Positive(estimatedCommercialValue, "valor comercial estimado");
        Description = Guard.Optional(description, nameof(Description), DescriptionMaxLength);
    }

    /// <summary>El kilometraje solo puede aumentar.</summary>
    public void UpdateMileage(int mileage)
    {
        if (mileage < Mileage)
        {
            throw new DomainException("El kilometraje no puede ser menor que el registrado.");
        }

        Mileage = mileage;
    }

    public void Publish(bool providerVerified)
    {
        if (!providerVerified)
        {
            throw new DomainException("El proveedor debe estar verificado para publicar vehículos.");
        }

        if (Status == VehicleStatus.Active)
        {
            throw new DomainException("El vehículo ya está publicado.");
        }

        Status = VehicleStatus.Active;
    }

    public void Deactivate() => Status = VehicleStatus.Inactive;

    public void SendToMaintenance() => Status = VehicleStatus.UnderMaintenance;

    private static string NormalizePlate(string licensePlate)
    {
        var plate = Guard.NotEmpty(licensePlate, nameof(LicensePlate), 15)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

        if (plate.Length is < 5 or > LicensePlateMaxLength || !plate.All(char.IsLetterOrDigit))
        {
            throw new DomainException("La placa debe tener entre 5 y 10 caracteres alfanuméricos.");
        }

        return plate;
    }
}
