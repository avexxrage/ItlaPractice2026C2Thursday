using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicles;

    public VehiclesController(IVehicleRepository vehicles) => _vehicles = vehicles;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleModel>>> GetAll([FromQuery] VehicleFilterModel filter, CancellationToken ct)
        => Ok(await _vehicles.GetModelsAsync(filter, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleModel>> GetById(int id, CancellationToken ct)
        => Ok(await _vehicles.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleModel>> Create(VehicleCreateModel model, CancellationToken ct)
    {
        var vehicle = await _vehicles.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleModel>> Update(int id, VehicleUpdateModel model, CancellationToken ct)
        => Ok(await _vehicles.UpdateAsync(id, model, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _vehicles.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<VehicleModel>> Publish(int id, CancellationToken ct)
        => Ok(await _vehicles.PublishAsync(id, ct));

    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<VehicleModel>> Deactivate(int id, CancellationToken ct)
        => Ok(await _vehicles.DeactivateAsync(id, ct));

    [HttpPost("{id:int}/maintenance")]
    public async Task<ActionResult<VehicleModel>> SendToMaintenance(int id, CancellationToken ct)
        => Ok(await _vehicles.SendToMaintenanceAsync(id, ct));
}
