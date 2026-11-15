using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles", table =>
        {
            table.HasCheckConstraint("CK_Vehicles_Year", $"\"Year\" >= {Vehicle.MinYear}");
            table.HasCheckConstraint("CK_Vehicles_Mileage", "\"Mileage\" >= 0");
        });
        builder.ConfigureBaseEntity();

        builder.Property(v => v.Brand).HasMaxLength(Vehicle.BrandMaxLength).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(Vehicle.ModelMaxLength).IsRequired();
        builder.Property(v => v.LicensePlate).HasMaxLength(Vehicle.LicensePlateMaxLength).IsRequired();
        builder.Property(v => v.Color).HasMaxLength(Vehicle.ColorMaxLength).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(Vehicle.DescriptionMaxLength);
        builder.Property(v => v.Transmission).AsText();
        builder.Property(v => v.FuelType).AsText();
        builder.Property(v => v.Category).AsText();
        builder.Property(v => v.Status).AsText();
        builder.Property(v => v.DailyRate).AsMoney(12);
        builder.Property(v => v.EstimatedCommercialValue).AsMoney();

        builder.HasOne(v => v.Provider)
            .WithMany(p => p.Vehicles)
            .HasForeignKey(v => v.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.LicensePlate).IsUnique();
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => new { v.Brand, v.Model });
    }
}
