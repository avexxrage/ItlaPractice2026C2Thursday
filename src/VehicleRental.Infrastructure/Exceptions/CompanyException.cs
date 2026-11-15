namespace VehicleRental.Infrastructure.Exceptions;

public sealed class CompanyException : PersistenceException
{
    public CompanyException(string message)
        : base(message)
    {
    }

    public static CompanyException DuplicateRnc(string rnc)
        => new($"Ya existe una empresa registrada con el RNC {rnc}.");

    public static CompanyException DuplicateCommercialRegistry(string registry)
        => new($"Ya existe una empresa con el registro mercantil {registry}.");

    public static CompanyException HasProvider(int companyId)
        => new($"La empresa {companyId} tiene un perfil de proveedor asociado y no puede eliminarse.");
}
