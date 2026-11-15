using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class VehicleAvailabilityRepository : BaseRepository<VehicleAvailability>, IVehicleAvailabilityRepository
{
    public VehicleAvailabilityRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<VehicleAvailabilityModel>> GetByVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Vehicle>(vehicleId, cancellationToken);

        var periods = await Entities.AsNoTracking()
            .Where(a => a.VehicleId == vehicleId)
            .OrderBy(a => a.StartDate)
            .ToListAsync(cancellationToken);

        return periods.Select(ToModel).ToList();
    }

    public async Task<VehicleAvailabilityModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var period = await Entities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VehicleAvailability), id);

        return ToModel(period);
    }

    public async Task<VehicleAvailabilityModel> CreateAsync(VehicleAvailabilityCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Vehicle>(model.VehicleId, cancellationToken);

        var period = new VehicleAvailability(model.VehicleId, model.StartDate, model.EndDate, model.IsAvailable, model.Notes);
        await EnsureNoOverlapAsync(period, excludeId: null, cancellationToken);

        await AddAsync(period, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return ToModel(period);
    }

    public async Task<VehicleAvailabilityModel> UpdateAsync(int id, VehicleAvailabilityUpdateModel model, CancellationToken cancellationToken = default)
    {
        var period = await GetRequiredAsync(id, cancellationToken);
        period.Update(model.StartDate, model.EndDate, model.IsAvailable, model.Notes);
        await EnsureNoOverlapAsync(period, excludeId: id, cancellationToken);

        await SaveChangesAsync(cancellationToken);
        return ToModel(period);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var period = await GetRequiredAsync(id, cancellationToken);
        Remove(period);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => !await Entities.AnyAsync(
            a => a.VehicleId == vehicleId && !a.IsAvailable && a.StartDate < endDate && startDate < a.EndDate,
            cancellationToken);

    private async Task EnsureNoOverlapAsync(VehicleAvailability period, int? excludeId, CancellationToken cancellationToken)
    {
        var overlaps = await Entities.AnyAsync(
            a => a.VehicleId == period.VehicleId
                && a.Id != excludeId
                && a.StartDate < period.EndDate
                && period.StartDate < a.EndDate,
            cancellationToken);

        if (overlaps)
        {
            throw VehicleException.OverlappingAvailability(period.VehicleId);
        }
    }

    private static VehicleAvailabilityModel ToModel(VehicleAvailability period) => new(
        period.Id,
        period.VehicleId,
        period.StartDate,
        period.EndDate,
        period.IsAvailable,
        period.Notes);
}
