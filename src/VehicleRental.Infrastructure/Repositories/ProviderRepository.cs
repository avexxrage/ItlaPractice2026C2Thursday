using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class ProviderRepository : BaseRepository<Provider>, IProviderRepository
{
    public ProviderRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<ProviderModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => await Project(Entities.AsNoTracking().OrderBy(p => p.Id)).ToListAsync(cancellationToken);

    public async Task<ProviderModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Project(Entities.AsNoTracking().Where(p => p.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Provider), id);

    public async Task<ProviderModel> CreateIndividualAsync(ProviderIndividualCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<User>(model.UserId, cancellationToken);
        await EnsureUserIsFreeAsync(model.UserId, cancellationToken);

        var provider = Provider.CreateIndividual(model.UserId);
        await AddAsync(provider, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(provider.Id, cancellationToken);
    }

    public async Task<ProviderModel> CreateRentACarAsync(ProviderCompanyCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Company>(model.CompanyId, cancellationToken);

        if (await Entities.AnyAsync(p => p.CompanyId == model.CompanyId, cancellationToken))
        {
            throw ProviderException.CompanyAlreadyProvider(model.CompanyId);
        }

        if (model.ManagerUserId is int managerId)
        {
            await EnsureExistsAsync<User>(managerId, cancellationToken);
            await EnsureUserIsFreeAsync(managerId, cancellationToken);
        }

        var provider = Provider.CreateRentACar(model.CompanyId, model.ManagerUserId);
        await AddAsync(provider, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(provider.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredAsync(id, cancellationToken);

        if (await Context.Vehicles.AnyAsync(v => v.ProviderId == id, cancellationToken))
        {
            throw ProviderException.HasVehicles(id);
        }

        Remove(provider);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<ProviderModel> VerifyAsync(int id, CancellationToken cancellationToken = default)
    {
        var provider = await Entities.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Provider), id);

        provider.Verify(companyVerified: provider.Company?.VerificationStatus == VerificationStatus.Verified);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task<ProviderModel> RejectAsync(int id, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredAsync(id, cancellationToken);
        provider.Reject();
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    private async Task EnsureUserIsFreeAsync(int userId, CancellationToken cancellationToken)
    {
        if (await Entities.AnyAsync(p => p.UserId == userId, cancellationToken))
        {
            throw ProviderException.UserAlreadyProvider(userId);
        }
    }

    private static IQueryable<ProviderModel> Project(IQueryable<Provider> query) => query.Select(p => new ProviderModel(
        p.Id,
        p.Type,
        p.VerificationStatus,
        p.VerifiedAt,
        p.UserId,
        p.User != null ? p.User.FirstName + " " + p.User.LastName : null,
        p.CompanyId,
        p.Company != null ? p.Company.TradeName : null,
        p.Vehicles.Count(),
        p.CreatedAt));
}
