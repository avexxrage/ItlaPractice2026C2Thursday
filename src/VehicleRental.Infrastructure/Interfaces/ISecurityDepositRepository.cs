using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

/// <summary>
/// Registra el estado del depósito según lo confirmado por el proveedor de pagos.
/// No ejecuta cobros, retenciones ni transferencias reales.
/// </summary>
public interface ISecurityDepositRepository : IBaseRepository<SecurityDeposit>
{
    Task<SecurityDepositModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SecurityDepositModel> GetByReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<SecurityDepositModel> RegisterHoldAsync(int id, SecurityDepositHoldModel model, CancellationToken cancellationToken = default);

    Task<SecurityDepositModel> ReleaseAsync(int id, CancellationToken cancellationToken = default);

    Task<SecurityDepositModel> RetainAsync(int id, SecurityDepositRetainModel model, CancellationToken cancellationToken = default);
}
