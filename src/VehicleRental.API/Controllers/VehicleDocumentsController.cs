using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/vehicle-documents")]
public sealed class VehicleDocumentsController : ControllerBase
{
    private readonly IVehicleDocumentRepository _documents;

    public VehicleDocumentsController(IVehicleDocumentRepository documents) => _documents = documents;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleDocumentModel>>> GetByVehicle([FromQuery] int vehicleId, CancellationToken ct)
        => Ok(await _documents.GetByVehicleAsync(vehicleId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDocumentModel>> GetById(int id, CancellationToken ct)
        => Ok(await _documents.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleDocumentModel>> Create(VehicleDocumentCreateModel model, CancellationToken ct)
    {
        var document = await _documents.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleDocumentModel>> Update(int id, VehicleDocumentUpdateModel model, CancellationToken ct)
        => Ok(await _documents.UpdateAsync(id, model, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _documents.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<VehicleDocumentModel>> Approve(int id, CancellationToken ct)
        => Ok(await _documents.ApproveAsync(id, ct));

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<VehicleDocumentModel>> Reject(int id, ReasonModel model, CancellationToken ct)
        => Ok(await _documents.RejectAsync(id, model.Reason, ct));
}
