using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface ICompanyRepository : IBaseRepository<Company>
{
    Task<IReadOnlyList<CompanyModel>> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<CompanyModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CompanyModel> CreateAsync(CompanyCreateModel model, CancellationToken cancellationToken = default);

    Task<CompanyModel> UpdateAsync(int id, CompanyUpdateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<CompanyModel> VerifyAsync(int id, CancellationToken cancellationToken = default);

    Task<CompanyModel> RejectAsync(int id, CancellationToken cancellationToken = default);
}
