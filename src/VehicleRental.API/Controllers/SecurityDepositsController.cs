using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

/// <summary>
/// Registro del estado del depósito. Los movimientos reales los ejecuta el proveedor de pagos;
/// estos endpoints solo registran lo que ese proveedor confirma.
/// </summary>
[ApiController]
[Route("api/security-deposits")]
public sealed class SecurityDepositsController : ControllerBase
{
    private readonly ISecurityDepositRepository _deposits;

    public SecurityDepositsController(ISecurityDepositRepository deposits) => _deposits = deposits;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SecurityDepositModel>> GetById(int id, CancellationToken ct)
        => Ok(await _deposits.GetModelByIdAsync(id, ct));

    [HttpGet("by-reservation/{reservationId:int}")]
    public async Task<ActionResult<SecurityDepositModel>> GetByReservation(int reservationId, CancellationToken ct)
        => Ok(await _deposits.GetByReservationAsync(reservationId, ct));

    [HttpPost("{id:int}/hold")]
    public async Task<ActionResult<SecurityDepositModel>> RegisterHold(int id, SecurityDepositHoldModel model, CancellationToken ct)
        => Ok(await _deposits.RegisterHoldAsync(id, model, ct));

    [HttpPost("{id:int}/release")]
    public async Task<ActionResult<SecurityDepositModel>> Release(int id, CancellationToken ct)
        => Ok(await _deposits.ReleaseAsync(id, ct));

    [HttpPost("{id:int}/retain")]
    public async Task<ActionResult<SecurityDepositModel>> Retain(int id, SecurityDepositRetainModel model, CancellationToken ct)
        => Ok(await _deposits.RetainAsync(id, model, ct));
}
