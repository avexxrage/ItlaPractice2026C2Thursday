using VehicleRental.Domain.Core;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Services;

/// <summary>
/// Orquesta la confirmación de una reserva: calcula el depósito dinámico con el servicio de dominio
/// y lo persiste junto con la confirmación a través del repositorio.
/// </summary>
public sealed class ReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly SecurityDepositCalculator _depositCalculator;

    public ReservationService(IReservationRepository reservations, SecurityDepositCalculator depositCalculator)
    {
        _reservations = reservations;
        _depositCalculator = depositCalculator;
    }

    public async Task<ReservationModel> ConfirmAsync(int reservationId, CancellationToken cancellationToken)
    {
        var input = await _reservations.GetDepositInputAsync(reservationId, cancellationToken);

        var depositAmount = _depositCalculator.Calculate(
            input.EstimatedCommercialValue,
            input.Category,
            input.RentalDays,
            input.RiskLevel);

        return await _reservations.ConfirmAsync(reservationId, depositAmount, cancellationToken);
    }
}
