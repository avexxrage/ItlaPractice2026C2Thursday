using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Persona registrada en la plataforma. Puede actuar como arrendatario y, si tiene un
/// <see cref="Entities.Provider"/> asociado, también como propietario particular.
/// </summary>
public sealed class User : Person
{
    private readonly List<Reservation> _reservations = new();

    private User()
    {
    }

    public User(string firstName, string lastName, string email, string phoneNumber, string identificationNumber)
    {
        SetName(firstName, lastName);
        SetContact(email, phoneNumber);
        SetIdentification(identificationNumber);
        Status = UserStatus.PendingVerification;
        RenterVerificationStatus = VerificationStatus.Pending;
        RegisteredAt = DateTime.UtcNow;
    }

    public UserStatus Status { get; private set; }

    /// <summary>Resultado de la verificación exigida para poder alquilar.</summary>
    public VerificationStatus RenterVerificationStatus { get; private set; }

    public DateTime RegisteredAt { get; private set; }

    public Provider? Provider { get; private set; }

    public IReadOnlyCollection<Reservation> Reservations => _reservations;

    public bool CanRent => Status == UserStatus.Active && RenterVerificationStatus == VerificationStatus.Verified;

    public void UpdateProfile(string firstName, string lastName, string email, string phoneNumber)
    {
        SetName(firstName, lastName);
        SetContact(email, phoneNumber);
    }

    public void Activate()
    {
        if (Status == UserStatus.Active)
        {
            throw new DomainException("El usuario ya está activo.");
        }

        Status = UserStatus.Active;
    }

    public void Suspend()
    {
        if (Status != UserStatus.Active)
        {
            throw new DomainException("Solo se puede suspender un usuario activo.");
        }

        Status = UserStatus.Suspended;
    }

    public void Deactivate() => Status = UserStatus.Inactive;

    public void VerifyAsRenter()
    {
        if (Status != UserStatus.Active)
        {
            throw new DomainException("El usuario debe estar activo para verificarlo como arrendatario.");
        }

        RenterVerificationStatus = VerificationStatus.Verified;
    }

    public void RejectRenterVerification() => RenterVerificationStatus = VerificationStatus.Rejected;
}
