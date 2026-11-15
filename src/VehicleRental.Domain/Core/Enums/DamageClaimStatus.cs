namespace VehicleRental.Domain.Core.Enums;

public enum DamageClaimStatus
{
    Submitted = 1,
    UnderReview = 2,

    /// <summary>La evaluación determinó responsabilidad del arrendatario.</summary>
    Accepted = 3,

    Rejected = 4,
}
