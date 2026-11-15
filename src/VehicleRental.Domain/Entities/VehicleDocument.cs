using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Documento legal del vehículo (matrícula, seguro, marbete...). Se guarda solo la referencia al archivo.
/// </summary>
public sealed class VehicleDocument : BaseEntity
{
    public const int DocumentNumberMaxLength = 50;
    public const int FileReferenceMaxLength = 500;
    public const int NotesMaxLength = 500;

    private VehicleDocument()
    {
    }

    public VehicleDocument(int vehicleId, DocumentType documentType, string? documentNumber, DateTime expirationDate, string? fileReference)
    {
        VehicleId = Guard.ValidId(vehicleId, nameof(VehicleId));
        DocumentType = Guard.Defined(documentType, nameof(DocumentType));
        Update(documentNumber, expirationDate, fileReference);
    }

    public int VehicleId { get; private set; }

    public Vehicle? Vehicle { get; private set; }

    public DocumentType DocumentType { get; private set; }

    public string? DocumentNumber { get; private set; }

    public DateTime ExpirationDate { get; private set; }

    /// <summary>Ruta o identificador del archivo en el almacenamiento externo.</summary>
    public string? FileReference { get; private set; }

    public VerificationStatus ValidationStatus { get; private set; }

    public string? ValidationNotes { get; private set; }

    public bool IsExpired(DateTime today) => ExpirationDate.Date < today.Date;

    /// <summary>Actualizar el documento lo devuelve a revisión.</summary>
    public void Update(string? documentNumber, DateTime expirationDate, string? fileReference)
    {
        DocumentNumber = Guard.Optional(documentNumber, nameof(DocumentNumber), DocumentNumberMaxLength);
        ExpirationDate = expirationDate.Date;
        FileReference = Guard.Optional(fileReference, nameof(FileReference), FileReferenceMaxLength);
        ValidationStatus = VerificationStatus.Pending;
        ValidationNotes = null;
    }

    public void Approve()
    {
        if (IsExpired(DateTime.UtcNow))
        {
            throw new DomainException("No se puede aprobar un documento vencido.");
        }

        ValidationStatus = VerificationStatus.Verified;
        ValidationNotes = null;
    }

    public void Reject(string reason)
    {
        ValidationNotes = Guard.NotEmpty(reason, "motivo del rechazo", NotesMaxLength);
        ValidationStatus = VerificationStatus.Rejected;
    }
}
