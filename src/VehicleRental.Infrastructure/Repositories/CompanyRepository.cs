using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<CompanyModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var companies = await Entities.AsNoTracking()
            .Include(c => c.Provider)
            .OrderBy(c => c.TradeName)
            .ToListAsync(cancellationToken);

        return companies.Select(ToModel).ToList();
    }

    public async Task<CompanyModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var company = await Entities.AsNoTracking()
            .Include(c => c.Provider)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Company), id);

        return ToModel(company);
    }

    public async Task<CompanyModel> CreateAsync(CompanyCreateModel model, CancellationToken cancellationToken = default)
    {
        var company = new Company(
            model.TradeName,
            model.Rnc,
            model.CommercialRegistry,
            model.ContactEmail,
            model.ContactPhone,
            model.Address);

        if (await Entities.AnyAsync(c => c.Rnc == company.Rnc, cancellationToken))
        {
            throw CompanyException.DuplicateRnc(company.Rnc);
        }

        if (await Entities.AnyAsync(c => c.CommercialRegistry == company.CommercialRegistry, cancellationToken))
        {
            throw CompanyException.DuplicateCommercialRegistry(company.CommercialRegistry);
        }

        await AddAsync(company, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return ToModel(company);
    }

    public async Task<CompanyModel> UpdateAsync(int id, CompanyUpdateModel model, CancellationToken cancellationToken = default)
    {
        var company = await GetRequiredAsync(id, cancellationToken);
        company.UpdateInformation(model.TradeName, model.ContactEmail, model.ContactPhone, model.Address);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var company = await GetRequiredAsync(id, cancellationToken);

        if (await Context.Providers.AnyAsync(p => p.CompanyId == id, cancellationToken))
        {
            throw CompanyException.HasProvider(id);
        }

        Remove(company);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<CompanyModel> VerifyAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, c => c.Verify(), cancellationToken);

    public Task<CompanyModel> RejectAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, c => c.Reject(), cancellationToken);

    private async Task<CompanyModel> ChangeAsync(int id, Action<Company> change, CancellationToken cancellationToken)
    {
        var company = await GetRequiredAsync(id, cancellationToken);
        change(company);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    private static CompanyModel ToModel(Company company) => new(
        company.Id,
        company.TradeName,
        company.Rnc,
        company.CommercialRegistry,
        company.ContactEmail,
        company.ContactPhone,
        company.Address,
        company.VerificationStatus,
        company.Provider?.Id,
        company.CreatedAt);
}
