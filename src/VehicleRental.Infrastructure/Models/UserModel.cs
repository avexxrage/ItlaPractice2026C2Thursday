using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record UserCreateModel
{
    [Required, StringLength(80)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 7)]
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>Cédula o pasaporte. Solo se devuelve enmascarado.</summary>
    [Required, StringLength(20, MinimumLength = 6)]
    public string IdentificationNumber { get; init; } = string.Empty;
}

public sealed record UserUpdateModel
{
    [Required, StringLength(80)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 7)]
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>Respuesta de usuario: la identificación se devuelve enmascarada.</summary>
public sealed record UserModel(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string MaskedIdentificationNumber,
    UserStatus Status,
    VerificationStatus RenterVerificationStatus,
    bool CanRent,
    int? ProviderId,
    DateTime RegisteredAt);
