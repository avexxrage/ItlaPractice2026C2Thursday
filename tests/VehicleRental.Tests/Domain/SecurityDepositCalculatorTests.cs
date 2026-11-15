using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Tests.Domain;

public class SecurityDepositCalculatorTests
{
    private readonly SecurityDepositCalculator _calculator = new(new SecurityDepositSettings());

    [Fact]
    public void Deposit_grows_with_category_risk_and_duration()
    {
        var economyLowShort = _calculator.Calculate(1_500_000m, VehicleCategory.Economy, 2, RiskLevel.Low);
        var luxury = _calculator.Calculate(1_500_000m, VehicleCategory.Luxury, 2, RiskLevel.Low);
        var highRisk = _calculator.Calculate(1_500_000m, VehicleCategory.Economy, 2, RiskLevel.High);
        var longer = _calculator.Calculate(1_500_000m, VehicleCategory.Economy, 10, RiskLevel.Low);

        Assert.True(luxury > economyLowShort);
        Assert.True(highRisk > economyLowShort);
        Assert.True(longer > economyLowShort);
    }

    [Fact]
    public void Deposit_uses_configured_formula()
    {
        // 1,000,000 * 3 % * 1.10 (Sedan) * 1.00 (Medium) * (1 + 0.02 * 5) = 36,300
        var amount = _calculator.Calculate(1_000_000m, VehicleCategory.Sedan, 5, RiskLevel.Medium);

        Assert.Equal(36_300m, amount);
    }

    [Fact]
    public void Deposit_never_goes_below_minimum()
    {
        var amount = _calculator.Calculate(10_000m, VehicleCategory.Economy, 1, RiskLevel.Low);

        Assert.Equal(new SecurityDepositSettings().MinimumAmount, amount);
    }

    [Fact]
    public void Invalid_inputs_are_rejected()
    {
        Assert.Throws<DomainException>(() => _calculator.Calculate(0m, VehicleCategory.Economy, 1, RiskLevel.Low));
        Assert.Throws<DomainException>(() => _calculator.Calculate(500_000m, VehicleCategory.Economy, 0, RiskLevel.Low));
    }
}
