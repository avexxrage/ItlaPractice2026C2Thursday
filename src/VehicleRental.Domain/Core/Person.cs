namespace VehicleRental.Domain.Core;

/// <summary>
/// Datos personales comunes a las personas que usan la plataforma.
/// </summary>
public abstract class Person : BaseEntity
{
    public const int NameMaxLength = 80;
    public const int EmailMaxLength = 150;
    public const int PhoneMaxLength = 20;
    public const int IdentificationMaxLength = 20;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    /// <summary>Cédula o pasaporte. Se normaliza sin guiones ni espacios.</summary>
    public string IdentificationNumber { get; private set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    protected void SetName(string firstName, string lastName)
    {
        FirstName = Guard.NotEmpty(firstName, nameof(FirstName), NameMaxLength);
        LastName = Guard.NotEmpty(lastName, nameof(LastName), NameMaxLength);
    }

    protected void SetContact(string email, string phoneNumber)
    {
        Email = Guard.Email(email, nameof(Email), EmailMaxLength);
        PhoneNumber = Guard.Phone(phoneNumber, nameof(PhoneNumber), PhoneMaxLength);
    }

    protected void SetIdentification(string identificationNumber)
    {
        var normalized = Guard.NotEmpty(identificationNumber, nameof(IdentificationNumber), IdentificationMaxLength)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

        if (normalized.Length < 6)
        {
            throw new DomainException("El número de identificación debe tener al menos 6 caracteres.");
        }

        IdentificationNumber = normalized;
    }
}
