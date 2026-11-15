using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class DamageClaimRepository : BaseRepository<DamageClaim>, IDamageClaimRepository
{
    public DamageClaimRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<DamageClaimModel>> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Reservation>(reservationId, cancellationToken);

        var claims = await Entities.AsNoTracking()
            .Where(c => c.ReservationId == reservationId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return claims.Select(ToModel).ToList();
    }

    public async Task<DamageClaimModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var claim = await Entities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(DamageClaim), id);

        return ToModel(claim);
    }

    public async Task<DamageClaimModel> CreateAsync(DamageClaimCreateModel model, CancellationToken cancellationToken = default)
    {
        var reservation = await Context.Reservations
            .Include(r => r.DamageClaims)
            .FirstOrDefaultAsync(r => r.Id == model.ReservationId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Reservation), model.ReservationId);

        var claim = reservation.OpenDamageClaim(model.Description, model.ClaimedAmount);

        if (model.VehicleInspectionId is int inspectionId)
        {
            claim.LinkInspection(await GetInspectionAsync(inspectionId, cancellationToken));
        }

        await SaveChangesAsync(cancellationToken);
        return ToModel(claim);
    }

    public async Task<DamageClaimModel> LinkInspectionAsync(int id, int inspectionId, CancellationToken cancellationToken = default)
    {
        var claim = await GetRequiredAsync(id, cancellationToken);
        claim.LinkInspection(await GetInspectionAsync(inspectionId, cancellationToken));
        await SaveChangesAsync(cancellationToken);
        return ToModel(claim);
    }

    public async Task<DamageClaimModel> StartReviewAsync(int id, CancellationToken cancellationToken = default)
    {
        var claim = await GetRequiredAsync(id, cancellationToken);
        claim.StartReview();
        await SaveChangesAsync(cancellationToken);
        return ToModel(claim);
    }

    public async Task<DamageClaimModel> AcceptAsync(int id, string resolutionNotes, CancellationToken cancellationToken = default)
    {
        var claim = await GetRequiredAsync(id, cancellationToken);

        var evidenceCount = claim.VehicleInspectionId is int inspectionId
            ? await Context.InspectionEvidences.CountAsync(e => e.VehicleInspectionId == inspectionId, cancellationToken)
            : 0;

        claim.Accept(resolutionNotes, evidenceCount);
        await SaveChangesAsync(cancellationToken);
        return ToModel(claim);
    }

    public async Task<DamageClaimModel> RejectAsync(int id, string resolutionNotes, CancellationToken cancellationToken = default)
    {
        var claim = await GetRequiredAsync(id, cancellationToken);
        claim.Reject(resolutionNotes);
        await SaveChangesAsync(cancellationToken);
        return ToModel(claim);
    }

    private async Task<VehicleInspection> GetInspectionAsync(int inspectionId, CancellationToken cancellationToken)
        => await Context.VehicleInspections.FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VehicleInspection), inspectionId);

    private static DamageClaimModel ToModel(DamageClaim claim) => new(
        claim.Id,
        claim.ReservationId,
        claim.VehicleInspectionId,
        claim.Description,
        claim.ClaimedAmount,
        claim.Status,
        claim.ResolutionNotes,
        claim.CreatedAt,
        claim.ResolvedAt);
}
