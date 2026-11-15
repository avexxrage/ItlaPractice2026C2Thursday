using Microsoft.EntityFrameworkCore;
using VehicleRental.API.Services;
using VehicleRental.Domain.Core;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Repositories;

namespace VehicleRental.API.Extensions;

/// <summary>
/// Registro en el contenedor de inyección de dependencias del contexto, los repositorios y servicios.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVehicleRentalInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("VehicleRental")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'VehicleRental'.");

        services.AddDbContext<VehicleRentalContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleDocumentRepository, VehicleDocumentRepository>();
        services.AddScoped<IVehicleAvailabilityRepository, VehicleAvailabilityRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ISecurityDepositRepository, SecurityDepositRepository>();
        services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();
        services.AddScoped<IDamageClaimRepository, DamageClaimRepository>();

        return services;
    }

    public static IServiceCollection AddVehicleRentalServices(this IServiceCollection services, IConfiguration configuration)
    {
        var depositSettings = configuration.GetSection("SecurityDeposit").Get<SecurityDepositSettings>() ?? new SecurityDepositSettings();
        services.AddSingleton(new SecurityDepositCalculator(depositSettings));
        services.AddScoped<ReservationService>();

        return services;
    }

    /// <summary>
    /// Crea la base de datos SQLite si no existe. No borra datos existentes.
    /// Cuando el proyecto adopte migraciones, sustituir por <c>Database.MigrateAsync()</c>.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<VehicleRentalContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
