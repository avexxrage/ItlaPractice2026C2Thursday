namespace VehicleRental.Domain.Core.Enums;

public enum VehicleStatus
{
    /// <summary>Registrado pero aún no publicado.</summary>
    Draft = 1,

    /// <summary>Publicado y disponible para reservas.</summary>
    Active = 2,

    /// <summary>Retirado temporalmente por el proveedor.</summary>
    Inactive = 3,

    UnderMaintenance = 4,
}
