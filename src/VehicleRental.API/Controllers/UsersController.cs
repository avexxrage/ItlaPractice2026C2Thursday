using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Models;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserModel>>> GetAll(CancellationToken ct)
        => Ok(await _users.GetModelsAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserModel>> GetById(int id, CancellationToken ct)
        => Ok(await _users.GetModelByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<UserModel>> Create(UserCreateModel model, CancellationToken ct)
    {
        var user = await _users.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserModel>> Update(int id, UserUpdateModel model, CancellationToken ct)
        => Ok(await _users.UpdateAsync(id, model, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _users.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<UserModel>> Activate(int id, CancellationToken ct)
        => Ok(await _users.ActivateAsync(id, ct));

    [HttpPost("{id:int}/suspend")]
    public async Task<ActionResult<UserModel>> Suspend(int id, CancellationToken ct)
        => Ok(await _users.SuspendAsync(id, ct));

    [HttpPost("{id:int}/verify-renter")]
    public async Task<ActionResult<UserModel>> VerifyRenter(int id, CancellationToken ct)
        => Ok(await _users.VerifyRenterAsync(id, ct));

    [HttpPost("{id:int}/reject-renter")]
    public async Task<ActionResult<UserModel>> RejectRenter(int id, CancellationToken ct)
        => Ok(await _users.RejectRenterAsync(id, ct));
}
