namespace VehicleRental.Domain.Core.Enums;

public enum SecurityDepositStatus
{
    /// <summary>Calculado; a la espera de que el proveedor de pagos confirme la retención.</summary>
    Pending = 1,

    /// <summary>Retenido según la confirmación del proveedor de pagos.</summary>
    Held = 2,

    /// <summary>Liberado completamente al arrendatario.</summary>
    Released = 3,

    /// <summary>Se retuvo una parte (o todo) para cubrir una reclamación aceptada.</summary>
    Retained = 4,
}
