using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Proveedor de vehículos: un propietario particular (asociado a un usuario) o una empresa Rent a Car.
/// </summary>
public sealed class Provider : BaseEntity
{
    private readonly List<Vehicle> _vehicles = new();

    private Provider()
    {
    }

    public static Provider CreateIndividual(int userId) => new()
    {
        UserId = Guard.ValidId(userId, nameof(UserId)),
        Type = ProviderType.Individual,
        VerificationStatus = VerificationStatus.Pending,
    };

    /// <param name="companyId">Empresa Rent a Car.</param>
    /// <param name="managerUserId">Usuario que administra la cuenta de la empresa (opcional).</param>
    public static Provider CreateRentACar(int companyId, int? managerUserId) => new()
    {
        CompanyId = Guard.ValidId(companyId, nameof(CompanyId)),
        UserId = managerUserId is null ? null : Guard.ValidId(managerUserId.Value, nameof(UserId)),
        Type = ProviderType.RentACarCompany,
        VerificationStatus = VerificationStatus.Pending,
    };

    /// <summary>Usuario asociado: dueño (particular) o administrador de la cuenta (empresa).</summary>
    public int? UserId { get; private set; }

    public User? User { get; private set; }

    public int? CompanyId { get; private set; }

    public Company? Company { get; private set; }

    public ProviderType Type { get; private set; }

    public VerificationStatus VerificationStatus { get; private set; }

    public DateTime? VerifiedAt { get; private set; }

    public IReadOnlyCollection<Vehicle> Vehicles => _vehicles;

    public bool CanPublishVehicles => VerificationStatus == VerificationStatus.Verified;

    /// <param name="companyVerified">Para empresas, la empresa debe estar verificada previamente.</param>
    public void Verify(bool companyVerified)
    {
        if (Type == ProviderType.RentACarCompany && !companyVerified)
        {
            throw new DomainException("La empresa Rent a Car debe estar verificada antes de verificar al proveedor.");
        }

        VerificationStatus = VerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        VerificationStatus = VerificationStatus.Rejected;
        VerifiedAt = null;
    }
}
