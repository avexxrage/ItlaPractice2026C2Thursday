using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", table =>
            table.HasCheckConstraint("CK_Reservations_Range", "\"EndDate\" > \"StartDate\""));
        builder.ConfigureBaseEntity();

        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate).IsRequired();
        builder.Property(r => r.DailyRate).AsMoney(12);
        builder.Property(r => r.TotalPrice).AsMoney();
        builder.Property(r => r.RiskLevel).AsText();
        builder.Property(r => r.Status).AsText();
        builder.Property(r => r.CancellationReason).HasMaxLength(Reservation.CancellationReasonMaxLength);
        builder.Ignore(r => r.RentalDays);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Renter)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.RenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación explícita con el proveedor para evitar ambigüedad con Vehicle.Provider.
        builder.HasOne(r => r.Provider)
            .WithMany()
            .HasForeignKey(r => r.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.VehicleId, r.StartDate, r.EndDate });
        builder.HasIndex(r => r.RenterId);
        builder.HasIndex(r => r.Status);
    }
}
