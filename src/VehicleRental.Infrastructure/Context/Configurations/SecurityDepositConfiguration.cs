using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class SecurityDepositConfiguration : IEntityTypeConfiguration<SecurityDeposit>
{
    public void Configure(EntityTypeBuilder<SecurityDeposit> builder)
    {
        builder.ToTable("SecurityDeposits");
        builder.ConfigureBaseEntity();

        builder.Property(d => d.CalculatedAmount).AsMoney();
        builder.Property(d => d.HeldAmount).AsMoney();
        builder.Property(d => d.RetainedAmount).AsMoney();
        builder.Property(d => d.Status).AsText();
        builder.Property(d => d.PaymentProviderReference).HasMaxLength(SecurityDeposit.PaymentReferenceMaxLength);
        builder.Property(d => d.Notes).HasMaxLength(SecurityDeposit.NotesMaxLength);

        // Una reserva tiene como máximo un depósito; el depósito depende de la reserva.
        builder.HasOne(d => d.Reservation)
            .WithOne(r => r.SecurityDeposit)
            .HasForeignKey<SecurityDeposit>(d => d.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.ReservationId).IsUnique();
    }
}
