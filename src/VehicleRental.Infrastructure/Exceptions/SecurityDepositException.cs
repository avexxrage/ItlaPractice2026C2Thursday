namespace VehicleRental.Infrastructure.Exceptions;

public sealed class SecurityDepositException : PersistenceException
{
    public SecurityDepositException(string message)
        : base(message)
    {
    }

    public static SecurityDepositException ReservationNotFinished(int reservationId)
        => new($"La reserva {reservationId} debe estar completada o cancelada para liberar el depósito.");

    public static SecurityDepositException UnresolvedClaims(int reservationId)
        => new($"La reserva {reservationId} tiene reclamaciones sin resolver; el depósito no puede liberarse todavía.");

    public static SecurityDepositException AcceptedClaimsPending(int reservationId)
        => new($"La reserva {reservationId} tiene reclamaciones aceptadas: use la retención en lugar de la liberación.");

    public static SecurityDepositException RetainExceedsClaim()
        => new("El importe a retener no puede superar el importe reclamado.");

    public static SecurityDepositException ClaimFromOtherReservation()
        => new("La reclamación no pertenece a la misma reserva que el depósito.");
}
