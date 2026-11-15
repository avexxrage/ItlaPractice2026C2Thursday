using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Core;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBaseEntity();

        builder.Property(u => u.FirstName).HasMaxLength(Person.NameMaxLength).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(Person.NameMaxLength).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(Person.EmailMaxLength).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(Person.PhoneMaxLength).IsRequired();
        builder.Property(u => u.IdentificationNumber).HasMaxLength(Person.IdentificationMaxLength).IsRequired();
        builder.Property(u => u.Status).AsText();
        builder.Property(u => u.RenterVerificationStatus).AsText();
        builder.Property(u => u.RegisteredAt).IsRequired();

        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.CanRent);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IdentificationNumber).IsUnique();
        builder.HasIndex(u => u.Status);
    }
}
