using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IProviderRepository : IBaseRepository<Provider>
{
    Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<ProviderModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProviderModel> CreateIndividualAsync(ProviderIndividualCreateModel model, CancellationToken cancellationToken = default);

    Task<ProviderModel> CreateRentACarAsync(ProviderCompanyCreateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ProviderModel> VerifyAsync(int id, CancellationToken cancellationToken = default);

    Task<ProviderModel> RejectAsync(int id, CancellationToken cancellationToken = default);
}
