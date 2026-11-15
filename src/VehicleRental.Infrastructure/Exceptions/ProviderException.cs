namespace VehicleRental.Infrastructure.Exceptions;

public sealed class ProviderException : PersistenceException
{
    public ProviderException(string message)
        : base(message)
    {
    }

    public static ProviderException UserAlreadyProvider(int userId)
        => new($"El usuario {userId} ya tiene un perfil de proveedor.");

    public static ProviderException CompanyAlreadyProvider(int companyId)
        => new($"La empresa {companyId} ya tiene un perfil de proveedor.");

    public static ProviderException HasVehicles(int providerId)
        => new($"El proveedor {providerId} tiene vehículos registrados y no puede eliminarse.");
}
