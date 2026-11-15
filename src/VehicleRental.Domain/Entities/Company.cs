using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Empresa Rent a Car. Publica su flota a través de su <see cref="Entities.Provider"/>.
/// </summary>
public sealed class Company : BaseEntity
{
    public const int TradeNameMaxLength = 150;
    public const int RncMaxLength = 11;
    public const int CommercialRegistryMaxLength = 30;
    public const int AddressMaxLength = 250;

    private Company()
    {
    }

    public Company(
        string tradeName,
        string rnc,
        string commercialRegistry,
        string contactEmail,
        string contactPhone,
        string address)
    {
        Rnc = NormalizeRnc(rnc);
        CommercialRegistry = Guard.NotEmpty(commercialRegistry, nameof(CommercialRegistry), CommercialRegistryMaxLength);
        UpdateInformation(tradeName, contactEmail, contactPhone, address);
        VerificationStatus = VerificationStatus.Pending;
    }

    public string TradeName { get; private set; } = string.Empty;

    /// <summary>Registro Nacional del Contribuyente (9 u 11 dígitos).</summary>
    public string Rnc { get; private set; } = string.Empty;

    /// <summary>Número de registro mercantil.</summary>
    public string CommercialRegistry { get; private set; } = string.Empty;

    public string ContactEmail { get; private set; } = string.Empty;

    public string ContactPhone { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public VerificationStatus VerificationStatus { get; private set; }

    public Provider? Provider { get; private set; }

    public void UpdateInformation(string tradeName, string contactEmail, string contactPhone, string address)
    {
        TradeName = Guard.NotEmpty(tradeName, nameof(TradeName), TradeNameMaxLength);
        ContactEmail = Guard.Email(contactEmail, nameof(ContactEmail), Person.EmailMaxLength);
        ContactPhone = Guard.Phone(contactPhone, nameof(ContactPhone), Person.PhoneMaxLength);
        Address = Guard.NotEmpty(address, nameof(Address), AddressMaxLength);
    }

    public void Verify() => VerificationStatus = VerificationStatus.Verified;

    public void Reject() => VerificationStatus = VerificationStatus.Rejected;

    private static string NormalizeRnc(string rnc)
    {
        var digits = Guard.NotEmpty(rnc, nameof(Rnc), 20).Replace("-", string.Empty).Replace(" ", string.Empty);
        if (digits.Any(c => !char.IsDigit(c)) || (digits.Length != 9 && digits.Length != 11))
        {
            throw new DomainException("El RNC debe tener 9 u 11 dígitos.");
        }

        return digits;
    }
}
