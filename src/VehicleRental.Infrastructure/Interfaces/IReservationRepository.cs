using VehicleRental.Domain.Entities;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.Infrastructure.Interfaces;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<IReadOnlyList<ReservationModel>> GetModelsAsync(ReservationFilterModel filter, CancellationToken cancellationToken = default);

    Task<ReservationModel> GetModelByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ReservationModel> CreateAsync(ReservationCreateModel model, CancellationToken cancellationToken = default);

    /// <summary>Datos del vehículo y la reserva que usa el cálculo del depósito.</summary>
    Task<ReservationDepositInputModel> GetDepositInputAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Confirma la reserva y crea su depósito obligatorio con el importe ya calculado.</summary>
    Task<ReservationModel> ConfirmAsync(int id, decimal calculatedDepositAmount, CancellationToken cancellationToken = default);

    Task<ReservationModel> StartAsync(int id, CancellationToken cancellationToken = default);

    Task<ReservationModel> CompleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ReservationModel> CancelAsync(int id, string reason, CancellationToken cancellationToken = default);
}
