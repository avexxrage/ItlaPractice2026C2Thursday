using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/inspections")]
public sealed class VehicleInspectionsController : ControllerBase
{
    private readonly IVehicleInspectionRepository _inspections;

    public VehicleInspectionsController(IVehicleInspectionRepository inspections) => _inspections = inspections;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleInspectionModel>>> GetByReservation([FromQuery] int reservationId, CancellationToken ct)
        => Ok(await _inspections.GetByReservationAsync(reservationId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleInspectionModel>> GetById(int id, CancellationToken ct)
        => Ok(await _inspections.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleInspectionModel>> Create(VehicleInspectionCreateModel model, CancellationToken ct)
    {
        var inspection = await _inspections.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = inspection.Id }, inspection);
    }

    [HttpGet("{id:int}/evidences")]
    public async Task<ActionResult<IReadOnlyList<InspectionEvidenceModel>>> GetEvidences(int id, CancellationToken ct)
        => Ok(await _inspections.GetEvidencesAsync(id, ct));

    [HttpPost("{id:int}/evidences")]
    public async Task<ActionResult<InspectionEvidenceModel>> AddEvidence(int id, InspectionEvidenceCreateModel model, CancellationToken ct)
    {
        var evidence = await _inspections.AddEvidenceAsync(id, model, ct);
        return CreatedAtAction(nameof(GetEvidences), new { id }, evidence);
    }
}
