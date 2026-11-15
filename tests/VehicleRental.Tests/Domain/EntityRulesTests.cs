using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Tests.Domain;

public class EntityRulesTests
{
    [Fact]
    public void User_normalizes_email_and_identification()
    {
        var user = new User("Ana", "Pérez", "Ana.Perez@Correo.com", "809-555-0101", "001-1234567-8");

        Assert.Equal("ana.perez@correo.com", user.Email);
        Assert.Equal("00112345678", user.IdentificationNumber);
        Assert.False(user.CanRent);
    }

    [Fact]
    public void User_can_rent_only_when_active_and_verified()
    {
        var user = new User("Ana", "Pérez", "ana@correo.com", "8095550101", "00112345678");
        Assert.Throws<DomainException>(user.VerifyAsRenter);

        user.Activate();
        user.VerifyAsRenter();

        Assert.True(user.CanRent);
    }

    [Fact]
    public void Company_requires_valid_rnc()
    {
        Assert.Throws<DomainException>(() =>
            new Company("Rent RD", "12AB", "RM-001", "info@rentrd.com", "8095550000", "Santo Domingo"));
    }

    [Fact]
    public void Rent_a_car_provider_requires_verified_company()
    {
        var provider = Provider.CreateRentACar(companyId: 1, managerUserId: null);

        Assert.Throws<DomainException>(() => provider.Verify(companyVerified: false));

        provider.Verify(companyVerified: true);
        Assert.True(provider.CanPublishVehicles);
    }

    [Fact]
    public void Vehicle_requires_verified_provider_to_publish_and_mileage_only_increases()
    {
        var vehicle = new Vehicle(1, "Toyota", "Corolla", 2022, "a-123456", "Blanco", TransmissionType.Automatic,
            FuelType.Gasoline, VehicleCategory.Sedan, 30_000, 3_000m, 1_200_000m, null);

        Assert.Equal("A123456", vehicle.LicensePlate);
        Assert.Throws<DomainException>(() => vehicle.Publish(providerVerified: false));
        Assert.Throws<DomainException>(() => vehicle.UpdateMileage(29_000));

        vehicle.Publish(providerVerified: true);
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }

    [Fact]
    public void Damage_claim_cannot_be_accepted_without_inspection_evidence()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var reservation = new Reservation(1, 2, 3, tomorrow, tomorrow.AddDays(2), 2000m, RiskLevel.Medium);
        reservation.Confirm(10_000m).RegisterHold(10_000m, "PSP-1");
        reservation.RegisterInspection(InspectionType.Delivery, 100, 100, null);
        reservation.Start();
        var returnInspection = reservation.RegisterInspection(InspectionType.Return, 300, 50, "Rayón en puerta");

        var claim = reservation.OpenDamageClaim("Rayón profundo en la puerta trasera", 8_000m);
        Assert.Equal(DamageClaimStatus.Submitted, claim.Status);

        claim.StartReview();
        Assert.Throws<DomainException>(() => claim.Accept("Daño confirmado", supportingEvidenceCount: 1));

        claim.LinkInspection(returnInspection);
        Assert.Throws<DomainException>(() => claim.Accept("Daño confirmado", supportingEvidenceCount: 0));

        claim.Accept("Daño confirmado con fotos de la devolución", supportingEvidenceCount: 2);
        Assert.Equal(DamageClaimStatus.Accepted, claim.Status);
    }
}
