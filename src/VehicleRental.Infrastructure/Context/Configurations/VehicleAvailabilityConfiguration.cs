using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class VehicleAvailabilityConfiguration : IEntityTypeConfiguration<VehicleAvailability>
{
    public void Configure(EntityTypeBuilder<VehicleAvailability> builder)
    {
        builder.ToTable("VehicleAvailabilities", table =>
            table.HasCheckConstraint("CK_VehicleAvailabilities_Range", "\"EndDate\" > \"StartDate\""));
        builder.ConfigureBaseEntity();

        builder.Property(a => a.StartDate).IsRequired();
        builder.Property(a => a.EndDate).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(VehicleAvailability.NotesMaxLength);

        builder.HasOne(a => a.Vehicle)
            .WithMany(v => v.Availabilities)
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.VehicleId, a.StartDate, a.EndDate });
    }
}
