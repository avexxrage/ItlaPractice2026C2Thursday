using System.Net.Mail;

namespace VehicleRental.Domain.Core;

/// <summary>
/// Validaciones reutilizables para proteger las invariantes de las entidades.
/// </summary>
public static class Guard
{
    public static string NotEmpty(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El campo {field} es obligatorio.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"El campo {field} no puede superar {maxLength} caracteres.");
        }

        return trimmed;
    }

    public static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NotEmpty(value, field, maxLength);
    }

    public static string Email(string? value, string field, int maxLength)
    {
        var email = NotEmpty(value, field, maxLength).ToLowerInvariant();
        if (!MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
        {
            throw new DomainException($"El campo {field} no tiene un formato de correo válido.");
        }

        return email;
    }

    public static string Phone(string? value, string field, int maxLength)
    {
        var phone = NotEmpty(value, field, maxLength);
        var digits = phone.Count(char.IsDigit);
        if (digits < 7 || phone.Any(c => !char.IsDigit(c) && c is not ('+' or '-' or ' ' or '(' or ')')))
        {
            throw new DomainException($"El campo {field} no es un teléfono válido.");
        }

        return phone;
    }

    public static decimal Positive(decimal value, string field)
    {
        if (value <= 0)
        {
            throw new DomainException($"El campo {field} debe ser mayor que cero.");
        }

        return value;
    }

    public static int NotNegative(int value, string field)
    {
        if (value < 0)
        {
            throw new DomainException($"El campo {field} no puede ser negativo.");
        }

        return value;
    }

    public static int ValidId(int value, string field)
    {
        if (value <= 0)
        {
            throw new DomainException($"El identificador {field} no es válido.");
        }

        return value;
    }

    public static void DateRange(DateTime start, DateTime end, string startField, string endField)
    {
        if (end <= start)
        {
            throw new DomainException($"La fecha {endField} debe ser posterior a {startField}.");
        }
    }

    public static T Defined<T>(T value, string field)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException($"El valor del campo {field} no es válido.");
        }

        return value;
    }
}
