using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Entities;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Core;
using VehicleRental.Infrastructure.Exceptions;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Repositories;

public sealed class VehicleInspectionRepository : BaseRepository<VehicleInspection>, IVehicleInspectionRepository
{
    public VehicleInspectionRepository(VehicleRentalContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<VehicleInspectionModel>> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<Reservation>(reservationId, cancellationToken);

        var inspections = await Entities.AsNoTracking()
            .Include(i => i.Evidences)
            .Where(i => i.ReservationId == reservationId)
            .OrderBy(i => i.InspectedAt)
            .ToListAsync(cancellationToken);

        return inspections.Select(ToModel).ToList();
    }

    public async Task<VehicleInspectionModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var inspection = await Entities.AsNoTracking()
            .Include(i => i.Evidences)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VehicleInspection), id);

        return ToModel(inspection);
    }

    public async Task<VehicleInspectionModel> CreateAsync(VehicleInspectionCreateModel model, CancellationToken cancellationToken = default)
    {
        var reservation = await Context.Reservations
            .Include(r => r.Inspections)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == model.ReservationId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Reservation), model.ReservationId);

        var recordedMileage = reservation.Vehicle!.Mileage;
        if (model.Mileage < recordedMileage)
        {
            throw InspectionException.MileageBelowVehicle(recordedMileage);
        }

        var inspection = reservation.RegisterInspection(model.Type, model.Mileage, model.FuelLevelPercentage, model.Observations);
        await SaveChangesAsync(cancellationToken);
        return ToModel(inspection);
    }

    public async Task<InspectionEvidenceModel> AddEvidenceAsync(int inspectionId, InspectionEvidenceCreateModel model, CancellationToken cancellationToken = default)
    {
        var inspection = await GetRequiredAsync(inspectionId, cancellationToken);
        var evidence = inspection.AddEvidence(model.FileReference, model.EvidenceType, model.Description);
        await SaveChangesAsync(cancellationToken);
        return ToModel(evidence);
    }

    public async Task<IReadOnlyList<InspectionEvidenceModel>> GetEvidencesAsync(int inspectionId, CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync<VehicleInspection>(inspectionId, cancellationToken);

        var evidences = await Context.InspectionEvidences.AsNoTracking()
            .Where(e => e.VehicleInspectionId == inspectionId)
            .OrderBy(e => e.RegisteredAt)
            .ToListAsync(cancellationToken);

        return evidences.Select(ToModel).ToList();
    }

    private static VehicleInspectionModel ToModel(VehicleInspection inspection) => new(
        inspection.Id,
        inspection.ReservationId,
        inspection.Type,
        inspection.Mileage,
        inspection.FuelLevelPercentage,
        inspection.Observations,
        inspection.InspectedAt,
        inspection.Evidences.Select(ToModel).ToList());

    private static InspectionEvidenceModel ToModel(InspectionEvidence evidence) => new(
        evidence.Id,
        evidence.VehicleInspectionId,
        evidence.FileReference,
        evidence.EvidenceType,
        evidence.Description,
        evidence.RegisteredAt);
}
