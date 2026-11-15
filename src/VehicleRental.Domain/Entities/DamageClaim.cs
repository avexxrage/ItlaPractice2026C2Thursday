using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Reclamación por daños de una reserva. La responsabilidad del arrendatario no se asume:
/// la reclamación pasa por revisión y solo se acepta si está sustentada en una inspección con evidencias.
/// </summary>
public sealed class DamageClaim : BaseEntity
{
    public const int DescriptionMaxLength = 2000;
    public const int ResolutionNotesMaxLength = 2000;

    private DamageClaim()
    {
    }

    internal DamageClaim(Reservation reservation, string description, decimal claimedAmount)
    {
        Reservation = reservation;
        ReservationId = reservation.Id;
        Description = Guard.NotEmpty(description, nameof(Description), DescriptionMaxLength);
        ClaimedAmount = Guard.Positive(claimedAmount, "importe reclamado");
        Status = DamageClaimStatus.Submitted;
    }

    public int ReservationId { get; private set; }

    public Reservation? Reservation { get; private set; }

    /// <summary>Inspección (normalmente la de devolución) que sustenta la reclamación.</summary>
    public int? VehicleInspectionId { get; private set; }

    public VehicleInspection? VehicleInspection { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal ClaimedAmount { get; private set; }

    public DamageClaimStatus Status { get; private set; }

    public string? ResolutionNotes { get; private set; }

    public DateTime? ResolvedAt { get; private set; }

    public bool IsResolved => Status is DamageClaimStatus.Accepted or DamageClaimStatus.Rejected;

    public void LinkInspection(VehicleInspection inspection)
    {
        ArgumentNullException.ThrowIfNull(inspection);
        EnsureOpen();

        if (inspection.ReservationId != ReservationId)
        {
            throw new DomainException("La inspección no pertenece a la reserva de la reclamación.");
        }

        VehicleInspection = inspection;
        VehicleInspectionId = inspection.Id;
    }

    public void StartReview()
    {
        if (Status != DamageClaimStatus.Submitted)
        {
            throw new DomainException("Solo se pueden poner en revisión reclamaciones recibidas.");
        }

        Status = DamageClaimStatus.UnderReview;
    }

    /// <summary>Acepta la responsabilidad tras la evaluación. Exige inspección vinculada con evidencias.</summary>
    public void Accept(string resolutionNotes, int supportingEvidenceCount)
    {
        if (Status != DamageClaimStatus.UnderReview)
        {
            throw new DomainException("La reclamación debe estar en revisión para aceptarla.");
        }

        if (VehicleInspectionId is null || supportingEvidenceCount <= 0)
        {
            throw new DomainException("Para aceptar la reclamación debe existir una inspección vinculada con evidencias.");
        }

        Resolve(DamageClaimStatus.Accepted, resolutionNotes);
    }

    public void Reject(string resolutionNotes)
    {
        EnsureOpen();
        Resolve(DamageClaimStatus.Rejected, resolutionNotes);
    }

    private void Resolve(DamageClaimStatus status, string resolutionNotes)
    {
        ResolutionNotes = Guard.NotEmpty(resolutionNotes, nameof(ResolutionNotes), ResolutionNotesMaxLength);
        Status = status;
        ResolvedAt = DateTime.UtcNow;
    }

    private void EnsureOpen()
    {
        if (IsResolved)
        {
            throw new DomainException("La reclamación ya fue resuelta.");
        }
    }
}
