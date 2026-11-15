using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("VehicleDocuments");
        builder.ConfigureBaseEntity();

        builder.Property(d => d.DocumentType).AsText();
        builder.Property(d => d.DocumentNumber).HasMaxLength(VehicleDocument.DocumentNumberMaxLength);
        builder.Property(d => d.ExpirationDate).IsRequired();
        builder.Property(d => d.FileReference).HasMaxLength(VehicleDocument.FileReferenceMaxLength);
        builder.Property(d => d.ValidationStatus).AsText();
        builder.Property(d => d.ValidationNotes).HasMaxLength(VehicleDocument.NotesMaxLength);

        builder.HasOne(d => d.Vehicle)
            .WithMany(v => v.Documents)
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.VehicleId, d.DocumentType });
    }
}
