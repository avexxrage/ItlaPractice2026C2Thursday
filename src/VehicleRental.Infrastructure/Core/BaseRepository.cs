using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Domain.Core;
using VehicleRental.Domain.Repository;
using VehicleRental.Infrastructure.Context;
using VehicleRental.Infrastructure.Exceptions;

namespace VehicleRental.Infrastructure.Core;

/// <summary>
/// Implementación genérica de <see cref="IBaseRepository{TEntity}"/> con EF Core.
/// Los repositorios concretos heredan de esta clase y agregan sus consultas específicas.
/// </summary>
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected BaseRepository(VehicleRentalContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Entities = context.Set<TEntity>();
    }

    protected VehicleRentalContext Context { get; }

    protected DbSet<TEntity> Entities { get; }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Entities.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Entities.AsNoTracking().OrderBy(e => e.Id).ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Entities.AsNoTracking().Where(predicate).OrderBy(e => e.Id).ToListAsync(cancellationToken);

    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Entities.AnyAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Entities.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Remove(entity);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new PersistenceException("Los datos fueron modificados por otra operación. Vuelva a intentarlo.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("No se pudieron guardar los cambios porque violan una restricción de la base de datos.", ex);
        }
    }

    /// <summary>Obtiene la entidad (con seguimiento) o lanza <see cref="EntityNotFoundException"/>.</summary>
    protected async Task<TEntity> GetRequiredAsync(int id, CancellationToken cancellationToken)
        => await GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException(typeof(TEntity).Name, id);

    /// <summary>Lanza <see cref="EntityNotFoundException"/> si no existe la entidad relacionada indicada.</summary>
    protected async Task EnsureExistsAsync<TOther>(int id, CancellationToken cancellationToken)
        where TOther : BaseEntity
    {
        if (!await Context.Set<TOther>().AnyAsync(e => e.Id == id, cancellationToken))
        {
            throw new EntityNotFoundException(typeof(TOther).Name, id);
        }
    }
}
