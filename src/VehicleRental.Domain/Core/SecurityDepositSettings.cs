using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Core;

/// <summary>
/// Parámetros del cálculo dinámico del depósito de garantía.
/// Los valores por defecto son PROVISIONALES y configurables (sección "SecurityDeposit" del appsettings):
/// la plataforma debe definir los valores reales con su área financiera/legal.
/// </summary>
public sealed class SecurityDepositSettings
{
    /// <summary>Porcentaje del valor comercial estimado que sirve de base (0.03 = 3 %).</summary>
    public decimal CommercialValuePercentage { get; set; } = 0.03m;

    /// <summary>Incremento proporcional por cada día de alquiler (0.02 = 2 % por día).</summary>
    public decimal DailyDurationIncrement { get; set; } = 0.02m;

    /// <summary>Días máximos que se toman en cuenta para el incremento por duración.</summary>
    public int MaxDurationDays { get; set; } = 30;

    /// <summary>Importe mínimo del depósito.</summary>
    public decimal MinimumAmount { get; set; } = 5000m;

    /// <summary>Multiplicador por categoría (clave = nombre de <see cref="VehicleCategory"/>).</summary>
    public Dictionary<string, decimal> CategoryMultipliers { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(VehicleCategory.Economy)] = 1.00m,
        [nameof(VehicleCategory.Compact)] = 1.00m,
        [nameof(VehicleCategory.Sedan)] = 1.10m,
        [nameof(VehicleCategory.Suv)] = 1.25m,
        [nameof(VehicleCategory.Pickup)] = 1.25m,
        [nameof(VehicleCategory.Van)] = 1.20m,
        [nameof(VehicleCategory.Luxury)] = 1.50m,
    };

    /// <summary>Multiplicador por nivel de riesgo (clave = nombre de <see cref="RiskLevel"/>).</summary>
    public Dictionary<string, decimal> RiskMultipliers { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(RiskLevel.Low)] = 0.90m,
        [nameof(RiskLevel.Medium)] = 1.00m,
        [nameof(RiskLevel.High)] = 1.30m,
    };
}
