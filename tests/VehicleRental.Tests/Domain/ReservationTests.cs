using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Tests.Domain;

public class ReservationTests
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    private static Reservation NewReservation(int days = 3)
        => new(vehicleId: 1, renterId: 2, providerId: 3, Tomorrow, Tomorrow.AddDays(days), dailyRate: 2500m, RiskLevel.Medium);

    [Fact]
    public void New_reservation_is_pending_and_calculates_total_price()
    {
        var reservation = NewReservation(days: 4);

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(4, reservation.RentalDays);
        Assert.Equal(10_000m, reservation.TotalPrice);
    }

    [Fact]
    public void Reservation_with_end_before_start_is_rejected()
    {
        Assert.Throws<DomainException>(() =>
            new Reservation(1, 2, 3, Tomorrow, Tomorrow.AddHours(-1), 2500m, RiskLevel.Low));
    }

    [Fact]
    public void Reservation_in_the_past_is_rejected()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        Assert.Throws<DomainException>(() =>
            new Reservation(1, 2, 3, yesterday, yesterday.AddDays(2), 2500m, RiskLevel.Low));
    }

    [Fact]
    public void Confirm_creates_mandatory_pending_deposit()
    {
        var reservation = NewReservation();

        var deposit = reservation.Confirm(15_000m);

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Same(deposit, reservation.SecurityDeposit);
        Assert.Equal(SecurityDepositStatus.Pending, deposit.Status);
        Assert.Equal(15_000m, deposit.CalculatedAmount);
    }

    [Fact]
    public void Cannot_start_without_held_deposit_and_delivery_inspection()
    {
        var reservation = NewReservation();
        var deposit = reservation.Confirm(15_000m);

        Assert.Throws<DomainException>(reservation.Start);

        deposit.RegisterHold(15_000m, "PSP-0001");
        Assert.Throws<DomainException>(reservation.Start);

        reservation.RegisterInspection(InspectionType.Delivery, 10_000, 100, null);
        reservation.Start();

        Assert.Equal(ReservationStatus.Active, reservation.Status);
    }

    [Fact]
    public void Return_mileage_cannot_be_lower_than_delivery_mileage()
    {
        var reservation = NewReservation();
        reservation.Confirm(15_000m).RegisterHold(15_000m, "PSP-0002");
        reservation.RegisterInspection(InspectionType.Delivery, 10_000, 100, null);
        reservation.Start();

        Assert.Throws<DomainException>(() => reservation.RegisterInspection(InspectionType.Return, 9_000, 80, null));
    }

    [Fact]
    public void Active_reservation_cannot_be_cancelled()
    {
        var reservation = NewReservation();
        reservation.Confirm(15_000m).RegisterHold(15_000m, "PSP-0003");
        reservation.RegisterInspection(InspectionType.Delivery, 10_000, 100, null);
        reservation.Start();

        Assert.Throws<DomainException>(() => reservation.Cancel("Cambio de planes"));
    }
}
