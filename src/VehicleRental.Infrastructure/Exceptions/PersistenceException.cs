namespace VehicleRental.Infrastructure.Exceptions;

/// <summary>
/// Excepción base de la capa de infraestructura: errores de persistencia o reglas que dependen de los datos guardados.
/// </summary>
public class PersistenceException : Exception
{
    public PersistenceException(string message)
        : base(message)
    {
    }

    public PersistenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
