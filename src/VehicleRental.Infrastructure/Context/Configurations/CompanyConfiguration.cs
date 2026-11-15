using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Core;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.ConfigureBaseEntity();

        builder.Property(c => c.TradeName).HasMaxLength(Company.TradeNameMaxLength).IsRequired();
        builder.Property(c => c.Rnc).HasMaxLength(Company.RncMaxLength).IsRequired();
        builder.Property(c => c.CommercialRegistry).HasMaxLength(Company.CommercialRegistryMaxLength).IsRequired();
        builder.Property(c => c.ContactEmail).HasMaxLength(Person.EmailMaxLength).IsRequired();
        builder.Property(c => c.ContactPhone).HasMaxLength(Person.PhoneMaxLength).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(Company.AddressMaxLength).IsRequired();
        builder.Property(c => c.VerificationStatus).AsText();

        builder.HasIndex(c => c.Rnc).IsUnique();
        builder.HasIndex(c => c.CommercialRegistry).IsUnique();
    }
}
