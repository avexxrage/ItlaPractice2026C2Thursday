using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/vehicle-availabilities")]
public sealed class VehicleAvailabilitiesController : ControllerBase
{
    private readonly IVehicleAvailabilityRepository _availabilities;

    public VehicleAvailabilitiesController(IVehicleAvailabilityRepository availabilities) => _availabilities = availabilities;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleAvailabilityModel>>> GetByVehicle([FromQuery] int vehicleId, CancellationToken ct)
        => Ok(await _availabilities.GetByVehicleAsync(vehicleId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleAvailabilityModel>> GetById(int id, CancellationToken ct)
        => Ok(await _availabilities.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleAvailabilityModel>> Create(VehicleAvailabilityCreateModel model, CancellationToken ct)
    {
        var period = await _availabilities.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = period.Id }, period);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleAvailabilityModel>> Update(int id, VehicleAvailabilityUpdateModel model, CancellationToken ct)
        => Ok(await _availabilities.UpdateAsync(id, model, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _availabilities.DeleteAsync(id, ct);
        return NoContent();
    }
}
