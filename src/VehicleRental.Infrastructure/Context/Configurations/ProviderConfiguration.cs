using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers", table => table.HasCheckConstraint(
            "CK_Providers_Type",
            "(\"Type\" = 'Individual' AND \"UserId\" IS NOT NULL AND \"CompanyId\" IS NULL) OR " +
            "(\"Type\" = 'RentACarCompany' AND \"CompanyId\" IS NOT NULL)"));
        builder.ConfigureBaseEntity();

        builder.Property(p => p.Type).AsText();
        builder.Property(p => p.VerificationStatus).AsText();
        builder.Ignore(p => p.CanPublishVehicles);

        // Un usuario puede ser como máximo un proveedor (particular o administrador de empresa).
        builder.HasOne(p => p.User)
            .WithOne(u => u.Provider)
            .HasForeignKey<Provider>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una empresa Rent a Car tiene un único perfil de proveedor que agrupa su flota.
        builder.HasOne(p => p.Company)
            .WithOne(c => c.Provider)
            .HasForeignKey<Provider>(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.CompanyId).IsUnique();
    }
}
