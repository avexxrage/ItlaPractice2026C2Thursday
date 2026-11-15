using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository _companies;

    public CompaniesController(ICompanyRepository companies) => _companies = companies;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyModel>>> GetAll(CancellationToken ct)
        => Ok(await _companies.GetModelsAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyModel>> GetById(int id, CancellationToken ct)
        => Ok(await _companies.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<CompanyModel>> Create(CompanyCreateModel model, CancellationToken ct)
    {
        var company = await _companies.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompanyModel>> Update(int id, CompanyUpdateModel model, CancellationToken ct)
        => Ok(await _companies.UpdateAsync(id, model, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _companies.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/verify")]
    public async Task<ActionResult<CompanyModel>> Verify(int id, CancellationToken ct)
        => Ok(await _companies.VerifyAsync(id, ct));

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<CompanyModel>> Reject(int id, CancellationToken ct)
        => Ok(await _companies.RejectAsync(id, ct));
}
