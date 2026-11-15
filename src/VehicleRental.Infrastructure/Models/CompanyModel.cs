using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record CompanyCreateModel
{
    [Required, StringLength(150)]
    public string TradeName { get; init; } = string.Empty;

    /// <summary>RNC de 9 u 11 dígitos (se aceptan guiones).</summary>
    [Required, StringLength(13, MinimumLength = 9)]
    public string Rnc { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string CommercialRegistry { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string ContactEmail { get; init; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 7)]
    public string ContactPhone { get; init; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; init; } = string.Empty;
}

public sealed record CompanyUpdateModel
{
    [Required, StringLength(150)]
    public string TradeName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string ContactEmail { get; init; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 7)]
    public string ContactPhone { get; init; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; init; } = string.Empty;
}

public sealed record CompanyModel(
    int Id,
    string TradeName,
    string Rnc,
    string CommercialRegistry,
    string ContactEmail,
    string ContactPhone,
    string Address,
    VerificationStatus VerificationStatus,
    int? ProviderId,
    DateTime CreatedAt);
