using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class VehicleDocumentRepository : BaseRepository<VehicleDocument>, IVehicleDocumentRepository
{
    public VehicleDocumentRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<VehicleDocumentModel>> GetByVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Vehicle>(vehicleId, cancellationToken);

        var documents = await Entities.AsNoTracking()
            .Where(d => d.VehicleId == vehicleId)
            .OrderBy(d => d.DocumentType).ThenBy(d => d.ExpirationDate)
            .ToListAsync(cancellationToken);

        return documents.Select(ToModel).ToList();
    }

    public async Task<VehicleDocumentModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await Entities.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VehicleDocument), id);

        return ToModel(document);
    }

    public async Task<VehicleDocumentModel> CreateAsync(VehicleDocumentCreateModel model, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Vehicle>(model.VehicleId, cancellationToken);

        var document = new VehicleDocument(model.VehicleId, model.DocumentType, model.DocumentNumber, model.ExpirationDate, model.FileReference);
        await AddAsync(document, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return ToModel(document);
    }

    public Task<VehicleDocumentModel> UpdateAsync(int id, VehicleDocumentUpdateModel model, CancellationToken cancellationToken = default)
        => ChangeAsync(id, d => d.Update(model.DocumentNumber, model.ExpirationDate, model.FileReference), cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await GetRequiredAsync(id, cancellationToken);
        Remove(document);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<VehicleDocumentModel> ApproveAsync(int id, CancellationToken cancellationToken = default)
        => ChangeAsync(id, d => d.Approve(), cancellationToken);

    public Task<VehicleDocumentModel> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
        => ChangeAsync(id, d => d.Reject(reason), cancellationToken);

    private async Task<VehicleDocumentModel> ChangeAsync(int id, Action<VehicleDocument> change, CancellationToken cancellationToken)
    {
        var document = await GetRequiredAsync(id, cancellationToken);
        change(document);
        await SaveChangesAsync(cancellationToken);
        return ToModel(document);
    }

    private static VehicleDocumentModel ToModel(VehicleDocument document) => new(
        document.Id,
        document.VehicleId,
        document.DocumentType,
        document.DocumentNumber,
        document.ExpirationDate,
        document.FileReference,
        document.ValidationStatus,
        document.ValidationNotes,
        document.IsExpired(DateTime.UtcNow));
}
