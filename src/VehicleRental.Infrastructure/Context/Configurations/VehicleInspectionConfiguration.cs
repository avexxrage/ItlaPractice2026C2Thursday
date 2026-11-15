using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class VehicleInspectionConfiguration : IEntityTypeConfiguration<VehicleInspection>
{
    public void Configure(EntityTypeBuilder<VehicleInspection> builder)
    {
        builder.ToTable("VehicleInspections", table =>
        {
            table.HasCheckConstraint("CK_VehicleInspections_Fuel", "\"FuelLevelPercentage\" BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_VehicleInspections_Mileage", "\"Mileage\" >= 0");
        });
        builder.ConfigureBaseEntity();

        builder.Property(i => i.Type).AsText();
        builder.Property(i => i.Observations).HasMaxLength(VehicleInspection.ObservationsMaxLength);
        builder.Property(i => i.InspectedAt).IsRequired();

        builder.HasOne(i => i.Reservation)
            .WithMany(r => r.Inspections)
            .HasForeignKey(i => i.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Una inspección de entrega y una de devolución por reserva.
        builder.HasIndex(i => new { i.ReservationId, i.Type }).IsUnique();
    }
}
