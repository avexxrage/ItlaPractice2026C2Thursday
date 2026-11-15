using Microsoft.AspNetCore.Mvc;
using VehicleRental.API.Services;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationRepository _reservations;
    private readonly ReservationService _reservationService;

    public ReservationsController(IReservationRepository reservations, ReservationService reservationService)
    {
        _reservations = reservations;
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationModel>>> GetAll([FromQuery] ReservationFilterModel filter, CancellationToken ct)
        => Ok(await _reservations.GetModelsAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationModel>> GetById(int id, CancellationToken ct)
        => Ok(await _reservations.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ReservationModel>> Create(ReservationCreateModel model, CancellationToken ct)
    {
        var reservation = await _reservations.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>Confirma la reserva y genera su depósito de garantía dinámico.</summary>
    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<ReservationModel>> Confirm(int id, CancellationToken ct)
        => Ok(await _reservationService.ConfirmAsync(id, ct));

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<ReservationModel>> Start(int id, CancellationToken ct)
        => Ok(await _reservations.StartAsync(id, ct));

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ReservationModel>> Complete(int id, CancellationToken ct)
        => Ok(await _reservations.CompleteAsync(id, ct));

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationModel>> Cancel(int id, ReasonModel model, CancellationToken ct)
        => Ok(await _reservations.CancelAsync(id, model.Reason, ct));
}
