namespace VehicleRental.Infrastructure.Exceptions;

public sealed class UserException : PersistenceException
{
    public UserException(string message)
        : base(message)
    {
    }

    public static UserException DuplicateEmail(string email)
        => new($"Ya existe un usuario registrado con el correo {email}.");

    public static UserException DuplicateIdentification()
        => new("Ya existe un usuario registrado con ese número de identificación.");

    public static UserException HasRelatedData(int userId)
        => new($"El usuario {userId} tiene reservas o un perfil de proveedor y no puede eliminarse; desactívelo en su lugar.");
}
