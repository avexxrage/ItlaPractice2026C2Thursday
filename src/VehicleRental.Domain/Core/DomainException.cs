namespace VehicleRental.Domain.Core;

/// <summary>
/// Se lanza cuando una operación viola una regla o invariante del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
