using System.ComponentModel.DataAnnotations;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Infrastructure.Models;

public sealed record VehicleDocumentCreateModel
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; init; }

    public DocumentType DocumentType { get; init; }

    [StringLength(50)]
    public string? DocumentNumber { get; init; }

    public DateTime ExpirationDate { get; init; }

    /// <summary>Referencia al archivo en el almacenamiento (no el archivo en sí).</summary>
    [StringLength(500)]
    public string? FileReference { get; init; }
}

public sealed record VehicleDocumentUpdateModel
{
    [StringLength(50)]
    public string? DocumentNumber { get; init; }

    public DateTime ExpirationDate { get; init; }

    [StringLength(500)]
    public string? FileReference { get; init; }
}

public sealed record VehicleDocumentModel(
    int Id,
    int VehicleId,
    DocumentType DocumentType,
    string? DocumentNumber,
    DateTime ExpirationDate,
    string? FileReference,
    VerificationStatus ValidationStatus,
    string? ValidationNotes,
    bool IsExpired);
