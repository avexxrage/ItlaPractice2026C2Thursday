using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Infrastructure.Models;

/// <summary>Motivo o notas para acciones de rechazo, cancelación o resolución.</summary>
public sealed record ReasonModel
{
    [Required]
    [StringLength(2000, MinimumLength = 3)]
    public string Reason { get; init; } = string.Empty;
}
