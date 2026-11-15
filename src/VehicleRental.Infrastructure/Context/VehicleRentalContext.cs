using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Core;
using VehicleRental.Domain.Entities;

namespace VehicleRental.Infrastructure.Context;

/// <summary>
/// Contexto de Entity Framework Core de la plataforma de alquiler de vehículos.
/// Las configuraciones Fluent API de cada entidad están en Context/Configurations.
/// </summary>
public class VehicleRentalContext : DbContext
{
    public VehicleRentalContext(DbContextOptions<VehicleRentalContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();

    public DbSet<VehicleAvailability> VehicleAvailabilities => Set<VehicleAvailability>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<SecurityDeposit> SecurityDeposits => Set<SecurityDeposit>();

    public DbSet<VehicleInspection> VehicleInspections => Set<VehicleInspection>();

    public DbSet<InspectionEvidence> InspectionEvidences => Set<InspectionEvidence>();

    public DbSet<DamageClaim> DamageClaims => Set<DamageClaim>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkAsUpdated();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleRentalContext).Assembly);
    }
}
