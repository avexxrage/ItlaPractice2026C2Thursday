using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/damage-claims")]
public sealed class DamageClaimsController : ControllerBase
{
    private readonly IDamageClaimRepository _claims;

    public DamageClaimsController(IDamageClaimRepository claims) => _claims = claims;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DamageClaimModel>>> GetByReservation([FromQuery] int reservationId, CancellationToken ct)
        => Ok(await _claims.GetByReservationAsync(reservationId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DamageClaimModel>> GetById(int id, CancellationToken ct)
        => Ok(await _claims.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<DamageClaimModel>> Create(DamageClaimCreateModel model, CancellationToken ct)
    {
        var claim = await _claims.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = claim.Id }, claim);
    }

    [HttpPost("{id:int}/link-inspection")]
    public async Task<ActionResult<DamageClaimModel>> LinkInspection(int id, DamageClaimLinkInspectionModel model, CancellationToken ct)
        => Ok(await _claims.LinkInspectionAsync(id, model.VehicleInspectionId, ct));

    [HttpPost("{id:int}/review")]
    public async Task<ActionResult<DamageClaimModel>> StartReview(int id, CancellationToken ct)
        => Ok(await _claims.StartReviewAsync(id, ct));

    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult<DamageClaimModel>> Accept(int id, ReasonModel model, CancellationToken ct)
        => Ok(await _claims.AcceptAsync(id, model.Reason, ct));

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<DamageClaimModel>> Reject(int id, ReasonModel model, CancellationToken ct)
        => Ok(await _claims.RejectAsync(id, model.Reason, ct));
}
