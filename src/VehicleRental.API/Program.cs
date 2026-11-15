using System.Text.Json.Serialization;
using VehicleRental.API.Extensions;
using VehicleRental.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddVehicleRentalInfrastructure(builder.Configuration);
builder.Services.AddVehicleRentalServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.EnsureDatabaseCreatedAsync();

app.Run();

/// <summary>Expuesto para pruebas de integración.</summary>
public partial class Program
{
}
