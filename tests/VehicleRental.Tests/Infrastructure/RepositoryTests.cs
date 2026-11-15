using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Models;
using VehicleRental.Infrastructure.Repositories;

namespace VehicleRental.Tests.Infrastructure;

public class RepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly SecurityDepositCalculator _calculator = new(new SecurityDepositSettings());

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task User_response_masks_identification_and_duplicate_email_is_rejected()
    {
        await using var context = _database.CreateContext();
        var users = new UserRepository(context);

        var user = await users.CreateAsync(NewUser("ana@correo.com", "00112345678"));

        Assert.Equal("*******5678", user.MaskedIdentificationNumber);
        await Assert.ThrowsAsync<UserException>(() => users.CreateAsync(NewUser("ANA@correo.com", "00199999999")));
    }

    [Fact]
    public async Task Duplicate_license_plate_is_rejected()
    {
        var (providerId, _) = await SeedVerifiedIndividualProviderAsync();
        await using var context = _database.CreateContext();
        var vehicles = new VehicleRepository(context);

        await vehicles.CreateAsync(NewVehicle(providerId, "A100001"));

        await Assert.ThrowsAsync<VehicleException>(() => vehicles.CreateAsync(NewVehicle(providerId, "a-100001")));
    }

    [Fact]
    public async Task Rent_a_car_company_can_publish_a_fleet()
    {
        int providerId;
        await using (var context = _database.CreateContext())
        {
            var companies = new CompanyRepository(context);
            var company = await companies.CreateAsync(new CompanyCreateModel
            {
                TradeName = "Rent RD",
                Rnc = "1-01-23456-7",
                CommercialRegistry = "RM-2026-001",
                ContactEmail = "info@rentrd.com",
                ContactPhone = "809-555-0000",
                Address = "Av. 27 de Febrero, Santo Domingo",
            });

            var providers = new ProviderRepository(context);
            var provider = await providers.CreateRentACarAsync(new ProviderCompanyCreateModel { CompanyId = company.Id });
            await Assert.ThrowsAsync<DomainException>(() => providers.VerifyAsync(provider.Id));

            await companies.VerifyAsync(company.Id);
            await providers.VerifyAsync(provider.Id);
            providerId = provider.Id;
        }

        await using (var context = _database.CreateContext())
        {
            var vehicles = new VehicleRepository(context);
            var first = await vehicles.CreateAsync(NewVehicle(providerId, "G200001"));
            var second = await vehicles.CreateAsync(NewVehicle(providerId, "G200002"));
            await vehicles.PublishAsync(first.Id);
            await vehicles.PublishAsync(second.Id);
        }

        await using (var context = _database.CreateContext())
        {
            var provider = await new ProviderRepository(context).GetModelByIdAsync(providerId);
            var fleet = await new VehicleRepository(context).GetModelsAsync(new VehicleFilterModel { ProviderId = providerId });

            Assert.Equal(ProviderType.RentACarCompany, provider.Type);
            Assert.Equal("Rent RD", provider.CompanyName);
            Assert.Equal(2, provider.VehicleCount);
            Assert.All(fleet, v => Assert.Equal(VehicleStatus.Active, v.Status));
        }
    }

    [Fact]
    public async Task Overlapping_reservations_and_blocked_periods_are_rejected()
    {
        var (vehicleId, renterId) = await SeedRentableVehicleAsync();
        var start = DateTime.UtcNow.Date.AddDays(5);

        await using var context = _database.CreateContext();
        var reservations = new ReservationRepository(context);
        await reservations.CreateAsync(NewReservation(vehicleId, renterId, start, start.AddDays(3)));

        await Assert.ThrowsAsync<ReservationException>(() =>
            reservations.CreateAsync(NewReservation(vehicleId, renterId, start.AddDays(2), start.AddDays(4))));

        var availabilities = new VehicleAvailabilityRepository(context);
        await availabilities.CreateAsync(new VehicleAvailabilityCreateModel
        {
            VehicleId = vehicleId,
            StartDate = start.AddDays(10),
            EndDate = start.AddDays(12),
            IsAvailable = false,
            Notes = "Mantenimiento programado",
        });

        await Assert.ThrowsAsync<ReservationException>(() =>
            reservations.CreateAsync(NewReservation(vehicleId, renterId, start.AddDays(11), start.AddDays(13))));
    }

    [Fact]
    public async Task Unverified_renter_cannot_reserve()
    {
        var (vehicleId, _) = await SeedRentableVehicleAsync();

        await using var context = _database.CreateContext();
        var unverified = await new UserRepository(context).CreateAsync(NewUser("nuevo@correo.com", "40211111111"));
        var start = DateTime.UtcNow.Date.AddDays(2);

        await Assert.ThrowsAsync<ReservationException>(() =>
            new ReservationRepository(context).CreateAsync(NewReservation(vehicleId, unverified.Id, start, start.AddDays(1))));
    }

    [Fact]
    public async Task Full_rental_flow_with_deposit_inspections_and_damage_claim()
    {
        var (vehicleId, renterId) = await SeedRentableVehicleAsync();
        var start = DateTime.UtcNow.Date.AddDays(1);
        int reservationId;
        int depositId;
        int returnInspectionId;
        int claimId;

        // Reserva y confirmación con depósito dinámico.
        await using (var context = _database.CreateContext())
        {
            var reservations = new ReservationRepository(context);
            var reservation = await reservations.CreateAsync(NewReservation(vehicleId, renterId, start, start.AddDays(4), RiskLevel.High));
            reservationId = reservation.Id;
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
            Assert.Equal(4 * 3_000m, reservation.TotalPrice);

            var input = await reservations.GetDepositInputAsync(reservationId);
            var amount = _calculator.Calculate(input.EstimatedCommercialValue, input.Category, input.RentalDays, input.RiskLevel);
            var confirmed = await reservations.ConfirmAsync(reservationId, amount);

            Assert.Equal(ReservationStatus.Confirmed, confirmed.Status);
            Assert.Equal(amount, confirmed.SecurityDepositAmount);
            Assert.Equal(SecurityDepositStatus.Pending, confirmed.SecurityDepositStatus);
            depositId = confirmed.SecurityDepositId!.Value;
        }

        // Retención confirmada por el proveedor de pagos, entrega e inicio.
        await using (var context = _database.CreateContext())
        {
            var deposits = new SecurityDepositRepository(context);
            var reservations = new ReservationRepository(context);
            var inspections = new VehicleInspectionRepository(context);

            await Assert.ThrowsAsync<DomainException>(() => reservations.StartAsync(reservationId));

            var deposit = await deposits.GetByReservationAsync(reservationId);
            await deposits.RegisterHoldAsync(depositId, new SecurityDepositHoldModel
            {
                HeldAmount = deposit.CalculatedAmount,
                PaymentProviderReference = "PSP-TEST-0001",
            });

            await inspections.CreateAsync(new VehicleInspectionCreateModel
            {
                ReservationId = reservationId,
                Type = InspectionType.Delivery,
                Mileage = 30_000,
                FuelLevelPercentage = 100,
            });

            var active = await reservations.StartAsync(reservationId);
            Assert.Equal(ReservationStatus.Active, active.Status);
        }

        // Devolución, finalización y actualización del kilometraje.
        await using (var context = _database.CreateContext())
        {
            var inspections = new VehicleInspectionRepository(context);
            var returnInspection = await inspections.CreateAsync(new VehicleInspectionCreateModel
            {
                ReservationId = reservationId,
                Type = InspectionType.Return,
                Mileage = 30_850,
                FuelLevelPercentage = 60,
                Observations = "Rayón en la puerta trasera derecha",
            });
            returnInspectionId = returnInspection.Id;

            var completed = await new ReservationRepository(context).CompleteAsync(reservationId);
            Assert.Equal(ReservationStatus.Completed, completed.Status);

            var vehicle = await new VehicleRepository(context).GetModelByIdAsync(vehicleId);
            Assert.Equal(30_850, vehicle.Mileage);
        }

        // Reclamación: necesita evaluación y evidencias antes de aceptarse.
        await using (var context = _database.CreateContext())
        {
            var claims = new DamageClaimRepository(context);
            var claim = await claims.CreateAsync(new DamageClaimCreateModel
            {
                ReservationId = reservationId,
                Description = "Rayón profundo en la puerta trasera derecha",
                ClaimedAmount = 12_000m,
                VehicleInspectionId = returnInspectionId,
            });
            claimId = claim.Id;

            await Assert.ThrowsAsync<SecurityDepositException>(() => new SecurityDepositRepository(context).ReleaseAsync(depositId));

            await claims.StartReviewAsync(claimId);
            await Assert.ThrowsAsync<DomainException>(() => claims.AcceptAsync(claimId, "Daño confirmado"));
        }

        await using (var context = _database.CreateContext())
        {
            var inspections = new VehicleInspectionRepository(context);
            await inspections.AddEvidenceAsync(returnInspectionId, new InspectionEvidenceCreateModel
            {
                FileReference = "evidencias/reserva-1/puerta.jpg",
                EvidenceType = EvidenceType.Photo,
                Description = "Foto del rayón",
            });

            var accepted = await new DamageClaimRepository(context).AcceptAsync(claimId, "Daño confirmado con la evidencia de la devolución");
            Assert.Equal(DamageClaimStatus.Accepted, accepted.Status);
        }

        // Aplicación del depósito a la reclamación aceptada.
        await using (var context = _database.CreateContext())
        {
            var deposits = new SecurityDepositRepository(context);
            await Assert.ThrowsAsync<SecurityDepositException>(() => deposits.ReleaseAsync(depositId));
            await Assert.ThrowsAsync<SecurityDepositException>(() => deposits.RetainAsync(depositId, new SecurityDepositRetainModel
            {
                DamageClaimId = claimId,
                Amount = 20_000m,
                Notes = "Excede lo reclamado",
            }));

            var retained = await deposits.RetainAsync(depositId, new SecurityDepositRetainModel
            {
                DamageClaimId = claimId,
                Amount = 12_000m,
                Notes = "Aplicado a la reclamación por rayón",
            });

            Assert.Equal(SecurityDepositStatus.Retained, retained.Status);
            Assert.Equal(12_000m, retained.RetainedAmount);
        }

        await using (var context = _database.CreateContext())
        {
            var evidences = await new VehicleInspectionRepository(context).GetEvidencesAsync(returnInspectionId);
            Assert.Single(evidences);
        }
    }

    [Fact]
    public async Task Cancelled_reservation_releases_held_deposit()
    {
        var (vehicleId, renterId) = await SeedRentableVehicleAsync();
        var start = DateTime.UtcNow.Date.AddDays(3);

        await using var context = _database.CreateContext();
        var reservations = new ReservationRepository(context);
        var deposits = new SecurityDepositRepository(context);

        var reservation = await reservations.CreateAsync(NewReservation(vehicleId, renterId, start, start.AddDays(2)));
        var confirmed = await reservations.ConfirmAsync(reservation.Id, 9_000m);
        var depositId = confirmed.SecurityDepositId!.Value;
        await deposits.RegisterHoldAsync(depositId, new SecurityDepositHoldModel { HeldAmount = 9_000m, PaymentProviderReference = "PSP-2" });

        await Assert.ThrowsAsync<SecurityDepositException>(() => deposits.ReleaseAsync(depositId));

        await reservations.CancelAsync(reservation.Id, "El arrendatario canceló con anticipación");
        var released = await deposits.ReleaseAsync(depositId);

        Assert.Equal(SecurityDepositStatus.Released, released.Status);
        Assert.NotNull(released.ReleasedAt);
    }

    private async Task<(int ProviderId, int OwnerUserId)> SeedVerifiedIndividualProviderAsync()
    {
        await using var context = _database.CreateContext();
        var users = new UserRepository(context);
        var owner = await users.CreateAsync(NewUser($"owner{Guid.NewGuid():N}@correo.com", RandomIdentification()));
        await users.ActivateAsync(owner.Id);

        var providers = new ProviderRepository(context);
        var provider = await providers.CreateIndividualAsync(new ProviderIndividualCreateModel { UserId = owner.Id });
        await providers.VerifyAsync(provider.Id);
        return (provider.Id, owner.Id);
    }

    private async Task<(int VehicleId, int RenterId)> SeedRentableVehicleAsync()
    {
        var (providerId, _) = await SeedVerifiedIndividualProviderAsync();

        await using var context = _database.CreateContext();
        var vehicles = new VehicleRepository(context);
        var vehicle = await vehicles.CreateAsync(NewVehicle(providerId, $"P{Random.Shared.Next(100000, 999999)}"));
        await vehicles.PublishAsync(vehicle.Id);

        var users = new UserRepository(context);
        var renter = await users.CreateAsync(NewUser($"renter{Guid.NewGuid():N}@correo.com", RandomIdentification()));
        await users.ActivateAsync(renter.Id);
        await users.VerifyRenterAsync(renter.Id);

        return (vehicle.Id, renter.Id);
    }

    private static string RandomIdentification() => Random.Shared.NextInt64(10_000_000_000, 99_999_999_999).ToString();

    private static UserCreateModel NewUser(string email, string identification) => new()
    {
        FirstName = "Luis",
        LastName = "Martínez",
        Email = email,
        PhoneNumber = "809-555-0101",
        IdentificationNumber = identification,
    };

    private static VehicleCreateModel NewVehicle(int providerId, string plate) => new()
    {
        ProviderId = providerId,
        Brand = "Toyota",
        Model = "Corolla",
        Year = 2022,
        LicensePlate = plate,
        Color = "Blanco",
        Transmission = TransmissionType.Automatic,
        FuelType = FuelType.Gasoline,
        Category = VehicleCategory.Sedan,
        Mileage = 30_000,
        DailyRate = 3_000m,
        EstimatedCommercialValue = 1_200_000m,
        Description = "Sedán en excelente estado",
    };

    private static ReservationCreateModel NewReservation(int vehicleId, int renterId, DateTime start, DateTime end, RiskLevel risk = RiskLevel.Medium)
        => new()
        {
            VehicleId = vehicleId,
            RenterId = renterId,
            StartDate = start,
            EndDate = end,
            RiskLevel = risk,
        };
}
