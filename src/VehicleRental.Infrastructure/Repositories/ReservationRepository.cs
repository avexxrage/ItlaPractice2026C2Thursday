using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Core.Enums;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<ReservationModel>> GetModelsAsync(ReservationFilterModel filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var query = WithDetails(Entities.AsNoTracking());

        if (filter.RenterId is not null)
        {
            query = query.Where(r => r.RenterId == filter.RenterId);
        }

        if (filter.VehicleId is not null)
        {
            query = query.Where(r => r.VehicleId == filter.VehicleId);
        }

        if (filter.ProviderId is not null)
        {
            query = query.Where(r => r.ProviderId == filter.ProviderId);
        }

        if (filter.Status is not null)
        {
            query = query.Where(r => r.Status == filter.Status);
        }

        var reservations = await query.OrderByDescending(r => r.StartDate).ToListAsync(cancellationToken);
        return reservations.Select(ToModel).ToList();
    }

    public async Task<ReservationModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await WithDetails(Entities.AsNoTracking()).FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Reservation), id);

        return ToModel(reservation);
    }

    public async Task<ReservationModel> CreateAsync(ReservationCreateModel model, CancellationToken cancellationToken = default)
    {
        var renter = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == model.RenterId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(User), model.RenterId);

        if (!renter.CanRent)
        {
            throw ReservationException.RenterNotAllowed(renter.Id);
        }

        var vehicle = await Context.Vehicles.AsNoTracking()
            .Include(v => v.Provider)
            .FirstOrDefaultAsync(v => v.Id == model.VehicleId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Vehicle), model.VehicleId);

        if (vehicle.Provider?.UserId == renter.Id)
        {
            throw ReservationException.OwnVehicle();
        }

        // La entidad valida fechas, precio y riesgo antes de consultar disponibilidad.
        var reservation = new Reservation(
            vehicle.Id,
            renter.Id,
            vehicle.ProviderId,
            model.StartDate,
            model.EndDate,
            vehicle.DailyRate,
            model.RiskLevel);

        if (vehicle.Status != VehicleStatus.Active || await IsBlockedAsync(vehicle.Id, reservation, cancellationToken))
        {
            throw ReservationException.VehicleNotAvailable(vehicle.Id);
        }

        var overlaps = await Entities.AnyAsync(
            r => r.VehicleId == vehicle.Id
                && (r.Status == ReservationStatus.Pending
                    || r.Status == ReservationStatus.Confirmed
                    || r.Status == ReservationStatus.Active)
                && r.StartDate < reservation.EndDate
                && reservation.StartDate < r.EndDate,
            cancellationToken);

        if (overlaps)
        {
            throw ReservationException.OverlappingReservation(vehicle.Id);
        }

        await AddAsync(reservation, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(reservation.Id, cancellationToken);
    }

    public async Task<ReservationDepositInputModel> GetDepositInputAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await Entities.AsNoTracking()
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Reservation), id);

        return new ReservationDepositInputModel(
            reservation.Id,
            reservation.Vehicle!.EstimatedCommercialValue,
            reservation.Vehicle.Category,
            reservation.RentalDays,
            reservation.RiskLevel);
    }

    public async Task<ReservationModel> ConfirmAsync(int id, decimal calculatedDepositAmount, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadForWorkflowAsync(id, cancellationToken);
        reservation.Confirm(calculatedDepositAmount);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task<ReservationModel> StartAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadForWorkflowAsync(id, cancellationToken);
        reservation.Start();
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task<ReservationModel> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadForWorkflowAsync(id, cancellationToken);
        reservation.Complete();

        // El kilometraje de devolución actualiza el registro del vehículo.
        var returnInspection = reservation.Inspections.Single(i => i.Type == InspectionType.Return);
        var vehicle = await Context.Vehicles.FirstAsync(v => v.Id == reservation.VehicleId, cancellationToken);
        vehicle.UpdateMileage(returnInspection.Mileage);

        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    public async Task<ReservationModel> CancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var reservation = await LoadForWorkflowAsync(id, cancellationToken);
        reservation.Cancel(reason);
        await SaveChangesAsync(cancellationToken);
        return await GetModelByIdAsync(id, cancellationToken);
    }

    private Task<bool> IsBlockedAsync(int vehicleId, Reservation reservation, CancellationToken cancellationToken)
        => Context.VehicleAvailabilities.AnyAsync(
            a => a.VehicleId == vehicleId
                && !a.IsAvailable
                && a.StartDate < reservation.EndDate
                && reservation.StartDate < a.EndDate,
            cancellationToken);

    private async Task<Reservation> LoadForWorkflowAsync(int id, CancellationToken cancellationToken)
        => await Entities
            .Include(r => r.SecurityDeposit)
            .Include(r => r.Inspections)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Reservation), id);

    private static IQueryable<Reservation> WithDetails(IQueryable<Reservation> query)
        => query
            .Include(r => r.Vehicle)
            .Include(r => r.Renter)
            .Include(r => r.SecurityDeposit);

    private static ReservationModel ToModel(Reservation reservation) => new(
        reservation.Id,
        reservation.VehicleId,
        reservation.Vehicle is null
            ? string.Empty
            : $"{reservation.Vehicle.Brand} {reservation.Vehicle.Model} {reservation.Vehicle.Year} ({reservation.Vehicle.LicensePlate})",
        reservation.RenterId,
        reservation.Renter?.FullName ?? string.Empty,
        reservation.ProviderId,
        reservation.StartDate,
        reservation.EndDate,
        reservation.RentalDays,
        reservation.DailyRate,
        reservation.TotalPrice,
        reservation.RiskLevel,
        reservation.Status,
        reservation.CancellationReason,
        reservation.SecurityDeposit?.Id,
        reservation.SecurityDeposit?.CalculatedAmount,
        reservation.SecurityDeposit?.Status,
        reservation.CreatedAt);
}
