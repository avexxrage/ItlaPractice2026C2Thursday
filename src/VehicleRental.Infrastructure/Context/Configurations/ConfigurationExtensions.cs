using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Core;

namespace VehicleRental.Infrastructure.Context.Configurations;

/// <summary>
/// Convenciones compartidas por las configuraciones de entidades.
/// </summary>
internal static class ConfigurationExtensions
{
    public const int EnumMaxLength = 30;

    /// <summary>Clave primaria y columnas de auditoría de <see cref="BaseEntity"/>.</summary>
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
    }

    /// <summary>Guarda el enum como texto legible y con longitud acotada.</summary>
    public static PropertyBuilder<TEnum> AsText<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum
        => property.HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();

    /// <summary>Precisión estándar para importes monetarios.</summary>
    public static PropertyBuilder<decimal> AsMoney(this PropertyBuilder<decimal> property, int precision = 14)
        => property.HasPrecision(precision, 2).IsRequired();
}
