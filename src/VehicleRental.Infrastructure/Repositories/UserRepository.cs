using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<UserModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var users = await Entities.AsNoTracking()
            .Include(u => u.Provider)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return users.Select(ToModel).ToList();
    }

    public async Task<UserModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await Entities.AsNoTracking()
            .Include(u => u.Provider)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(User), id);

        return ToModel(user);
    }

    public async Task<UserModel> CreateAsync(UserCreateModel model, CancellationToken cancellationToken = default)
    {
        var user = new User(model.FirstName, model.LastName, model.Email, model.PhoneNumber, model.IdentificationNumber);

        if (await Entities.AnyAsync(u => u.Email == user.Email, cancellationToken))
        {
            throw UserException.DuplicateEmail(user.Email);
        }

        if (await Entities.AnyAsync(u => u.IdentificationNumber == user.IdentificationNumber, cancellationToken))
        {
            throw UserException.DuplicateIdentification();
        }

        await AddAsync(user, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return ToModel(user);
    }

    public async Task<UserModel> UpdateAsync(int id, UserUpdateModel model, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredAsync(id, cancellationToken);
        user.UpdateProfile(model.FirstName, model.LastName, model.Email, model.PhoneNumber);

        if (await Entities.AnyAsync(u => u.Id != id && u.Email == user.Email, cancellationToken))
        {
            throw UserException.DuplicateEmail(user.Email);
        }

        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredAsync(id, cancellationToken);

        var hasRelatedData = await Context.Reservations.AnyAsync(r => r.RenterId == id, cancellationToken)
            || await Context.Providers.AnyAsync(p => p.UserId == id, cancellationToken);

        if (hasRelatedData)
        {
            throw UserException.HasRelatedData(id);
        }

        Remove(user);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<UserModel> ActivateAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, u => u.Activate(), cancellationToken);

    public Task<UserModel> SuspendAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, u => u.Suspend(), cancellationToken);

    public Task<UserModel> VerifyRenterAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, u => u.VerifyAsRenter(), cancellationToken);

    public Task<UserModel> RejectRenterAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, u => u.RejectRenterVerification(), cancellationToken);

    private async Task<UserModel> ChangeAsync(int id, Action<User> change, CancellationToken cancellationToken)
    {
        var user = await GetRequiredAsync(id, cancellationToken);
        change(user);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    private static UserModel ToModel(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.PhoneNumber,
        SensitiveData.Mask(user.IdentificationNumber),
        user.Status,
        user.RenterVerificationStatus,
        user.CanRent,
        user.Provider?.Id,
        user.RegisteredAt);
}
