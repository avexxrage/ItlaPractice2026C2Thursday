using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<IReadOnlyList<UserModel>> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<UserModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel> CreateAsync(UserCreateModel model, CancellationToken cancellationToken = default);

    Task<UserModel> UpdateAsync(int id, UserUpdateModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel> ActivateAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel> SuspendAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel> VerifyRenterAsync(int id, CancellationToken cancellationToken = default);

    Task<UserModel> RejectRenterAsync(int id, CancellationToken cancellationToken = default);
}
