namespace VehicleRental.Infrastructure.Exceptions;

public sealed class DamageClaimException : PersistenceException
{
    public DamageClaimException(string message)
        : base(message)
    {
    }

    public static DamageClaimException NotAccepted(int claimId)
        => new($"La reclamación {claimId} no ha sido aceptada tras la evaluación.");
}
