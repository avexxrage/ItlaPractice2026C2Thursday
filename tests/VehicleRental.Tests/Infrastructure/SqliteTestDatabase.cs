using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Infrastructure.Context;

namespace VehicleRental.Tests.Infrastructure;

/// <summary>
/// Base de datos SQLite en memoria compartida entre varios contextos de una misma prueba.
/// Usa el mismo motor que la API, así se validan relaciones, índices y restricciones reales.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<VehicleRentalContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<VehicleRentalContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Cada operación usa un contexto nuevo para comprobar lo que realmente quedó guardado.</summary>
    public VehicleRentalContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
