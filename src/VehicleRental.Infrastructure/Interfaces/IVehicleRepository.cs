using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IVehicleRepository : IBaseRepository<Vehicle>
{
    Task<IReadOnlyList<VehicleModel>> GetModelsAsync(VehicleFilterModel filter, CancellationToken cancellationToken = default);

    Task<VehicleModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleModel> CreateAsync(VehicleCreateModel model, CancellationToken cancellationToken = default);

    Task<VehicleModel> UpdateAsync(int id, VehicleUpdateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleModel> PublishAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleModel> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleModel> SendToMaintenanceAsync(int id, CancellationToken cancellationToken = default);
}
