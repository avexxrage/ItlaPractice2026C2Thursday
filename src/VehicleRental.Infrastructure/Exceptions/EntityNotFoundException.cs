namespace VehicleRental.Infrastructure.Exceptions;

/// <summary>
/// No existe un registro con el identificador solicitado.
/// </summary>
public sealed class EntityNotFoundException : PersistenceException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"No existe {entityName} con Id {id}.")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public string EntityName { get; }

    public int EntityId { get; }
}
