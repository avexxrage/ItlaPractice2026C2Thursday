using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

/// <summary>Inspecciones de entrega/devolución y sus evidencias.</summary>
public interface IVehicleInspectionRepository : IBaseRepository<VehicleInspection>
{
    Task<IReadOnlyList<VehicleInspectionModel>> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<VehicleInspectionModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<VehicleInspectionModel> CreateAsync(VehicleInspectionCreateModel model, CancellationToken cancellationToken = default);

    Task<InspectionEvidenceModel> AddEvidenceAsync(int inspectionId, InspectionEvidenceCreateModel model, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InspectionEvidenceModel>> GetEvidencesAsync(int inspectionId, CancellationToken cancellationToken = default);
}
