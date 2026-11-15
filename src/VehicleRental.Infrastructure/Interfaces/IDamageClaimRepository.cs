using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IDamageClaimRepository : IBaseRepository<DamageClaim>
{
    Task<IReadOnlyList<DamageClaimModel>> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<DamageClaimModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DamageClaimModel> CreateAsync(DamageClaimCreateModel model, CancellationToken cancellationToken = default);

    Task<DamageClaimModel> LinkInspectionAsync(int id, int inspectionId, CancellationToken cancellationToken = default);

    Task<DamageClaimModel> StartReviewAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Acepta la reclamación solo si la inspección vinculada tiene evidencias.</summary>
    Task<DamageClaimModel> AcceptAsync(int id, string resolutionNotes, CancellationToken cancellationToken = default);

    Task<DamageClaimModel> RejectAsync(int id, string resolutionNotes, CancellationToken cancellationToken = default);
}
