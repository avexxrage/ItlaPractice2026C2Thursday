using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Evidencia (foto, video o documento) de una inspección. Se guarda solo la referencia al archivo.
/// </summary>
public sealed class InspectionEvidence : BaseEntity
{
    public const int FileReferenceMaxLength = 500;
    public const int DescriptionMaxLength = 250;

    private InspectionEvidence()
    {
    }

    internal InspectionEvidence(VehicleInspection inspection, string fileReference, EvidenceType evidenceType, string? description)
    {
        VehicleInspection = inspection;
        VehicleInspectionId = inspection.Id;
        FileReference = Guard.NotEmpty(fileReference, nameof(FileReference), FileReferenceMaxLength);
        EvidenceType = Guard.Defined(evidenceType, nameof(EvidenceType));
        Description = Guard.Optional(description, nameof(Description), DescriptionMaxLength);
        RegisteredAt = DateTime.UtcNow;
    }

    public int VehicleInspectionId { get; private set; }

    public VehicleInspection? VehicleInspection { get; private set; }

    public string FileReference { get; private set; } = string.Empty;

    public EvidenceType EvidenceType { get; private set; }

    public string? Description { get; private set; }

    public DateTime RegisteredAt { get; private set; }
}
