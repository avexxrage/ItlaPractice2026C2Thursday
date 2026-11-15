using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/providers")]
public sealed class ProvidersController : ControllerBase
{
    private readonly IProviderRepository _providers;

    public ProvidersController(IProviderRepository providers) => _providers = providers;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProviderModel>>> GetAll(CancellationToken ct)
        => Ok(await _providers.GetModelsAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProviderModel>> GetById(int id, CancellationToken ct)
        => Ok(await _providers.GetModelByIdAsync(id, ct));

    [HttpPost("individual")]
    public async Task<ActionResult<ProviderModel>> CreateIndividual(ProviderIndividualCreateModel model, CancellationToken ct)
    {
        var provider = await _providers.CreateIndividualAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = provider.Id }, provider);
    }

    [HttpPost("rent-a-car")]
    public async Task<ActionResult<ProviderModel>> CreateRentACar(ProviderCompanyCreateModel model, CancellationToken ct)
    {
        var provider = await _providers.CreateRentACarAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = provider.Id }, provider);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _providers.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/verify")]
    public async Task<ActionResult<ProviderModel>> Verify(int id, CancellationToken ct)
        => Ok(await _providers.VerifyAsync(id, ct));

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ProviderModel>> Reject(int id, CancellationToken ct)
        => Ok(await _providers.RejectAsync(id, ct));
}
