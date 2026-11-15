namespace VehicleRental.Domain.Core.Enums;

public enum DocumentType
{
    /// <summary>Matrícula del vehículo.</summary>
    Registration = 1,

    /// <summary>Póliza de seguro.</summary>
    Insurance = 2,

    /// <summary>Marbete / revista vigente.</summary>
    TaxSticker = 3,

    TechnicalInspection = 4,
    Other = 5,
}
