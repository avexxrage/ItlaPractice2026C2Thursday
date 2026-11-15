namespace VehicleRental.Domain.Core;

/// <summary>
/// Clase base de todas las entidades del dominio: identificador y datos de auditoría.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Registra la fecha de la última modificación (la usa el contexto al guardar).</summary>
    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
