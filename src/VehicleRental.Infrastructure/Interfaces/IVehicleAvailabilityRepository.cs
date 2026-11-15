using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IVehicleAvailabilityRepository : IBaseRepository<VehicleAvailability>
{
    Task<IReadOnlyList<VehicleAvailabilityModel>> GetByVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    Task<VehicleAvailabilityModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleAvailabilityModel> CreateAsync(VehicleAvailabilityCreateModel model, CancellationToken cancellationToken = default);

    Task<VehicleAvailabilityModel> UpdateAsync(int id, VehicleAvailabilityUpdateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>true si no hay un período bloqueado que se cruce con el rango indicado.</summary>
    Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
