namespace VehicleRental.Infrastructure.Core;

/// <summary>
/// Utilidades para no exponer datos sensibles completos en los modelos de respuesta.
/// </summary>
public static class SensitiveData
{
    /// <summary>Muestra solo los últimos 4 caracteres: "00112345678" → "*******5678".</summary>
    public static string Mask(string? value, int visibleCharacters = 4)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Length <= visibleCharacters)
        {
            return new string('*', value.Length);
        }

        return new string('*', value.Length - visibleCharacters) + value[^visibleCharacters..];
    }
}
