using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IVehicleDocumentRepository : IBaseRepository<VehicleDocument>
{
    Task<IReadOnlyList<VehicleDocumentModel>> GetByVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    Task<VehicleDocumentModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleDocumentModel> CreateAsync(VehicleDocumentCreateModel model, CancellationToken cancellationToken = default);

    Task<VehicleDocumentModel> UpdateAsync(int id, VehicleDocumentUpdateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleDocumentModel> ApproveAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleDocumentModel> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
}
