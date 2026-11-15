using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<VehicleModel>> GetModelsAsync(VehicleFilterModel filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var query = Entities.AsNoTracking().Include(v => v.Provider).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(v => EF.Functions.Like(v.Brand, $"%{filter.Brand.Trim()}%"));
        }

        if (filter.Category is not null)
        {
            query = query.Where(v => v.Category == filter.Category);
        }

        if (filter.Status is not null)
        {
            query = query.Where(v => v.Status == filter.Status);
        }

        if (filter.ProviderId is not null)
        {
            query = query.Where(v => v.ProviderId == filter.ProviderId);
        }

        var vehicles = await query.OrderBy(v => v.Brand).ThenBy(v => v.Model).ToListAsync(cancellationToken);
        return vehicles.Select(ToModel).ToList();
    }

    public async Task<VehicleModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await Entities.AsNoTracking()
            .Include(v => v.Provider)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Vehicle), id);

        return ToModel(vehicle);
    }

    public async Task<VehicleModel> CreateAsync(VehicleCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Provider>(model.ProviderId, cancellationToken);

        var vehicle = new Vehicle(
            model.ProviderId,
            model.Brand,
            model.Model,
            model.Year,
            model.LicensePlate,
            model.Color,
            model.Transmission,
            model.FuelType,
            model.Category,
            model.Mileage,
            model.DailyRate,
            model.EstimatedCommercialValue,
            model.Description);

        if (await Entities.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate, cancellationToken))
        {
            throw VehicleException.DuplicatePlate(vehicle.LicensePlate);
        }

        await AddAsync(vehicle, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(vehicle.Id, cancellationToken);
    }

    public async Task<VehicleModel> UpdateAsync(int id, VehicleUpdateModel model, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetRequiredAsync(id, cancellationToken);

        vehicle.UpdateDetails(
            model.Brand,
            model.Model,
            model.Year,
            model.Color,
            model.Transmission,
            model.FuelType,
            model.Category,
            model.DailyRate,
            model.EstimatedCommercialValue,
            model.Description);
        vehicle.UpdateMileage(model.Mileage);

        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetRequiredAsync(id, cancellationToken);

        if (await Context.Reservations.AnyAsync(r => r.VehicleId == id, cancellationToken))
        {
            throw VehicleException.HasReservations(id);
        }

        Remove(vehicle);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<VehicleModel> PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await Entities.Include(v => v.Provider).FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Vehicle), id);

        vehicle.Publish(providerVerified: vehicle.Provider?.CanPublishVehicles == true);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public Task<VehicleModel> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, v => v.Deactivate(), cancellationToken);

    public Task<VehicleModel> SendToMaintenanceAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, v => v.SendToMaintenance(), cancellationToken);

    private async Task<VehicleModel> ChangeAsync(int id, Action<Vehicle> change, CancellationToken cancellationToken)
    {
        var vehicle = await GetRequiredAsync(id, cancellationToken);
        change(vehicle);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    private static VehicleModel ToModel(Vehicle vehicle) => new(
        vehicle.Id,
        vehicle.ProviderId,
        vehicle.Provider!.Type,
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.LicensePlate,
        vehicle.Color,
        vehicle.Transmission,
        vehicle.FuelType,
        vehicle.Category,
        vehicle.Mileage,
        vehicle.DailyRate,
        vehicle.EstimatedCommercialValue,
        vehicle.Description,
        vehicle.Status,
        vehicle.CreatedAt);
}
