using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Core;

/// <summary>
/// Servicio de dominio que calcula el importe del depósito de garantía a partir del valor comercial,
/// la categoría del vehículo, la duración del alquiler y el nivel de riesgo.
/// Solo calcula un importe: no retiene ni mueve dinero. La retención real la ejecuta un proveedor de pagos.
/// El depósito NO es el límite máximo de responsabilidad del arrendatario.
/// </summary>
public sealed class SecurityDepositCalculator
{
    private readonly SecurityDepositSettings _settings;

    public SecurityDepositCalculator(SecurityDepositSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.CommercialValuePercentage <= 0 || settings.MinimumAmount < 0 ||
            settings.DailyDurationIncrement < 0 || settings.MaxDurationDays < 1)
        {
            throw new DomainException("La configuración del depósito de garantía no es válida.");
        }

        _settings = settings;
    }

    public decimal Calculate(decimal estimatedCommercialValue, VehicleCategory category, int rentalDays, RiskLevel riskLevel)
    {
        Guard.Positive(estimatedCommercialValue, "valor comercial estimado");
        if (rentalDays < 1)
        {
            throw new DomainException("La duración del alquiler debe ser de al menos un día.");
        }

        var baseAmount = estimatedCommercialValue * _settings.CommercialValuePercentage;
        var durationFactor = 1m + (_settings.DailyDurationIncrement * Math.Min(rentalDays, _settings.MaxDurationDays));
        var amount = baseAmount
            * Multiplier(_settings.CategoryMultipliers, category.ToString())
            * Multiplier(_settings.RiskMultipliers, riskLevel.ToString())
            * durationFactor;

        return Math.Round(Math.Max(amount, _settings.MinimumAmount), 2, MidpointRounding.AwayFromZero);
    }

    private static decimal Multiplier(IReadOnlyDictionary<string, decimal> multipliers, string key)
    {
        if (!multipliers.TryGetValue(key, out var value) || value <= 0)
        {
            throw new DomainException($"No hay un multiplicador de depósito válido configurado para '{key}'.");
        }

        return value;
    }
}
