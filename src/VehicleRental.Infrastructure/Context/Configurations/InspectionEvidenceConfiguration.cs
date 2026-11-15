using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context.Configurations;

internal sealed class InspectionEvidenceConfiguration : IEntityTypeConfiguration<InspectionEvidence>
{
    public void Configure(EntityTypeBuilder<InspectionEvidence> builder)
    {
        builder.ToTable("InspectionEvidences");
        builder.ConfigureBaseEntity();

        builder.Property(e => e.FileReference).HasMaxLength(InspectionEvidence.FileReferenceMaxLength).IsRequired();
        builder.Property(e => e.EvidenceType).AsText();
        builder.Property(e => e.Description).HasMaxLength(InspectionEvidence.DescriptionMaxLength);
        builder.Property(e => e.RegisteredAt).IsRequired();

        builder.HasOne(e => e.VehicleInspection)
            .WithMany(i => i.Evidences)
            .HasForeignKey(e => e.VehicleInspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
