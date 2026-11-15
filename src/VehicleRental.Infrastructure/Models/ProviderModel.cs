using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

/// <summary>Alta de un propietario particular a partir de un usuario existente.</summary>
public sealed record ProviderIndividualCreateModel
{
    [Range(1, int.MaxValue)]
    public int UserId { get; init; }
}

/// <summary>Alta de una empresa Rent a Car como proveedor.</summary>
public sealed record ProviderCompanyCreateModel
{
    [Range(1, int.MaxValue)]
    public int CompanyId { get; init; }

    /// <summary>Usuario que administrará la cuenta de la empresa (opcional).</summary>
    [Range(1, int.MaxValue)]
    public int? ManagerUserId { get; init; }
}

public sealed record ProviderModel(
    int Id,
    ProviderType Type,
    VerificationStatus VerificationStatus,
    DateTime? VerifiedAt,
    int? UserId,
    string? UserFullName,
    int? CompanyId,
    string? CompanyName,
    int VehicleCount,
    DateTime CreatedAt);
