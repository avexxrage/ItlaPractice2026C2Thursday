using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class SecurityDepositRepository : BaseRepository<SecurityDeposit>, ISecurityDepositRepository
{
    public SecurityDepositRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<SecurityDepositModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var deposit = await Entities.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(SecurityDeposit), id);

        return ToModel(deposit);
    }

    public async Task<SecurityDepositModel> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Reservation>(reservationId, cancellationToken);

        var deposit = await Entities.AsNoTracking().FirstOrDefaultAsync(d => d.ReservationId == reservationId, cancellationToken)
            ?? throw new EntityNotFoundException($"{nameof(SecurityDeposit)} para la reserva", reservationId);

        return ToModel(deposit);
    }

    public async Task<SecurityDepositModel> RegisterHoldAsync(int id, SecurityDepositHoldModel model, CancellationToken cancellationToken = default)
    {
        var deposit = await GetRequiredAsync(id, cancellationToken);
        deposit.RegisterHold(model.HeldAmount, model.PaymentProviderReference);
        await SaveChangesAsync(cancellationToken);
        return ToModel(deposit);
    }

    public async Task<SecurityDepositModel> ReleaseAsync(int id, CancellationToken cancellationToken = default)
    {
        var deposit = await Entities.Include(d => d.Reservation).FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(SecurityDeposit), id);

        var reservation = deposit.Reservation!;
        if (reservation.Status is not (ReservationStatus.Completed or ReservationStatus.Cancelled))
        {
            throw SecurityDepositException.ReservationNotFinished(reservation.Id);
        }

        var claimStatuses = await Context.DamageClaims
            .Where(c => c.ReservationId == reservation.Id)
            .Select(c => c.Status)
            .ToListAsync(cancellationToken);

        if (claimStatuses.Any(s => s is DamageClaimStatus.Submitted or DamageClaimStatus.UnderReview))
        {
            throw SecurityDepositException.UnresolvedClaims(reservation.Id);
        }

        if (claimStatuses.Contains(DamageClaimStatus.Accepted))
        {
            throw SecurityDepositException.AcceptedClaimsPending(reservation.Id);
        }

        deposit.Release();
        await SaveChangesAsync(cancellationToken);
        return ToModel(deposit);
    }

    public async Task<SecurityDepositModel> RetainAsync(int id, SecurityDepositRetainModel model, CancellationToken cancellationToken = default)
    {
        var deposit = await GetRequiredAsync(id, cancellationToken);

        var claim = await Context.DamageClaims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.DamageClaimId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(DamageClaim), model.DamageClaimId);

        if (claim.ReservationId != deposit.ReservationId)
        {
            throw SecurityDepositException.ClaimFromOtherReservation();
        }

        if (claim.Status != DamageClaimStatus.Accepted)
        {
            throw DamageClaimException.NotAccepted(claim.Id);
        }

        if (model.Amount > claim.ClaimedAmount)
        {
            throw SecurityDepositException.RetainExceedsClaim();
        }

        deposit.Retain(model.Amount, model.Notes);
        await SaveChangesAsync(cancellationToken);
        return ToModel(deposit);
    }

    private static SecurityDepositModel ToModel(SecurityDeposit deposit) => new(
        deposit.Id,
        deposit.ReservationId,
        deposit.CalculatedAmount,
        deposit.HeldAmount,
        deposit.RetainedAmount,
        deposit.Status,
        deposit.PaymentProviderReference,
        deposit.Notes,
        deposit.CreatedAt,
        deposit.HeldAt,
        deposit.ReleasedAt);
}
