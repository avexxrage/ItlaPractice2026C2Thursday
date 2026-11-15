using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class DamageClaimConfiguration : IEntityTypeConfiguration<DamageClaim>
{
    public void Configure(EntityTypeBuilder<DamageClaim> builder)
    {
        builder.ToTable("DamageClaims");
        builder.ConfigureBaseEntity();

        builder.Property(c => c.Description).HasMaxLength(DamageClaim.DescriptionMaxLength).IsRequired();
        builder.Property(c => c.ClaimedAmount).AsMoney();
        builder.Property(c => c.Status).AsText();
        builder.Property(c => c.ResolutionNotes).HasMaxLength(DamageClaim.ResolutionNotesMaxLength);
        builder.Ignore(c => c.IsResolved);

        // Las reclamaciones son historial: no se borran en cascada.
        builder.HasOne(c => c.Reservation)
            .WithMany(r => r.DamageClaims)
            .HasForeignKey(c => c.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict para no crear múltiples rutas de borrado en cascada (Reservation → Inspection).
        builder.HasOne(c => c.VehicleInspection)
            .WithMany()
            .HasForeignKey(c => c.VehicleInspectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ReservationId, c.Status });
    }
}
