using VehicleRental.Domain.Core;
using VehicleRental.Domain.Core.Enums;

namespace VehicleRental.Domain.Entities;

/// <summary>
/// Depósito de garantía obligatorio de una reserva confirmada. La custodia la realiza un proveedor de pagos
/// autorizado; aquí solo se registran los importes y referencias que ese proveedor confirma.
/// </summary>
public sealed class SecurityDeposit : BaseEntity
{
    public const int PaymentReferenceMaxLength = 100;
    public const int NotesMaxLength = 500;

    private SecurityDeposit()
    {
    }

    internal SecurityDeposit(Reservation reservation, decimal calculatedAmount)
    {
        Reservation = reservation;
        ReservationId = reservation.Id;
        CalculatedAmount = Guard.Positive(calculatedAmount, "importe calculado del depósito");
        Status = SecurityDepositStatus.Pending;
    }

    public int ReservationId { get; private set; }

    public Reservation? Reservation { get; private set; }

    public decimal CalculatedAmount { get; private set; }

    /// <summary>Importe que el proveedor de pagos confirmó como retenido.</summary>
    public decimal HeldAmount { get; private set; }

    /// <summary>Parte del importe retenido aplicada a una reclamación aceptada.</summary>
    public decimal RetainedAmount { get; private set; }

    public SecurityDepositStatus Status { get; private set; }

    /// <summary>Referencia de la operación en el proveedor de pagos (no contiene datos de tarjeta).</summary>
    public string? PaymentProviderReference { get; private set; }

    public string? Notes { get; private set; }

    public DateTime? HeldAt { get; private set; }

    public DateTime? ReleasedAt { get; private set; }

    /// <summary>Registra la retención confirmada por el proveedor de pagos.</summary>
    public void RegisterHold(decimal heldAmount, string paymentProviderReference)
    {
        if (Status != SecurityDepositStatus.Pending)
        {
            throw new DomainException("Solo se puede registrar la retención de un depósito pendiente.");
        }

        if (heldAmount < CalculatedAmount)
        {
            throw new DomainException("El importe retenido no puede ser menor que el importe calculado.");
        }

        HeldAmount = heldAmount;
        PaymentProviderReference = Guard.NotEmpty(paymentProviderReference, "referencia del proveedor de pagos", PaymentReferenceMaxLength);
        HeldAt = DateTime.UtcNow;
        Status = SecurityDepositStatus.Held;
    }

    /// <summary>Libera el depósito completo (sin reclamaciones aceptadas pendientes).</summary>
    public void Release()
    {
        if (Status != SecurityDepositStatus.Held)
        {
            throw new DomainException("Solo se puede liberar un depósito retenido.");
        }

        Status = SecurityDepositStatus.Released;
        ReleasedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica parte del depósito a una reclamación aceptada; el resto se libera.
    /// Si el daño supera el depósito, la diferencia se gestiona en la reclamación (el depósito no es un tope).
    /// </summary>
    public void Retain(decimal amount, string notes)
    {
        if (Status != SecurityDepositStatus.Held)
        {
            throw new DomainException("Solo se puede retener un depósito que esté retenido.");
        }

        Guard.Positive(amount, "importe a retener");
        if (amount > HeldAmount)
        {
            throw new DomainException("No se puede retener más del importe depositado.");
        }

        RetainedAmount = amount;
        Notes = Guard.NotEmpty(notes, nameof(Notes), NotesMaxLength);
        Status = SecurityDepositStatus.Retained;
        ReleasedAt = DateTime.UtcNow;
    }
}
